using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern
{
   [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
   internal class TableAttribute : Attribute
   {
      public string Name { get; set; }
      public string InheritanceColumn { get; set; }
      public TableAttribute(string name, string InheritanceColumn = null)
      {
         this.Name = name;
         this.InheritanceColumn = InheritanceColumn;
      }
   }
}
