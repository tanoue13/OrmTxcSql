using System;
using OrmTxcSql.Data;
using OrmTxcSql.Npgsql.Utils;

namespace OrmTxcSql.Npgsql.Data
{
    /// <summary>
    /// NpgsqlParameterの値に変換するコンバータ。
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class NpgsqlParameterValueConverter : IParameterValueConverter
    {
        /// <summary>
        /// 変換する。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        /// <remarks>
        /// 開発者向けコメント：日付/時刻データ型について<br/>
        /// ・ C# 、および、 Npgsql において、 .NET6 以降で破壊的な変更がある。<br/>
        /// ・ C#： DateOnly型、TimeOnly型の導入。<br/>
        /// ・ Npgsql： <c>timestamp with time zone</c>は、 UTC タイムスタンプを表す。<c>timestamp without time zone</c>は、ローカル、または、 unspecified を表す。<br/>
        /// <see href="https://www.npgsql.org/doc/types/datetime.html#timestamps-and-timezones"/><br/>
        /// <see href="https://www.npgsql.org/doc/release-notes/6.0.html#major-changes-to-timestamp-mapping"/><br/>
        /// </remarks>
#if NET6_0_OR_GREATER
        public object Convert(object? value, Type? targetType, object? parameter)
#else
        public object Convert(object value, Type targetType, object parameter)
#endif
        {
            if (null == value)
            {
                // nullの場合、DBNullを戻す。
                return DBNull.Value;
            }
            //
            Type valueType = value.GetType();
            if (typeof(DBNull).Equals(valueType))
            {
                // DBNullを戻す。
                return DBNull.Value;
            }
            else if (typeof(string).Equals(valueType))
            {
                // string型を変換する。
                var sValue = value as string;
                if (null != sValue)
                {
                    return value;
                }
                else
                {
                    return DBNull.Value;
                }
            }
            else if (typeof(int?).Equals(valueType))
            {
                // int型を変換する。
                int? iValue = value as int?;
                if (iValue.HasValue)
                {
                    return value;
                }
                else
                {
                    return DBNull.Value;
                }
            }
            else if (typeof(decimal?).Equals(valueType))
            {
                // int型を変換する。
                decimal? dValue = value as decimal?;
                if (dValue.HasValue)
                {
                    return value;
                }
                else
                {
                    return DBNull.Value;
                }
            }
#if NET6_0_OR_GREATER
            else if (typeof(DateOnly?).Equals(valueType))
            {
                // DateTime型を変換する。
                DateOnly? dValue = value as DateOnly?;
                if (dValue.HasValue)
                {
                    return value;
                }
                else
                {
                    return DBNull.Value;
                }
            }
#endif
            else if (typeof(DateTime?).Equals(valueType))
            {
                // DateTime型を変換する。
                DateTime? dValue = value as DateTime?;
                if (dValue.HasValue)
                {
                    return DateTimeUtils.ToUniversalTime(dValue.Value);
                }
                else
                {
                    return DBNull.Value;
                }
            }
            else if (typeof(DateTime).Equals(valueType))
            {
                // DateTime型を変換する。
                if (value is DateTime dValue)
                {
                    return DateTimeUtils.ToUniversalTime(dValue);
                }
                else
                {
                    return value;
                }
            }
            // fool-proof
            return value;
        }
    }
}
