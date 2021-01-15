using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern
{
   public interface EntityInterface
   {
      int Insert(bool insertIncludeID = false);
      bool Update();
      bool Delete();
   }
}
