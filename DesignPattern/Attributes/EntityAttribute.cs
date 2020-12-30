using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern
{
   [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
   public class EntityAttribute : Attribute
   {
      public string Column { get; set; }
      public DATATYPE DataType { get; set; }
      public bool isPrimaryKey { get; set; }
      public object DefaultValue { get; set; }
      public PropertyInfo PropertyInfo { get; set; }

      public EntityAttribute()
      {
         this.Column = "";
         this.isPrimaryKey = false;
         this.DefaultValue = "";
         this.DataType = DATATYPE.STRING;
      }
      public EntityAttribute(string column, object DefaultValue, DATATYPE DataType, bool isPrimaryKey = false)
      {
         this.Column = column;
         this.isPrimaryKey = isPrimaryKey;
         this.DefaultValue = DefaultValue;
         this.DataType = DataType;
      }
   }

}
