using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Npgsql;
using NpgsqlTypes;
using OrmTxcSql.Daos;
using OrmTxcSql.Data;

namespace OrmTxcSql.Npgsql.Data
{

    public class NpgsqlServer : DbServer<NpgsqlConnection>
    {

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
            object value = property.GetValue(obj);
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
        public static void AddParameterOrReplace(IDbCommand command, string parameterName, NpgsqlDbType dbType, object value)
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


        private bool OnRemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // 証明書ストアの現在のユーザ（Windowsアカウント）の「個人」を開く
            X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            try
            {
                // 証明書ストアを開く
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                // 証明書を選ぶ（絞り込む）
                DateTime now = DateTime.Now;
                X509Certificate2 rootCrt = store.Certificates.Cast<X509Certificate2>()
                    // 条件：有効期間内のもののみ対象とする。
                    .Where(cert => cert.NotBefore < now)
                    .Where(cert => cert.NotAfter > now)
                    // （ここまで）
                    // 条件：発行者で絞り込む。
                    .Where(cert => cert.Issuer.Contains("NOK"))
                    .Where(cert => cert.Issuer.Contains("Root"))
                    .Where(cert => cert.Issuer.Contains("Certificate"))
                    .Where(cert => cert.Issuer.Contains("Authority"))
                    // （ここまで）
                    // ソート：有効期間が最も遅いものを使用する。
                    .OrderByDescending(cert => cert.NotAfter)
                    .FirstOrDefault();
                //
                // ルート証明書が見つからない場合、エラーとする。
                if (null == rootCrt)
                {
                    return false;
                }
                //
                // 証明書チェーン内に信頼する証明書と一致するものがあれば OK とする
                foreach (X509ChainElement element in chain.ChainElements.Cast<X509ChainElement>())
                {
                    bool isValid = rootCrt.Thumbprint.Equals(element.Certificate.Thumbprint);
                    if (isValid)
                    {
                        Console.WriteLine($"VALID: {rootCrt.Thumbprint} - {element.Certificate.Thumbprint}");
                        return true;
                    }
                }
            }
            finally
            {
                store.Close();
            }
            //
            //
            // 信頼されないSSL証明書を使用している場合：
            // ServicePointManager.ServerCertificateValidationCallback
            //     = new RemoteCertificateValidationCallback(OnRemoteCertificateValidationCallback);
            //
            return false;
        }


        public override void Execute(IEnumerable<IDao> daos, Action<IDbTransaction> action)
        {
            // base.Execute(daos, action);
            //
            using (var connection = new NpgsqlConnection())
            {
                connection.ConnectionString = this.DataSource.GetConnectionString();
                connection.UserCertificateValidationCallback = new RemoteCertificateValidationCallback(OnRemoteCertificateValidationCallback);
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
                        //this.Commit(tx);
                    }
                    //catch (DbException ex)
                    //{
                    //    LogUtils.GetErrorLogger().Error(ex);
                    //    // トランザクションをロールバックする。
                    //    this.Rollback(tx);
                    //    //
                    //    // 例外を投げる。（丸投げ）
                    //    throw;
                    //}
                    catch (Exception ex)
                    {
                        //LogUtils.GetErrorLogger().Error(ex);
                        // トランザクションをロールバックする。
                        //this.Rollback(tx);
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
        /// ［拡張］接続のオープン、クローズのみ管理する。
        /// </summary>
        public void Connect(Action<NpgsqlConnection> action)
        {
            using (var connection = new NpgsqlConnection())
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
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        #region "更新系処理に関する処理"
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
