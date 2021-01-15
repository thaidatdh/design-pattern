using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern
{
   public static class EntityQueryExtension
   {
      public static Query<T, S> Where<T, S>(this Query<T, S> query, Expression<Func<T, bool>> where)
      {
         query.WhereExpression = where;
         return query;
      }
      public static Query<T, S> Limit<T, S>(this Query<T, S> query, int? limit)
      {
         if (query == null)
         {
            query = new Query<T, S>();
         }
         query.LimitValue = limit;
         return query;
      }
      public static Query<T, S> OrderBy<T, S>(this Query<T, S> query, Expression<Func<T, string>> orderBy, ORDER orderType = ORDER.Ascending)
      {
         if (query == null)
         {
            query = new Query<T, S>();
         }
         query.OrderList.Add(orderBy);
         query.OrderTypeList.Add(orderType);
         return query;
      }
      public static IEnumerable<T> QueryEntity<T, S>(this Query<T, S> query)
      {
         return DatabaseContext.GetInstance().GetEntityListComplexQuery(query);
      }
      public static IEnumerable<S> QuerySelect<T, S>(this Query<T, S> query)
      {
         return DatabaseContext.GetInstance().GetListComplexSelectField(query);
      }
      public static UpdateQuery<T> Where<T>(this UpdateQuery<T> query, Expression<Func<T, bool>> where)
      {
         if (query == null)
         {
            query = new UpdateQuery<T>();
         }
         query.WhereExpression = where;
         return query;
      }
      public static int QueryUpdate<T>(this UpdateQuery<T> query)
      {
         return DatabaseContext.GetInstance().UpdateComplexQuery(query);
      }
   }
}
