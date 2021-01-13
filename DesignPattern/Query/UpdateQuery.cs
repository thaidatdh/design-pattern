using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern
{
   public class UpdateQuery<T>
   {
      public Expression<Func<T, bool>> UpdateExpression { get; set; }
      public Expression<Func<T, bool>> WhereExpression { get; set; }
      public UpdateQuery()
      {
         UpdateExpression = null;
         WhereExpression = null;
      }
   }
}
