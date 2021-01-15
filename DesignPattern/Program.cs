using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesignPattern.Entity;
namespace DesignPattern
{
   class Program
   {
      static void Main(string[] args)
      {
         EntityService.InitEntityProperty();
         //Khởi tạo
         DatabaseFactory dbFactory = new DatabaseFactory();
         DatabaseAbstract myDatabase = dbFactory.CreateDatabase(DATABASE_TYPE.SQLSERVER, @"Server=.;Database=DP;Integrated Security = True;");
         DatabaseContext db = new DatabaseContext();
         db.CreateInstance(myDatabase);
         //Các hành động cơ bản trên database
         //DatabaseContext.GetInstance().ExecuteSqlQuery("update users set first_name = 'a'");
         //DatabaseContext.GetInstance().ExecuteSqlScalar("update users set first_name = 'a';update users set last_name = 'b';");
         //DataTable tb = DatabaseContext.GetInstance().ExcuteSelectSqlQuery("select * from book where book_id = 100");
         //Insert 1 entity
         AuthorEntity author = new AuthorEntity();
         author.Name = "Test";
         author.Insert();

         //Uodate 1 entity
         author.Name = "Update Test";
         author.Update();

         //Delete 1 entity
         author.Delete();

         //Select tất cả entity của 1 đối tượng
         //SQL: SELECT * FROM BOOK
         List<BookEntity> listBooks = BookEntity.GetAll();
         var staffAlldemo = StaffEntity.GetAll();
         //Select danh sách các entity thỏa mãn yêu cầu 
         //Ví dụ: lấy tối đa 5 kết quả, bao gồm các book có author_id = 1, sắp xếp giảm dần theo tên sau đó tăng dần theo số trang
         //MYSQL: SELECT * FROM BOOK WHERE AUTHOR_ID = 1 ORDER BY NAME DESC THEN BY PAGE ASC LIMIT 5 
         //SQL SERVER: SELECT TOP 5 * FROM BOOK WHERE AUTHOR_ID = 1 ORDER BY NAME DESC THEN BY PAGE ASC
         listBooks = BookEntity.Where(n => n.AuthorId == 1)
            .OrderBy(n => n.Name, ORDER.Descending).OrderBy(n => n.Page, ORDER.Ascending)
            .Limit(5).QueryEntity().ToList();

         //Select Entity có 1-1 foreign key (Staff - User)
         //SQL: SELECT * FROM USERS AS u JOIN STAFF AS s ON u.USER_ID = s.USER_ID WHERE u.LAST_NAME LIKE '%Admin%'
         List<StaffEntity> listStaff = StaffEntity.Where(n => n.LastName.Contains("Admin")).QueryEntity().ToList();

         //Select 1 cột của entity (Ví dụ: lấy cột Name của book có author_id = 1)
         //SQL: SELECT NAME FROM BOOK WHERE AUTHOR_ID = 1
         List<string> listBookName = BookEntity.Select(n => n.Name).Where(n => n.AuthorId == 1).QuerySelect().ToList();


         //Select nhiều cột của entity (Ví dụ: lấy cột Name và BookId của book có author_id = 1)
         //SQL: SELECT BOOK_ID, NAME FROM BOOK WHERE AUTHOR_ID = 1
         var listBookCustomType = BookEntity.Select(n => new { n.BookId, n.Name }).Where(n => n.AuthorId == 1).QuerySelect().ToList();

         //Update một/nhiều dòng của 1 table khi dòng đó thỏa mãn điều kiện
         //Ví dụ: cập nhật Barcode và page cho tất cả book có author_id = 1
         //SQL: UPDATE BOOK SET BARCODE = 'Barcode', PAGE = '10' WHERE AUTHOR_ID = 1
         int rows = BookEntity.Update(n => n.Barcode.Equals("Barcode") && n.Page.Equals("10")).Where(n => n.AuthorId == 1).QueryUpdate();
         //Equals và ==
         //Delete một/nhiều dòng của 1 table khi dòng đó thỏa mãn điều kiện
         //SQL: DELETE FROM BOOK WHERE AUTHOR_ID = 53
         rows = BookEntity.DeleteWhere(n => n.AuthorId == 53);

         //Demo Lazy Loading lấy book của một author (1-n)
         AuthorEntity authorEntity = AuthorEntity.Where(n => n.AuthorId == 51).QueryEntity().ToList().First();
         List<BookEntity> listBookOfAuthor = authorEntity.GetBooks();

         //Demo bulk insert bằng cách chuyển data từ SQL SERVER sang MYSQL
         var staffs = StaffEntity.GetAll();
         var users = UserEntity.GetAll();
         var auth = AuthorEntity.GetAll();
         var books = BookEntity.GetAll();
         List<int> staff_userid = staffs.Select(n => n.UserId).ToList();
         users = users.Where(n => !staff_userid.Contains(n.UserId)).ToList();
         //Đóng connection SQL Server
         DatabaseContext.GetInstance().CloseConnection();

         //Sử dụng MySQL

         DatabaseAbstract mysqlDatabase = dbFactory.CreateDatabase(DATABASE_TYPE.MYSQL, @"Server=localhost;Port=3306;Database=dp;Uid=root;Pwd=hello123;");
         db.CreateInstance(mysqlDatabase);
         //Bulk insert
         StaffEntity.BulkInsert(staffs, true);
         UserEntity.BulkInsert(users, true);
         AuthorEntity.BulkInsert(auth, true);
         BookEntity.BulkInsert(books, true);

         //Demo bulk Update ở MYSQL
         listBooks = BookEntity.Where(n => n.AuthorId == 115).QueryEntity().ToList();
         int temp = 0;
         foreach (BookEntity book in listBooks)
         {
            book.Name = book.Name + (++temp);
         }
         BookEntity.BulkUpdate(listBooks);

         //Demo bulk Delete ở MYSQL
         BookEntity.BulkDelete(listBooks);

         //Demo Delete all (truncate) ở MYSQL
         BookEntity.DeleteAll();

         //Đóng kết nối CSDL
         DatabaseContext.GetInstance().CloseConnection();
      }
   }
}