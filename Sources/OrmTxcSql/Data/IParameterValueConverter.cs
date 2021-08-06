using System;

namespace OrmTxcSql.Data
{

    public interface IParameterValueConverter
    {

        object Convert(object value, Type targetType, object parameter);

    }

}
