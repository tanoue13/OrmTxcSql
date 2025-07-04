using System.Diagnostics.CodeAnalysis;
using System.Net.Security;

namespace OrmTxcSql.Data
{
    /// <summary>
    /// データソースのインターフェイス
    /// </summary>
    /// <remarks>
    /// ・DbServerクラス（および、そのサブクラス）において接続文字列を取得するために利用されるインターフェイス。
    /// ・GetConnectionString()メソッドで取得される文字列が接続文字列として使用されます。
    /// </remarks>
    public interface IDataSource
    {
        /// <summary>
        /// 接続文字列を取得します。
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// 接続文字列を戻す処理の実装には何パターンか考えられる。<br/>
        /// １．固定値を戻す。<br/>
        /// ２．選択肢をプロパティで公開し、プロパティに応じた接続文字列を戻す。<br/>
        /// ３．接続文字列をプロパティで公開し、プロパティの値を戻す。<br/>
        /// </remarks>
        string GetConnectionString();

        /// <summary>
        /// <see cref="RemoteCertificateValidationCallback"/>を取得します。
        /// </summary>
        /// <returns></returns>
#if NET6_0_OR_GREATER
        [return: MaybeNull]
#endif
        RemoteCertificateValidationCallback GetRemoteCertificateValidationCallback();
    }
}
