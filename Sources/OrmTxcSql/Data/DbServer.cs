﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrmTxcSql.Daos;
using OrmTxcSql.Utils;

namespace OrmTxcSql.Data
{
    /// <summary>
    /// DBMSとの接続、トランザクション管理を行う基底クラス。
    /// </summary>
    /// <typeparam name="TConnection"></typeparam>
    public abstract class DbServer<TConnection> where TConnection : IDbConnection, new()
    {

        /// <summary>
        /// データソースを取得または設定します。
        /// </summary>
        public virtual DataSource DataSource { protected get; set; } = new ConnectionStringDataSource();

        /// <summary>
        /// <see cref="ExecuteAsync(IEnumerable{IDao}, Action{IDbTransaction})"/>.
        /// </summary>
        /// <param name="dao"></param>
        /// <param name="action"></param>
        public void ExecuteAsync(IDao dao, Action action) => this.Execute(new IDao[] { dao }, action);
        /// <summary>
        /// <see cref="ExecuteAsync(IEnumerable{IDao}, Action{IDbTransaction})"/>
        /// </summary>
        /// <param name="daos"></param>
        /// <param name="action"></param>
        public void ExecuteAsync(IEnumerable<IDao> daos, Action action) => this.Execute(daos, tx => { action(); });
        /// <summary>
        /// <see cref="ExecuteAsync(IEnumerable{IDao}, Action{IDbTransaction})"/>
        /// </summary>
        /// <param name="dao"></param>
        /// <param name="action"></param>
        public void ExecuteAsync(IDao dao, Action<IDbTransaction> action) => this.Execute(new IDao[] { dao }, action);
        /// <summary>
        /// トランザクション管理下において、<paramref name="action"/>を実行する。（非同期）
        /// </summary>
        /// <param name="daos">トランザクションに参加する<see cref="IDao"/>のコレクション</param>
        /// <param name="action">トランザクション管理下で実行される処理</param>
        public abstract ValueTask ExecuteAsync(IEnumerable<IDao> daos, Action<IDbTransaction> action);

        /// <summary>
        /// <see cref="Execute(IEnumerable{IDao}, Action{IDbTransaction})"/>.
        /// </summary>
        /// <param name="dao"></param>
        /// <param name="action"></param>
        public void Execute(IDao dao, Action action) => this.Execute(new IDao[] { dao }, action);
        /// <summary>
        /// <see cref="Execute(IEnumerable{IDao}, Action{IDbTransaction})"/>
        /// </summary>
        /// <param name="daos"></param>
        /// <param name="action"></param>
        public void Execute(IEnumerable<IDao> daos, Action action) => this.Execute(daos, tx => { action(); });
        /// <summary>
        /// <see cref="Execute(IEnumerable{IDao}, Action{IDbTransaction})"/>
        /// </summary>
        /// <param name="dao"></param>
        /// <param name="action"></param>
        public void Execute(IDao dao, Action<IDbTransaction> action) => this.Execute(new IDao[] { dao }, action);

