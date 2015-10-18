using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easysql
{
    public class MySqlDatabase : BaseDatabase
    {
        private String _connString;

        /// <summary>
        /// 通过连接字符串获得连接
        /// </summary>
        /// <param name="connString">server=数据库地址;database=数据库名;Persist Security Info=False;uid=用户名;pwd=密码</param>
        public MySqlDatabase(String connString)
        {
            this._connString = connString;
        }

        public MySqlDatabase(String server,String database,String user,String password)
        {
            this._connString = String.Format("server={0};database={1};Persist Security Info=False;uid={2};pwd={3}",server,database,user,password);
        }

        protected override DbDataAdapter CreateAdapter(DbCommand cmd)
        {
            return new MySqlDataAdapter(cmd as MySqlCommand);
        }

        protected override DbConnection GetConnection()
        {
            DbConnection conn = new MySqlConnection(_connString);
            conn.Open();
            return conn;
        }

    }
}
