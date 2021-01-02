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
      protected Dictionary<string, EntityProperty> EntityMap = new Dictionary<string, EntityProperty>(); // table name -> entity properties
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
         { "like \"{0}%\"", "not like \"{0}%\""},
         { "like \"%{0}\"", "not like \"%{0}\""},
         { "like \"%{0}%\"", "not like \"%{0}%\""},
      };
      private static Dictionary<string, string> methodType = new Dictionary<string, string>()
      {
         { "Equals",  "=" },
         { "StartsWith",  "like \"{0}%\""},
         { "EndsWith",  "like \"%{0}\""},
         { "Contains",  "like \"%{0}%\""},
      };
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
      public abstract void CloseConnection();
      private static string GenerateSelectQuery<T>(string wherePart = "", string otherPart = "")
      {
         string tableName = EntityService.GetTableName<T>();
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
         string query = GenerateSelectQuery<T>();
         return GetEntityList<T>(query);
      }
      private string HandleQueryValue(object objectValue, string methodAction = "")
      {
         if (objectValue != null)
         {
            var type = objectValue.GetType();
            if (type == typeof(string) && !methodAction.StartsWith("like"))
               return "'" + objectValue + "'";
            if (type == typeof(bool))
               return objectValue.ToString().ToLower().Equals("true") ? "1" : "0";
         }
         else
         {
            return "null";
         }

         return objectValue.ToString();
      }
      private string CreateLinkInheritancePart<T>(Type type, string alphabet = "n", Dictionary<Type, string> alphabetExpressionMap = null)
      {
         var tableName = EntityService.GetTableName(type);
         var inheritanceColumn = EntityService.GetInheritanceColumn(type);

         alphabetExpressionMap[type] = alphabet;

         if (type.BaseType != null && type.BaseType != typeof(Object) && type.BaseType != typeof(IEntity<T>))
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
               var columnName = String.Format("{0}.{1}", alphabetExpressionMap[leftExpresson.Member.DeclaringType], EntityService.GetCustomAttribute(leftExpresson.Member.ReflectedType, valueName).Column);
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
               var columnName = String.Format("{0}.{1}", alphabetExpressionMap[leftExpresson.Member.DeclaringType], EntityService.GetCustomAttribute(leftExpresson.Member.ReflectedType, valueName).Column);
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
               var columnName = String.Format("{0}.{1}", alphabetExpressionMap[(bodyExpression.Object as MemberExpression).Member.ReflectedType], EntityService.GetCustomAttribute((bodyExpression.Object as MemberExpression).Member.ReflectedType, valueName).Column);
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
         string sql =  String.Format("SELECT * FROM {0} WHERE {1}", fromLink, query.Replace("\"", "'"));
         
         return GetEntityList<T>(sql);
      }

   }
}
