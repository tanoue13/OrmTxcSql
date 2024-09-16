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
        public static void Dump<TEntity>(IEnumerable<TEntity> entities, int maxNumberOfLines = 10, string delimiter = ",\t")
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
        public static void Dump<TEntity>(TEntity entity, int maxNumberOfLines = 10, string delimiter = ",\t")
            where TEntity : AbstractEntity
            => DebugUtils.Dump<TEntity>(new TEntity[] { entity }, maxNumberOfLines, delimiter);

        /// <summary>
        /// <paramref name="action"/>を実行し、その処理時間を計測します。
        /// </summary>
        /// <param name="name">ログに出力する名前</param>
        /// <param name="action">処理時間を計測する処理</param>
        /// <returns></returns>
        public static TimeSpan RunWithTimeMeasurement(string name, Action action)
        {
            // 開始時間を取得する。（ログにも出力）
            DateTime startTime = DateTime.Now;
            Debug.WriteLine(String.Format("BEGIN: {0}; {1}", startTime.ToString(), name));
            //
            // 処理を実行する。
            action();
            //
            // 終了時間を取得し、経過時間を算出する。（ログにも出力）
            DateTime endTime = DateTime.Now;
            TimeSpan timeSpan = endTime.Subtract(startTime);
            Debug.WriteLine(String.Format("END:   {0}; {1}; timespan : {2:F12}", endTime.ToString(), name, timeSpan.TotalSeconds));
            //
            // 結果を戻す。
            return timeSpan;
        }

    }
}
