using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Npgsql;
using OrmTxcSql.Data;

namespace NpgsqlSample01.Data
{
    public class NpgsqlDataSource : DataSource
    {
        /// <summary>
        /// 接続文字列ビルダ
        /// </summary>
        private readonly NpgsqlConnectionStringBuilder _connectionStringBuilder;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public NpgsqlDataSource()
        {
            // 接続文字列ビルダを生成する。
            _connectionStringBuilder = new NpgsqlConnectionStringBuilder()
            {
                Host = "localhost",
                Database = "sample01",
                Username = "montblanc",
                Password = "mississippi",
#if NET6_0_OR_GREATER
                IncludeErrorDetail = true,
#else
                IncludeErrorDetails = true,
#endif
                ApplicationName = this.GetType().FullName,
            };
        }

        public override string GetConnectionString()
        {
            string connectionString = _connectionStringBuilder.ToString();
            return connectionString;
        }
#if NET6_0_OR_GREATER
        [return: MaybeNull]
#endif
        public override RemoteCertificateValidationCallback GetRemoteCertificateValidationCallback()
        {
            // SSLを使用する場合、リモートの証明書を検証する。
            SslMode[] useSsl = new SslMode[] { SslMode.Prefer, SslMode.Require };
            if (useSsl.Contains(_connectionStringBuilder.SslMode))
            {
                return new RemoteCertificateValidationCallback(OnRemoteCertificateValidationCallback);
            }
            //
            // fool-proof
            return null;
        }
#if NET6_0_OR_GREATER
        private bool OnRemoteCertificateValidationCallback(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
#else
        private bool OnRemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
#endif
        {
            if (SslPolicyErrors.None == sslPolicyErrors)
            {
                // エラーがない場合、ＯＫとする。
                return true;
            }
            else
            {
                // 証明書のサブジェクトが接続先ホスト名でない場合、エラーとする。
                if (!(certificate?.Subject.Contains($"CN={_connectionStringBuilder.Host}") ?? false))
                {
                    return false;
                }
                //
                // 証明書ストアの現在のユーザ（Windowsアカウント）の「信頼されたルート証明機関」を開く。
                X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                try
                {
                    // 証明書ストアを開く。
                    store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                    // 証明書を選ぶ（絞り込む）。
                    DateTime now = DateTime.Now;
                    var rootCrt = store.Certificates.Cast<X509Certificate2>()
                        // 条件：有効期間内のもののみ対象とする。
                        .Where(crt => crt.NotBefore < now)
                        .Where(crt => crt.NotAfter > now)
                        // （ここまで）

                        //// （サンプル）条件：発行者で絞り込む。
                        //.Where(crt => crt.Issuer.Contains("Root"))
                        //.Where(crt => crt.Issuer.Contains("Certificate"))
                        //.Where(crt => crt.Issuer.Contains("Authority"))
                        //// （ここまで）

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
                    // 証明書チェーン内に信頼する証明書と一致するものがあればＯＫとする。
                    bool chainContainsRootCrt = (chain?.ChainElements.Cast<X509ChainElement>() ?? Enumerable.Empty<X509ChainElement>())
                        .Select(element => element.Certificate)
                        // 一致するものが１件以上あればＯＫとする。
                        .Any(crt => rootCrt.Thumbprint.Equals(crt.Thumbprint));
                    // 結果を戻す。
                    return chainContainsRootCrt;
                }
                finally
                {
                    store.Close();
                }
            }
            //
            // fool-proof
#pragma warning disable CS0162 // 到達できないコードが検出されました
            return false;
#pragma warning restore CS0162 // 到達できないコードが検出されました
            //
            // 開発者向けコメント：Npgsql以外の場合、下のようなロジックを記述する必要があるらしい。
            //
            // 信頼されないSSL証明書を使用している場合：
            // ServicePointManager.ServerCertificateValidationCallback
            //     = new RemoteCertificateValidationCallback(OnRemoteCertificateValidationCallback);
            //
        }
    }
}
