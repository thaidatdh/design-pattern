using DesignPattern.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern.Proxy
{
   class ListBookProxy : ListInterface<BookEntity>
   {
      private ListInterface<BookEntity> ListBookEntity;
      private int AuthorId;
      public ListBookProxy(int AuthorId)
      {
         this.AuthorId = AuthorId;
      }

      public List<BookEntity> Gets()
      {
         if (ListBookEntity == null)
         {
            ListBookEntity = new ListBookEntity(this.AuthorId);
         }
         return ListBookEntity.Gets();
      }
   }
}
