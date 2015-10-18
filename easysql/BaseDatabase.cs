using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;

namespace easysql
{
    /// <summary>
    /// 数据库连接类基类
    /// 数据库操作类基类
    /// 线程不安全
    /// 每次只能使用一个事务
    /// </summary>
    public abstract class BaseDatabase : IDisposable
    {
        private DbConnection _dbConnection;//数据库连接
        private DbTransaction _dbTranscation;//事务对象
        protected string _paramNamePrefix;
        protected string _paramPrefix;
        protected int _executeTimeout = 60;//执行超时时间
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
                    DataTable dt = new DataTable();
                    adp.Fill(dt);
                    return dt;
                }
            }
        }

        #endregion 数据库基本方法


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




        #endregion 事务



    }
}
