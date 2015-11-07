using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;
using easysql.dialect;
using System.Reflection;
namespace easysql
{
    /// <summary>
    /// 数据库连接类基类
    /// 数据库操作类基类
    /// 在使用事务时线程不安全
    /// 每次只能使用一个事务
    /// </summary>
    public abstract class BaseDatabase : IDisposable
    {
        private DbConnection _dbConnection;//数据库连接
        private DbTransaction _dbTranscation;//事务对象
        protected string _paramNamePrefix;
        protected string _paramPrefix;
        protected int _executeTimeout = 20;//执行超时时间

        private Dialect _dialect;

        public BaseDatabase(Dialect dialect,String paramNamePrefix,String paramPrefix)
        {
            this._dialect = dialect;
            this._paramNamePrefix = paramNamePrefix;
            this._paramPrefix = paramPrefix;
        }

        //获取连接的方法(获取并打开)
        protected abstract DbConnection GetConnection();
        //关闭释放连接的方法
        protected virtual void closeConnection(DbConnection connection)
        {
            if(connection != null)
            {
                connection.Close();
                connection.Dispose();
            }
        }
        protected abstract DbDataAdapter CreateAdapter(DbCommand cmd);

        protected abstract string AutoIncreSql();

        public void Dispose()
        {
            if (_dbConnection != null)
            {
                closeConnection(_dbConnection);
                _dbConnection = null;
            }
            if(_dbTranscation != null)
            {
                _dbTranscation.Dispose();
                _dbTranscation = null;
            }
        }

        #region 数据库基本方法
        /// <summary>
        /// 执行更新操作
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramValues"></param>
        /// <returns></returns>
        public virtual int Execute(String sql,params Object[] paramValues)
        {
            using (DbCommand cmd = CreateDbCommand())
            {
                RetreatCmd(sql, cmd, paramValues);
                int rowCount = cmd.ExecuteNonQuery();
                return rowCount;  
            }
        }

        /// <summary>
        /// sql查询
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramValues"></param>
        /// <returns></returns>
        public virtual DataTable QueryDataTable(String sql,params Object[] paramValues)
        {
            using (DbCommand cmd = CreateDbCommand())
            {
                RetreatCmd(sql, cmd, paramValues);
                using (DbDataAdapter adp = CreateAdapter(cmd))
                {
                    DataTable dt = new DataTable("query");
                    adp.Fill(dt);
                    return dt;
                }
            }
        }

        public virtual Object ExecuteScalar(String sql, params Object[] paramValues)
        {
            return QueryScalar(sql, paramValues);
        }
        public virtual Object QueryScalar(String sql,params Object[] paramValues)
        {
            using (DbCommand cmd = CreateDbCommand())
            {
                RetreatCmd(sql, cmd, paramValues);
                Object result = cmd.ExecuteScalar();
                return result;
            }
        }


        #endregion 数据库基本方法


        #region 数据库扩展方法
        public virtual DataTable QueryDataTable(int start,int maxResult,String sql,params Object[] paramValues)
        {
            int i = 0;
            var paramList = new List<Object>();
            sql = this._dialect.getLimitString(sql, start, maxResult, ref i,paramList);
            return QueryDataTable(sql,paramList.ToArray());
        }
        /// <summary>
        /// 查询并将结果转换为实体集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="paramValues"></param>
        /// <returns></returns>
        public virtual List<T> Query<T>(String sql,params Object[] paramValues) where T :new()
        {
            var dt = this.QueryDataTable(sql, paramValues);
            return _dialect.TableToList<T>(dt);
        }

        public virtual List<T> Query<T>(int start,int maxResult,String sql,params Object[] paramValues) where T : new()
        {
            var dt = this.QueryDataTable(start, maxResult, sql, paramValues);
            return _dialect.TableToList<T>(dt);
        }

        /// <summary>
        /// 根据一定的规则查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tbname"></param>
        /// <param name="bean">根据bean计算出查询where语句查询,可以为null</param>
        /// <param name="restrains"></param>
        /// <returns></returns>
        public virtual List<T> Query<T>(String tbname,T bean,params Restrain[] restrains) where T :new()
        {

            StringBuilder sqlWhere = new StringBuilder();
            List<Object> paramValueList = new List<Object>();
            int i = 0;
            _dialect.TransWhere<T>(bean, sqlWhere, paramValueList, ref i, restrains);

            String sql = "select * from " + tbname + " where 1=1 " + sqlWhere;
            sql = _dialect.getOrderString(sql, paramValueList, restrains.ToArray());

            sql = _dialect.getLimitString(sql, ref i, paramValueList, restrains);

            return Query<T>(sql, paramValueList.ToArray());
        }
        /// <summary>
        /// 添加对象到数据库,
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tbname"></param>
        /// <param name="model"></param>
        /// <param name="autoSetId"></param>
        public virtual void Add<T>(String tbname,T model,Boolean autoSetId)
        {
            if (model == null)
            {
                throw new Exception("添加的对象不能为空");
            }
            var sql1 = new StringBuilder();
            var sql2 = new StringBuilder();
            var i = 0;
            var paramValueList = new List<Object>();
            Type type = model.GetType();
            PropertyInfo[] pis = type.GetProperties();
            foreach(PropertyInfo pi in pis)
            {
                var name = pi.Name;
                var value = pi.GetValue(model, null);
                if(value == null || value.Equals(_dialect.DefaultForType(pi.GetType())))
                {
                    //如果值是默认状态,则跳过
                    continue;
                }

                sql1.Append(name + ",");
                sql2.AppendFormat("{{{0}}},", i++);
                paramValueList.Add(value);
            }

            if (sql1.Length > 0)
            {
                sql1.Remove(sql1.Length - 1, 1);//移除最后的","
                sql2.Remove(sql2.Length - 1, 1);
            }

            var sql = "insert into " + tbname + " (" + sql1 + ") values (" + sql2 + ")";
            if (autoSetId)
            {
                var cmdText = sql + " " + AutoIncreSql();
                object id = int.Parse(ExecuteScalar(cmdText, paramValueList.ToArray()).ToString());
                SetIdValue(model, id);//设置model的Id属性的值
            }
            else
            {
                Execute(sql, paramValueList.ToArray());
            }

        }

