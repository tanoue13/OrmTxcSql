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
        public DateTime? ColumnDate { get; set; }

        [Column("column_time_without_time_zone")]
        public TimeSpan? ColumnTimeWithoutTimeZone { get; set; }

        [Column("column_time_with_time_zone")]
        public DateTimeOffset? ColumnTimeWithTimeZone { get; set; }

#if NET6_0_OR_GREATER
        [Column("column_net6_date")]
        public DateOnly? ColumnNet6Date { get; set; }
        [Column("column_net6_time_without_time_zone")]
        public TimeOnly? ColumnNet6TimeWithoutTimeZone { get; set; }
        [Column("column_net6_time_with_time_zone")]
        public DateTimeOffset? ColumnNet6TimeWithTimeZone { get; set; }
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
