using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OrmTxcSql.Attributes;
using OrmTxcSql.Utils;

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
            // DataTable.Columnsを取得する。
            DataColumnCollection dataColumnCollection = dataRow.Table.Columns;
            // DataTable.Columnsに存在するカラム属性のみを取得する。
            IEnumerable<PropertyInfo> properties = this.GetColumnAttributes()
                // 変換
                .Select(prop => new
                {
                    PropertyInfo = prop,
                    ColumnName = prop.GetCustomAttribute<ColumnAttribute>(false)?.ColumnName ?? String.Empty
                })
                // カラム名を取得できない場合、対象外とする。
                .Where(src => !String.IsNullOrEmpty(src.ColumnName))
                // DataTable.Columnsに存在するカラム属性のみを取得する。
                .Where(src => dataColumnCollection.Contains(src.ColumnName))
                // 選択
                .Select(src => src.PropertyInfo);
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
            // プロパティのカラム属性からカラム名を取得する。
            // カラム名を取得できない場合、例外を投げる。（呼び出し元で検証済みの前提であり、通常はありえない状況）
            string columnName = propertyInfo.GetCustomAttribute<ColumnAttribute>(false)?.ColumnName
                ?? throw new ArgumentException($"{typeof(ColumnAttribute).Name} is not set on {propertyInfo.Name}.");
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
