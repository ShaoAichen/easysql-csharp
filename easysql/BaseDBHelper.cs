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

        public int Execute(String sql,params Object[] paramValues)
        {
            using(BaseDatabase db = CreateDatabase())
            {
                return db.Execute(sql, paramValues);
            }
        }
        public DataTable QueryDataTable(String sql,params Object[] paramValues)
        {
            using(BaseDatabase db = CreateDatabase())
            {
                return db.QueryDataTable(sql, paramValues);
            }
        }




    }
}
