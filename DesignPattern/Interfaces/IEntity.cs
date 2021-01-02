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
      public static IEnumerable<T> Where(Expression<Func<T, bool>> predicate)
      {
         return CustomDatabase.Database.GetEntityListWhere<T>(predicate);
      }
   }
}
