using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern.Entity
{
   [Table("USERS")]
   public class UserEntity : IEntity<UserEntity>
   {
      public UserEntity() { }
      public UserEntity(Object data)
      {
         EntityService.PassValueByAttribute<UserEntity>(data, this);
      }
      [Entity(Column = "USER_ID", DataType = DATATYPE.GENERATED_ID, isPrimaryKey = true)]
      public int UserId { get; set; }
      [Entity(Column = "FIRST_NAME", DataType = DATATYPE.STRING)]
      public string FirstName { get; set; }
      [Entity(Column = "LAST_NAME", DataType = DATATYPE.STRING)]
      public string LastName { get; set; }
      [Entity(Column = "DOB", DataType = DATATYPE.STRING)]
      public string DOB { get; set; }
      [Entity(Column = "ADDRESS", DataType = DATATYPE.STRING)]
      public string Address { get; set; }
      [Entity(Column = "PHONE", DataType = DATATYPE.STRING)]
      public string Phone { get; set; }
      [Entity(Column = "EMAIL", DataType = DATATYPE.STRING)]
      public string Email { get; set; }
      [Entity(Column = "GENDER", DataType = DATATYPE.STRING, DefaultValue = "NOT_SPECIFY")]
      public string Gender { get; set; }
      [Entity(Column = "NOTE", DataType = DATATYPE.STRING)]
      public string Note { get; set; }
      [Entity(Column = "PHOTO_LINK", DataType = DATATYPE.STRING, DefaultValue = "")]
      public string PhotoLink { get; set; }
      [Entity(Column = "USER_TYPE", DataType = DATATYPE.STRING, DefaultValue = "USER")]
      public string UserType { get; set; }

      public override int Insert()
      {
         throw new NotImplementedException();
      }

      public override bool Update()
      {
         throw new NotImplementedException();
      }
   }
}