        /// <summary>
        /// 将对象添加到数据库中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tbname"></param>
        /// <param name="model"></param>
        public virtual void Add<T>(String tbname,T model)
        {
            Add<T>(tbname, model, false);
        }
        /// <summary>
        /// 将对象添加到数据库中,并设置添加记录的id到model中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tbname"></param>
        /// <param name="model"></param>
        public virtual void Add_AutoSetId<T>(String tbname,T model)
        {
            Add<T>(tbname, model, true);
        }
        /// <summary>
        /// 删除满足条件的所有记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tbname"></param>
        /// <param name="bean"></param>
        /// <param name="restrains">只采用条件类型的,不采用Order,start,maxResult等</param>
        /// <returns></returns>
        public virtual int Del<T>(String tbname,T bean,params Restrain[] restrains)
        {
            var sqlWhere = new StringBuilder();
            var paramValueList = new List<Object>();
            int i = 0;
            _dialect.TransWhere<T>(bean, sqlWhere, paramValueList,ref i, restrains);

            var sql = "delete from " + tbname + " where 1=1 " + sqlWhere;

            return Execute(sql, paramValueList.ToArray());

        }

        public Boolean DelById(String tbname,int id)
        {
            var sql = "delete from " + tbname + " where id={0}";
            return Execute(sql, id) == 1;
        }

        /// <summary>
        /// 修改id为model.id的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tbname"></param>
        /// <param name="model"></param>
        /// <param name="mustPros">可以指定一定要修改的参数</param>
        /// <returns>返回是否更改了记录,如果没有需要更改的参数也会返回false</returns>
        public Boolean Modify<T>(String tbname,T model,String[] mustPros)
        {
            HashSet<string> hs = new HashSet<string>();
            if (mustPros != null)
            {
                foreach (var key in mustPros)
                {
                    hs.Add(key.ToLower());
                }
            }
            

            var sql1 = new StringBuilder();
            var i = 0;
            var paramValueList = new List<Object>();
            Type type = model.GetType();
            PropertyInfo[] pis = type.GetProperties();
            foreach (PropertyInfo pi in pis)
            {
                String name = pi.Name;
                Object value = pi.GetValue(model, null);

                if (!hs.Contains(name))
                {
                    if (value==null || value.Equals(_dialect.DefaultForType(pi.PropertyType)))
                    {
                        continue;
                    }
                    else if (name.ToLower().Equals("id"))
                    {
                        continue;
                    }
                }
                sql1.AppendFormat("{0}={{{1}}},", name, i++);

                if (value is DateTime && (DateTime)value == DateTime.MinValue)
                {
                    paramValueList.Add(null);
                }
                else
                {
                    paramValueList.Add(value);
                }
            }

            if (sql1.Length == 0)
            {
                //没有需要修改的
                return false;
            }
            sql1 = sql1.Remove(sql1.Length - 1, 1);
            var sql = String.Format("update {0} set {1} where Id={{{2}}}", tbname, sql1, i++);

            object id = GetIdValue(model);
            paramValueList.Add(id);
            return Execute(sql, paramValueList.ToArray()) == 1;
        }

