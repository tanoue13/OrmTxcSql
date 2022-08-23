using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
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
        /// 検索する。（１件）
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public abstract TEntity SelectByPk(TEntity entity);

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
            DataTable dt = new DataTable();
            using (var adapter = new TDbDataAdapter())
            {
                adapter.SelectCommand = command;
                adapter.Fill(dt);
            }
            //
            // 末尾をトリムする場合、トリムする。
            if (this.TrimEnd)
            {
                // 文字列型のDataColumnを対象とする。（トリム）
                IEnumerable<int> ordinals = dt.Columns.Cast<DataColumn>()
                    .Where(dataColumn => typeof(string).Equals(dataColumn.DataType))
                    .Select(x => x.Ordinal);
                foreach (int ordinal in ordinals)
                {
                    // 値がnullでないDataRowのみを対象とする。（トリム）
                    IEnumerable<DataRow> dataRows = dt.Rows.Cast<DataRow>()
                        .Where(dataRow => !dataRow.IsNull(ordinal));
                    foreach (DataRow dataRow in dataRows)
                    {
                        // トリムした値を設定する。
                        string value = dataRow[ordinal] as string;
                        dataRow[ordinal] = value.TrimEnd();
                    }
                }
                // 開発者向けコメント（2021.08.17田上）
                // DataRowに対する書き込み操作は、同期する必要があるとのこと。
                // したがって、非同期処理（Parallel.ForEach）を使用すると例外が投げられる。
            }
            // 内部処理での変更内容をコミットする。
            dt.AcceptChanges();
            //
            // 結果を戻す。
            return dt;
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
            TEntity result = this.SelectByPk(entity);
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
