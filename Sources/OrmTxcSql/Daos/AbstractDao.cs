using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using OrmTxcSql.Attributes;
using OrmTxcSql.Entities;
using OrmTxcSql.Utils;

namespace OrmTxcSql.Daos
{
    /// <summary>
    /// Daoの基底クラス。
    /// </summary>
    public abstract class AbstractDao<TEntity, TDbCommand, TDbDataAdapter> : IDao
        where TEntity : AbstractEntity, new()
        where TDbCommand : DbCommand, new()
        where TDbDataAdapter : DbDataAdapter, new()
    {

        /// <summary>
        /// <c>PrimaryKeyAttribute</c>が設定されていない<c>TEntity</c>において<c>UpdateByPk</c>, <c>SelectByPk</c>が呼び出された場合に、
        /// 投げられる例外に設定されるメッセージ。
        /// </summary>
        protected static readonly string MissingPrimaryKeyExceptionMessage = $"{nameof(PrimaryKeyAttribute)} is not found in {typeof(TEntity).Name}.";

        /// <summary>
        /// 例外に設定されるメッセージ：INSERT文を実行する必要がないエンティティが渡された場合
        /// </summary>
        protected static readonly string ArgumentExceptionMessageForNoNeedToInsert = $"No property values are set.";
        /// <summary>
        /// 例外に設定されるメッセージ：UPDATE文を実行する必要がないエンティティが渡された場合
        /// </summary>
        protected static readonly string ArgumentExceptionMessageForNoNeedToUpdate = $"No property values are different to those in data source.";

        /// <summary>
        /// 例外に設定されるメッセージ：対象プロパティが与えられていない場合
        /// </summary>
        protected static readonly string ArgumentExceptionMessageForNoTargetPropertyIsGiven = $"No target property is given.";

        IEnumerable<IDbCommand> IDao.Commands { get => this._commandCollection; }
        private readonly IEnumerable<IDbCommand> _commandCollection;

        /// <summary>
        /// <typeparamref name="TDbCommand"/>を取得します。
        /// </summary>
        protected TDbCommand Command { get; } = new TDbCommand();

        /// <summary>
        /// 文字列型のフィールドについて、末尾をトリムするかどうかを取得または設定します。
        /// </summary>
        public bool TrimEnd { protected get; set; } = true;

        /// <summary>
        /// コンストラクタ.
        /// </summary>
        public AbstractDao()
        {
            this._commandCollection = new IDbCommand[] { this.Command };
        }

        /// <summary>
        /// 新規登録する。（１件）
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns></returns>
        public abstract int Insert(TEntity entity);

        /// <summary>
        /// 更新する。（１件）
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns></returns>
        public abstract int UpdateByPk(TEntity entity);
        /// <summary>
        /// 更新する。（１件）（非null項目のみ）
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns></returns>
        public abstract int UpdateUnlessNullByPk(TEntity entity);

        /// <summary>
        /// 検索する。（１件）
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
#if NET6_0_OR_GREATER
        public abstract TEntity? SelectByPk(TEntity entity);
#else
        public abstract TEntity SelectByPk(TEntity entity);
#endif

        /// <summary>
        /// 検索する。（複数件）
        /// </summary>
        /// <param name="entity">検索条件</param>
        /// <returns></returns>
        public abstract TEntity[] Select(TEntity entity);

        /// <summary>
        /// SELECT文実行時の共通処理。
        /// </summary>
        /// <param name="command">command</param>
        /// <returns></returns>
        protected DataTable GetDataTable(TDbCommand command)
        {
            // ログを出力する。
            LogUtils.LogSql(command);
            //
            // コマンドを実行する。
            DataTable dataTable = new DataTable();
            using (var adapter = new TDbDataAdapter())
            {
                adapter.SelectCommand = command;
                adapter.Fill(dataTable);
            }
            //
            // 末尾をトリムする場合、トリムする。
            if (this.TrimEnd)
            {
                AbstractDao<TEntity, TDbCommand, TDbDataAdapter>.TrimEndStringTypeColumnValues(dataTable);
            }
            // 内部処理での変更内容をコミットする。
            dataTable.AcceptChanges();
            //
            // 結果を戻す。
            return dataTable;
        }
        /// <summary>
        /// SELECT文実行時の共通処理。
        /// </summary>
        /// <param name="command">command</param>
        /// <returns></returns>
        protected async Task<DataTable> GetDataTableAsync(TDbCommand command)
        {
            // ログを出力する。
            LogUtils.LogSql(command);
            //
            // コマンドを実行する。
            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                // DataTableを初期化する。（スキーマ情報が設定された空のDataTableを生成）
#if NET6_0_OR_GREATER
                DataTable dataTable = await AbstractDao<TEntity, TDbCommand, TDbDataAdapter>.CreateEmptyDataTableWithSchemaAsync(reader);
#else
                DataTable dataTable = AbstractDao<TEntity, TDbCommand, TDbDataAdapter>.CreateEmptyDataTableWithSchema(reader);
#endif
                //
                while (await reader.ReadAsync())
                {
                    // 新規レコードを生成する。（ここではDataTableに追加しない）
                    DataRow dataRow = dataTable.NewRow();
                    // 開発者向けコメント（2024.07.09田上）：
                    // DataTableへの追加は、全フィールドの値を設定した後で実施すること。
                    // 不正なデータ（文字化けなど）が含まれるフィールドが存在する場合に
                    // フィールドの値が途中まで設定された中途半端なレコードを取得することを防ぐため。
                    //
                    // フィールドの値を設定する。
                    foreach (DataColumn dataColumn in dataTable.Columns)
                    {
                        string columnName = dataColumn.ColumnName;
                        int ordinal = reader.GetOrdinal(columnName);
                        dataRow[columnName] = reader.GetValue(ordinal);
                    }
                    //
                    // 新規レコードをDataTableに追加する。
                    dataTable.Rows.Add(dataRow);
                }
                //
                // 末尾をトリムする場合、トリムする。
                if (this.TrimEnd)
                {
                    AbstractDao<TEntity, TDbCommand, TDbDataAdapter>.TrimEndStringTypeColumnValues(dataTable);
                }
                // 内部処理での変更内容をコミットする。
                dataTable.AcceptChanges();
                //
                // 結果を戻す。
                return dataTable;
            }
        }
#if NET6_0_OR_GREATER
        /// <summary>
        /// スキーマ情報が設定された空のDataTableを生成する。
        /// </summary>
        /// <param name="reader"><see cref="DbDataReader"/></param>
        /// <returns></returns>
        private static async Task<DataTable> CreateEmptyDataTableWithSchemaAsync(DbDataReader reader)
        {
            // スキーマ情報を取得する。
            IReadOnlyCollection<DbColumn> dbColumns = await reader.GetColumnSchemaAsync();
            //
            // 空のDataTableを生成する。
            DataTable dataTable = new();
            //
            // カラムを追加する。
            foreach (DbColumn dbColumn in dbColumns)
            {
                dataTable.Columns.Add(
                    new DataColumn()
                    {
                        ColumnName = dbColumn.ColumnName,
                        DataType = dbColumn.DataType,
                    }
                );
            }
            //
            // 結果を戻す。
            return dataTable;
        }
#else
        /// <summary>
        /// スキーマ情報が設定された空のDataTableを生成する。
        /// </summary>
        /// <param name="reader"><see cref="DbDataReader"/></param>
        /// <returns></returns>
        private static DataTable CreateEmptyDataTableWithSchema(DbDataReader reader)
        {
            // スキーマ情報を取得する。
            DataTable schemaTable = reader.GetSchemaTable();
            //
            // 空のDataTableを生成する。
            var dataTable = new DataTable();
            //
            // カラムを追加する。
            foreach (DataRow dataRow in schemaTable.Rows)
            {
                var dataColumn = new DataColumn();
                dataTable.Columns.Add(dataColumn);
                //
                dataColumn.ColumnName = dataRow["ColumnName"].ToString();
                dataColumn.DataType = Type.GetType(dataRow["DataType"].ToString());
            }
            //
            // 結果を戻す。
            return dataTable;
        }
#endif
        /// <summary>
        /// 文字列型のDataColumnの値の末尾をトリムする。
        /// </summary>
        /// <param name="dataTable"></param>
        private static void TrimEndStringTypeColumnValues(DataTable dataTable)
        {
            // 文字列型のDataColumnを対象とする。（トリム）
            IEnumerable<int> ordinals = dataTable.Columns.Cast<DataColumn>()
                .Where(dataColumn => typeof(string).Equals(dataColumn.DataType))
                .Select(x => x.Ordinal);
            foreach (int ordinal in ordinals)
            {
                // 値がnullでないDataRowのみを対象とする。（トリム）
                IEnumerable<DataRow> dataRows = dataTable.Rows.Cast<DataRow>()
                    .Where(dataRow => !dataRow.IsNull(ordinal));
                foreach (DataRow dataRow in dataRows)
                {
                    // トリムした値を設定する。
#if NET6_0_OR_GREATER
                    string? value = dataRow[ordinal] as string;
                    dataRow[ordinal] = value?.TrimEnd();
#else
                    string value = dataRow[ordinal] as string;
                    dataRow[ordinal] = value.TrimEnd();
#endif
                }
            }
            // 開発者向けコメント（2021.08.17田上）
            // DataRowに対する書き込み操作は、同期する必要があるとのこと。
            // したがって、非同期処理（Parallel.ForEach）を使用すると例外が投げられる。
        }

