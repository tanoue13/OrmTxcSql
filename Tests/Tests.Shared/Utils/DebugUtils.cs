using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using OrmTxcSql.Attributes;
using OrmTxcSql.Entities;

namespace OrmTxcSql.Tests.Utils
{
    /// <summary>
    /// デバッグ用のユーティリティクラス。
    /// </summary>
    public class DebugUtils
    {
        public static void DumpEntity<TEntity>(IEnumerable<TEntity> entities, int maxNumberOfLines = 10, string delimiter = ",")
            where TEntity : AbstractEntity
        {
            // カラム属性を取得する。
            PropertyInfo[] properties = typeof(TEntity)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => prop.GetCustomAttribute<ColumnAttribute>(false) != null)
                .ToArray();
            // カラム名を取得する。
            string[] columnNames = properties
                .Select(prop => prop.GetCustomAttribute<ColumnAttribute>(false)?.ColumnName ?? String.Empty)
                .Where(columnName => !String.IsNullOrEmpty(columnName))
                .ToArray();
            //
            // 見出し行を出力する。
            Debug.Write("No");
            foreach (string columnName in columnNames)
            {
                Debug.Write(delimiter);
                Debug.Write("\t");
                Debug.Write(columnName);
            }
            Debug.WriteLine("");
            // 内容を出力する。
            int numberOfLines = 0;
            foreach (TEntity entity in entities)
            {
                // 最大件数を超える場合、処理を終了する。
                if (numberOfLines >= maxNumberOfLines)
                {
                    break;
                }
                // 行数を出力する。（行数はインクリメント済み）
                Debug.Write(++numberOfLines);
                // プロパティ値を出力する。
                foreach (PropertyInfo property in properties)
                {
                    Debug.Write(delimiter);
                    Debug.Write("\t");
                    Debug.Write(property.GetValue(entity));
                }
                Debug.WriteLine("");
            }
        }
    }
}
