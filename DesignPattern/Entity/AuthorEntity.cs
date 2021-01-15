using DesignPattern.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern.Entity
{
   [Table("AUTHOR")]
   public class AuthorEntity : IEntity<AuthorEntity>, EntityInterface
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

      public bool Delete()
      {
         return DatabaseContext.GetInstance().DeleteEntity<AuthorEntity>(this.AuthorId);
      }

      public int Insert(bool insertIncludeID = false)
      {
         return DatabaseContext.GetInstance().InsertEntity<AuthorEntity>(this, insertIncludeID);
      }

      public bool Update()
      {
         return DatabaseContext.GetInstance().UpdateEntity<AuthorEntity>(this);
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

