using System;
using System.Collections.Generic;
using System.Data;
#if NET6_0_OR_GREATER
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif
using System.Linq;
using System.Reflection;
using System.Text;
using OrmTxcSql.Attributes;
using OrmTxcSql.Daos;
using OrmTxcSql.Data;
using OrmTxcSql.SqlClient.Data;
using OrmTxcSql.SqlClient.Entities;
using OrmTxcSql.Utils;

namespace OrmTxcSql.SqlClient.Daos
{
    /// <summary>
    /// SQL Server用のdao。BaseEntityのサブクラスに対してInsert, UpdateByPk, FindByPkを実装済み。
    /// </summary>
    public abstract class SqlDao<TEntity> : AbstractDao<TEntity, SqlCommand, SqlDataAdapter>
        where TEntity : SqlEntity, new()
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="entity"></param>
        /// <param name="enableOptimisticConcurrency"></param>
        /// <returns></returns>
        protected override int ExecuteNonQuery(SqlCommand command, TEntity entity, bool enableOptimisticConcurrency = true)
            => SqlServer.ExecuteNonQuery(command, enableOptimisticConcurrency);

        /// <summary>
        /// 新規登録する。（１件）
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns></returns>
        public override int Insert(TEntity entity)
        {
            //
            // コマンドの準備に必要なオブジェクトを生成する。
            SqlCommand command = this.Command;
            //
            // コマンドを準備する。
            this.BuildInsertCommand(command, entity);
            //
            // コマンドを実行する。
            int result = this.ExecuteNonQuery(command, entity);
            //
            // 結果を戻す。
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="entity"></param>
        protected virtual void BuildInsertCommand(SqlCommand command, TEntity entity)
        {
            // コマンドの準備に必要なオブジェクトを生成する。
            var columnStringBuilder = new StringBuilder();
            var valueStringBuilder = new StringBuilder();
            //
            // 対象項目を反復処理し、項目名とSQLパラメータを設定する。
            IEnumerator<KeyValuePair<string, PropertyInfo>> pairs = entity.GetColumnAttributes()
                // UID属性なし
                .Where(prop => null == prop.GetCustomAttribute<UIDAttribute>(false))
                // 変換
                .Select(prop => new
                {
                    PropertyInfo = prop,
                    ColumnName = prop.GetCustomAttribute<ColumnAttribute>(false)?.ColumnName ?? String.Empty
                })
                // カラム名を取得できない場合、対象外とする。
                .Where(src => !String.IsNullOrEmpty(src.ColumnName))
                // ディクショナリ（カラム名→プロパティ）に変換する。
                .ToDictionary(src => src.ColumnName, src => src.PropertyInfo)
                // IEnumeratorを取得する。
                .GetEnumerator();
            if (pairs.MoveNext())
            {
                // １件目についての処理：
                {
                    KeyValuePair<string, PropertyInfo> pair = pairs.Current;
                    string columnName = pair.Key;
                    string parameterName = "@" + columnName;
                    // 項目名を追加する。
                    columnStringBuilder.Append(columnName);
                    // 項目値（SQLパラメータ）を追加する。
                    valueStringBuilder.Append(parameterName);
                    // SQLパラメータに登録値を設定する。
                    PropertyInfo propertyInfo = pair.Value;
                    SqlServer.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
                }
                // ２件目以降についての処理：
                while (pairs.MoveNext())
                {
                    KeyValuePair<string, PropertyInfo> pair = pairs.Current;
                    string columnName = pair.Key;
                    string parameterName = "@" + columnName;
                    // 項目名を追加する。
                    columnStringBuilder.Append(" , ").Append(columnName);
                    // 項目値（SQLパラメータ）を追加する。
                    valueStringBuilder.Append(" , ").Append(parameterName);
                    // SQLパラメータに登録値を設定する。
                    PropertyInfo propertyInfo = pair.Value;
                    SqlServer.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
                }
            }
            else
            {
                // fool-proof: 対象項目が存在しない場合、例外を投げる。
                throw new ArgumentException(ArgumentExceptionMessageForNoTargetPropertyIsGiven);
            }
            //
            // テーブル名を取得する。
            string tableName = entity.GetTableName();
            // コマンドテキストを生成する。
            var builder = new StringBuilder();
            builder.Append(" insert into ").Append(tableName).Append(" (");
            builder.Append(columnStringBuilder.ToString());
            builder.Append(this.GetCommonFieldForInsert(true));
            builder.Append(" ) values (");
            builder.Append(valueStringBuilder.ToString());
            builder.Append(this.GetCommonFieldForInsertValue(true));
            builder.Append(" )");
            //
            // コマンドテキストを設定する。
            command.CommandText = builder.ToString();
            // データソースにコマンドを準備する。
            command.Prepare();
        }

        /// <summary>
        /// 更新する。（１件）
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns></returns>
        public override int UpdateByPk(TEntity entity)
        {
            //
            // コマンドの準備に必要なオブジェクトを生成する。
            SqlCommand command = this.Command;
            //
            // コマンドを準備する。
            this.BuildUpdateByPkCommand(command, entity);
            //
            // コマンドを実行する。
            int result = this.ExecuteNonQuery(command, entity);
            //
            // 結果を戻す。
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="entity"></param>
        protected virtual void BuildUpdateByPkCommand(SqlCommand command, TEntity entity)
        {
            // 更新対象項目を取得する。（主キー属性なし、UID属性なしのカラムのみ）
            IEnumerable<PropertyInfo> properties = entity.GetColumnAttributes()
                .Where(prop => null == prop.GetCustomAttribute<PrimaryKeyAttribute>(false))
                .Where(prop => null == prop.GetCustomAttribute<UIDAttribute>(false));
            //
            // コマンドを準備する。
            this.BuildUpdateByPkCommand(command, entity, properties);
        }
        private void BuildUpdateByPkCommand(SqlCommand command, TEntity entity, IEnumerable<PropertyInfo> properties)
        {
            // コマンドの準備に必要なオブジェクトを生成する。
            var builder = new StringBuilder();
            //
            // 主キー項目を反復処理する。
            IEnumerator<KeyValuePair<string, PropertyInfo>> pairs = EntityUtils
                // 主キー属性を取得する。
                .GetPrimaryKeyAttributes<TEntity>()
                // 変換
                .Select(prop => new
                {
                    PropertyInfo = prop,
                    ColumnName = prop.GetCustomAttribute<ColumnAttribute>(false)?.ColumnName ?? String.Empty
                })
                // カラム名を取得できない場合、対象外とする。
                .Where(src => !String.IsNullOrEmpty(src.ColumnName))
                // ディクショナリ（カラム名→プロパティ）に変換する。
                .ToDictionary(src => src.ColumnName, src => src.PropertyInfo)
                // IEnumeratorを取得する。
                .GetEnumerator();
            if (pairs.MoveNext())
            {
                // １件目についての処理：
                {
                    KeyValuePair<string, PropertyInfo> pair = pairs.Current;
                    string columnName = pair.Key;
                    string parameterName = "@" + columnName + "_org";
                    // 項目名を追加する。
                    builder.Append(String.Format(" where x.{0} = {1}", columnName, parameterName));
                    // SQLパラメータに登録値を設定する。
                    PropertyInfo propertyInfo = pair.Value;
                    SqlServer.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
                }
                // ２件目以降についての処理：
                while (pairs.MoveNext())
                {
                    KeyValuePair<string, PropertyInfo> pair = pairs.Current;
                    string columnName = pair.Key;
                    string parameterName = "@" + columnName + "_org";
                    // 項目名を追加する。
                    builder.Append(String.Format(" and x.{0} = {1}", columnName, parameterName));
                    // SQLパラメータに登録値を設定する。
                    PropertyInfo propertyInfo = pair.Value;
                    SqlServer.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
                }
                // 共通項目についての処理：
                builder.Append(this.GetCommonFieldForUpdateCondition("x", true));
            }
            else
            {
                // fool-proof: 主キー属性ありのカラムが存在しない場合、例外を投げる。
                throw new MissingPrimaryKeyException(MissingPrimaryKeyExceptionMessage);
            }
            //
            this.BuildUpdateCommand(command, entity, properties, builder.ToString());
        }
        private void BuildUpdateCommand(SqlCommand command, TEntity entity, IEnumerable<PropertyInfo> properties, string filter)
        {
            // コマンドの準備に必要なオブジェクトを生成する。
            var columnStringBuilder = new StringBuilder();
            //
            // 対象項目を反復処理し、更新列とSQLパラメータを設定する。
            IEnumerator<KeyValuePair<string, PropertyInfo>> pairs = properties
                // 変換
                .Select(prop => new
                {
                    PropertyInfo = prop,
                    ColumnName = prop.GetCustomAttribute<ColumnAttribute>(false)?.ColumnName ?? String.Empty
                })
                // カラム名を取得できない場合、対象外とする。
                .Where(src => !String.IsNullOrEmpty(src.ColumnName))
                // ディクショナリ（カラム名→プロパティ）に変換する。
                .ToDictionary(src => src.ColumnName, src => src.PropertyInfo)
                // IEnumeratorを取得する。
                .GetEnumerator();
            if (pairs.MoveNext())
            {
                // １件目についての処理：
                {
                    KeyValuePair<string, PropertyInfo> pair = pairs.Current;
                    string columnName = pair.Key;
                    string parameterName = "@" + columnName;
                    // 項目名を追加する。
                    columnStringBuilder.Append(String.Format(" {0} = {1}", columnName, parameterName));
                    // SQLパラメータに登録値を設定する。
                    PropertyInfo propertyInfo = pair.Value;
                    SqlServer.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
                }
                // ２件目以降についての処理：
                while (pairs.MoveNext())
                {
                    KeyValuePair<string, PropertyInfo> pair = pairs.Current;
                    string columnName = pair.Key;
                    string parameterName = "@" + columnName;
                    // 項目名を追加する。
                    columnStringBuilder.Append(String.Format(" , {0} = {1}", columnName, parameterName));
                    // SQLパラメータに登録値を設定する。
                    PropertyInfo propertyInfo = pair.Value;
                    SqlServer.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
                }
                // 共通項目についての処理：
                columnStringBuilder.Append(this.GetCommonFieldForUpdate(true));
            }
            else
            {
                // fool-proof: 更新対象項目が存在しない場合、例外を投げる。
                throw new ArgumentException(ArgumentExceptionMessageForNoTargetPropertyIsGiven);
            }
            //
            // テーブル名を取得する。
            string tableName = entity.GetTableName();
            // コマンドテキストを生成する。
            // 開発者向けコメント：（fool-proof：条件の前に、空白を１つはさむ）
            // OK: update table_name set a = @a, b = @b, c = @c where x = @x
            // NG: update table_name set a = @a, b = @b, c = @cwhere x = @x
            var builder = new StringBuilder();
            builder.Append(" update ").Append(tableName);
            builder.Append(" set ");
            builder.Append(columnStringBuilder.ToString());
            builder.Append(" from ").Append(tableName).Append(" as x");
            builder.Append(" "); // fool-proof
            builder.Append(filter);
            //
            // コマンドテキストを設定する。
            command.CommandText = builder.ToString();
            // データソースにコマンドを準備する。
            command.Prepare();
        }

        /// <summary>
        /// 更新する。（１件）（非null項目のみ）
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns></returns>
        public override int UpdateUnlessNullByPk(TEntity entity)
        {
            //
            // コマンドの準備に必要なオブジェクトを生成する。
            SqlCommand command = this.Command;
            //
            // コマンドを準備する。
            this.BuildUpdateUnlessNullByPk(command, entity);
            //
            // コマンドを実行する。
            int result = this.ExecuteNonQuery(command, entity);
            //
            // 結果を戻す。
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="entity"></param>
        protected virtual void BuildUpdateUnlessNullByPk(SqlCommand command, TEntity entity)
        {
            // 更新対象項目：主キー属性なし、UID属性なし、非null項目のカラムのみ
            IEnumerable<PropertyInfo> properties = entity.GetColumnAttributes()
                // 主キー属性なし
                .Where(prop => null == prop.GetCustomAttribute<PrimaryKeyAttribute>(false))
                // UID属性なし
                .Where(prop => null == prop.GetCustomAttribute<UIDAttribute>(false))
                // 非null項目
                .Where(prop => null != prop.GetValue(entity));
            //
            // コマンドを準備する。
            this.BuildUpdateByPkCommand(command, entity, properties);
        }

        /// <summary>
        /// 検索する。（１件）
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
#if NET6_0_OR_GREATER
        public override TEntity? SelectByPk(TEntity entity)
#else
        public override TEntity SelectByPk(TEntity entity)
#endif
        {
            //
            // コマンドの準備に必要なオブジェクトを生成する。
            SqlCommand command = this.Command;
            //
            // コマンドを準備する。
            this.BuildSelectByPkCommand(command, entity);
            //
            // コマンドを実行する。
            DataTable dt = this.GetDataTable(command);
            //
            // 結果を戻す。
            switch (dt.Rows.Count)
            {
                case 0:
                    {
                        // 検索結果が０件の場合、nullを戻す。
                        return null;
                    }
                case 1:
                    {
                        // 検索結果が１件の場合、オブジェクトに変換する。
                        TEntity result = dt.AsEnumerable<TEntity>().Single();
                        // 結果を戻す。
                        return result;
                    }
                default:
                    {
                        // fool-proof
                        // 検索結果が２件以上の場合、例外を投げる。
                        var exception = new TooManyRowsException();
                        throw exception;
                    }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="entity"></param>
        protected virtual void BuildSelectByPkCommand(SqlCommand command, TEntity entity)
        {
            // コマンドの準備に必要なオブジェクトを生成する。
            var builder = new StringBuilder();
            //
            // 主キー項目を反復処理する。
            IEnumerator<KeyValuePair<string, PropertyInfo>> pairs = EntityUtils
                // 主キー属性を取得する。
                .GetPrimaryKeyAttributes<TEntity>()
                // 変換
                .Select(prop => new
                {
                    PropertyInfo = prop,
                    ColumnName = prop.GetCustomAttribute<ColumnAttribute>(false)?.ColumnName ?? String.Empty
                })
                // カラム名を取得できない場合、対象外とする。
                .Where(src => !String.IsNullOrEmpty(src.ColumnName))
                // ディクショナリ（カラム名→プロパティ）に変換する。
                .ToDictionary(src => src.ColumnName, src => src.PropertyInfo)
                // IEnumeratorを取得する。
                .GetEnumerator();
            if (pairs.MoveNext())
            {
                // １件目についての処理：
                {
                    KeyValuePair<string, PropertyInfo> pair = pairs.Current;
                    string columnName = pair.Key;
                    string parameterName = "@" + columnName;
                    // 項目名を追加する。
                    builder.Append(String.Format(" where x.{0} = {1}", columnName, parameterName));
                    // SQLパラメータに登録値を設定する。
                    PropertyInfo propertyInfo = pair.Value;
                    SqlServer.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
                }
                // ２件目以降についての処理：
                while (pairs.MoveNext())
                {
                    KeyValuePair<string, PropertyInfo> pair = pairs.Current;
                    string columnName = pair.Key;
                    string parameterName = "@" + columnName;
                    // 項目名を追加する。
                    builder.Append(String.Format(" and x.{0} = {1}", columnName, parameterName));
                    // SQLパラメータに登録値を設定する。
                    PropertyInfo propertyInfo = pair.Value;
                    SqlServer.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
                }
                // 共通項目についての処理：
                builder.Append(this.GetCommonFieldForSelectCondition("x", true));
            }
            else
            {
                // fool-proof: 主キー属性ありのカラムが存在しない場合、例外を投げる。
                throw new MissingPrimaryKeyException(MissingPrimaryKeyExceptionMessage);
            }
            //
            this.BuildSelectCommand(command, entity, builder.ToString());
        }
        private void BuildSelectCommand(SqlCommand command, TEntity entity, string filter)
        {
            // テーブル名を取得する。
            string tableName = entity.GetTableName();
            // カラム名を取得する。
            string[] columnNames = entity.GetColumnAttributes()
                // 変換
                .Select(prop => new
                {
                    PropertyInfo = prop,
                    ColumnName = prop.GetCustomAttribute<ColumnAttribute>(false)?.ColumnName ?? String.Empty
                })
                // カラム名を取得できない場合、対象外とする。
                .Where(src => !String.IsNullOrEmpty(src.ColumnName))
                // カラム名を取得する。
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
            builder.Append(" "); // fool-proof
            builder.Append(filter);
            //
            // コマンドテキストを設定する。
            command.CommandText = builder.ToString();
            // データソースにコマンドを準備する。
            command.Prepare();
        }
    }
}