        /// <summary>
        /// レコードの存在有無を確認し、INSERT文、または、UPDATE文を実行する。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>影響を受けた行の数</returns>
        /// <remarks>
        /// ・このメソッドは、エンティティをデータソースに登録する際に登録したいエンティティがデータソースに存在する／存在しないを気にせずに使用できます。<br/>
        /// 　- 登録したいエンティティがデータソースに存在しない場合、新規登録されます。（INSERT文）<br/>
        /// 　- 登録したいエンティティがデータソースに存在する場合、更新されます。（UPDATE文）<br/>
        /// <br/>
        /// ・ただし、このメソッドを使用する際は、データソースに何らかの変更が発生することを想定しているため、次の場合には例外が投げられます。<br/>
        /// 　- 登録したいエンティティのプロパティに値が設定されたものが存在しない。（登録時に設定される値が存在しないため）<br/>
        /// 　- 登録したいエンティティのプロパティ値がデータソースに存在するエンティティのプロパティとすべて一致している。（更新時に変更が発生しないため）<br/>
        /// ・このような場合が発生する状況は呼び出し元でプロパティ値の設定が漏れていることがほとんどだと考えられるため、例外が投げられるようにしています。
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// ・<paramref name="entity"/> のプロパティに値が設定されたものが存在しない。（登録時に設定される値が存在せず、INSERT文を実行する意味がないため）<br/>
        /// ・<paramref name="entity"/> のプロパティ値がデータソースに存在するエンティティのプロパティとすべて一致している。（更新時に変更が発生せず、UPDATE文を実行する意味がないため）<br/>
        /// </exception>
        public int InsertOrUpdateByPk(TEntity entity)
        {
#if NET6_0_OR_GREATER
            TEntity? result = this.SelectByPk(entity);
#else
            TEntity result = this.SelectByPk(entity);
#endif
            if (null == result)
            {
                if (entity.HasEqualPropertyValues(new TEntity()))
                {
                    // 例外を投げる：引数のエンティティが初期状態と等価な場合、INSERT文を実行しない。
                    throw new ArgumentException(ArgumentExceptionMessageForNoNeedToInsert, nameof(entity));
                }
                else
                {
                    // INSERT文を実行する。
                    return this.Insert(entity);
                }
            }
            else
            {
                if (entity.HasEqualPropertyValues(result))
                {
                    // 例外を投げる：引数のエンティティが検索結果と等価な場合、UPDATE文を実行しない。
                    throw new ArgumentException(ArgumentExceptionMessageForNoNeedToUpdate, nameof(entity));
                }
                else
                {
                    // UPDATE文を実行する。
                    return this.UpdateByPk(entity);
                }
            }
        }

