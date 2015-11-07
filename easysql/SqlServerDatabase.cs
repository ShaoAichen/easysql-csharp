using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using easysql.dialect;
namespace easysql
{
    public class SqlServerDatabase : BaseDatabase
    {
        private String _connString;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connString">connString">user id=用户名;password=密码;initial catalog=数据库名;Server=服务地址</param>
        public SqlServerDatabase(String connString,Dialect dialect):base(dialect, "@Param", "@Param")
        {
            this._connString = connString;
        }

        protected override string AutoIncreSql()
        {
            return " select @@identity ";
        }

        protected override DbDataAdapter CreateAdapter(DbCommand cmd)
        {
            return new SqlDataAdapter(cmd as SqlCommand);
        }

        protected override DbConnection GetConnection()
        {
            DbConnection conn = new SqlConnection(_connString);
            conn.Open();
            return conn;
        }
    }
}
