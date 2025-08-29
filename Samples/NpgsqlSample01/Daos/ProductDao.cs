using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Npgsql;
using NpgsqlSample01.Entities;
using OrmTxcSql.Attributes;
using OrmTxcSql.Utils;

namespace NpgsqlSample01.Daos
{
    public class ProductDao : BaseNpgsqlDao<ProductEntity>
    {
        public override ProductEntity[] Select(ProductEntity entity)
        {
            // テーブル名を取得する。
            string tableName = EntityUtils.GetTableName<ProductEntity>();
            // カラム名を取得する。
            string[] columnNames = EntityUtils.GetColumnAttributes<ProductEntity>()
                .Select(prop => new
                {
                    PropertyInfo = prop,
                    ColumnName = prop.GetCustomAttribute<ColumnAttribute>(false)?.ColumnName ?? String.Empty
                })
                .Where(src => !String.IsNullOrEmpty(src.ColumnName))
                .Select(src => src.ColumnName)
                .ToArray();
            //
            // コマンドテキストを生成する。
            // 開発者向けコメント：（fool-proof：条件の前に、空白を１つはさむ）
            // OK: select a, b, c from table_name where x = @x
            // NG: select a, b, c from table_namewhere x = @x
            var builder = new StringBuilder();
            builder.Append(" select");
            builder.Append(String.Join(",", columnNames.Select(columnName => String.Format(" x.{0}", columnName))));
            builder.Append(this.GetCommonFieldForSelect("x"));
            builder.Append(" from ").Append(tableName).Append(" as x");
            //
            //
            // コマンドとテキストを設定する。
            NpgsqlCommand command = this.Command;
            command.CommandText = builder.ToString();
            // パラメータを設定する。（主要条件）
            // パラメータを設定する。（その他）
            //
            // データソースにコマンドを準備する。
            command.Prepare();
            //
            // コマンドを実行する。
            DataTable dt = this.GetDataTable(command);
            //
            // 結果を戻す。
            ProductEntity[] result = dt.AsEnumerable<ProductEntity>().ToArray();
            // 結果を戻す。
            return result;
        }
    }
}
