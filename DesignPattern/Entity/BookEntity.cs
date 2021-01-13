using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace DesignPattern.Entity
{
   [Table("BOOK")]
   public class BookEntity : IEntity<BookEntity>
   {
      public BookEntity() { }
      public BookEntity(Object data)
      {
         EntityService.PassValueByAttribute<BookEntity>(data, this);
      }
      [Entity(Column = "BOOK_ID", DataType = DATATYPE.GENERATED_ID, isPrimaryKey = true)]
      public int BookId { get; set; }
      [Entity(Column = "NAME", DataType = DATATYPE.STRING)]
      public string Name { get; set; }
      [Entity(Column = "BARCODE", DataType = DATATYPE.STRING)]
      public string Barcode { get; set; }
      [Entity(Column = "FORMAT", DataType = DATATYPE.STRING)]
      public string Format { get; set; }
      [Entity(Column = "SIZE", DataType = DATATYPE.STRING)]
      public string Size { get; set; }
      [Entity(Column = "PAGE", DataType = DATATYPE.STRING)]
      public string Page { get; set; }
      [Entity(Column = "DESCRIPTION", DataType = DATATYPE.STRING)]
      public string Description { get; set; }
      [Entity(Column = "PRICE", DataType = DATATYPE.BIGINT)]
      public long Price { get; set; }
      [Entity(Column = "AUTHOR_ID", DataType = DATATYPE.INTEGER)]
      public int AuthorId { get; set; }
      [Entity(Column = "IS_DELETED", DataType = DATATYPE.BOOLEAN)]
      public bool IsDeleted { get; set; }

      public override bool Delete()
      {
         return DatabaseContext.Database.DeleteEntity<BookEntity>(this.BookId);
      }

      public override int Insert(bool insertIncludeID = false)
      {
         return DatabaseContext.Database.InsertEntity<BookEntity>(this, insertIncludeID);
      }

      public override bool Update()
      {
         return DatabaseContext.Database.UpdateEntity<BookEntity>(this);
      }
   }
}