        /// <summary>
        /// トランザクション管理下において、<paramref name="action"/>を実行する。
        /// </summary>
        /// <param name="daos">トランザクションに参加する<see cref="IDao"/>のコレクション</param>
        /// <param name="action">トランザクション管理下で実行される処理</param>
        public virtual void Execute(IEnumerable<IDao> daos, Action<IDbTransaction> action)
        {
            using (var connection = new TConnection())
            {
                connection.ConnectionString = this.DataSource.GetConnectionString();
                connection.Open();
                using (var tx = connection.BeginTransaction())
                {
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
                        LogUtils.GetErrorLogger().Error(ex);
                        // トランザクションをロールバックする。
                        this.Rollback(tx);
                        //
                        // 例外を投げる。（丸投げ）
                        throw;
                    }
                    catch (Exception ex)
                    {
                        LogUtils.GetErrorLogger().Error(ex);
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
        private void Commit(IDbTransaction tx)
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
        private void Rollback(IDbTransaction tx)
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
        /// <summary>
        /// 接続を閉じる。
        /// </summary>
        /// <param name="connection"></param>
        /// <remarks>
        /// 参照：<see cref="IDbConnection.Close"/>
        /// </remarks>
        private void CloseConnection(IDbConnection connection)
        {
            // 接続を閉じる。
            // 開発者向けコメント：
            // ・アプリケーションでは、例外を生成せずに Close を複数回呼び出すことができる。
            connection.Close();
        }

        /// <summary>
        /// トランザクションを設定する。
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="dao"></param>
        public void SetTransaction(IDbTransaction tx, IDao dao)
        {
            IEnumerable<IDbCommand> commands = dao.Commands ?? Enumerable.Empty<IDbCommand>();
            foreach (IDbCommand command in commands)
            {
                command.Connection = tx.Connection;
                command.Transaction = tx;
            }
        }

        /// <summary>
        /// コマンドにパラメータを追加する。パラメータが存在する場合、置き換える。
        /// </summary>
        /// <param name="command">command</param>
        /// <param name="parameter">parameter</param>
        /// <remarks></remarks>
        protected static void AddParameterOrReplace(IDbCommand command, IDataParameter parameter)
        {
            DbServer<TConnection>.AddParameter(command, parameter, true);
        }
        /// <summary>
        /// コマンドにパラメータを追加する。パラメータが存在する場合、置き換えない。
        /// </summary>
        /// <param name="command">command</param>
        /// <param name="parameter">parameter</param>
        /// <remarks></remarks>
        protected static void AddParameterIfNotExists(IDbCommand command, IDataParameter parameter)
        {
            DbServer<TConnection>.AddParameter(command, parameter, false);
        }
        /// <summary>
        /// コマンドにパラメータを追加する。パラメータが存在する場合、overwriteIfExistsに従い処理する。
        /// </summary>
        /// <param name="command">command</param>
        /// <param name="parameter">parameter</param>
        /// <param name="overwriteIfExists"></param>
        /// <remarks></remarks>
        private static void AddParameter(IDbCommand command, IDataParameter parameter, bool overwriteIfExists = true)
        {
            string parameterName = parameter.ParameterName;
            if (command.Parameters.Contains(parameterName))
            {
                // 存在する場合、かつ、上書き可能の場合、パラメータを上書きする。
                if (overwriteIfExists)
                {
                    IDataParameter dataParameter = (IDataParameter)command.Parameters[parameterName];
                    dataParameter.Value = parameter.Value;
                }
            }
            else
            {
                // 存在しない場合、パラメータを追加する。
                command.Parameters.Add(parameter);
            }
        }

        #region "更新系処理に関する処理"
        /// <summary>
        /// SQLステートメントを実行する。楽観的排他制御が有効、かつ、更新件数が０件の場合、例外が投げられる。
        /// </summary>
        /// <param name="command"><see cref="IDbCommand"/></param>
        /// <param name="enableOptimisticConcurrency">楽観的排他制御の実施有無</param>
        /// <returns>SQLステートメントを実行した結果、影響を受けた行の数</returns>
        protected static int ExecuteNonQuery(IDbCommand command, bool enableOptimisticConcurrency = true)
        {
            try
            {
                // ログを出力する。
                LogUtils.LogSql(command);
                //
                // SQLを実行する。
                int count = command.ExecuteNonQuery();
                // 楽観的同時実行排他制御が有効、かつ、更新件数が０件の場合、例外を投げる。
                if (enableOptimisticConcurrency && (count == 0))
                {
                    string message = DbServer<TConnection>.GetDBConcurrencyExceptionMessage(command, count);
                    // 例外を投げる。
                    var exception = new DBConcurrencyException(message);
                    throw exception;
                }
                // 結果を戻す。
                return count;
            }
            catch (DbException ex)
            {
                // ログを出力する。
                LogUtils.GetErrorLogger().Error(ex);
                //
                // 例外を投げる。（丸投げ）
                throw;
            }
        }
        /// <summary>
        /// 同時実行排他制御の失敗を通知するエラーメッセージを戻す。
        /// </summary>
        /// <param name="command"><see cref="IDbCommand"/></param>
        /// <param name="numberOfRowsAffected">SQLステートメントにより影響を受けた行の数</param>
        /// <returns></returns>
        protected static string GetDBConcurrencyExceptionMessage(IDbCommand command, int? numberOfRowsAffected = null)
        {
            string commandText = command.CommandText;
            string parameters = DbServer<TConnection>.GetParametersString(command.Parameters);
            var builder = new StringBuilder();
            builder.Append("同時実行排他制御エラーが発生しました。");
            builder.Append(" SQL: ").Append(commandText).Append(" ;");
            builder.Append(" PARAMETERS: ").Append(parameters);
            if (numberOfRowsAffected.HasValue)
            {
                builder.Append(" ;");
                builder.Append(" The number of rows affected: ").Append(numberOfRowsAffected);
            }
            // 結果を戻す。
            return builder.ToString();
        }
        #endregion

        #region "便利機能"
        /// <summary>
        /// SQLパラメータの文字列を戻す。
        /// </summary>
        /// <param name="parameterCollection">SQLパラメータのコレクション</param>
        /// <param name="delimiter">区切り文字</param>
        /// <returns>SQLパラメータの文字列</returns>
        /// <remarks>
        /// １行に全SQLパラメータを出力する場合、delimiterの指定は不要。デフォルトの区切り文字", "を使用した文字列を戻します。
        /// １行に１パラメータずつ出力したい場合は、区切り文字に改行文字を設定してください。
        /// </remarks>
        public static string GetParametersString(IDataParameterCollection parameterCollection, string delimiter = ", ")
        {
            //
            // ローカル関数を定義する。（戻り値：パラメータ名=パラメタ値）
            string parameterNameAndValue(IDataParameter parameter) => $"{parameter?.ParameterName}={parameter?.Value}";
            //
            // パラメータ文字列
            var builder = new StringBuilder();
            //
            IEnumerator<IDataParameter> enumerator = parameterCollection.Cast<IDataParameter>().GetEnumerator();
            if (enumerator.MoveNext())
            {
                // １件目の処理
                {
                    IDataParameter parameter = enumerator.Current;
                    // パラメータ名、値を追加する。
                    builder.Append(parameterNameAndValue(parameter));
                }
                // ２件目以降の処理
                while (enumerator.MoveNext())
                {
                    IDataParameter parameter = enumerator.Current;
                    // デリミタ、パラメータ名、値を追加する。
                    builder.Append(delimiter);
                    builder.Append(parameterNameAndValue(parameter));
                }
            }
            // 結果を戻す。
            return builder.ToString();
        }
        #endregion

    }
}
