using System;
using NpgsqlSample01.Entities;

namespace NpgsqlSample01.Daos
{
    public class AutoNumberingDao : BaseNpgsqlDao<AutoNumberingEntity>
    {
        public override AutoNumberingEntity[] Select(AutoNumberingEntity entity)
        {
            throw new NotImplementedException();
        }
    }
}
