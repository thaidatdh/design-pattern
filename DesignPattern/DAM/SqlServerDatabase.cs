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
         string primaryKeyColumn = EntityService.GetPrimaryColumn<T>();
         if (!String.IsNullOrEmpty(primaryKeyColumn))
         {
            primaryKeyColumn = "OUTPUT INSERTED." + primaryKeyColumn;
         }
         if (String.IsNullOrEmpty(columns) || String.IsNullOrEmpty(values))
            return null;
         string result = String.Format("INSERT INTO {0} ({1}) {2} VALUES ({3})", tableName, columns.ToString(), primaryKeyColumn, values.ToString());
         
         if (insertIncludeID)
         {
            string IdentityInsertOn = "SET IDENTITY_INSERT " + tableName + " ON;\n";
            string IdentityInsertOff = "\nSET IDENTITY_INSERT " + tableName + " OFF;";
            result = IdentityInsertOn + result + IdentityInsertOff;
         }
         return result;
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
            limitPart = "TOP " + limitPart + " ";
         }
         string result = String.Format("SELECT {0}{1} FROM {2} {3} {4}", limitPart, selectPart, fromPart, wherePart, orderPart).Trim();
         return result;
      }
   }
}
