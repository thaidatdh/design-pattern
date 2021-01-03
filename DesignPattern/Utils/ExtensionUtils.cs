using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern
{
   public static class ExtensionUtils
   {
      public static TValue GetValue<TKey,TValue>(this Dictionary<TKey,TValue> dictionary, TKey key)
      {
         return dictionary.TryGetValue(key, out TValue value) ? value : default(TValue);
      }
      public static int ToInt(this object value)
      {
         try
         {
            return Convert.ToInt32(value);
         }
         catch
         {
            return 0;
         }
      }
      public static long ToLong(this object value)
      {
         try
         {
            return Convert.ToInt64(value);
         }
         catch
         {
            return 0;
         }
      }
      public static bool ToBoolean(this object value)
      {
         try
         {
            return Convert.ToBoolean(value);
         }
         catch
         {
            return false;
         }
      }
      public static double ToDouble(this object value, int digits = -1)
      {
         try
         {
            if (digits == -1)
               return Convert.ToDouble(value);
            return Math.Round(Convert.ToDouble(value), digits);
         }
         catch
         {
            return 0;
         }
      }
      public static DateTime ToDateTime(this object obj)
      {
         try
         {
            return DateTime.Parse(obj.ToString());
         }
         catch
         {
            return DateTime.Now;
         }
      }
      public static string ToNotNullString(this object obj)
      {
         try
         {
            return obj.ToString();
         }
         catch
         {
            return "";
         }
      }
   }
}
