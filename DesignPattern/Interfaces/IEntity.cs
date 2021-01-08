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
         return CustomDatabase.Database.GetAllEntityList<T>();
      }
      public abstract int Insert();
      public abstract bool Update();
      public abstract bool Delete();
      public static bool DeleteAll()
      {
         return CustomDatabase.Database.TruncateTable<T>();
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
         return CustomDatabase.Database.DeleteWhereQuery<T>(where);
      }
   }
}
