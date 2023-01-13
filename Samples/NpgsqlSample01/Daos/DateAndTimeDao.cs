using NpgsqlSample01.Entities;

namespace NpgsqlSample01.Daos
{
    public class DateAndTimeDao : BaseNpgsqlDao<DateAndTimeEntity>
    {
        public override DateAndTimeEntity[] Select(DateAndTimeEntity entity)
        {
            throw new System.NotImplementedException();
        }
    }
}
