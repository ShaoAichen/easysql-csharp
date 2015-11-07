using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using easysql.dialect;
namespace easysql
{
    public class SqlServerDBHelper : BaseDBHelper
    {
        private String _connString;
        private Dialect Dialect { get; set; }

        public SqlServerDBHelper(String connString)
        {
            this._connString = connString;
        }
        public override BaseDatabase CreateDatabase()
        {
            if(Dialect == null)
            {
                Dialect = new SqlServerDialect();
            }

            return DatabaseFactory.CreateSqlServerDatabase(_connString,Dialect);
        }
    }
}
