using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern.Entity
{
   [Table("STAFF", "USER_ID")]
   public class StaffEntity : UserEntity, EntityInterface
   {
      public StaffEntity() : base() { }
      public StaffEntity(Object data) : base((Object)data)
      {
         EntityService.PassValueByAttribute<StaffEntity>(data, this);
      }
      [Entity(Column = "STAFF_ID", DataType = DATATYPE.GENERATED_ID, isPrimaryKey = true)]
      public int StaffId { get; set; }
      [Entity(Column = "USER_ID", DataType = DATATYPE.INTEGER)]
      public int UserId { get; set; }
      [Entity(Column = "USERNAME", DataType = DATATYPE.STRING)]
      public string Username { get; set; }
      [Entity(Column = "PASSWORD", DataType = DATATYPE.STRING)]
      public string Password { get; set; }
      [Entity(Column = "SALARY", DataType = DATATYPE.BIGINT)]
      public long Salary { get; set; }
      [Entity(Column = "START_DATE", DataType = DATATYPE.STRING)]
      public string StartDate { get; set; }
      [Entity(Column = "END_DATE", DataType = DATATYPE.STRING)]
      public string EndDate { get; set; }
      [Entity(Column = "ACTIVE", DataType = DATATYPE.BOOLEAN)]
      public bool Active { get; set; }
      public new bool Delete()
      {
         return DatabaseContext.GetInstance().DeleteEntity<UserEntity>(this.UserId) && DatabaseContext.GetInstance().DeleteEntity<StaffEntity>(this.StaffId);
      }

      public new int Insert(bool insertIncludeID = false)
      {
         int userId = DatabaseContext.GetInstance().InsertEntity<UserEntity>(this, insertIncludeID);
         this.UserId = userId;
         return DatabaseContext.GetInstance().InsertEntity<StaffEntity>(this, insertIncludeID);
      }

      public new bool Update()
      {
         return DatabaseContext.GetInstance().UpdateEntity<StaffEntity>(this);
      }
      public static new List<StaffEntity> GetAll()
      {
         return DatabaseContext.GetInstance().GetAllEntityList<StaffEntity>();
      }
      public static new bool DeleteAll()
      {
         return DatabaseContext.GetInstance().TruncateTable<StaffEntity>();
      }
      public static Query<StaffEntity, S> Select<S>(Expression<Func<StaffEntity, S>> select)
      {
         Query<StaffEntity, S> query = new Query<StaffEntity, S>();
         query.SelectExpression = select;
         return query;
      }
      public static Query<StaffEntity, object> Where(Expression<Func<StaffEntity, bool>> where)
      {
         Query<StaffEntity, object> query = new Query<StaffEntity, object>();
         query.WhereExpression = where;
         return query;
      }
      public static UpdateQuery<StaffEntity> Update(Expression<Func<StaffEntity, bool>> update)
      {
         UpdateQuery<StaffEntity> query = new UpdateQuery<StaffEntity>();
         query.UpdateExpression = update;
         return query;
      }
      public static int DeleteWhere(Expression<Func<StaffEntity, bool>> where)
      {
         return DatabaseContext.GetInstance().DeleteWhereQuery<StaffEntity>(where);
      }
      public static void BulkInsert(List<StaffEntity> listEntity, bool insertIncludeID = false)
      {
         foreach (StaffEntity entity in listEntity)
            entity.Insert(insertIncludeID);
      }
   }
}
