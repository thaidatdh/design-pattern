using DesignPattern.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern.Entity
{
   [Table("AUTHOR")]
   public class AuthorEntity : IEntity<AuthorEntity>
   {
      public AuthorEntity() { }
      public AuthorEntity(Object data)
      {
         EntityService.PassValueByAttribute<AuthorEntity>(data, this);
      }
      [Entity(Column = "AUTHOR_ID", DataType = DATATYPE.GENERATED_ID, isPrimaryKey = true)]
      public int AuthorId { get; set; }
      [Entity(Column = "NAME", DataType = DATATYPE.STRING)]
      public string Name { get; set; }
      [Entity(Column = "NOTE", DataType = DATATYPE.STRING)]
      public string Note { get; set; }
      [Entity(Column = "IS_DELETED", DataType = DATATYPE.BOOLEAN)]
      public bool IsDeleted { get; set; }

      public override bool Delete()
      {
         return CustomDatabase.Database.DeleteEntity<AuthorEntity>(this.AuthorId);
      }

      public override int Insert(bool insertIncludeID = false)
      {
         return CustomDatabase.Database.InsertEntity<AuthorEntity>(this, insertIncludeID);
      }

      public override bool Update()
      {
         return CustomDatabase.Database.UpdateEntity<AuthorEntity>(this);
      }
      ListBookProxy _books;
      public List<BookEntity> GetBooks(bool isReload = false)
      {
         if (_books == null || isReload)
         {
            _books = new ListBookProxy(this.AuthorId);
         }
         return _books.Gets();
      }
   }
}

