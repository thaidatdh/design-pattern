using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern
{
   public class Query<T, S>
   {
      public Expression<Func<T, S>> SelectExpression { get; set; }
      public Expression<Func<T, bool>> WhereExpression { get; set; }
      public List<Expression<Func<T, string>>> OrderList { get; set; }
      public List<ORDER> OrderTypeList { get; set; }
      public int? LimitValue { get; set; }
      public Query()
      {
         SelectExpression = null;
         WhereExpression = null;
         LimitValue = null;
         OrderList = new List<Expression<Func<T, string>>>();
         OrderTypeList = new List<ORDER>();
      }
   }
}
