using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OrmTxcSql.Attributes;
using OrmTxcSql.Npgsql.Entities;
using OrmTxcSql.Utils;

namespace NpgsqlSample01.Entities
{
    /// <summary>
    /// PostgreSQLのテーブルの共通項目を保持するエンティティクラス。
    /// </summary>
    public abstract class BaseNpgsqlEntity : NpgsqlEntity
    {
        /// <summary>
        /// バージョンNo.を取得または設定します。
        /// </summary>
        [Column("version_no")]
        public int? VersionNo { get; set; }

        /// <summary>
        /// アクティブフラグを取得または設定します。
        /// </summary>
        [Column("active_flag")]
#if NET6_0_OR_GREATER
        public string? ActiveFlag { get; set; }
#else
        public string ActiveFlag { get; set; }
#endif
        /// <summary>
        /// 登録日時を取得または設定します。
        /// </summary>
        [Column("toroku_nitiji")]
        public DateTime? TorokuNitiji { get; set; }
        /// <summary>
        /// 登録ユーザ名を取得または設定します。
        /// </summary>
        [Column("toroku_username")]
#if NET6_0_OR_GREATER
        public string? TorokuUsername { get; set; }
#else
        public string TorokuUsername { get; set; }
#endif

        /// <summary>
        /// 更新日時を取得または設定します。
        /// </summary>
        [Column("kosin_nitiji")]
        public DateTime? KosinNitiji { get; set; }
        /// <summary>
        /// 更新ユーザ名を取得または設定します。
        /// </summary>
        [Column("kosin_username")]
#if NET6_0_OR_GREATER
        public string? KosinUsername { get; set; }
#else
        public string KosinUsername { get; set; }
#endif

        public override void SetProperties(DataRow dataRow)
        {
            // 基底クラスの処理を置き換える。
            // base.SetProperties(dataRow);
            //
            // DataTable.Columnsを取得する。
            DataColumnCollection dataColumnCollection = dataRow.Table.Columns;
            // DataTable.Columnsに存在するカラム属性のみを取得する。（共通項目を含む）
            IEnumerable<PropertyInfo> properties = this.GetColumnAttributes()
                .Concat(EntityUtils.GetColumnAttributes<BaseNpgsqlEntity>())
                .Select(prop => new
                {
                    PropertyInfo = prop,
                    ColumnName = prop.GetCustomAttribute<ColumnAttribute>(false)?.ColumnName ?? String.Empty
                })
                .Where(src => !String.IsNullOrEmpty(src.ColumnName))
                .Where(src => dataColumnCollection.Contains(src.ColumnName))
                .Select(src => src.PropertyInfo);
            //
            // プロパティを設定する。
            Parallel.ForEach(properties, property =>
            {
                this.SetProperty(dataRow, property);
            });
        }
        protected override void SetProperty(DataRow dataRow, PropertyInfo propertyInfo)
        {
            // 基底クラスの処理を置き換える。
            // base.SetProperty(dataRow, propertyInfo);
            //
            // dataRowから値を取得する。
            var value = this.GetValue(dataRow, propertyInfo);
            // 値を設定する。
            propertyInfo.SetValue(this, value);
        }
#if NET6_0_OR_GREATER
        [return: MaybeNull]
#endif
        private object GetValue(DataRow dataRow, PropertyInfo propertyInfo)
        {
            string columnName = propertyInfo.GetCustomAttribute<ColumnAttribute>(false)?.ColumnName ?? String.Empty;
            // DBNullの場合、nullを設定する。
            if (dataRow.IsNull(columnName))
            {
                return null;
            }
            //
            object value = dataRow[columnName];
            // <例外処理>
            if (value is decimal decValue)
            {
                // プロパティの型にあわせて変換する。
                Type type = propertyInfo.PropertyType;
                if (new Type[] { typeof(int), typeof(int?) }.Contains(type))
                {
                    return (int)decValue;
                }
                else
                {
                    return decValue;
                }
            }
#if NET6_0_OR_GREATER
            else if (value is DateTime dateTimeValue)
            {
                // プロパティの型にあわせて変換する。
                Type type = propertyInfo.PropertyType;
                if (new Type[] { typeof(DateOnly), typeof(DateOnly?) }.Contains(type))
                {
                    return DateOnly.FromDateTime(dateTimeValue);
                }
                else
                {
                    return dateTimeValue;
                }
            }
            else if (value is TimeSpan timeSpanValue)
            {
                // プロパティの型にあわせて変換する。
                Type type = propertyInfo.PropertyType;
                if (new Type[] { typeof(TimeOnly), typeof(TimeOnly?) }.Contains(type))
                {
                    return TimeOnly.FromTimeSpan(timeSpanValue);
                }
                else
                {
                    return timeSpanValue;
                }
            }
#endif
            // </例外処理>
            //
            // fool-proof
            return value;
        }

        #region"BaseNpgsqlDao用のプロパティ（定数）"
        /// <summary>
        /// バージョンNo.の開始値。
        /// </summary>
        public static readonly int InitialVersionNo = 0;
        /// <summary>
        /// アクティブを表すアクティブフラグの値。
        /// </summary>
        public static readonly string ActiveFlagIsActive = "A";
        #endregion
    }
}
