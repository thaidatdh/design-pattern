using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;

namespace DesignPattern
{
   public class DatabaseContext
   {
      private static DatabaseAbstract Database { get; set; }
      public void CreateInstance(DatabaseAbstract database)
      {
         DatabaseContext.Database = database;
      }
      public static DatabaseAbstract GetInstance()
      {
         return DatabaseContext.Database;
      }
   }
}
