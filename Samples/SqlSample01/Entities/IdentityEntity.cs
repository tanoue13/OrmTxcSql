using OrmTxcSql.Attributes;

namespace SqlSample01.Entities
{
    [Table("identity_table")]
    public class IdentityEntity : BaseSqlEntity
    {
        [Column("uid"), PrimaryKey, UID]
        public int Uid { get; set; }

#if NET6_0_OR_GREATER
        [Column("description")]
        public string? Description { get; set; }
#else
        [Column("description")]
        public string Description { get; set; }
#endif
    }
}
