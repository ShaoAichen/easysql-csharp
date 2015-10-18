using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easysql
{
    public class DatabaseFactory
    {
        public static MySqlDatabase CreateMySqlDatabase(String connString)
        {
            return new MySqlDatabase(connString);
        }
        public static MySqlDatabase CreateMySqlDatabase(String server,String database,String user,String password)
        {
            return new MySqlDatabase(server, database, user, password);
        }


    }
}
