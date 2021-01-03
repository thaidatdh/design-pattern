using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern
{
   class Program
   {
      static void Main(string[] args)
      {
         DatabaseFactory dbFactory = new DatabaseFactory();
         DatabaseAbstract myDatabase = dbFactory.CreateDatabase(DATABASE_TYPE.SQLSERVER, @"Server=.;Database=BSDB;Integrated Security = True;");
         CustomDatabase db = new CustomDatabase();
         db.CreateInstance(myDatabase);
         //Entity.UserEntity t = new Entity.UserEntity();
         //t.Email = "abc";
         var t = Entity.UserEntity.Where(n => n.UserId == 10).FirstOrDefault();
         t.Update();
      }
   }
}