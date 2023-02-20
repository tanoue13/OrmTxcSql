using System;
using SqlSample01.Entities;

namespace SqlSample01.Daos
{
    public class NoPrimaryKeyDao : BaseSqlDao<NoPrimaryKeyEntity>
    {
        public override NoPrimaryKeyEntity[] Select(NoPrimaryKeyEntity entity)
        {
            throw new NotImplementedException();
        }
    }
}