        /// <summary>
        /// 修改Id为model.Id的数据
        /// 无法将int类型修改为0,无法将DateTime类型修改为DateTime.minValue....
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tbname"></param>
        /// <param name="model"></param>
        /// <returns>返回是否更改了记录,如果没有需要更改的参数也会返回false</returns>
        public Boolean Modify<T>(String tbname, T model)
        {
            return Modify<T>(tbname, model, null);
        }
        /// <summary>
        /// 通过约束修改记录(id字段将无效),返回修改记录的条数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tbname"></param>
        /// <param name="bean"></param>
        /// <param name="mustPros">必须修改的字段</param>
        /// <param name="restrains"></param>
        /// <returns></returns>
        public int ModifyByRestrain<T>(String tbname,T model,String[] mustPros,params Restrain[] restrains)
        {
            HashSet<string> hs = new HashSet<string>();
            if (mustPros != null)
            {
                foreach (var key in mustPros)
                {
                    hs.Add(key.ToLower());
                }
            }


            var sql1 = new StringBuilder();
            var i = 0;
            var paramValueList = new List<Object>();
            Type type = model.GetType();
            PropertyInfo[] pis = type.GetProperties();
            foreach (PropertyInfo pi in pis)
            {
                String name = pi.Name;
                Object value = pi.GetValue(model, null);

                if (!hs.Contains(name))
                {
                    if (value == null || value.Equals(_dialect.DefaultForType(pi.PropertyType)))
                    {
                        continue;
                    }
                    else if (name.ToLower().Equals("id"))
                    {
                        continue;
                    }
                }
                sql1.AppendFormat("{0}={{{1}}},", name, i++);

                if (value is DateTime && (DateTime)value == DateTime.MinValue)
                {
                    paramValueList.Add(null);
                }
                else
                {
                    paramValueList.Add(value);
                }
            }

            if (sql1.Length == 0)
            {
                //没有需要修改的
                return 0;
            }
            sql1 = sql1.Remove(sql1.Length - 1, 1);
            StringBuilder sqlWhere = new StringBuilder();

            _dialect.TransWhere<T>(default(T), sqlWhere, paramValueList, ref i, restrains);
            var sql = String.Format("update {0} set {1} where 1=1 and {2}", tbname, sql1,sqlWhere.ToString());

            return Execute(sql, paramValueList.ToArray());
        }




        #endregion 数据库扩展方法



        #region 辅助方法
        /// <summary>
        /// 重新格式化处理查询语句并生成查询参数
        /// 即select * from tb where id={0} and name={1}转化为类似select * from tb where id=@params1,@parms2的形式,并将参数值添加到cmd中
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="cmd"></param>
        /// <param name="paramValues"></param>
        protected void RetreatCmd(String sql, DbCommand cmd, object[] paramValues)
        {
            Console.WriteLine("执行:"+sql);
            cmd.CommandTimeout = _executeTimeout;
            //如果无参数则直接返回
            if (paramValues == null || paramValues.Length == 0)
            {
                cmd.CommandText = sql;
                return;
            }
            String[] paramsName = new string[paramValues.Length];
            for (int i = 0; i < paramValues.Length; i++)
            {
                //创建查询新参数
                DbParameter oneParam = cmd.CreateParameter();
                paramsName[i] = _paramPrefix + i.ToString();
                oneParam.ParameterName = _paramNamePrefix + i.ToString();
                oneParam.Value = paramValues[i] ?? DBNull.Value;//这里有修改的空间,判断是否为初始值
                //添加至command对象参数集中
                cmd.Parameters.Add(oneParam);
            }
            sql = String.Format(sql, paramsName.ToArray());

            cmd.CommandText = sql;

        }

        private void SetIdValue(Object model, Object value)
        {
            if (model == null)
            {
                return;
            }

            var ps = model.GetType().GetProperties();
            foreach (var p in ps)
            {
                if (p.Name.ToUpper().Equals("ID"))
                {
                    p.SetValue(model, value, null);
                    continue;
                }
            }

        }
        private Object GetIdValue(Object model)
        {
            var ps = model.GetType().GetProperties();
            foreach (var p in ps)
            {
                if (p.Name.ToUpper().Equals("ID"))
                {
                    return p.GetValue(model, null);
                }
            }
            return null;
        }


        #endregion 辅助方法

        #region 事务核心辅助方法
        private DbCommand CreateDbCommand()
        {
            if (_dbConnection == null)
            {
                _dbConnection = GetConnection();
            }
            var cmd = _dbConnection.CreateCommand();
            if(_dbTranscation != null)
            {
                cmd.Transaction = _dbTranscation;
            }

            return cmd;
        }

        #endregion 事务核心辅助方法

        #region 事务
        /// <summary>
        /// 开启事务
        /// </summary>
        public void BeginTransaction()
        {

            if(_dbTranscation != null)
            {
                throw new Exception("开启事务失败,有线程正在开启事务并没有释放(该类的对象每次只能使用一个事务)");
            }
            if(_dbConnection == null)
            {
                _dbConnection = GetConnection();
            }

            _dbTranscation = _dbConnection.BeginTransaction();
        }
        /// <summary>
        /// 提交事务,并释放(dispose后并设置为null)
        /// 如果没有事务则直接返回
        /// </summary>
        public void CommitTranscation()
        {
            if(_dbTranscation == null)
            {
                return;
            }
            try
            {
                _dbTranscation.Commit();
            }catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                _dbTranscation.Dispose();
                _dbTranscation = null;
            }
        }

        /// <summary>
        /// 回滚事务,并释放
        /// </summary>
        public void RollbackTranscation()
        {
            if (_dbTranscation == null)
            {
                return;
            }
            try
            {
                _dbTranscation.Rollback();
            }
            finally
            {
                _dbTranscation.Dispose();
                _dbTranscation = null;
            }
        }

        #endregion 事务



    }
}
