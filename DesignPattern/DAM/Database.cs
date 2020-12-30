using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.IO;

namespace DesignPattern
{
   public abstract class Database
   {
      protected Dictionary<string, EntityProperty> EntityMap = new Dictionary<string, EntityProperty>(); // table name -> entity properties
      protected Dictionary<Type, List<PropertyInfo>> EntityProperties;
      protected object Connection;
      protected string ConnectionString { get; set; }
      public void SetConnectionString(string connectionString)
      {
         ConnectionString = connectionString;
      }
      protected abstract object GetConnection();
      public abstract bool Open();
      public abstract object CreateCommand(string sql);
      public abstract DataTable ExcuteSelectQuery(object command, string TableName = "TABLE");
      public abstract int ExecuteQuery(object command);
      public abstract int ExecuteInsertQuery(object command);
      public abstract void CloseConnection();

      public void InitEntityProperty()
      {
         EntityProperties = Assembly.GetExecutingAssembly().GetTypes().Where(n => n.IsClass && n.Namespace == "DesignPattern.Entity").ToDictionary(k => k, v => v.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic
             | System.Reflection.BindingFlags.Instance
             | System.Reflection.BindingFlags.DeclaredOnly).ToList());
      }
      public void GetProperties<T>(ref List<PropertyInfo> list)
      {
         if (EntityProperties == null || EntityProperties.Count == 0)
            InitEntityProperty();

         list = EntityProperties[typeof(T)];
      }
      public void GetProperties(Type type, ref List<PropertyInfo> list)
      {
         if (EntityProperties == null || EntityProperties.Count == 0)
            InitEntityProperty();

         list = EntityProperties[type];
      }
      public EntityAttribute GetCustomAttribute<T>(string propertyName)
      {
         PropertyInfo propertyInfo = typeof(T).GetProperties().FirstOrDefault(n => n.Name.Equals(propertyName));
         if (propertyInfo == null) return null;

         Object[] attribute = propertyInfo.GetCustomAttributes(typeof(EntityAttribute), true);

         if (attribute.Length == 0)
         {
            return null;
         }
         List<EntityAttribute> lst = (from n in attribute select (EntityAttribute)n).ToList();
         EntityAttribute attr = lst.FirstOrDefault();
         if (attr != null)
         {
            attr.PropertyInfo = propertyInfo;
            return attr;
         }

         return null;
      }
      public EntityAttribute GetCustomAttribute(Type type, string propertyName)
      {
         PropertyInfo propertyInfo = type.GetProperty(propertyName);
         Object[] attribute = propertyInfo.GetCustomAttributes(typeof(EntityAttribute), true);

         if (attribute.Length > 0)
         {
            if (attribute.Length == 1)
            {
               EntityAttribute myAttribute = (EntityAttribute)attribute[0];
               myAttribute.PropertyInfo = propertyInfo;
               return myAttribute;
            }
            else
            {
               List<EntityAttribute> list = attribute.Select(x => (EntityAttribute)x).ToList();
               foreach (EntityAttribute attr in list)
               {
                  if (attr != null)
                  {
                     attr.PropertyInfo = propertyInfo;
                     return attr;
                  }
               }
            }
         }
         return null;
      }
   }
}
