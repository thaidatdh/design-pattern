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
   public class CustomDatabase
   {
      public static DatabaseAbstract Database;
      public void CreateInstance(DatabaseAbstract database)
      {
         CustomDatabase.Database = database;
      }
   }
}
