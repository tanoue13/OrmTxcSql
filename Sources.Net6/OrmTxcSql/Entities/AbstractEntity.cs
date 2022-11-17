using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OrmTxcSql.Attributes;

namespace OrmTxcSql.Entities
{
    /// <summary>
    /// Entityの基底クラス。
    /// </summary>
    public abstract class AbstractEntity
    {
        /// <summary>
        /// <paramref name="dataRow"/>から値を取得し、プロパティを設定する。
        /// </summary>
        /// <param name="dataRow"></param>
        public virtual void SetProperties(DataRow dataRow)
        {
#if NET6_0_OR_GREATER
#nullable disable
#endif
            // DataTable.Columnsを取得する。
            DataColumnCollection dataColumnCollection = dataRow.Table.Columns;
            // DataTable.Columnsに存在するカラム属性のみを取得する。
            IEnumerable<PropertyInfo> properties = this.GetColumnAttributes()
                .Where(prop => dataColumnCollection.Contains(prop.GetCustomAttribute<ColumnAttribute>(false).ColumnName));
#if NET6_0_OR_GREATER
#nullable restore
#endif
            //
            // プロパティを設定する。
            Parallel.ForEach(properties, property =>
            {
                this.SetProperty(dataRow, property);
            });
        }
        /// <summary>
        /// <paramref name="dataRow"/>から値を取得し、<paramref name="propertyInfo"/>で指定されたプロパティに設定する。
        /// </summary>
        /// <param name="dataRow"></param>
        /// <param name="propertyInfo"></param>
        protected virtual void SetProperty(DataRow dataRow, PropertyInfo propertyInfo)
        {
#if NET6_0_OR_GREATER
#nullable disable
#endif
            string columnName = propertyInfo.GetCustomAttribute<ColumnAttribute>(false).ColumnName;
#if NET6_0_OR_GREATER
#nullable restore
#endif
            if (dataRow.IsNull(columnName))
            {
                // DBNullの場合、nullを設定する。
                propertyInfo.SetValue(this, null);
            }
            else
            {
                // 取得した値を設定する。
                object value = dataRow[columnName];
                propertyInfo.SetValue(this, value);
            }
        }
    }
}
