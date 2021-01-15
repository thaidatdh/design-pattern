using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern
{
   public abstract class IEntity<T>
   {
      public static List<T> GetAll()
      {
         return DatabaseContext.GetInstance().GetAllEntityList<T>();
      }
      public static bool DeleteAll()
      {
         return DatabaseContext.GetInstance().TruncateTable<T>();
      }
      public static Query<T, S> Select<S>(Expression<Func<T, S>> select)
      {
         Query<T, S> query = new Query<T, S>();
         query.SelectExpression = select;
         return query;
      }
      public static Query<T, object> Where(Expression<Func<T, bool>> where)
      {
         Query<T, object> query = new Query<T, object>();
         query.WhereExpression = where;
         return query;
      }
      public static UpdateQuery<T> Update(Expression<Func<T, bool>> update)
      {
         UpdateQuery<T> query = new UpdateQuery<T>();
         query.UpdateExpression = update;
         return query;
      }
      public static int DeleteWhere(Expression<Func<T, bool>> where)
      {
         return DatabaseContext.GetInstance().DeleteWhereQuery<T>(where);
      }
      public static void BulkDelete(List<T> listEntity)
      {
         DatabaseContext.GetInstance().BulkDelete(listEntity);
      }
      public static void BulkUpdate(List<T> listEntity)
      {
         DatabaseContext.GetInstance().BulkUpdate(listEntity);
      }
      public static void BulkInsert(List<T> listEntity, bool insertIncludeID = false)
      {
         foreach (T entity in listEntity)
            ((EntityInterface)entity).Insert(insertIncludeID);
      }
   }
}
