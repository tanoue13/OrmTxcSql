using System;
using OrmTxcSql.Attributes;

namespace SqlSample01.Entities
{
    [Table("date_and_time_table")]
    public class DateAndTimeEntity : BaseSqlEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// ・開発者向けコメント：
        /// 　SQL Server の予約済みキーワードに key が含まれるため、暫定対応としてカラム物理名を変更しています。
        /// </remarks>
        [Column("column_key"), PrimaryKey]
#if NET6_0_OR_GREATER
        public string? Key { get; set; }
#else
        public string Key { get; set; }
#endif

        [Column("column_time")]
        public TimeSpan? ColumnTime { get; set; }

        [Column("column_date")]
        public DateTime? ColumnDate { get; set; }

        [Column("column_date_time")]
        public DateTime? ColumnDateTime { get; set; }

        [Column("column_date_time_2")]
        public DateTime? ColumnDateTime2 { get; set; }

        [Column("column_date_time_offset")]
        public DateTimeOffset? ColumnDateTimeOffset { get; set; }

#if NET6_0_OR_GREATER
        [Column("column_net6_date")]
        public DateOnly? ColumnNet6Date { get; set; }
        [Column("column_net6_time")]
        public TimeOnly? ColumnNet6Time { get; set; }
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
