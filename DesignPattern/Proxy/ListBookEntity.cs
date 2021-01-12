using DesignPattern.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern.Proxy
{
   public class ListBookEntity : ListInterface<BookEntity>
   {
      private List<BookEntity> ListEntity { get; set; }
      public ListBookEntity(int AuthorId) 
      {
         ListEntity = BookEntity.Where(n => n.AuthorId == AuthorId).QueryEntity().ToList();
      }

      public List<BookEntity> Gets()
      {
         return ListEntity;
      }
   }
}
