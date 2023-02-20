using SqlSample01.Entities;

namespace SqlSample01.Daos
{
    public class IdentityDao : BaseSqlDao<IdentityEntity>
    {
        public override IdentityEntity[] Select(IdentityEntity entity)
        {
            throw new System.NotImplementedException();
        }
    }
}
