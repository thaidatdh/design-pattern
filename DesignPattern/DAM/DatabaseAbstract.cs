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
using System.Linq.Expressions;

namespace DesignPattern
{
   public abstract class DatabaseAbstract
   {
      private static Dictionary<ExpressionType, string> actionType = new Dictionary<ExpressionType, string>()
      {
         { ExpressionType.Equal, "=" },
         {  ExpressionType.AndAlso, "and" },
         {  ExpressionType.OrElse, "or" },
         {  ExpressionType.GreaterThan, ">" },
         {  ExpressionType.GreaterThanOrEqual, ">=" },
         {  ExpressionType.LessThan, "<" },
         {  ExpressionType.LessThanOrEqual, "<=" },
         {  ExpressionType.NotEqual, "<>" }
      };
      private static Dictionary<string, string> reservedActionType = new Dictionary<string, string>()
      {
         {  "=", "<>" },
         {  "<>", "=" },
         {  ">", "<=" },
         {  ">=", "<" },
         {  "<", ">=" },
         {  "<=", ">" },
         {  "and", "or" },
         {  "or", "and" },
         { "like \'{0}%\'", "not like \'{0}%\'"},
         { "like \'%{0}\'", "not like \'%{0}\'"},
         { "like \'%{0}%\'", "not like \'%{0}%\'"},
      };
      private static Dictionary<string, string> methodType = new Dictionary<string, string>()
      {
         { "Equals",  "=" },
         { "StartsWith",  "like \'{0}%\'"},
         { "EndsWith",  "like \'%{0}\'"},
         { "Contains",  "like \'%{0}%\'"},
      };
      private static List<string> updateType = new List<string>() { "Equals", "=" };
      protected object Connection;
      protected string ConnectionString { get; set; }
      public void SetConnectionString(string connectionString)
      {
         ConnectionString = connectionString;
      }
      protected abstract object GetConnection();
      public abstract bool IsOpened();
      public abstract object CreateCommand(string sql);
      public abstract DataTable ExcuteSelectQuery(object command, string TableName = "TABLE");
      public abstract int ExecuteQuery(object command);
      public abstract int ExecuteInsertQuery(object command);
      public abstract int ExecuteSqlQuery(string sql);
      public abstract object ExecuteScalar(object command);
      public abstract object ExecuteSqlScalar(string sql);
      public abstract void CloseConnection();
      private static string GenerateSelectQuery<T>(string tableName = "", string wherePart = "", string otherPart = "")
      {
         if (String.IsNullOrEmpty(tableName))
            tableName = EntityService.GetTableName<T>();

         return String.Format("SELECT * FROM {0} {1} {2}", tableName, wherePart, otherPart).Trim();
      }
      public List<T> GetEntityList<T>(string sql)
      {
         object command = CreateCommand(sql);
         DataTable dt = ExcuteSelectQuery(command);
         if (dt != null)
         {
            return dt.Rows.Cast<DataRow>().Select(n => EntityService.GetObject<T>(n)).ToList();
         }
         return new List<T>();
      }
      public List<T> GetAllEntityList<T>()
      {
         string table = CreateLinkInheritancePart<T>(typeof(T));
         string query = GenerateSelectQuery<T>(table);
         return GetEntityList<T>(query);
      }
      private string ParseDataSQL(object data, DATATYPE dataType, object defaultValue = null)
      {
         if (data == null)
         {
            if (defaultValue != null)
            {
               data = defaultValue;
            }
         }

         switch (dataType)
         {
            case DATATYPE.STRING:
               if ((data == null || (string)data == "") && defaultValue != null)
               {
                  data = defaultValue;
               }
               return HandleQueryValue(data.ToNotNullString());
            case DATATYPE.INTEGER:
            case DATATYPE.BIGINT:
               if (data == null && defaultValue != null)
               {
                  data = defaultValue;
               }
               return data == null ? "0" : data.ToNotNullString();
            case DATATYPE.DOUBLE:
               if (data == null && defaultValue != null)
               {
                  data = defaultValue;
               }
               return data == null ? "0.00" : data.ToNotNullString();
            case DATATYPE.GENERATED_ID:
               if ((data == null || Convert.ToInt32(data) == 0))
               {
                  if (defaultValue != null)
                     return defaultValue.ToNotNullString();
                  return null;
               }
               return data.ToNotNullString();
            case DATATYPE.BOOLEAN:
               return data == null ? "0" : (data.ToBoolean() == true ? "1" : "0");
            case DATATYPE.TIMESTAMP:
            case DATATYPE.DATE:
               if (data != null && !data.ToString().Equals(""))
                  return HandleQueryValue(data);
               return HandleQueryValue(DateTime.Now);
            default:
               return null;
         }
      }
      protected void GenerateInsertColumnValuePart<T>(T entity, out string columnPart, out string valuePart, bool insertIncludeID = false)
      {
         EntityProperty entityProperty = null;
         columnPart = "";
         valuePart = "";
         string tableName = EntityService.GetTableName<T>();
         if (EntityService.EntityMap.ContainsKey(tableName))
         {
            entityProperty = EntityService.EntityMap.GetValue(tableName);
         }
         else
         {
            entityProperty = EntityService.GetEntityProperties<T>(tableName);
            EntityService.EntityMap[tableName] = entityProperty;
         }
         if (entityProperty == null) { return; }
         int index = 1;
         if (entityProperty.Properties.Count > 0)
         {
            StringBuilder columns = new StringBuilder();
            StringBuilder values = new StringBuilder();

            foreach (PropertyInfo info in entityProperty.Properties)
            {
               EntityAttribute attr = entityProperty.AttributeDictionary.GetValue(info.Name);
               if (attr == null || (attr.isPrimaryKey && !insertIncludeID))
                  continue;

               if (index > 1)
               {
                  columns.Append(", ");
                  values.Append(", ");
               }

               columns.Append(attr.Column);

               object value = typeof(T).GetProperty(info.Name).GetValue(entity, (object[])null);
               EntityAttribute attribute = entityProperty.AttributeDictionary.GetValue(info.Name);
               string dataSql = ParseDataSQL(value, attribute.DataType, attribute.DefaultValue);
               values.Append(dataSql);

               index++;
            }
            columnPart = columns.ToString();
            valuePart = values.ToString();
         }
      }
      protected abstract string GenerateInsertQuery<T>(T entity, bool insertIncludeID = false);
      private string GenerateUpdateQuery<T>(T entity)
      {
         Hashtable paraMap = new Hashtable();
         string result = "";

         EntityProperty entityProperty = null;
         string tableName = EntityService.GetTableName<T>();
         if (EntityService.EntityMap.ContainsKey(tableName))
         {
            entityProperty = EntityService.EntityMap.GetValue(tableName);
         }
         else
         {
            entityProperty = EntityService.GetEntityProperties<T>(tableName);
            EntityService.EntityMap[tableName] = entityProperty;
         }
         if (entityProperty == null) { return null; }
         if (entityProperty.Properties.Count > 0)
         {
            string columns = "";
            string whereClause = "";

            foreach (PropertyInfo info in entityProperty.Properties)
            {
               EntityAttribute attribute = entityProperty.AttributeDictionary.GetValue(info.Name);
               if (attribute == null || attribute.isPrimaryKey) continue;
               if (!String.IsNullOrEmpty(columns))
                  columns += ", ";

               columns += attribute.Column;

               object value = typeof(T).GetProperty(info.Name).GetValue(entity, (object[])null);
               string dataSql = ParseDataSQL(value, attribute.DataType, attribute.DefaultValue);
               columns += " = " + dataSql;
            }

            EntityAttribute attributeKey = entityProperty.PrimaryKeyAttribute;
            if (attributeKey != null)
            {
               object value = typeof(T).GetProperty(attributeKey.PropertyInfo.Name).GetValue(entity, (object[])null);
               string dataSql = ParseDataSQL(value, attributeKey.DataType, attributeKey.DefaultValue);

               whereClause += attributeKey.Column + " = " + dataSql;
            }
            if (whereClause.Equals("")) { return null; }
            result += String.Format("UPDATE {0} SET {1} WHERE {2}", tableName, columns, whereClause);
         }

         if (result == "")
            return null;
         return result;
      }
      private string GenerateDeleteQuery<T>(long ID = 0, bool isDeleteAll = false)
      {
         
         string result = "";
         if (ID == 0 && !isDeleteAll)
         {
            return null;
         }
         EntityProperty entityProperty = null;
         string tableName = EntityService.GetTableName<T>();
         if (EntityService.EntityMap.ContainsKey(tableName))
         {
            entityProperty = EntityService.EntityMap.GetValue(tableName);
         }
         else
         {
            entityProperty = EntityService.GetEntityProperties<T>(tableName);
            EntityService.EntityMap[tableName] = entityProperty;
         }
         if (entityProperty == null) { return null; }
         if (entityProperty.Properties.Count > 0)
         {
            string whereClause = "";

            if (ID != 0)
            {
               if (entityProperty.PrimaryKeyAttribute != null)
               {
                  whereClause += entityProperty.PrimaryKeyAttribute.Column + " = " + ID;
               }
               if (whereClause.Equals("") || ID == 0) { return null; }
               result += String.Format("DELETE FROM {0} WHERE {1}", tableName, whereClause);
            }
            else
            {
               result += String.Format("DELETE FROM {0}", tableName);
            }
         }

         if (result == "")
            return null;
         return result;
      }
      protected string[] GenerateComplexSelectComponent<T,S>(Query<T,S> query)
      {
         Type objectType = typeof(T);

         var tableName = EntityService.GetTableName(objectType);
         if (String.IsNullOrEmpty(tableName))
            return null;
         Dictionary<Type, string> alphabetExpressionMap = new Dictionary<Type, string>();
         string fromPart = tableName;
         string wherePart = "";
         if (query.WhereExpression != null)
         {
            fromPart = CreateLinkInheritancePart<T>(objectType, query.WhereExpression.Parameters[0].Name, alphabetExpressionMap);
            wherePart = CreateWherePart(query.WhereExpression.Body, alphabetExpressionMap);
         }
         string selectPart = "*";
         if (query.SelectExpression != null)
         {
            selectPart = CreateSelectPart(query.SelectExpression, alphabetExpressionMap);
         }
         string orderPart = "";
         if (query.OrderList.Count > 0)
         {
            List<string> orderList = new List<string>();
            for (int i = 0; i < query.OrderList.Count; i++)
            {
               var orderExpression = query.OrderList[i];
               ORDER orderType = query.OrderTypeList[i];
               string orderLinkPart = CreateOrderByPart(orderExpression, orderType, alphabetExpressionMap);
               if (!String.IsNullOrEmpty(orderLinkPart))
               {
                  orderList.Add(orderLinkPart);
               }
            }
            orderPart = String.Join(",", orderList);
         }
         string limit = query.LimitValue == null ? "" : query.LimitValue.Value.ToString();
         return new string[] { selectPart, fromPart, wherePart, orderPart, limit };
      }
      protected abstract string GenerateComplexSelectQuery<T, S>(Query<T, S> query);
      public int InsertEntity<T>(T entity, bool insertIncludeID = false)
      {
         string query = GenerateInsertQuery<T>(entity, insertIncludeID);
         object command = CreateCommand(query);
         int id = ExecuteInsertQuery(command);
         EntityService.SetPrimaryKeyData<T>(entity, id);
         return id;
      }
      public bool UpdateEntity<T>(T entity)
      {
         string query = GenerateUpdateQuery<T>(entity);
         object command = CreateCommand(query);
         int rowEffected = ExecuteQuery(command);
         return rowEffected > 0;
      }
      public bool DeleteEntity<T>(long ID)
      {
         string query = GenerateDeleteQuery<T>(ID);
         object command = CreateCommand(query);
         int rowEffected = ExecuteQuery(command);
         return rowEffected > 0;
      }
      public bool TruncateTable<T>()
      {
         string query = GenerateDeleteQuery<T>(0, true);
         object command = CreateCommand(query);
         int rowEffected = ExecuteQuery(command);
         return rowEffected > 0;
      }
      private string HandleQueryValue(object objectValue, string methodAction = "")
      {
         if (objectValue != null)
         {
            var type = objectValue.GetType();
            if (type == typeof(string) && !methodAction.StartsWith("like"))
               return "'" + objectValue.ToNotNullString().Replace("\'","\'\'") + "'";
            if (type == typeof(bool))
               return objectValue.ToString().ToLower().Equals("true") ? "1" : "0";
            if (type == typeof(DateTime))
               return "'" + objectValue.ToNotNullString() + "'";
         }
         else
         {
            return "null";
         }

         return objectValue.ToString();
      }
      private string CreateLinkInheritancePart<T>(Type type, string alphabet = "n", Dictionary<Type, string> alphabetExpressionMap = null)
      {
         if (alphabetExpressionMap == null)
            alphabetExpressionMap = new Dictionary<Type, string>();

         var tableName = EntityService.GetTableName(type);
         var inheritanceColumn = EntityService.GetInheritanceColumn(type);

         alphabetExpressionMap[type] = alphabet;

         if (type.BaseType != null && type.BaseType != typeof(Object) && !type.BaseType.ToString().Contains("IEntity"))
         {
            string nextAlphabet = alphabet.Remove(alphabet.Length - 1, 1) + Convert.ToChar(Convert.ToUInt16(alphabet[alphabet.Length - 1]) + 1);

            var baseTableName = EntityService.GetTableName(type.BaseType);
            return CreateLinkInheritancePart<T>(type.BaseType, nextAlphabet, alphabetExpressionMap) + " " + String.Format("JOIN {0} AS {1} ON {2}.{3} = {1}.{3}", tableName, alphabet, nextAlphabet, inheritanceColumn);
         }

         return String.Format("{0} AS {1}", tableName, alphabet);
      }
      private string CreateWherePart(Expression body, Dictionary<Type, string> alphabetExpressionMap)
      {
         string result = "";

         Expression mainBody = body;

         bool isReserved = false;

         string formatExpressionString = mainBody.ToString().StartsWith("(") ? "({0} {1} {2})" : "{0} {1} {2}";
         if (body is UnaryExpression)
         {
            mainBody = (mainBody as UnaryExpression).Operand;
            formatExpressionString = "not ({0} {1} {2})";
            isReserved = true;
         }

         if (mainBody is BinaryExpression)
         {
            var action = mainBody.NodeType;

            var bodyExpression = (mainBody as BinaryExpression);
            if (bodyExpression.Left is MemberExpression && bodyExpression.Right is ConstantExpression)
            {
               var leftExpresson = bodyExpression.Left as MemberExpression;
               var rightExpression = bodyExpression.Right as ConstantExpression;

               var valueName = leftExpresson.Member.Name;
               var columnName = EntityService.GetCustomAttribute(leftExpresson.Member.ReflectedType, valueName).Column;
               if (alphabetExpressionMap.ContainsKey(leftExpresson.Member.DeclaringType))
                  columnName = String.Format("{0}.{1}", alphabetExpressionMap[leftExpresson.Member.DeclaringType], columnName);
               var value = HandleQueryValue(rightExpression.Value);

               string ActionType = string.Empty;
               if (value == null || value.ToLower().Equals("null"))
                  ActionType = "is";
               else
               {
                  if (!isReserved) ActionType = actionType.GetValue(action);
                  else ActionType = reservedActionType[actionType[action]];
               }

               result = String.Format(formatExpressionString, columnName, ActionType, value);
            }
            else if (bodyExpression.Left is MemberExpression && bodyExpression.Right is MemberExpression)
            {
               var leftExpresson = bodyExpression.Left as MemberExpression;
               var rightExpression = bodyExpression.Right as MemberExpression;

               var valueName = leftExpresson.Member.Name;
               var columnName = EntityService.GetCustomAttribute(leftExpresson.Member.ReflectedType, valueName).Column;
               if (alphabetExpressionMap.ContainsKey(leftExpresson.Member.DeclaringType))
                  columnName = String.Format("{0}.{1}", alphabetExpressionMap[leftExpresson.Member.DeclaringType], columnName);
               var value = HandleQueryValue(Expression.Lambda(rightExpression).Compile().DynamicInvoke());

               string ActionType = string.Empty;
               if (value == null || value.ToLower().Equals("null"))
                  ActionType = "is";
               else
               {
                  if (!isReserved) ActionType = actionType.GetValue(action);
                  else ActionType = reservedActionType.GetValue(actionType.GetValue(action));
               }

               result = String.Format(formatExpressionString, columnName, ActionType, value);
            }
            else
            {
               result += String.Format(formatExpressionString, CreateWherePart(bodyExpression.Left, alphabetExpressionMap), actionType[action], CreateWherePart(bodyExpression.Right, alphabetExpressionMap));
            }
         }
         else if (mainBody is MethodCallExpression)
         {
            var bodyExpression = (mainBody as MethodCallExpression);
            if (methodType.ContainsKey(bodyExpression.Method.Name))
            {
               var methodExpressionAction = methodType[bodyExpression.Method.Name];

               var valueName = (bodyExpression.Object as MemberExpression).Member.Name;
               var columnName = EntityService.GetCustomAttribute((bodyExpression.Object as MemberExpression).Member.ReflectedType, valueName).Column;
               if (alphabetExpressionMap.ContainsKey((bodyExpression.Object as MemberExpression).Member.ReflectedType))
                  columnName = String.Format("{0}.{1}", alphabetExpressionMap[(bodyExpression.Object as MemberExpression).Member.ReflectedType], columnName);
               string value = (bodyExpression.Arguments[0] is ConstantExpression) ? HandleQueryValue((bodyExpression.Arguments[0] as ConstantExpression).Value, methodExpressionAction) : HandleQueryValue(Expression.Lambda(bodyExpression.Arguments[0] as MemberExpression).Compile().DynamicInvoke(), methodExpressionAction);

               if (methodExpressionAction.Contains("like"))
               {
                  if (!isReserved)
                     result = String.Format("{0} {1}", columnName, String.Format(methodExpressionAction, value));
                  else result = String.Format("{0} {1}", columnName, String.Format(reservedActionType[methodExpressionAction], value));
               }
               else
               {
                  if (!isReserved)
                     result = String.Format("{0} {1} {2}", columnName, methodExpressionAction, value);
                  else result = String.Format("{0} {1} {2}", columnName, reservedActionType[methodExpressionAction], value);
               }
            }
         }
         return result;
      }
      private string CreateUpdatePart(Expression body)
      {
         string result = "";

         Expression mainBody = body;

         string formatExpressionString = "{0} {1} {2}";
         if (body is UnaryExpression)
         {
            return "";
         }

         if (mainBody is BinaryExpression)
         {
            var action = mainBody.NodeType;

            var bodyExpression = (mainBody as BinaryExpression);
            if (bodyExpression.Left is MemberExpression && bodyExpression.Right is ConstantExpression)
            {
               var leftExpresson = bodyExpression.Left as MemberExpression;
               var rightExpression = bodyExpression.Right as ConstantExpression;

               var valueName = leftExpresson.Member.Name;
               var columnName = EntityService.GetCustomAttribute(leftExpresson.Member.ReflectedType, valueName).Column;
               var value = HandleQueryValue(rightExpression.Value);

               string ActionType = actionType.GetValue(action);
               if (!updateType.Contains(ActionType))
                  return "";

               result = String.Format(formatExpressionString, columnName, ActionType, value);
            }
            else if (bodyExpression.Left is MemberExpression && bodyExpression.Right is MemberExpression)
            {
               var leftExpresson = bodyExpression.Left as MemberExpression;
               var rightExpression = bodyExpression.Right as MemberExpression;

               var valueName = leftExpresson.Member.Name;
               var columnName = EntityService.GetCustomAttribute(leftExpresson.Member.ReflectedType, valueName).Column;
               var value = HandleQueryValue(Expression.Lambda(rightExpression).Compile().DynamicInvoke());

               string ActionType = ActionType = actionType.GetValue(action);
               if (!updateType.Contains(ActionType))
                  return "";

               result = String.Format(formatExpressionString, columnName, ActionType, value);
            }
            else
            {
               result += String.Format(formatExpressionString, CreateUpdatePart(bodyExpression.Left), ",", CreateUpdatePart(bodyExpression.Right));
            }
         }
         else if (mainBody is MethodCallExpression)
         {
            var bodyExpression = (mainBody as MethodCallExpression);
            if (methodType.ContainsKey(bodyExpression.Method.Name))
            {
               
               var methodExpressionAction = methodType[bodyExpression.Method.Name];
               if (!updateType.Contains(methodExpressionAction))
                  return "";
               var valueName = (bodyExpression.Object as MemberExpression).Member.Name;
               var columnName = EntityService.GetCustomAttribute((bodyExpression.Object as MemberExpression).Member.ReflectedType, valueName).Column;
               string value = (bodyExpression.Arguments[0] is ConstantExpression) ? HandleQueryValue((bodyExpression.Arguments[0] as ConstantExpression).Value, methodExpressionAction) : HandleQueryValue(Expression.Lambda(bodyExpression.Arguments[0] as MemberExpression).Compile().DynamicInvoke(), methodExpressionAction);

               result = String.Format("{0} {1} {2}", columnName, methodExpressionAction, value);
            }
         }
         return result.Trim(new char[] { ' ', ',' });
      }
      private  string CreateSelectPart<T, S>(Expression<Func<T, S>> selectExpression, Dictionary<Type, string> alphabetExpressionMap)
      {
         List<string> selectList = new List<string>();

         if (selectExpression.Body.GetType().GetProperties().Any(n => n.Name.Equals("Arguments")))
         {
            IEnumerable<Expression> value = ((dynamic)selectExpression.Body).Arguments;
            foreach (MemberExpression memEx in value)
            {
               EntityAttribute attribute = EntityService.GetCustomAttribute(memEx.Member.DeclaringType, memEx.Member.Name);
               if (attribute == null)
                  continue;

               if (alphabetExpressionMap.Count == 0 && memEx.Member.DeclaringType != typeof(T))
                  throw new Exception("Column " + attribute.Column + " not exist on table " + EntityService.GetTableName(typeof(T)) + ".");

               string columnName = (alphabetExpressionMap.Count != 0) ? String.Format("{0}.{1}", alphabetExpressionMap[memEx.Member.ReflectedType], attribute.Column) : attribute.Column;
               selectList.Add(columnName);
            }
         }
         else
         {
            MemberExpression memEx = (MemberExpression)selectExpression.Body;

            EntityAttribute attribute = EntityService.GetCustomAttribute(memEx.Member.DeclaringType, memEx.Member.Name);
            if (attribute != null)
            {
               if (alphabetExpressionMap.Count == 0 && memEx.Member.DeclaringType != typeof(T))
                  throw new Exception("Column " + attribute.Column + " not exist on table " + EntityService.GetTableName(typeof(T)) + ".");

               string columnName = (alphabetExpressionMap.Count != 0) ? String.Format("{0}.{1}", alphabetExpressionMap[memEx.Member.ReflectedType], attribute.Column) : attribute.Column;
               selectList.Add(columnName);
            }
         }
         return String.Join(",", selectList);
      }
      private string CreateOrderByPart<T, O>(Expression<Func<T, O>> orderExpression, ORDER orderType, Dictionary<Type, string> alphabetExpressionMap)
      {
         List<string> orderList = new List<string>();
         string orderString = orderType == ORDER.Ascending ? "ASC" : "DESC";
         if (orderExpression.Body.GetType().GetProperties().Any(n => n.Name.Equals("Arguments")))
         {
            IEnumerable<Expression> value = ((dynamic)orderExpression.Body).Arguments;
            foreach (MemberExpression memEx in value)
            {
               EntityAttribute attribute = EntityService.GetCustomAttribute(memEx.Member.DeclaringType, memEx.Member.Name);
               if (attribute == null)
                  continue;

               if (alphabetExpressionMap.Count == 0 && memEx.Member.DeclaringType != typeof(T))
                  throw new Exception("Column " + attribute.Column + " not exist on table " + EntityService.GetTableName(typeof(T)) + ".");

               string columnName = (alphabetExpressionMap.Count != 0) ? String.Format("{0}.{1}", alphabetExpressionMap[memEx.Member.ReflectedType], attribute.Column) : attribute.Column;
               string result = String.Format("{0} {1}", columnName, orderString);
               orderList.Add(result);
            }
         }
         else
         {
            MemberExpression memEx = (MemberExpression)orderExpression.Body;

            EntityAttribute attribute = EntityService.GetCustomAttribute(memEx.Member.DeclaringType, memEx.Member.Name);
            if (attribute != null)
            {
               if (alphabetExpressionMap.Count == 0 && memEx.Member.DeclaringType != typeof(T))
                  throw new Exception("Column " + attribute.Column + " not exist on table " + EntityService.GetTableName(typeof(T)) + ".");

               string columnName = (alphabetExpressionMap.Count != 0) ? String.Format("{0}.{1}", alphabetExpressionMap[memEx.Member.ReflectedType], attribute.Column) : attribute.Column;
               string result = String.Format("{0} {1}", columnName, orderString);
               orderList.Add(result);
            }
         }
         return String.Join(",", orderList);
      }
      public IEnumerable<S> GetPropertyValueList<S>(string sql)
      {
         List<S> result = new List<S>();
         object command = CreateCommand(sql);
         DataTable dt = ExcuteSelectQuery(command);
         if (dt != null)
         {
            bool dynamicType = typeof(S).Name.Contains("AnonymousType");
            foreach (DataRow dr in dt.Rows)
            {
               if (dynamicType)
               {
                  var rowValue = new List<object>();
                  for (Int32 i = 0; i < dt.Columns.Count; i++)
                  {
                     var value = dr[i];
                     if (value is DBNull)
                        value = EntityService.GetDefaultGeneric(dt.Columns[i].DataType);
                     rowValue.Add(value);
                  }
                  
                  result.Add((S)Activator.CreateInstance(typeof(S), rowValue.ToArray()));
               }
               else
               {
                  for (Int32 i = 0; i < dt.Columns.Count; i++)
                  {
                     var value = dr[i];
                     if (value is DBNull)
                        value = EntityService.GetDefaultGeneric(dt.Columns[i].DataType);

                     if (typeof(S) == typeof(int))
                        result.Add((S)((object)value.ToInt()));
                     else if (typeof(S) == typeof(long))
                        result.Add((S)((object)value.ToLong()));
                     else result.Add((S)value);
                  }
               }
            }
         }
         return result;
      }
      public IEnumerable<T> GetPropertyValueEntityList<T, S>(string sql)
      {
         List<T> result = new List<T>();
         object command = CreateCommand(sql);
         DataTable dt = ExcuteSelectQuery(command);
         if (dt != null)
         {
            bool dynamicType = typeof(S).Name.Contains("AnonymousType");
            foreach (DataRow dr in dt.Rows)
            {
               if (dynamicType)
               {
                  var rowValue = new List<object>();
                  T entity = Activator.CreateInstance<T>();
                  for (Int32 i = 0; i < dt.Columns.Count; i++)
                  {
                     var value = dr[i];
                     if (value is DBNull)
                        value = EntityService.GetDefaultGeneric(dt.Columns[i].DataType);
                     EntityService.SetDataByColumnName(entity, dt.Columns[i].ColumnName, value);
                  }
                  result.Add((T)entity);
               }
               else
               {
                  for (Int32 i = 0; i < dt.Columns.Count; i++)
                  {
                     var value = dr[i];
                     if (value is DBNull)
                        value = EntityService.GetDefaultGeneric(dt.Columns[i].DataType);
                     T entity = Activator.CreateInstance<T>();
                     EntityService.SetDataByColumnName(entity, dt.Columns[i].ColumnName, value);
                     result.Add((T)entity);
                  }
               }
               
            }
         }
         return result;
      }
      public IEnumerable<T> GetEntityListWhere<T>(Expression<Func<T, bool>> predicate)
      {
         if (predicate == null)
            return null;

         Type objectType = typeof(T);

         var tableName = EntityService.GetTableName(objectType);
         if (String.IsNullOrEmpty(tableName))
            return null;

         Dictionary<Type, string> alphabetExpressionMap = new Dictionary<Type, string>();
         var fromLink = CreateLinkInheritancePart<T>(objectType, predicate.Parameters[0].Name, alphabetExpressionMap);
         var query = CreateWherePart(predicate.Body, alphabetExpressionMap);
         string sql =  String.Format("SELECT * FROM {0} WHERE {1}", fromLink, query);
         
         return GetEntityList<T>(sql);
      }
      public IEnumerable<T> GetEntityListComplexQuery<T,S>(Query<T, S> query)
      {
         string queryString = GenerateComplexSelectQuery(query);
         if (query.SelectExpression == null)
            return GetEntityList<T>(queryString);
         else
            return GetPropertyValueEntityList<T, S>(queryString);
      }
      public IEnumerable<S> GetListComplexSelectField<T, S>(Query<T, S> query)
      {
         string queryString = GenerateComplexSelectQuery(query);
         return GetPropertyValueList<S>(queryString);
      }
      protected string GenerateComplexUpdateQuery<T>(UpdateQuery<T> query)
      {
         Type objectType = typeof(T);

         var tableName = EntityService.GetTableName(objectType);
         if (String.IsNullOrEmpty(tableName))
            return "";
         Dictionary<Type, string> alphabetExpressionMap = new Dictionary<Type, string>();
         var wherePart = CreateWherePart(query.WhereExpression.Body, alphabetExpressionMap);
         if (String.IsNullOrEmpty(tableName) || String.IsNullOrEmpty(wherePart))
            return "";

         string updatePart = CreateUpdatePart(query.UpdateExpression.Body);
         string result = String.Format("UPDATE {0} SET {1} WHERE {2}", tableName, updatePart, wherePart);
         return result;
      }
      public int UpdateComplexQuery<T>(UpdateQuery<T> query)
      {
         if (query.UpdateExpression == null || query.WhereExpression == null)
            return 0;
         string queryString = GenerateComplexUpdateQuery(query);
         object command = CreateCommand(queryString);
         return ExecuteQuery(command);
      }
      private string GenerateDeleteWhereQuery<T>(Expression<Func<T, bool>> whereExpression)
      {
         Type objectType = typeof(T);

         var tableName = EntityService.GetTableName(objectType);
         if (String.IsNullOrEmpty(tableName))
            return "";
         Dictionary<Type, string> alphabetExpressionMap = new Dictionary<Type, string>();
         var wherePart = CreateWherePart(whereExpression.Body, alphabetExpressionMap);
         if (String.IsNullOrEmpty(tableName) || String.IsNullOrEmpty(wherePart))
            return "";

         string result = String.Format("DELETE FROM {0} WHERE {1}", tableName, wherePart);
         return result;
      }
      public int DeleteWhereQuery<T>(Expression<Func<T, bool>> whereExpression)
      {
         string sql = GenerateDeleteWhereQuery(whereExpression);
         object command = CreateCommand(sql);
         return ExecuteQuery(command);
      }

      public void BulkUpdate<T>(List<T> listEntity)
      {
         List<string> listQuery = listEntity.Select(n => GenerateUpdateQuery(n)).ToList();
         string query = String.Join(";", listQuery);
         object command = CreateCommand(query);
         ExecuteScalar(command);
      }
      public void BulkDelete<T>(List<T> listEntity)
      {
         string TableName = EntityService.GetTableName<T>();
         List<int> listPrimaryId = listEntity.Select(n => EntityService.GetPrimaryKeyValue(n).ToInt()).ToList();
         List<string> listQuery = listPrimaryId.Select(id => GenerateDeleteQuery<T>(id)).ToList();
         string query = String.Join(";", listQuery);
         object command = CreateCommand(query);
         ExecuteScalar(command);
      }
   }
}
