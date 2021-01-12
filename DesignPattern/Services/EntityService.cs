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
      public static void PassValueByAttribute<T>(Object data, object entity)
      {
         string tableName = GetTableName<T>();
         EntityProperty entityProperty = null;
         if (EntityMap.ContainsKey(tableName))
         {
            entityProperty = EntityMap.GetValue(tableName);
         }
         else
         {
            entityProperty = GenerateEntityMapProperty<T>();
         }
         foreach (PropertyInfo info in entityProperty.Properties)
         {
            EntityAttribute entityAttr = entityProperty.AttributeDictionary.GetValue(info.Name);
            if (entityAttr == null) continue;
            object columnValue = GetValue(entityAttr.Column, entityAttr.DataType, data);
            if (columnValue == null) continue;
            var columnValueType = columnValue.GetType();
            if (columnValueType == typeof(DBNull)) continue;

            switch (entityAttr.DataType)
            {
               case DATATYPE.BOOLEAN:
                  info.SetValue(entity, columnValue.ToBoolean()); break;
               case DATATYPE.BIGINT:
                  info.SetValue(entity, columnValue.ToLong()); break;
               case DATATYPE.GENERATED_ID:
               case DATATYPE.INTEGER:
                  info.SetValue(entity, columnValue.ToInt()); break;
               case DATATYPE.DATE:
                  info.SetValue(entity, Convert.ToString(columnValue)); break;
               case DATATYPE.DOUBLE:
                  info.SetValue(entity, columnValue.ToDouble()); break;
               case DATATYPE.STRING:
                  if (columnValue != null) info.SetValue(entity, columnValue.ToString());
                  else info.SetValue(entity, columnValue);
                  break;
               case DATATYPE.TIMESTAMP:
                  if (columnValue != null) info.SetValue(entity, columnValue.ToString().ToDateTime());
                  break;
               default:
                  info.SetValue(entity, columnValue); break;
            }
         }
      }
      private static EntityProperty GenerateEntityMapProperty<T>()
      {
         string tableName = GetTableName<T>();
         EntityProperty entityProperty = new EntityProperty();
         entityProperty.TableName = tableName;

         List<PropertyInfo> propertyInfos = new List<PropertyInfo>();
         GetProperties<T>(ref propertyInfos);

         Type baseType = entityProperty.GetType().BaseType;
         while (baseType != null && !(baseType is Object))
         {
            GetProperties(baseType, ref propertyInfos);
            baseType = baseType.BaseType;
         }

         entityProperty.Properties = propertyInfos;
         entityProperty.AttributeDictionary = new Dictionary<string, EntityAttribute>();
         foreach (PropertyInfo info in propertyInfos)
         {
            EntityAttribute entityAttr = GetCustomAttribute<T>(info.Name);

            entityProperty.AttributeDictionary[info.Name] = entityAttr;
            if (entityAttr.isPrimaryKey)
            {
               entityProperty.PrimaryKeyAttribute = entityAttr;
               entityProperty.PrimaryKeyPropertyName = entityAttr.PropertyInfo.Name;
            }
         }
         EntityMap[tableName] = entityProperty;
         return entityProperty;
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
         EntityProperties = Assembly.GetExecutingAssembly().GetTypes().Where(n => n.IsClass && n.Namespace == "DesignPattern.Entity").ToDictionary(k => k, v => v.GetProperties(System.Reflection.BindingFlags.Public
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
      public static string GetInheritanceColumn(Type type)
      {
         string str = type.Name;
         if (type.IsDefined(typeof(TableAttribute), true))
            str = ((TableAttribute[])type.GetCustomAttributes(typeof(TableAttribute), true))[0].InheritanceColumn;
         return str;
      }
      public static string GetPrimaryColumn<T>()
      {
         if (EntityProperties == null || EntityProperties.Count == 0)
            InitEntityProperty();
         EntityProperty entityProperty = null;
         string tableName = GetTableName<T>();
         if (EntityMap.ContainsKey(tableName))
         {
            entityProperty = EntityMap.GetValue(tableName);
         }
         else
         {
            entityProperty = GenerateEntityMapProperty<T>();
         }
         if (entityProperty == null) return null;
         return entityProperty.PrimaryKeyAttribute.Column;
      }
      public static void SetPrimaryKeyData<T>(T obj, object value)
      {
         if (EntityProperties == null || EntityProperties.Count == 0)
            InitEntityProperty();
         EntityProperty entityProperty = null;
         string tableName = GetTableName<T>();
         if (EntityMap.ContainsKey(tableName))
         {
            entityProperty = EntityMap.GetValue(tableName);
         }
         else
         {
            entityProperty = GenerateEntityMapProperty<T>();
         }
         if (entityProperty == null) return;
         
         PropertyInfo primaryKeyPropertyInfo = entityProperty.Properties.Where(p => p.Name.Equals(entityProperty.PrimaryKeyPropertyName)).FirstOrDefault();
         if (primaryKeyPropertyInfo != null)
         {
            primaryKeyPropertyInfo.SetValue(obj, value);
         }
      }
      public static void SetData<T>(T obj, string propertyName, object value)
      {
         if (EntityProperties == null || EntityProperties.Count == 0)
            InitEntityProperty();
         EntityProperty entityProperty = null;
         string tableName = GetTableName<T>();
         if (EntityMap.ContainsKey(tableName))
         {
            entityProperty = EntityMap.GetValue(tableName);
         }
         else
         {
            entityProperty = GenerateEntityMapProperty<T>();
         }
         if (entityProperty == null) return;

         PropertyInfo propertyInfo = entityProperty.Properties.Where(p => p.Name.Equals(propertyName)).FirstOrDefault();
         if (propertyInfo != null)
         {
            propertyInfo.SetValue(obj, value);
         }
      }
      public static void SetDataByColumnName<T>(T obj, string columnName, object value)
      {
         if (EntityProperties == null || EntityProperties.Count == 0)
            InitEntityProperty();
         EntityProperty entityProperty = null;
         string tableName = GetTableName<T>();
         if (EntityMap.ContainsKey(tableName))
         {
            entityProperty = EntityMap.GetValue(tableName);
         }
         else
         {
            entityProperty = GenerateEntityMapProperty<T>();
         }
         if (entityProperty == null) return;

         PropertyInfo propertyInfo = entityProperty.Properties.Where(p => p.GetCustomAttribute<EntityAttribute>().Column.Equals(columnName)).FirstOrDefault();
         if (propertyInfo != null)
         {
            propertyInfo.SetValue(obj, value);
         }
      }
      public static EntityProperty GetEntityProperties<T>(string tableName)
      {
         EntityProperty result = new EntityProperty();
         List<PropertyInfo> m_propertyInfos = new List<PropertyInfo>();
         GetProperties<T>(ref m_propertyInfos);

         if (tableName.Equals("") || m_propertyInfos.Count == 0) { return null; }

         result.TableName = tableName;
         result.Properties = m_propertyInfos;
         result.AttributeDictionary = new Dictionary<string, EntityAttribute>();

         if (m_propertyInfos.Count > 0)
         {
            foreach (PropertyInfo info in m_propertyInfos)
            {
               EntityAttribute attr = GetCustomAttribute<T>(info.Name);
               if (attr == null) continue;
               result.AttributeDictionary[info.Name] = attr;
               if (attr.isPrimaryKey && result.PrimaryKeyAttribute == null)
               {
                  result.PrimaryKeyAttribute = attr;
                  result.PrimaryKeyPropertyName = info.Name;
               }
            }
         }
         return result;
      }
      public static T GetDefaultGeneric<T>(T input) => default(T);
   }
}
