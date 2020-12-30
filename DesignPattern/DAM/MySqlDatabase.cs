using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace DesignPattern.DAM
{
   public class MySqlDatabase : Database
   {
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
         int rowAffected = Convert.ToInt32(cmd.ExecuteScalar());
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

      public override bool Open()
      {
         MySqlConnection connection = (MySqlConnection)GetConnection();
         if (connection != null && connection.State.Equals(ConnectionState.Open))
            return true;
         return false;
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
   }
}
