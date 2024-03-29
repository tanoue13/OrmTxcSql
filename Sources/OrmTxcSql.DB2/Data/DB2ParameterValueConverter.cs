﻿using System;
using IBM.Data.DB2.iSeries;
using OrmTxcSql.Data;

namespace OrmTxcSql.DB2.Data
{
    /// <summary>
    /// DB2Parameterの値に変換するコンバータ。
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class DB2ParameterValueConverter : IParameterValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">DB2Typeを指定する。<paramref name="value"/>がNullの場合、DB2Typeに応じた初期値を使用する。</param>
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
                return DB2ParameterValueConverter.GetDataSourceNullValue(targetType, parameter);
            }
            //
            Type valueType = value.GetType();
            if (typeof(DBNull).Equals(valueType))
            {
                // DBNullを戻す。
                return DB2ParameterValueConverter.GetDataSourceNullValue(targetType, parameter);
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
                    return DB2ParameterValueConverter.GetDataSourceNullValue(targetType, parameter);
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
                    return DB2ParameterValueConverter.GetDataSourceNullValue(targetType, parameter);
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
                    return DB2ParameterValueConverter.GetDataSourceNullValue(targetType, parameter);
                }
            }
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
                    return DB2ParameterValueConverter.GetDataSourceNullValue(targetType, parameter);
                }
            }
            // fool-proof
            return value;
        }
        /// <summary>
        /// データソースにおいて、nullとして扱われる値を取得する。（データ型に応じた値が戻される）
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
#if NET6_0_OR_GREATER
        private static object GetDataSourceNullValue(Type? targetType, object? parameter)
#else
        private static object GetDataSourceNullValue(Type targetType, object parameter)
#endif
        {
            switch (parameter)
            {
                case iDB2DbType.iDB2Numeric:
                    {
                        return Decimal.Zero;
                    }
                case iDB2DbType.iDB2Decimal:
                    {
                        return Decimal.Zero;
                    }
                case iDB2DbType.iDB2Char:
                    {
                        return String.Empty;
                    }
                case iDB2DbType.iDB2VarChar:
                    {
                        return String.Empty;
                    }
                default:
                    {
                        string message = "文字列／数値以外の型が存在します。";
                        var exception = new NotSupportedException(message);
                        throw exception;
                    }
            }
        }

    }

}
