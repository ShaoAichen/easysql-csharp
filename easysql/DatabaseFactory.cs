using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using easysql.dialect;
namespace easysql
{
    /// <summary>
    /// 数据库对象生成类
    /// </summary>
    public class DatabaseFactory
    {

        public static SqlServerDatabase CreateSqlServerDatabase(String connString, Dialect dialect)
        {
            return new SqlServerDatabase(connString,dialect);
        }



    }

}
