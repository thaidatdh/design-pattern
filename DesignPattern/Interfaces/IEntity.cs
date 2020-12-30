using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern
{
   interface IEntity
   {
      void Insert();
      bool Delete();
      bool Update();
   }
}
