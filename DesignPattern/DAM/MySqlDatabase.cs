using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace DesignPattern
{
   public class MySqlDatabase : DatabaseAbstract
   {
      public MySqlDatabase(string connectionString) 
      { 
         this.ConnectionString = connectionString; 
      }
      public override void CloseConnection()
      {
         MySqlConnection connection = (MySqlConnection)GetConnection();
         if (connection != null && connection.State.Equals(ConnectionState.Open))
            connection.Close();
      }

      public override object CreateCommand(string sql)
      {
         MySqlCommand command = new MySqlCommand(sql, (MySqlConnection)GetConnection());
         return command;
      }

      public override DataTable ExcuteSelectQuery(object command, string TableName = "TABLE")
      {
         MySqlCommand cmd = (MySqlCommand)command;
         
         DataTable result = new DataTable(TableName);
         MySqlDataReader sqlDataAdapter = cmd.ExecuteReader();
         result.Load(sqlDataAdapter);
         sqlDataAdapter.Close();
         cmd.Dispose();
         return result;
      }

      public override int ExecuteInsertQuery(object command)
      {
         MySqlCommand cmd = (MySqlCommand)command;
         cmd.ExecuteNonQuery();
         int rowAffected = cmd.LastInsertedId.ToInt();
         cmd.Dispose();
         return rowAffected;
      }

      public override int ExecuteQuery(object command)
      {
         MySqlCommand cmd = (MySqlCommand)command;
         int rowAffected = cmd.ExecuteNonQuery();
         cmd.Dispose();
         return rowAffected;
      }

      public override bool IsOpened()
      {
         MySqlConnection connection = (MySqlConnection)GetConnection();
         if (connection != null && connection.State.Equals(ConnectionState.Open))
            return true;
         return false;
      }

      protected override string GenerateInsertQuery<T>(T entity, bool insertIncludeID = false)
      {
         string tableName = EntityService.GetTableName<T>();
         GenerateInsertColumnValuePart<T>(entity, out string columns, out string values, insertIncludeID);
         if (String.IsNullOrEmpty(columns) || String.IsNullOrEmpty(values))
            return null;
         string result = String.Format("INSERT INTO {0} ({1}) VALUES ({2})", tableName, columns.ToString(), values.ToString());
         return result;
      }

      protected override object GetConnection()
      {
         if (Connection == null || !((MySqlConnection)Connection).State.Equals(ConnectionState.Open))
         {
            Connection = new MySqlConnection(ConnectionString);
            ((MySqlConnection)Connection).Open();
         }
         return Connection;
      }
      protected override string GenerateComplexSelectQuery<T, S>(Query<T, S> query)
      {
         string[] queryParts = GenerateComplexSelectComponent(query);
         string selectPart = queryParts[0].Trim();
         string fromPart = queryParts[1].Trim();
         string wherePart = queryParts[2].Trim();
         string orderPart = queryParts[3].Trim();
         string limitPart = queryParts[4].Trim();
         if (String.IsNullOrEmpty(fromPart))
         {
            return "";
         }
         if (!String.IsNullOrEmpty(wherePart))
         {
            wherePart = "WHERE " + wherePart;
         }
         if (!String.IsNullOrEmpty(orderPart))
         {
            orderPart = "ORDER BY " + orderPart;
         }
         if (!String.IsNullOrEmpty(limitPart))
         {
            limitPart = "LIMIT " + limitPart;
         }
         string result = String.Format("SELECT {0} FROM {1} {2} {3} {4}", selectPart, fromPart, wherePart, orderPart, limitPart).Trim();
         return result;
      }

      public override object ExecuteScalar(object command)
      {
         MySqlCommand cmd = (MySqlCommand)command;
         object result = cmd.ExecuteScalar();
         cmd.Dispose();
         return result;
      }
   }
}
