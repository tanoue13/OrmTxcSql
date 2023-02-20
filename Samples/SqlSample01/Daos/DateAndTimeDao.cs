using SqlSample01.Entities;

namespace SqlSample01.Daos
{
    public class DateAndTimeDao : BaseSqlDao<DateAndTimeEntity>
    {
        public override DateAndTimeEntity[] Select(DateAndTimeEntity entity)
        {
            throw new System.NotImplementedException();
        }
    }
}
