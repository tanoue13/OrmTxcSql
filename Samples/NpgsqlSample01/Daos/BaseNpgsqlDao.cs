using System;
using System.Data;
using System.Linq;
using System.Text;
using Npgsql;
using NpgsqlSample01.Entities;
using NpgsqlTypes;
using OrmTxcSql.Npgsql.Daos;
using OrmTxcSql.Npgsql.Data;

namespace NpgsqlSample01.Daos
{
    /// <summary>
    /// Daoの基底クラス。<br/>
    /// <see cref="BaseNpgsqlEntity"/>のサブクラスに対してInsert, UpdateByPk, SelectByPkにおける共通項目の扱いを実装する。<br/>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <remarks>
    /// 同時実行排他制御による更新時の異常等はNpgsqlServer#ExecuteNonQuery()で処理し、
    /// このクラスでは例外処理等は実施しない。
    /// </remarks>
    public abstract class BaseNpgsqlDao<TEntity> : NpgsqlDao<TEntity> where TEntity : BaseNpgsqlEntity, new()
    {

        /// <summary>
        /// コンストラクタ.
        /// </summary>
        public BaseNpgsqlDao() : base()
        {
            // コマンドにSQL変数の初期値を設定する。
            this.SetDefaultParameterValues();
        }

        #region"BaseEntity（共通項目）に関するプロパティや処理"

        /// <summary>
        /// コマンドにSQL変数の初期値を設定する。
        /// </summary>
        private void SetDefaultParameterValues()
        {
            IDbCommand command = this.Command;
            //
            // （共通項目）バージョンNo.（INSERT文用のデフォルト値）
            NpgsqlServer.AddParameterIfNotExists(command, "@common_field_version_no_default_value", NpgsqlDbType.Numeric, BaseNpgsqlEntity.InitialVersionNo);
            // （共通項目）バージョンNo.（UPDATE文条件用）
            NpgsqlServer.AddParameterIfNotExists(command, "@common_field_version_no", NpgsqlDbType.Numeric, DBNull.Value);
            // （共通項目）アクティブフラグ（INSERT文用のデフォルト値）
            NpgsqlServer.AddParameterIfNotExists(command, "@common_field_active_flag_default_value", NpgsqlDbType.Varchar, BaseNpgsqlEntity.ActiveFlagIsActive);
            // （共通項目）アクティブフラグ（SELECT文検索条件用）
            NpgsqlServer.AddParameterIfNotExists(command, "@common_field_active_flag_is_active", NpgsqlDbType.Varchar, BaseNpgsqlEntity.ActiveFlagIsActive);
            // （共通項目）登録日時、更新日時
            NpgsqlServer.AddParameterIfNotExists(command, "@common_field_nitiji", NpgsqlDbType.TimestampTz, DBNull.Value);
            // （共通項目）登録ユーザ名、更新ユーザ名
            NpgsqlServer.AddParameterIfNotExists(command, "@common_field_username", NpgsqlDbType.Varchar, Environment.UserName);
        }

        protected override string GetCommonFieldForSelect(string tableAlias, bool appendDelimiter = true)
        {
            // 項目を宣言する。
            string[] columnNames = new string[] {
                "version_no",
                "active_flag",
                "toroku_nitiji", "toroku_username",
                 "kosin_nitiji",  "kosin_username",
            };
            //
            var builder = new StringBuilder();
            if (appendDelimiter)
            {
                // 区切り文字を付加する。
                builder.Append(" ,");
            }
            builder.Append(" ");
            if (!String.IsNullOrWhiteSpace(tableAlias))
            {
                // テーブル別名を付加したカラム名を追加する。
                builder.Append(String.Join(",", columnNames.Select(columnName => String.Format(" {0}.{1}", tableAlias, columnName))));
            }
            else
            {
                // カラム名を追加する。
                builder.Append(String.Join(",", columnNames.Select(columnName => String.Format(" {0}", columnName))));
            }
            // 結果を戻す。
            return builder.ToString();
        }
        protected override string GetCommonFieldForSelectCondition(string tableAlias, bool appendDelimiter = true)
        {
            var builder = new StringBuilder();
            // 区切り文字を付加する。
            if (appendDelimiter)
            {
                builder.Append(" and");
            }
            builder.Append(" ");
            // テーブル別名を追加する。
            if (!String.IsNullOrWhiteSpace(tableAlias))
            {
                builder.Append(tableAlias).Append(".");
            }
            builder.Append("active_flag = @common_field_active_flag_is_active");
            // 結果を戻す。
            return builder.ToString();
        }

        protected override string GetCommonFieldForInsert(bool appendDelimiter = true)
        {
            var builder = new StringBuilder();
            if (appendDelimiter)
            {
                builder.Append(" , ");
            }
            builder.Append(" version_no");
            builder.Append(" , active_flag");
            builder.Append(" , toroku_nitiji");
            builder.Append(" , toroku_username");
            builder.Append(" , kosin_nitiji");
            builder.Append(" , kosin_username");
            // 結果を戻す。
            return builder.ToString();
        }
        protected override string GetCommonFieldForInsertValue(bool appendDelimiter = true)
        {
            var builder = new StringBuilder();
            if (appendDelimiter)
            {
                builder.Append(" , ");
            }
            builder.Append(" coalesce(@common_field_version_no, @common_field_version_no_default_value)");
            builder.Append(" , @common_field_active_flag_default_value");
            builder.Append(" , coalesce(@common_field_nitiji, current_timestamp)");
            builder.Append(" , @common_field_username");
            builder.Append(" , coalesce(@common_field_nitiji, current_timestamp)");
            builder.Append(" , @common_field_username");
            // 結果を戻す。
            return builder.ToString();
        }

        protected override string GetCommonFieldForUpdate(bool appendDelimiter = true)
        {
            var builder = new StringBuilder();
            if (appendDelimiter)
            {
                builder.Append(" , ");
            }
            builder.Append(" version_no = version_no + 1");
            builder.Append(" , kosin_nitiji = coalesce(@common_field_nitiji, current_timestamp)");
            builder.Append(" , kosin_username = @common_field_username");
            // 結果を戻す。
            return builder.ToString();
        }
        protected override string GetCommonFieldForUpdateCondition(string tableAlias, bool appendDelimiter = true)
        {
            var builder = new StringBuilder();
            if (appendDelimiter)
            {
                builder.Append(" and");
            }
            builder.Append(" ");
            // テーブル別名を追加する。
            if (!String.IsNullOrWhiteSpace(tableAlias))
            {
                builder.Append(tableAlias).Append(".");
            }
            builder.Append("version_no = @common_field_version_no");
            // 結果を戻す。
            return builder.ToString();
        }

        #endregion

        protected override int ExecuteNonQuery(NpgsqlCommand command, TEntity entity, bool enableOptimisticConcurrency = true)
        {
            // 共通項目（バージョンNo.）のパラメータを設定する。
            NpgsqlServer.AddParameterOrReplace(command, "@common_field_version_no", NpgsqlDbType.Numeric, entity.VersionNo);
            //
            // コマンドを実行する。
            int result = NpgsqlServer.ExecuteNonQuery(command, enableOptimisticConcurrency);
            //
            // 結果を戻す。
            return result;
        }

    }
}
