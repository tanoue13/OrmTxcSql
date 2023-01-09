using System;
using NpgsqlSample01.Entities;

namespace NpgsqlSample01.Daos
{
    public class NoPrimaryKeyDao : BaseNpgsqlDao<NoPrimaryKeyEntity>
    {
        public override NoPrimaryKeyEntity[] Select(NoPrimaryKeyEntity entity)
        {
            throw new NotImplementedException();
        }
    }
}