        /// <summary>
        /// 更新系SQL（INSERT, UPDATE, DELETE）を実行し、影響を受ける行の数を戻します。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="entity"></param>
        /// <param name="enableOptimisticConcurrency">楽観的排他制御を有効にする場合、true</param>
        /// <returns>影響を受けた行の数</returns>
        protected abstract int ExecuteNonQuery(TDbCommand command, TEntity entity, bool enableOptimisticConcurrency = true);

        #region"BaseEntity（共通項目）に関するプロパティや処理（メソッド名前を共通化するため、抽象メソッドとして定義）"

        /// <summary>
        /// SELECT文用の共通項目文字列を戻す。
        /// </summary>
        /// <param name="appendDelimiter">区切り文字の付加</param>
        /// <returns></returns>
        protected string GetCommonFieldForSelect(bool appendDelimiter = true)
        {
            return this.GetCommonFieldForSelect(String.Empty, appendDelimiter);
        }
        /// <summary>
        /// SELECT文用の共通項目文字列を戻す。
        /// </summary>
        /// <param name="tableAlias">テーブル別名</param>
        /// <param name="appendDelimiter">区切り文字の付加</param>
        /// <returns></returns>
        /// <remarks>
        /// 共通項目の前の区切り文字（, ）が必要な場合、引数（appendDelimiter）は設定不要です。
        /// 共通項目の前に区切り文字（, ）が不要な場合、引数（appendDelimiter）にfalseを設定してください。
        /// <example>
        /// 使用方法：
        /// <code>
        /// SELECT
        ///     field1
        ///   , field2
        ///   , field3
        ///   BaseDao.GetCommonFieldForSelect()
        /// WHERE
        ///   key_field1 = @key_value1
        ///   AND key_field2 = @key_value2
        ///   BaseDao.GetCommonFieldForSelectCondition()
        /// </code>
        /// </example>
        /// </remarks>
        protected virtual string GetCommonFieldForSelect(string tableAlias, bool appendDelimiter = true)
        {
            return String.Empty;
        }

