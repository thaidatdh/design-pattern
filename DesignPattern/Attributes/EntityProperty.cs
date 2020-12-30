using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern
{
   public class EntityProperty
   {
      public string TableName { get; set; }
      public List<PropertyInfo> Properties { get; set; }
      public Dictionary<string, EntityAttribute> AttributeDictionary { get; set; }
      public EntityAttribute PrimaryKeyAttribute { get; set; }
      public string PrimaryKeyPropertyName { get; set; }
   }
}
