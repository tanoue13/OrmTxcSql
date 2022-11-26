using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Npgsql;
using NpgsqlTypes;
using OrmTxcSql.Daos;
using OrmTxcSql.Data;

namespace OrmTxcSql.Npgsql.Data
{
    /// <summary>
    /// DBMS（PostgreSQL）との接続、トランザクション管理を行うクラス。
    /// </summary>
    public class NpgsqlServer : DbServer<NpgsqlConnection>
    {
        /// <summary>
        /// トランザクション管理下において、<paramref name="action"/>を実行する。
        /// </summary>
        /// <param name="daos">トランザクションに参加する<see cref="IDao"/>のコレクション</param>
        /// <param name="action">トランザクション管理下で実行される処理</param>
        public override void Execute(IEnumerable<IDao> daos, Action<IDbTransaction> action)
        {
            using (var connection = new NpgsqlConnection())
            {
                connection.ConnectionString = this.DataSource.GetConnectionString();
                connection.UserCertificateValidationCallback = this.DataSource.GetRemoteCertificateValidationCallback();
                //
                //LogUtils.GetDataLogger().Debug("Connection is being opened.");
                connection.Open();
                //LogUtils.GetDataLogger().Debug("Connection has been opened.");
                //
                //LogUtils.GetDataLogger().Debug("Transaction is starting.");
                using (var tx = connection.BeginTransaction())
                {
                    //LogUtils.GetDataLogger().Debug("Transaction has started.");
                    //
                    // 前処理：コマンドに接続とトランザクションを設定する。
                    foreach (IDao dao in daos)
                    {
                        IEnumerable<IDbCommand> commands = dao.Commands ?? Enumerable.Empty<IDbCommand>();
                        foreach (IDbCommand command in commands)
                        {
                            // 接続を設定する。
                            command.Connection = connection;
                            // トランザクションを設定する。
                            command.Transaction = tx;
                        }
                    }
                    //
                    // メイン処理：実装クラスのexecute()を実行する。
                    try
                    {
                        // メイン処理を実行する。
                        action(tx);
                        //
                        // トランザクションをコミットする。
                        this.Commit(tx);
                    }
                    catch (DbException ex)
                    {
                        //LogUtils.GetDataLogger().Error(ex);
                        //LogUtils.GetErrorLogger().Error(ex);
                        // トランザクションをロールバックする。
                        this.Rollback(tx);
                        //
                        // 例外を投げる。（丸投げ）
                        throw;
                    }
                    catch (Exception ex)
                    {
                        //LogUtils.GetDataLogger().Error(ex);
                        //LogUtils.GetErrorLogger().Error(ex);
                        // トランザクションをロールバックする。
                        this.Rollback(tx);
                        //
                        // 例外を投げる。（丸投げ）
                        throw;
                    }
                }
                //
                // 接続を閉じる。
                this.CloseConnection(connection);
            }
        }
        /// <summary>
        /// トランザクションをコミットする。
        /// </summary>
        /// <param name="tx"></param>
        private void Commit(NpgsqlTransaction tx)
        {
            try
            {
                // トランザクションをコミットする。
#if NET6_0_OR_GREATER
                // The NpgsqlTransaction.IsCompleted property has been removed.
                // cf. https://www.npgsql.org/doc/release-notes/5.0.html
                //LogUtils.GetDataLogger().Debug("Transaction is being committed.");
                tx.Commit();
                //LogUtils.GetDataLogger().Debug("Transaction has been committed.");
#else
                if (!tx.IsCompleted)
                {
                    //LogUtils.GetDataLogger().Debug("Transaction is being committed.");
                    tx.Commit();
                    //LogUtils.GetDataLogger().Debug("Transaction has been committed.");
                }
                else
                {
                    //LogUtils.GetDataLogger().Debug("Commit statement is skipped because the transaction has been completed.");
                }
#endif
            }
            catch (InvalidOperationException ex)
            {
                // トランザクションは、既にコミットまたはロールバックされています。
                // または、接続が切断されています。
                //LogUtils.GetDataLogger().Error(ex);
                //LogUtils.GetErrorLogger().Error(ex);
            }
            catch (Exception ex)
            {
                // トランザクションのコミット中にエラーが発生しました。
                //LogUtils.GetDataLogger().Error(ex);
                //LogUtils.GetErrorLogger().Error(ex);
            }
        }
        /// <summary>
        /// トランザクションをロールバックする。
        /// </summary>
        /// <param name="tx"></param>
        private void Rollback(NpgsqlTransaction tx)
        {
            try
            {
                // トランザクションをロールバックする。
#if NET6_0_OR_GREATER
                // The NpgsqlTransaction.IsCompleted property has been removed.
                // cf. https://www.npgsql.org/doc/release-notes/5.0.html
                //LogUtils.GetDataLogger().Debug("Transaction is being rollbacked.");
                tx.Rollback();
                //LogUtils.GetDataLogger().Debug("Transaction has been rollbacked.");
#else
                if (!tx.IsCompleted)
                {
                    //LogUtils.GetDataLogger().Debug("Transaction is being rollbacked.");
                    tx.Rollback();
                    //LogUtils.GetDataLogger().Debug("Transaction has been rollbacked.");
                }
                else
                {
                    //LogUtils.GetDataLogger().Debug("Rollback statement is skipped because the transaction has been completed.");
                }
#endif
            }
            catch (InvalidOperationException ex)
            {
                // トランザクションは、既にコミットまたはロールバックされています。
                // または、接続が切断されています。
                //LogUtils.GetDataLogger().Error(ex);
                //LogUtils.GetErrorLogger().Error(ex);
            }
            catch (Exception ex)
            {
                // トランザクションのロールバック中にエラーが発生しました。
                //LogUtils.GetDataLogger().Error(ex);
                //LogUtils.GetErrorLogger().Error(ex);
            }
        }

