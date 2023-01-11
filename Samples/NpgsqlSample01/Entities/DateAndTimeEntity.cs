using System;
using OrmTxcSql.Attributes;

namespace NpgsqlSample01.Entities
{
    [Table("date_and_time_table")]
    public class DateAndTimeEntity : BaseNpgsqlEntity
    {
        [Column("key"), PrimaryKey]
#if NET6_0_OR_GREATER
        public string? Key { get; set; }
#else
        public string Key { get; set; }
#endif

        [Column("column_timestamp_without_time_zone")]
        public DateTime? ColumnTimestampWithoutTimeZone { get; set; }

        [Column("column_timestamp_with_time_zone")]
        public DateTime? ColumnTimestampWithTimeZone { get; set; }

        [Column("column_date")]
#if NET6_0_OR_GREATER
        public DateOnly? ColumnDate { get; set; }
#else
        public DateTime? ColumnDate { get; set; }
#endif

        [Column("column_time_without_time_zone")]
#if NET6_0_OR_GREATER
        public TimeOnly? ColumnTimeWithoutTimeZone { get; set; }
#else
        public DateTime? ColumnTimeWithoutTimeZone { get; set; }
#endif

        [Column("column_time_with_time_zone")]
#if NET6_0_OR_GREATER
        public TimeOnly? ColumnTimeWithTimeZone { get; set; }
#else
        public DateTime? ColumnTimeWithTimeZone { get; set; }
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
