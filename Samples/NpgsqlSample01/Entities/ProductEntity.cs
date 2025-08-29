using OrmTxcSql.Attributes;

namespace NpgsqlSample01.Entities
{
    [Table("products")]
    public class ProductEntity : BaseNpgsqlEntity
    {
#if NET6_0_OR_GREATER
        [Column("product_code"), PrimaryKey]
        public string? ProductCode { get; set; }
        [Column("description")]
        public string? Description { get; set; }
#else
        [Column("product_code"), PrimaryKey]
        public string ProductCode { get; set; }
        [Column("description")]
        public string Description { get; set; }
#endif
    }
}
