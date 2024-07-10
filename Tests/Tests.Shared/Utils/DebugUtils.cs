using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using OrmTxcSql.Attributes;
using OrmTxcSql.Entities;

namespace OrmTxcSql.Tests.Utils
{
    /// <summary>
    /// デバッグ用のユーティリティクラス。
    /// </summary>
    public class DebugUtils
    {
        public static void DumpEntity<TEntity>(IEnumerable<TEntity> entities, int maxNumberOfLines = 10, string delimiter = ",\t")
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
            // エンティティ名を取得する。
            string entityName = typeof(TEntity).Name;
            //
            //
            // エンティティ名を出力する。
            Debug.WriteLine($"ENTITY NAME: {entityName}");
            // 見出し行を出力する。
            {
                var builder = new StringBuilder();
                builder.Append("No");
                foreach (string columnName in columnNames)
                {
                    builder.Append(delimiter);
                    builder.Append(columnName);
                }
                Debug.WriteLine(builder.ToString());
            }
            // 内容を出力する。
            int numberOfLines = 0;
            foreach (TEntity entity in entities.Take(maxNumberOfLines))
            {
                var builder = new StringBuilder();
                // 行数を出力する。（行数はインクリメント済み）
                builder.Append($"{++numberOfLines:D2}");
                // プロパティ値を出力する。
                foreach (PropertyInfo property in properties)
                {
                    builder.Append(delimiter);
                    if (property.GetValue(entity) is DateTime dateTime)
                    {
                        builder.Append(dateTime.ToString("yyyy/MM/dd HH:mm:ss.fffffff"));
                    }
                    else if (property.GetValue(entity) is TimeSpan timeSpan)
                    {
                        builder.Append(timeSpan.ToString(@"hh\:mm\:ss\.fffffff"));
                    }
#if NET6_0_OR_GREATER
                    else if (property.GetValue(entity) is TimeOnly timeOnly)
                    {
                        builder.Append(timeOnly.ToString("HH:mm:ss.fffffff"));
                    }
#endif
                    else
                    {
                        builder.Append(property.GetValue(entity));
                    }
                }
                Debug.WriteLine(builder.ToString());
            }
        }
        public static void DumpEntity<TEntity>(TEntity entity, int maxNumberOfLines = 10, string delimiter = ",\t")
            where TEntity : AbstractEntity
            => DebugUtils.DumpEntity<TEntity>(new TEntity[] { entity }, maxNumberOfLines, delimiter);
    }
}
