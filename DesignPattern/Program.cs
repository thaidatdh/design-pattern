using System;
using System.Collections.Generic;
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
         /*var t1 = Entity.UserEntity.Select(n => n.FirstName).QuerySelect();
         var t2 = Entity.UserEntity.Select(n => n.FirstName)
           .Where(n => n.UserType.Equals("CUSTOMER")).QueryEntity();
         var t5 = Entity.UserEntity
            .Where(n => n.UserType.Equals("CUSTOMER"))
            .OrderBy(n => n.UserId.ToString()).OrderBy(n => n.LastName).Limit(10).QueryAll();*/
         var t3 = Entity.UserEntity.Select(n => new { n.FirstName, n.LastName })
            .Where(n => n.UserType.Equals("CUSTOMER"))
            .OrderBy(n => n.UserId.ToString()).OrderBy(n => n.LastName).Limit(10).QuerySelect();
         
      }
   }
}