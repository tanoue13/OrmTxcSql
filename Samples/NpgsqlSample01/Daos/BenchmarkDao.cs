using System;
using NpgsqlSample01.Entities;

namespace NpgsqlSample01.Daos
{
    public class BenchmarkDao : BaseNpgsqlDao<BenchmarkEntity>
    {
        public override BenchmarkEntity[] Select(BenchmarkEntity entity)
        {
            throw new NotImplementedException();
        }
    }
}
