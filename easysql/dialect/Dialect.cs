using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace easysql.dialect
{
    public abstract class Dialect
    {
        /// <summary>
        /// 将DataRow对象转换成T对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <returns></returns>
        public virtual T RowToEntity<T>(DataRow dr) where T : new()
        {
            if (dr == null)
            {
                return default(T);
            }
            var model = new T();
            Type type = model.GetType();
            PropertyInfo[] pis = type.GetProperties();
            foreach (var p in pis)
            {
                object val = null;
                try
                {
                    val = dr[p.Name];
                }
                catch
                {
                    //如果dr里面没有该列，则跳过
                    continue;
                }

                if (val == DBNull.Value)
                {
                    continue;
                }

                if (p.PropertyType.Equals(typeof(DateTime)))
                {
                    if (val.ToString().Length == 0)
                    {
                        continue;
                    }
                    val = DateTime.Parse(val.ToString());
                }
                else if (p.PropertyType.Equals(typeof(int)))
                {
                    if (val.ToString().Length == 0)
                    {
                        continue;
                    }
                    val = int.Parse(val.ToString());
                }
                p.SetValue(model, val, null);
            }
            return model;
        }

        /// <summary>
        /// 将DataTable对象转换为List<T>对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        public virtual List<T> TableToList<T>(DataTable dt) where T : new()
        {
            if (dt == null)
            {
                return null;
            }
            var list = new List<T>();
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(RowToEntity<T>(dr));
            }
            return list;
        }

        public abstract String getLimitString(String sql, int? start, int? maxResult,ref int i,List<Object> paramValueList);
        public virtual String getLimitString(String sql,ref int i,List<Object> paramValueList,params Restrain[] restrains)
        {

            int? start = null;
            int? maxResult = null;
            foreach(Restrain restrain  in restrains)
            {
                switch (restrain.RestrainType)
                {
                    case RestrainType.start:
                        start = (int)restrain.Values[0];
                        break;
                    case RestrainType.maxResult:
                        maxResult = (int)restrain.Values[0];
                        break;
                }
            }

            return getLimitString(sql, start,maxResult,ref i,paramValueList);
        }
        public virtual String getOrderString(String sql, List<Object> paramValueList, params Restrain[] restrains)
        {
            StringBuilder sqlOrder = new StringBuilder("");
            foreach (Restrain restrain in restrains)
            {
                switch (restrain.RestrainType)
                {
                    case RestrainType.order:
                    case RestrainType.orderdesc:
                        if (sqlOrder.Length == 0)
                        {
                            sqlOrder.Append(" order by " + restrain.Key);
                        }
                        else
                        {
                            sqlOrder.Append("," + restrain.Key);
                        }
                        if (restrain.RestrainType == RestrainType.orderdesc)
                        {
                            sqlOrder.Append(" desc");
                        }
                        break;
                }
            }

            return sql + " " + sqlOrder.ToString();
        }

        public virtual void TransWhere<T>(T bean,StringBuilder sqlWhere,List<Object> paramValueList,ref int i,params Restrain[] restrains)
        {
            if (bean != null && sqlWhere != null)
            {
                //根据bean找出约束
                Type type = bean.GetType();

                PropertyInfo[] pis = type.GetProperties();
                foreach (PropertyInfo pi in pis)
                {
                    String name = pi.Name;
                    Object value = pi.GetValue(bean, null);
                    var pType = pi.PropertyType;
                    if (value == null || value.Equals(DefaultForType(pType)))
                    {
                        //没有赋值或值为原始值，跳过
                        continue;
                    }
                    if (pType.Equals(typeof(String)))
                    {
                        if (value.ToString().Length != 0)
                        {
                            //如果是字符串类型，则加入like约束
                            sqlWhere.AppendFormat(" and {0} like {{{1}}}", name, i++);
                            paramValueList.Add("%" + value + "%");
                        }
                    }
                    else if (pType.IsValueType)
                    {
                        //如果是值类型,则加入等于约束
                        sqlWhere.AppendFormat(" and {0}={{{1}}}", name, i++);
                        paramValueList.Add(value);
                    }

                }
            }

            foreach (var restrain in restrains)
            {
                if (sqlWhere != null)
                {
                    switch (restrain.RestrainType)
                    {
                        case RestrainType.between:
                            var start = restrain.Values[0];
                            var end = restrain.Values[1];
                            if (start != null && !start.Equals(DefaultForType(start.GetType())))
                            {
                                sqlWhere.AppendFormat(" and {0}>={{{1}}}", restrain.Key, i++);
                                paramValueList.Add(start);
                            }
                            if (end != null && !end.Equals(DefaultForType(end.GetType())))
                            {
                                sqlWhere.AppendFormat(" and {0}<={{{1}}}", restrain.Key, i++);
                                paramValueList.Add(end);
                            }
                            break;
                        case RestrainType.inc:
                        case RestrainType.notin:
                            var arr = new List<String>();
                            foreach (var obj in restrain.Values)
                            {
                                arr.Add("'" + obj.ToString().Replace("'", "''") + "'");
                            }
                            String ci = " in ";
                            if (restrain.RestrainType == RestrainType.notin)
                            {
                                ci = " not in ";
                            }
                            sqlWhere.Append(" and " + restrain.Key + " " + ci + " (" + string.Join(",", arr) + ")");
                            break;
                        case RestrainType.eq:
                            sqlWhere.AppendFormat(" and {0}={{{1}}}", restrain.Key, i++);
                            paramValueList.Add(restrain.Values[0]);
                            break;
                        case RestrainType.lt:
                            sqlWhere.AppendFormat(" and {0}<{{{1}}}", restrain.Key, i++);
                            paramValueList.Add(restrain.Values[0]);
                            break;
                        case RestrainType.gt:
                            sqlWhere.AppendFormat(" and {0}>{{{1}}}", restrain.Key, i++);
                            paramValueList.Add(restrain.Values[0]);
                            break;
                        case RestrainType.not:
                            sqlWhere.AppendFormat(" and {0} != {{{1}}}", restrain.Key, i++);
                            paramValueList.Add(restrain.Values[0]);
                            break;
                        case RestrainType.like:
                            sqlWhere.AppendFormat(" and {0} like {{{1}}}", restrain.Key, i++);
                            paramValueList.Add(restrain.Values[0]);
                            break;

                        case RestrainType.add:
                            sqlWhere.Append(" and " + restrain.Values[0] + " ");
                            break;

                    }


                }
            }
        }




   


        public virtual object DefaultForType(Type targetType)
        {
            return !targetType.IsValueType || IsNullableType(targetType) ? null : Activator.CreateInstance(targetType);
        }
        /// <summary>
        /// 判断一个类型是否可以为null
        /// </summary>
        /// <param name="theType"></param>
        /// <returns></returns>
        public virtual bool IsNullableType(Type theType)
        {
            return (theType.IsGenericType && theType.
              GetGenericTypeDefinition().Equals
              (typeof(Nullable<>)));
        }
    }
}
