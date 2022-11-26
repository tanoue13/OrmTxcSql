using System;
using System.Diagnostics.CodeAnalysis;

namespace OrmTxcSql.Data
{
    /// <summary>
    /// <see cref="System.Data.IDataParameter">IDataParameter</see>の値に変換するコンバータ。
    /// </summary>
    public interface IParameterValueConverter
    {
        /// <summary>
        /// 変換する。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
#if NET6_0_OR_GREATER
        object Convert([AllowNull] object value, [AllowNull] Type targetType, [AllowNull] object parameter);
#else
        object Convert(object value, Type targetType, object parameter);
#endif
    }
}
