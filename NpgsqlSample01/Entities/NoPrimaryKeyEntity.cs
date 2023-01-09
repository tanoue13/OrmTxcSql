using OrmTxcSql.Attributes;

namespace NpgsqlSample01.Entities
{
    [Table("no_primary_key_table")]
    public class NoPrimaryKeyEntity : BaseNpgsqlEntity
    {
#if NET6_0_OR_GREATER
        [Column("column_a")]
        public string? ColumnA { get; set; }
        [Column("column_b")]
        public string? ColumnB { get; set; }
        [Column("column_c")]
        public string? ColumnC { get; set; }
#else
        [Column("column_a")]
        public string ColumnA { get; set; }
        [Column("column_b")]
        public string ColumnB { get; set; }
        [Column("column_c")]
        public string ColumnC { get; set; }
#endif
    }
}
