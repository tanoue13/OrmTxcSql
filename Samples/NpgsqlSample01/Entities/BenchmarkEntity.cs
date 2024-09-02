using OrmTxcSql.Attributes;

namespace NpgsqlSample01.Entities
{
    [Table("benchmark_table")]
    public class BenchmarkEntity : BaseNpgsqlEntity
    {
        [Column("key"), PrimaryKey]
        public int Key { get; set; }

#if NET6_0_OR_GREATER
#nullable disable
#endif
        [Column("column_string_01")]
        public string ColumnString01 { get; set; }
        [Column("column_string_02")]
        public string ColumnString02 { get; set; }
        [Column("column_string_03")]
        public string ColumnString03 { get; set; }
        [Column("column_string_04")]
        public string ColumnString04 { get; set; }
        [Column("column_string_05")]
        public string ColumnString05 { get; set; }
        [Column("column_string_06")]
        public string ColumnString06 { get; set; }
        [Column("column_string_07")]
        public string ColumnString07 { get; set; }
        [Column("column_string_08")]
        public string ColumnString08 { get; set; }
        [Column("column_string_09")]
        public string ColumnString09 { get; set; }
        [Column("column_string_10")]
        public string ColumnString10 { get; set; }
        [Column("column_string_11")]
        public string ColumnString11 { get; set; }
        [Column("column_string_12")]
        public string ColumnString12 { get; set; }
        [Column("column_string_13")]
        public string ColumnString13 { get; set; }
        [Column("column_string_14")]
        public string ColumnString14 { get; set; }
        [Column("column_string_15")]
        public string ColumnString15 { get; set; }
        [Column("column_string_16")]
        public string ColumnString16 { get; set; }
        [Column("column_string_17")]
        public string ColumnString17 { get; set; }
        [Column("column_string_18")]
        public string ColumnString18 { get; set; }
        [Column("column_string_19")]
        public string ColumnString19 { get; set; }
        [Column("column_string_20")]
        public string ColumnString20 { get; set; }
        [Column("column_string_21")]
        public string ColumnString21 { get; set; }
        [Column("column_string_22")]
        public string ColumnString22 { get; set; }
        [Column("column_string_23")]
        public string ColumnString23 { get; set; }
        [Column("column_string_24")]
        public string ColumnString24 { get; set; }
        [Column("column_string_25")]
        public string ColumnString25 { get; set; }
        [Column("column_string_26")]
        public string ColumnString26 { get; set; }
        [Column("column_string_27")]
        public string ColumnString27 { get; set; }
        [Column("column_string_28")]
        public string ColumnString28 { get; set; }
        [Column("column_string_29")]
        public string ColumnString29 { get; set; }
        [Column("column_string_30")]
        public string ColumnString30 { get; set; }
        [Column("column_string_31")]
        public string ColumnString31 { get; set; }
        [Column("column_string_32")]
        public string ColumnString32 { get; set; }
        [Column("column_string_33")]
        public string ColumnString33 { get; set; }
        [Column("column_string_34")]
        public string ColumnString34 { get; set; }
        [Column("column_string_35")]
        public string ColumnString35 { get; set; }
        [Column("column_string_36")]
        public string ColumnString36 { get; set; }
        [Column("column_string_37")]
        public string ColumnString37 { get; set; }
        [Column("column_string_38")]
        public string ColumnString38 { get; set; }
        [Column("column_string_39")]
        public string ColumnString39 { get; set; }
        [Column("column_string_40")]
        public string ColumnString40 { get; set; }
        [Column("column_string_41")]
        public string ColumnString41 { get; set; }
        [Column("column_string_42")]
        public string ColumnString42 { get; set; }
        [Column("column_string_43")]
        public string ColumnString43 { get; set; }
        [Column("column_string_44")]
        public string ColumnString44 { get; set; }
        [Column("column_string_45")]
        public string ColumnString45 { get; set; }
        [Column("column_string_46")]
        public string ColumnString46 { get; set; }
        [Column("column_string_47")]
        public string ColumnString47 { get; set; }
        [Column("column_string_48")]
        public string ColumnString48 { get; set; }
        [Column("column_string_49")]
        public string ColumnString49 { get; set; }
        [Column("column_string_50")]
        public string ColumnString50 { get; set; }
#if NET6_0_OR_GREATER
#nullable restore
#endif



#if NET6_0_OR_GREATER
        [Column("description")]
        public string? Description { get; set; }
#else
        [Column("description")]
        public string Description { get; set; }
#endif
    }
}
