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
         MoveDataFromMSSqltoMySql();
      }
      private static void MoveDataFromMSSqltoMySql()
      {
         DatabaseFactory dbFactory = new DatabaseFactory();
         DatabaseAbstract myDatabase = dbFactory.CreateDatabase(DATABASE_TYPE.SQLSERVER, @"Server=.;Database=DP;Integrated Security = True;");
         CustomDatabase db = new CustomDatabase();
         db.CreateInstance(myDatabase);

         /*var users = Entity.UserEntity.GetAll();
         var staffs = Entity.StaffEntity.GetAll();
         var auth = Entity.AuthorEntity.GetAll();
         var books = Entity.BookEntity.GetAll();*/
         EntityService.InitEntityProperty();
         Entity.AuthorEntity.DeleteWhere(n => n.AuthorId == 1);
         DatabaseAbstract mysqlDatabase = dbFactory.CreateDatabase(DATABASE_TYPE.MYSQL, @"Server=localhost;Port=3306;Database=dp;Uid=root;Pwd=hello123;");
         db.CreateInstance(mysqlDatabase);
         /*foreach (var u in users)
         {
            u.Insert(true);
         }

         foreach (var s in staffs)
         {
            s.Insert(true);
         }

         foreach (var s in auth)
         {
            s.Insert(true);
         }

         foreach (var s in books)
         {
            s.Insert(true);
         }*/
      }
   }
}