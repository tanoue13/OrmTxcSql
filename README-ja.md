# OrmTxcSql

O/R mapping, Transaction control, SQL

## 概要

OrmTxcSqlは、.NETアプリケーション向けの軽量なO/Rマッピング、トランザクション制御、SQL実行ライブラリです。

## パッケージ

- **OrmTxcSql** - コアライブラリ
- **OrmTxcSql.Npgsql** - PostgreSQL対応
- **OrmTxcSql.SqlClient** - SQL Server対応
- **OrmTxcSql.DB2** - IBM DB2/iSeries対応

## 対応フレームワーク

- .NET 8.0
- .NET 6.0
- .NET Framework 4.8
- .NET Framework 4.6.2

## インストール

```bash
# PostgreSQL
dotnet add package OrmTxcSql.Npgsql

# SQL Server
dotnet add package OrmTxcSql.SqlClient

# IBM DB2/iSeries
dotnet add package OrmTxcSql.DB2
```

## 基本的な使用方法

### エンティティの定義

```csharp
[Table("products")]
public class ProductEntity : BaseNpgsqlEntity
{
    [Column("product_code"), PrimaryKey]
    public string ProductCode { get; set; }
    [Column("description")]
    public string Description { get; set; }
}
```

### DAOの実装

```csharp
public class ProductDao : BaseNpgsqlDao<ProductEntity>
{
    public override ProductEntity[] Select(ProductEntity entity)
    {
        // table name
        string tableName = EntityUtils.GetTableName<ProductEntity>();
        // column names
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
        // sql
        var builder = new StringBuilder();
        builder.Append(" select");
        builder.Append(String.Join(",", columnNames.Select(columnName => String.Format(" x.{0}", columnName))));
        builder.Append(this.GetCommonFieldForSelect("x"));
        builder.Append(" from ").Append(tableName).Append(" as x");
        builder.Append(" where");
        builder.Append("   x.product_code like @parameter_product_code_prefix || '%'");
        builder.Append(this.GetCommonFieldForSelectCondition("x", true));
        //
        // set up command text
        NpgsqlCommand command = this.Command;
        command.CommandText = builder.ToString();
        // set up command parameters
        NpgsqlServer.AddParameterOrReplace(command, "@parameter_product_code_prefix", NpgsqlDbType.Varchar, entity.ProductCode);
        //
        // prepara command
        command.Prepare();
        //
        // return result
        DataTable dt = this.GetDataTable(command);
        ProductEntity[] result = dt.AsEnumerable<ProductEntity>().ToArray();
        return result;
    }
}
```

### データソースの設定

```csharp
// PostgreSQL
var dataSource = new NpgsqlDataSource("Host=localhost;Database=mydb;Username=user;Password=pass");

// SQL Server
var dataSource = new SqlClientDataSource("Server=localhost;Database=mydb;Integrated Security=true");
```

## 主な機能

- シンプルなO/Rマッピング
- 自動トランザクション管理
- パラメータ化クエリ対応
- 複数データベース対応
- 軽量で高性能

## ライセンス

MIT License

## リポジトリ

https://github.com/tanoue13/OrmTxcSql
