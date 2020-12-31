using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern
{
   public class DatabaseFactory
   {
      public DatabaseAbstract CreateDatabase(DATABASE_TYPE databaseType, string connectionString)
      {
         switch (databaseType)
         {
            case DATABASE_TYPE.SQLSERVER:
               return new SqlServerDatabase(connectionString);
            case DATABASE_TYPE.MYSQL:
               return new MySqlDatabase(connectionString);
            default:
               return null;
         }
      }
   }
}
