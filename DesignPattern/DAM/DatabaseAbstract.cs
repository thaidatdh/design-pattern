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
   }
}
