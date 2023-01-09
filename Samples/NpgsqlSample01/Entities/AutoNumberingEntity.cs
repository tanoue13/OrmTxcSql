using OrmTxcSql.Attributes;

namespace NpgsqlSample01.Entities
{
    [Table("auto_numbering_table")]
    public class AutoNumberingEntity : BaseNpgsqlEntity
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