        /// <summary>
        /// ［拡張］接続のオープン、クローズのみ管理する。
        /// </summary>
        public void Connect(Action<NpgsqlConnection> action)
        {
            using (var connection = new NpgsqlConnection())
            {
                connection.ConnectionString = this.DataSource.GetConnectionString();
                connection.UserCertificateValidationCallback = this.DataSource.GetRemoteCertificateValidationCallback();
                connection.Open();
                //
                // メイン処理
                try
                {
                    // メイン処理を実行する。
                    action(connection);
                    //
                }
                catch (NpgsqlException)
                {
                    // 例外を投げる。（丸投げ）
                    throw;
                }
                catch (Exception)
                {
                    // 例外を投げる。（丸投げ）
                    throw;
                }
                // 接続を閉じる。
                this.CloseConnection(connection);
            }
        }
        /// <summary>
        /// 接続を閉じる。
        /// </summary>
        /// <param name="connection"></param>
        private void CloseConnection(NpgsqlConnection connection)
        {
            // 接続を閉じる。
            //LogUtils.GetDataLogger().Debug($"Connection is being closed. [ConnectionState: {Enum.GetName(typeof(ConnectionState), connection.State)}]");
            connection.Close();
            //LogUtils.GetDataLogger().Debug($"Connection has been closed.");
        }

        private static IParameterValueConverter ParameterValueConverter { get; set; } = new NpgsqlParameterValueConverter();

        /// <summary>
        /// コマンドにパラメータを追加する。パラメータが存在する場合、置き換える。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterName"></param>
        /// <param name="obj"></param>
        /// <param name="property"></param>
        /// <remarks></remarks>
        public static void AddParameterOrReplace(IDbCommand command, string parameterName, object obj, PropertyInfo property)
        {
            // パラメータのデータ型を取得する。
            Type propertyType = property.PropertyType;
            NpgsqlDbType dbType = NpgsqlDbType.Varchar; //default
            if (new Type[] { typeof(decimal), typeof(decimal?) }.Contains(propertyType))
            {
                dbType = NpgsqlDbType.Numeric;
            }
            else if (new Type[] { typeof(int), typeof(int?) }.Contains(propertyType))
            {
                dbType = NpgsqlDbType.Numeric;
            }
            else if (new Type[] { typeof(DateTime), typeof(DateTime?) }.Contains(propertyType))
            {
                dbType = NpgsqlDbType.TimestampTz;
            }
            else
            {
                // fool-proof
                dbType = NpgsqlDbType.Varchar;
            }
            // パラメータに設定する値を取得する。
#if NET6_0_OR_GREATER
            object? value = property.GetValue(obj);
#else
            object value = property.GetValue(obj);
#endif
            //
            // nullを考慮し、下のメソッド経由で設定する。
            NpgsqlServer.AddParameterOrReplace(command, parameterName, dbType, value);
        }

        /// <summary>
        /// コマンドにパラメータを追加する。パラメータが存在する場合、置き換える。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="value"></param>
        /// <remarks>値がnullの場合、DBNull.Valueに変換して設定する。</remarks>
#if NET6_0_OR_GREATER
        public static void AddParameterOrReplace(IDbCommand command, string parameterName, NpgsqlDbType dbType, [AllowNull] object value)
#else
        public static void AddParameterOrReplace(IDbCommand command, string parameterName, NpgsqlDbType dbType, object value)
#endif
        {
            IDataParameter parameter = new NpgsqlParameter(parameterName, dbType);
            parameter.Value = NpgsqlServer.ParameterValueConverter.Convert(value, null, null);
            DbServer<NpgsqlConnection>.AddParameterOrReplace(command, parameter);
        }
        /// <summary>
        /// コマンドにパラメータを追加する。パラメータが存在する場合、置き換えない。
        /// </summary>
        /// <param name="command">command</param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="value"></param>
        /// <remarks></remarks>
        public static void AddParameterIfNotExists(IDbCommand command, string parameterName, NpgsqlDbType dbType, object value)
        {
            IDataParameter parameter = new NpgsqlParameter(parameterName, dbType);
            parameter.Value = NpgsqlServer.ParameterValueConverter.Convert(value, null, null);
            DbServer<NpgsqlConnection>.AddParameterIfNotExists(command, parameter);
        }

        #region "更新系処理に関する処理"
        /// <summary>
        /// <see cref="DbServer{TConnection}.ExecuteNonQuery(IDbCommand, bool)"/>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="enableOptimisticConcurrency"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(NpgsqlCommand command, bool enableOptimisticConcurrency = true)
        {
            try
            {
                int count = DbServer<NpgsqlConnection>.ExecuteNonQuery(command, enableOptimisticConcurrency);
                return count;
            }
            catch (NpgsqlException ex)
            {
                switch (ex.ErrorCode)
                {
                    case 23505:
                        {
                            // 一意性違反 unique_violation
                            string message = DbServer<NpgsqlConnection>.GetDBConcurrencyExceptionMessage(command);
                            // 例外を投げる。
                            var exception = new DBConcurrencyException(message);
                            throw exception;
                        }
                    default:
                        {
                            // 例外を投げる。（丸投げ）
                            throw;
                        }
                }

            }
        }
        #endregion
    }
}
