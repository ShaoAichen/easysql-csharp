using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easysql
{
    public class DBHelperFactory
    {
        public static SqlServerDBHelper CreateSqlServerDBHelper(String connString)
        {
            return new SqlServerDBHelper(connString);
        }
    }

    /// <summary>
    /// 等价于DBHelperFactory,用于简写
    /// </summary>
    public class DHF:DBHelperFactory
    {

    }
}
