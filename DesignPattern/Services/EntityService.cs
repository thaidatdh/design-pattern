using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern
{
   public class EntityService
   {
      protected static Dictionary<Type, List<PropertyInfo>> EntityProperties;
      public static Dictionary<string, EntityProperty> EntityMap = new Dictionary<string, EntityProperty>(); // table name -> entity properties
      public static void PassValueByAttribute<T>(Object data, object dto)
      {
         string tableName = GetTableName<T>();
         EntityProperty entity = null;
         if (EntityMap.ContainsKey(tableName))
         {
            entity = EntityMap.GetValue(tableName);
         }
         else
         {
            entity = new EntityProperty();
            entity.TableName = tableName;

            List<PropertyInfo> propertyInfos = new List<PropertyInfo>();
            GetProperties<T>(ref propertyInfos);

            Type baseType = dto.GetType().BaseType;
            while (baseType != null && !(baseType is Object))
            {
               GetProperties(baseType, ref propertyInfos);
               baseType = baseType.BaseType;
            }

            entity.Properties = propertyInfos;
            entity.AttributeDictionary = new Dictionary<string, EntityAttribute>();
            foreach (PropertyInfo info in propertyInfos)
            {
               EntityAttribute dtoAttr = GetCustomAttribute<T>(info.Name);

               entity.AttributeDictionary[info.Name] = dtoAttr;
            }
            EntityMap[tableName] = entity;
         }
         foreach (PropertyInfo info in entity.Properties)
         {
            EntityAttribute dtoAttr = entity.AttributeDictionary.GetValue(info.Name);
            if (dtoAttr == null) continue;
            object columnValue = GetValue(dtoAttr.Column, dtoAttr.DataType, data);
            if (columnValue == null) continue;
            var columnValueType = columnValue.GetType();
            if (columnValueType == typeof(DBNull)) continue;

            switch (dtoAttr.DataType)
            {
               case DATATYPE.BOOLEAN:
                  info.SetValue(dto, columnValue.ToBoolean()); break;
               case DATATYPE.BIGINT:
                  info.SetValue(dto, columnValue.ToLong()); break;
               case DATATYPE.GENERATED_ID:
               case DATATYPE.INTEGER:
                  info.SetValue(dto, columnValue.ToInt()); break;
               case DATATYPE.DATE:
                  info.SetValue(dto, Convert.ToString(columnValue)); break;
               case DATATYPE.DOUBLE:
                  info.SetValue(dto, columnValue.ToDouble()); break;
               case DATATYPE.STRING:
                  if (columnValue != null) info.SetValue(dto, columnValue.ToString());
                  else info.SetValue(dto, columnValue);
                  break;
               case DATATYPE.TIMESTAMP:
                  if (columnValue != null) info.SetValue(dto, columnValue.ToString().ToDateTime());
                  break;
               default:
                  info.SetValue(dto, columnValue); break;
            }
         }
      }
      public static object GetValue(string columnName, DATATYPE dataType, Object data)
      {
         DataRow dr = (DataRow)data;
         if (!dr.Table.Columns.Contains(columnName)) return "";

         switch (dataType)
         {
            case DATATYPE.BOOLEAN:
               return (dr[columnName] == null || dr[columnName].Equals(0)) ? false : dr[columnName];
            case DATATYPE.GENERATED_ID:
            case DATATYPE.INTEGER:
               return dr[columnName];
            case DATATYPE.STRING:
               return dr[columnName];
            case DATATYPE.TIMESTAMP:
               object dataValue = dr[columnName];
               return dataValue;
            case DATATYPE.DOUBLE:
               return dr[columnName];
            case DATATYPE.BIGINT:
               return dr[columnName];
            case DATATYPE.DATE:
               return dr[columnName] == DBNull.Value ? null : dr[columnName];
            default:
               return dr[columnName];
         }
      }
      public static T GetObject<T>(params object[] lstArgument)
      {
         return (T)Activator.CreateInstance(typeof(T), lstArgument);
      }
      public static void InitEntityProperty()
      {
         EntityProperties = Assembly.GetExecutingAssembly().GetTypes().Where(n => n.IsClass && n.Namespace == "DesignPattern.Entity").ToDictionary(k => k, v => v.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic
             | System.Reflection.BindingFlags.Instance
             | System.Reflection.BindingFlags.DeclaredOnly).ToList());
      }
      public static void GetProperties<T>(ref List<PropertyInfo> list)
      {
         if (EntityProperties == null || EntityProperties.Count == 0)
            InitEntityProperty();

         list = EntityProperties[typeof(T)];
      }
      public static void GetProperties(Type type, ref List<PropertyInfo> list)
      {
         if (EntityProperties == null || EntityProperties.Count == 0)
            InitEntityProperty();

         list = EntityProperties[type];
      }
      public static EntityAttribute GetCustomAttribute<T>(string propertyName)
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
      public static EntityAttribute GetCustomAttribute(Type type, string propertyName)
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
      public static string GetTableName(Type type)
      {
         string str = type.Name;
         if (type.IsDefined(typeof(TableAttribute), true))
            str = ((TableAttribute[])type.GetCustomAttributes(typeof(TableAttribute), true))[0].Name;
         return str;
      }
      public static string GetTableName<T>()
      {
         string str = typeof(T).Name;
         if (typeof(T).IsDefined(typeof(TableAttribute), true))
            str = ((TableAttribute[])typeof(T).GetCustomAttributes(typeof(TableAttribute), true))[0].Name;
         return str;
      }
   }
}
