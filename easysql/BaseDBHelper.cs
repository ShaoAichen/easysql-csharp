using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easysql
{
    public abstract class BaseDBHelper
    {
        public abstract BaseDatabase CreateDatabase();

        #region 数据库基础
        public virtual int Execute(String sql,params Object[] paramValues)
        {
            using(BaseDatabase db = CreateDatabase())
            {
                return db.Execute(sql, paramValues);
            }
        }
        public virtual DataTable QueryDataTable(String sql,params Object[] paramValues)
        {
            using(BaseDatabase db = CreateDatabase())
            {
                return db.QueryDataTable(sql, paramValues);
            }
        }

        public virtual Object QueryScalar(String sql,params Object[] paramValues)
        {
            using(BaseDatabase db = CreateDatabase())
            {
                return db.QueryScalar(sql, paramValues);
            }
        }

        #endregion 数据库基础

        #region 数据库扩展方法
        public virtual DataTable QueryDataTable(int start, int maxResult, String sql, params Object[] paramValues)
        {
           using(BaseDatabase db = CreateDatabase())
            {
                return db.QueryDataTable(start, maxResult, sql, paramValues);
            }
        }
        /// <summary>
        /// 查询并将结果转换为实体集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="paramValues"></param>
        /// <returns></returns>
        public virtual List<T> Query<T>(String sql, params Object[] paramValues) where T : new()
        {
            using(BaseDatabase db = CreateDatabase())
            {
                return db.Query<T>(sql, paramValues);
            }
        }

        public virtual List<T> Query<T>(int start, int maxResult, String sql, params Object[] paramValues) where T : new()
        {
            using(BaseDatabase db = CreateDatabase())
            {
                return db.Query<T>(start, maxResult, sql, paramValues);
            }
        }

        public virtual List<T> Query<T>(String tbname, T bean, params Restrain[] restrains) where T : new()
        {
            using (BaseDatabase db = CreateDatabase())
            {
                return db.Query<T>(tbname,bean, restrains);
            }
        }

        public virtual void Add<T>(String tbname, T model, Boolean autoSetId)
        {
            using (BaseDatabase db = CreateDatabase())
            {
                db.Add<T>(tbname, model, autoSetId);
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
            using (BaseDatabase db = CreateDatabase())
            {
                db.Add<T>(tbname, model);
            }
        }
        /// <summary>
        /// 将对象添加到数据库中,并设置添加记录的id到model中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tbname"></param>
        /// <param name="model"></param>
        public virtual void Add_AutoSetId<T>(String tbname, T model)
        {
            using (BaseDatabase db = CreateDatabase())
            {
                db.Add_AutoSetId<T>(tbname, model);
            }
        }



        /// <summary>
        /// 删除满足条件的所有记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tbname"></param>
        /// <param name="bean"></param>
        /// <param name="restrains">只采用条件类型的,不采用Order,start,maxResult等</param>
        /// <returns></returns>
        public virtual int Del<T>(String tbname, T bean, params Restrain[] restrains)
        {
            using(BaseDatabase db = CreateDatabase())
            {
                return db.Del<T>(tbname, bean, restrains);
            }

        }

        public Boolean DelById(String tbname, int id)
        {
            using(BaseDatabase db = CreateDatabase())
            {
                return db.DelById(tbname, id);
            }
        }

        /// <summary>
        /// 修改id为model.id的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tbname"></param>
        /// <param name="model"></param>
        /// <param name="mustPros">可以指定一定要修改的参数</param>
        /// <returns>返回是否更改了记录,如果没有需要更改的参数也会返回false</returns>
        public Boolean Modify<T>(String tbname, T model, String[] mustPros)
        {
           using(BaseDatabase db = CreateDatabase())
            {
                return db.Modify<T>(tbname, model, mustPros);
            }
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
            using(BaseDatabase db = CreateDatabase()){
                return db.Modify<T>(tbname, model);
            }
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
        public int ModifyByRestrain<T>(String tbname, T model, String[] mustPros, params Restrain[] restrains)
        {
            using(BaseDatabase db = CreateDatabase())
            {
                return db.ModifyByRestrain<T>(tbname, model, mustPros, restrains);
            }
        }




        #endregion 数据库扩展方法




    }
}
