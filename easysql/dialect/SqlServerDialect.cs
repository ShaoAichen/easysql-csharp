using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace easysql.dialect
{
    public class SqlServerDialect : Dialect
    {
        public override string getLimitString(string sql, int? start, int? maxResult,ref int i,List<Object> paramList)
        {
            if (start == null && maxResult == null)
            {
                return sql;
            }
            int selectIndex = sql.ToLower().IndexOf("select");

            var m = new Regex("select\\sdistinct").Match(sql.ToLower());
            int selectDistinctIndex = m.Success ? m.Index : -1;

            sql = sql.Insert(selectIndex + (selectDistinctIndex == selectIndex ? 15 : 6), " top 100 percent ");

            String str = @"select * from (
                select *,row_number() over (order by current_timestamp) as _easysql_rn from ("
                + sql +
                @") easysql_query 
                  ) easysql_result where 1=1 ";


            if (start != null)
            {
                str += " and _easysql_rn>={" + i++ + "} ";
                paramList.Add(start);
            }

            if(maxResult != null)
            {
                if(start==null || start < 1)
                {
                    str += " and _easysql_rn<={" + i++ + "} ";
                    paramList.Add(maxResult);
                }
                else
                {
                    str += " and _easysql_rn<{" + i++ + "} ";
                    paramList.Add(start + maxResult);
                }
            }
            return str;

        }

    }
}
