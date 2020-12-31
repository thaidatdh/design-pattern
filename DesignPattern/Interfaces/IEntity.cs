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
      public static int Insert(T dto)
      {
         throw new NotImplementedException();
      }
      public static bool Update(T dto)
      {
         throw new NotImplementedException();
      }
   }
}
