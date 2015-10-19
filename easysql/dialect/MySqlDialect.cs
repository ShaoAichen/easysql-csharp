using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easysql.dialect
{
    public class MySqlDialect : Dialect
    {
        public override string getLimitString(string sql, int? start, int? maxResult, ref int i, List<object> paramList)
        {
            paramList.Add(start);
            paramList.Add(maxResult);
            return sql + " limit {" + i++ + "},{" + i++ + "}";
        }
    }
}
