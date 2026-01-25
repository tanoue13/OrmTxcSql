using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Linq;
using System.Threading.Tasks;
using OrmTxcSql.Daos;
using OrmTxcSql.Data;
using OrmTxcSql.Utils;

namespace OrmTxcSql.Odbc.Data
{
    /// <summary>
    /// DBMSとの接続、トランザクション管理を行うクラス。
    /// </summary>
    public class OdbcServer : DbServer<OdbcConnection>
    {
        /// <summary>
        /// ［拡張］接続のオープン、クローズのみ管理する。
        /// </summary>
        public void Connect(Action<OdbcConnection> action)
        {
            using (var connection = new OdbcConnection())
            {
                connection.ConnectionString = this.DataSource.GetConnectionString();
                connection.Open();
                //
                // メイン処理
                try
                {
                    // メイン処理を実行する。
                    action(connection);
                    //
                }
                catch (OdbcException)
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
        private void CloseConnection(OdbcConnection connection)
        {
            // 接続を閉じる。
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        /// <summary>
        /// トランザクション管理下において、<paramref name="action"/>を実行する。（非同期）
        /// </summary>
        /// <param name="daos">トランザクションに参加する<see cref="IDao"/>のコレクション</param>
        /// <param name="action">トランザクション管理下で実行される処理</param>
        /// <returns></returns>
        [Obsolete("プロバイダーによる実装かどうか未確認のため、通常の Execute(IEnumerable<IDao> daos, Action<IDbTransaction> action) を使用ください。")]
        public override async ValueTask ExecuteAsync(IEnumerable<IDao> daos, Action<IDbTransaction> action)
        {
            //throw new NotImplementedException();
            using (var connection = new OdbcConnection())
            {
                connection.ConnectionString = this.DataSource.GetConnectionString();
                //
                LogUtils.GetDataLogger().Debug("Connection is being opened.");
                await connection.OpenAsync();
                LogUtils.GetDataLogger().Debug("Connection has been opened.");
                //
                LogUtils.GetDataLogger().Debug("Transaction is starting.");
                using (OdbcTransaction tx = connection.BeginTransaction())
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
#if NET6_0_OR_GREATER
                        await this.CommitAsync(tx);
#else
                        this.Commit(tx);
#endif
                    }
                    catch (DbException ex)
                    {
                        LogUtils.GetDataLogger().Error(ex);
                        LogUtils.GetErrorLogger().Error(ex);
                        // トランザクションをロールバックする。
#if NET6_0_OR_GREATER
                        await this.RollbackAsync(tx);
#else
                        this.Rollback(tx);
#endif
                        //
                        // 例外を投げる。（丸投げ）
                        throw;
                    }
                    catch (Exception ex)
                    {
                        LogUtils.GetDataLogger().Error(ex);
                        LogUtils.GetErrorLogger().Error(ex);
                        // トランザクションをロールバックする。
#if NET6_0_OR_GREATER
                        await this.RollbackAsync(tx);
#else
                        this.Rollback(tx);
#endif
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
        private void Commit(OdbcTransaction tx)
        {
            try
            {
                // トランザクションをコミットする。
                tx.Commit();
            }
            catch (InvalidOperationException ex)
            {
                // トランザクションは、既にコミットまたはロールバックされています。
                // または、接続が切断されています。
                LogUtils.GetErrorLogger().Error(ex);
            }
            catch (Exception ex)
            {
                // トランザクションのコミット中にエラーが発生しました。
                LogUtils.GetErrorLogger().Error(ex);
            }
        }
        /// <summary>
        /// トランザクションをロールバックする。
        /// </summary>
        /// <param name="tx"></param>
        private void Rollback(OdbcTransaction tx)
        {
            try
            {
                // トランザクションをロールバックする。
                tx.Rollback();
            }
            catch (InvalidOperationException ex)
            {
                // トランザクションは、既にコミットまたはロールバックされています。
                // または、接続が切断されています。
                LogUtils.GetErrorLogger().Error(ex);
            }
            catch (Exception ex)
            {
                // トランザクションのロールバック中にエラーが発生しました。
                LogUtils.GetErrorLogger().Error(ex);
            }
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// トランザクションをコミットする。（非同期）
        /// </summary>
        /// <param name="tx"></param>
        private async ValueTask CommitAsync(OdbcTransaction tx)
        {
            try
            {
                // トランザクションをコミットする。
                await tx.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                // トランザクションは、既にコミットまたはロールバックされています。
                // または、接続が切断されています。
                LogUtils.GetErrorLogger().Error(ex);
            }
            catch (Exception ex)
            {
                // トランザクションのコミット中にエラーが発生しました。
                LogUtils.GetErrorLogger().Error(ex);
            }
        }
        /// <summary>
        /// トランザクションをロールバックする。（非同期）
        /// </summary>
        /// <param name="tx"></param>
        private async ValueTask RollbackAsync(OdbcTransaction tx)
        {
            try
            {
                // トランザクションをロールバックする。
                await tx.RollbackAsync();
            }
            catch (InvalidOperationException ex)
            {
                // トランザクションは、既にコミットまたはロールバックされています。
                // または、接続が切断されています。
                LogUtils.GetErrorLogger().Error(ex);
            }
            catch (Exception ex)
            {
                // トランザクションのロールバック中にエラーが発生しました。
                LogUtils.GetErrorLogger().Error(ex);
            }
        }
        /// <summary>
        /// 接続を閉じる。（非同期）
        /// </summary>
        /// <param name="connection"></param>
        private async ValueTask CloseConnectionAsync(OdbcConnection connection)
        {
            // 接続を閉じる。
            //LogUtils.GetDataLogger().Debug($"Connection is being closed. [ConnectionState: {Enum.GetName(typeof(ConnectionState), connection.State)}]");
            await connection.CloseAsync();
            //LogUtils.GetDataLogger().Debug($"Connection has been closed.");
        }
#endif

    }
}
