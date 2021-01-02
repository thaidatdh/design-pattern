using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern
{
   class SqlServerDatabase : DatabaseAbstract
   {
      public SqlServerDatabase(string connectionString)
      {
         this.ConnectionString = connectionString;
      }
      public override void CloseConnection()
      {
         SqlConnection connection = (SqlConnection)GetConnection();
         if (connection != null && connection.State.Equals(ConnectionState.Open))
            connection.Close();
      }

      public override object CreateCommand(string sql)
      {
         SqlCommand command = new SqlCommand(sql, (SqlConnection)GetConnection());
         return command;
      }

      public override DataTable ExcuteSelectQuery(object command, string TableName = "TABLE")
      {
         SqlCommand cmd = (SqlCommand)command;
         SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
         DataTable result = new DataTable(TableName);
         dataAdapter.Fill(result);
         dataAdapter.Dispose();
         cmd.Dispose();
         return result;
      }

      public override int ExecuteInsertQuery(object command)
      {
         SqlCommand cmd = (SqlCommand)command;
         int rowAffected = cmd.ExecuteScalar().ToInt();
         cmd.Dispose();
         return rowAffected;
      }

      public override int ExecuteQuery(object command)
      {
         SqlCommand cmd = (SqlCommand)command;
         int rowAffected = cmd.ExecuteNonQuery();
         cmd.Dispose();
         return rowAffected;
      }

      public override bool IsOpened()
      {
         SqlConnection connection = (SqlConnection)GetConnection();
         if (connection != null && connection.State.Equals(ConnectionState.Open))
            return true;
         return false;
      }

      protected override object GetConnection()
      {
         if (Connection == null || !((SqlConnection)Connection).State.Equals(ConnectionState.Open))
         {
            Connection = new SqlConnection(ConnectionString);
            ((SqlConnection)Connection).Open();
         }
         return Connection;
      }
      protected override string GenerateInsertQuery<T>(T entity, bool insertIncludeID = false)
      {
         string tableName = EntityService.GetTableName<T>();
         GenerateInsertColumnValuePart<T>(entity, out string columns, out string values, insertIncludeID);
         if (String.IsNullOrEmpty(columns) || String.IsNullOrEmpty(values))
            return null;
         string result = String.Format("INSERT INTO {0} ({1}) VALUES ({2})", tableName, columns.ToString(), values.ToString());
         
         if (insertIncludeID)
         {
            string IdentityInsertOn = "SET IDENTITY_INSERT " + tableName + " ON;\n";
            string IdentityInsertOff = "\nSET IDENTITY_INSERT " + tableName + " OFF;";
            result = IdentityInsertOn + result + IdentityInsertOff;
         }
         return result;
      }
   }
}
