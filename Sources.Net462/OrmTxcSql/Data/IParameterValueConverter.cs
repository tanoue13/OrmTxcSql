using System;

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
        object Convert(object value, Type targetType, object parameter);
    }
}