        /// <summary>
        /// SELECT文用のWHERE句文字列を戻す。
        /// </summary>
        /// <param name="appendDelimiter">区切り文字の付加</param>
        /// <returns></returns>
        /// <remarks>
        /// 共通項目の前の区切り文字（ AND ）が必要な場合、引数（appendDelimiter）は設定不要です。
        /// 共通項目の前に区切り文字（ AND ）が不要な場合、引数（appendDelimiter）にfalseを設定してください。
        /// </remarks>
        protected string GetCommonFieldForSelectCondition(bool appendDelimiter = true)
        {
            return this.GetCommonFieldForSelectCondition(String.Empty, appendDelimiter);
        }
        /// <summary>
        /// SELECT文用のWHERE句文字列を戻す。
        /// </summary>
        /// <param name="tableAlias">テーブル別名</param>
        /// <param name="appendDelimiter">区切り文字の付加</param>
        /// <returns></returns>
        /// <remarks>
        /// 共通項目の前の区切り文字（ AND ）が必要な場合、引数（appendDelimiter）は設定不要です。
        /// 共通項目の前に区切り文字（ AND ）が不要な場合、引数（appendDelimiter）にfalseを設定してください。
        /// </remarks>
        protected virtual string GetCommonFieldForSelectCondition(string tableAlias, bool appendDelimiter = true)
        {
            return String.Empty;
        }

