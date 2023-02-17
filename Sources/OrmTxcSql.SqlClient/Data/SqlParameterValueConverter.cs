using System;
using System.Data;
using System.Data.SqlClient;
using OrmTxcSql.Data;

namespace OrmTxcSql.SqlClient.Data
{
    /// <summary>
    /// SqlParameterの値に変換するコンバータ。
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class SqlParameterValueConverter : IParameterValueConverter
    {
        /// <summary>
        /// 変換する。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
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
                // decimal型を変換する。
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
                // DateOnly型を変換する。
                DateOnly? dValue = value as DateOnly?;
                if (dValue.HasValue)
                {
                    return ToDateTime(dValue.Value);
                }
                else
                {
                    return DBNull.Value;
                }
            }
            else if (typeof(DateOnly).Equals(valueType))
            {
                // DateOnly型を変換する。
                if (value is DateOnly dValue)
                {
                    return ToDateTime(dValue);
                }
                else
                {
                    return value;
                }
            }
#endif
            else if (typeof(DateTime?).Equals(valueType))
            {
                // DateTime型を変換する。
                DateTime? dValue = value as DateTime?;
                if (dValue.HasValue)
                {
                    return value;
                }
                else
                {
                    return DBNull.Value;
                }
            }
            // fool-proof
            return value;
        }
#if NET6_0_OR_GREATER
        private static readonly TimeOnly s_timeOnlyZero = new TimeOnly(0, 0, 0);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>
        /// 開発者向けコメント：
        /// <br/>・<see cref="SqlParameter.SqlDbType"/>が<see cref="SqlDbType.Date"/>の<see cref="SqlParameter"/>において、
        /// 　　<see cref="SqlParameter.Value"/>に<see cref="DateOnly"/>型の値を設定すると<see cref="InvalidCastException"/>が投げられる。
        /// <br/>・原因については、要調査。
        /// <br/>・暫定回避策として、<see cref="DateTime"/>に変換した値を設定する。
        /// <code>
        ///  メッセージ: 
        ///  System.InvalidCastException : Failed to convert parameter value from a DateOnly to a DateTime.
        ///    ----> System.InvalidCastException : Object must implement IConvertible.
        /// </code>
        /// </remarks>
        private static DateTime ToDateTime(DateOnly value)
            => value.ToDateTime(s_timeOnlyZero);
#endif
    }
}