        /// <summary>
        /// INSERT文用の共通項目文字列を戻す。
        /// </summary>
        /// <param name="appendDelimiter">区切り文字の付加</param>
        /// <returns></returns>
        /// <remarks>
        /// 共通項目の前の区切り文字（, ）が必要な場合、引数（appendDelimiter）は設定不要です。
        /// 共通項目の前に区切り文字（, ）が不要な場合、引数（appendDelimiter）にfalseを設定してください。
        /// <example>
        /// 使用方法：
        /// <code>
        /// INSERT INTO table_name (
        ///     field1
        ///   , field2
        ///   , field3
        ///   BaseDao.GetCommonItemForInsert()
        /// ) VALUES (
        ///     @value1
        ///   , @value2
        ///   , @value3
        ///   BaseDao.GetCommonItemForInsertValue()
        /// )
        /// </code>
        /// </example>
        /// </remarks>
        protected virtual string GetCommonFieldForInsert(bool appendDelimiter = true)
        {
            return String.Empty;
        }
        /// <summary>
        /// INSERT文用のVALUE句文字列を戻す。
        /// </summary>
        /// <param name="appendDelimiter">区切り文字の付加</param>
        /// <returns></returns>
        /// <remarks>
        /// 共通項目の前の区切り文字（, ）が必要な場合、引数（appendDelimiter）は設定不要です。
        /// 共通項目の前に区切り文字（, ）が不要な場合、引数（appendDelimiter）にfalseを設定してください。
        /// </remarks>
        protected virtual string GetCommonFieldForInsertValue(bool appendDelimiter = true)
        {
            return String.Empty;
        }

        /// <summary>
        /// UPDATE文用の共通項目文字列を戻す。
        /// </summary>
        /// <param name="appendDelimiter">区切り文字の付加</param>
        /// <returns></returns>
        /// <remarks>
        /// 共通項目の前の区切り文字（, ）が必要な場合、引数（appendDelimiter）は設定不要です。
        /// 共通項目の前に区切り文字（, ）が不要な場合、引数（appendDelimiter）にfalseを設定してください。
        /// <example>
        /// 使用方法：
        /// <code>
        /// UPDATE table_name 
        /// SET
        ///     field1 = @value1
        ///   , field2 = @value2
        ///   , field3 = @value3
        ///   BaseDao.GetCommonItemForUpdate()
        /// WHERE
        ///   key_field1 = @key_value1
        ///   AND key_field2 = @key_value2
        ///   BaseDao.GetCommonItemForUpdateCondition()
        /// </code>
        /// </example>
        /// </remarks>
        protected virtual string GetCommonFieldForUpdate(bool appendDelimiter = true)
        {
            return String.Empty;
        }
        /// <summary>
        /// UPDATE文用のWHERE句文字列を戻す。
        /// </summary>
        /// <param name="appendDelimiter">区切り文字の付加</param>
        /// <returns></returns>
        /// <remarks>
        /// 共通項目の前の区切り文字（ AND ）が必要な場合、引数（appendDelimiter）は設定不要です。
        /// 共通項目の前に区切り文字（ AND ）が不要な場合、引数（appendDelimiter）にfalseを設定してください。
        /// </remarks>
        protected string GetCommonFieldForUpdateCondition(bool appendDelimiter = true)
        {
            return this.GetCommonFieldForUpdateCondition(String.Empty, appendDelimiter);
        }
        /// <summary>
        /// UPDATE文用のWHERE句文字列を戻す。
        /// </summary>
        /// <param name="tableAlias">テーブル別名</param>
        /// <param name="appendDelimiter">区切り文字の付加</param>
        /// <returns></returns>
        /// <remarks>
        /// 共通項目の前の区切り文字（ AND ）が必要な場合、引数（appendDelimiter）は設定不要です。
        /// 共通項目の前に区切り文字（ AND ）が不要な場合、引数（appendDelimiter）にfalseを設定してください。
        /// </remarks>
        protected virtual string GetCommonFieldForUpdateCondition(string tableAlias, bool appendDelimiter = true)
        {
            return String.Empty;
        }

        #endregion

    }
}
