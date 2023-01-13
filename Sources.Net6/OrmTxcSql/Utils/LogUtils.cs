using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NLog;

namespace OrmTxcSql.Utils
{
    /// <summary>
    /// ログに関するユーティリティクラス。
    /// </summary>
    public class LogUtils
    {

        /// <summary>
        /// ロガーの接頭辞
        /// </summary>
        private static readonly string s_loggerPrefix = "OrmTxcSql";

        /// <summary>
        /// データロガー
        /// </summary>
        private static readonly Logger s_dataLogger = LogManager.GetLogger($"{s_loggerPrefix}.Data");

        /// <summary>
        /// エラーロガー
        /// </summary>
        private static readonly Logger s_errorLogger = LogManager.GetLogger($"{s_loggerPrefix}.Error");

        /// <summary>
        /// SQLロガー
        /// </summary>
        private static readonly Logger s_sqlLogger = LogManager.GetLogger($"{s_loggerPrefix}.Sql");

        //// <summary>
        //// 監査ロガーを取得します。
        //// </summary>
        //// <returns></returns>
        //// <remarks>監査ロガーは、アプリケーションの起動、終了、ログインなどの情報を記録します。</remarks>
        //public static ILog GetAuditLogger()
        //{
        //    ILog logger = LogManager.GetLogger("AuditLogger");
        //    return logger;
        //}

        /// <summary>
        /// データロガーを取得します。
        /// </summary>
        /// <returns></returns>
        /// <remarks>エラーロガーは、業務エラー、システムエラー、例外などの情報を記録します。</remarks>
        public static Logger GetDataLogger() => LogUtils.s_dataLogger;

        /// <summary>
        /// エラーロガーを取得します。
        /// </summary>
        /// <returns></returns>
        /// <remarks>エラーロガーは、業務エラー、システムエラー、例外などの情報を記録します。</remarks>
        public static Logger GetErrorLogger() => LogUtils.s_errorLogger;

        /// <summary>
        /// SQLロガーを取得します。
        /// </summary>
        /// <returns></returns>
        /// <remarks>SQLロガーは、トランザクション、SQLなどの情報を記録します。</remarks>
        public static Logger GetSqlLogger() => LogUtils.s_sqlLogger;

        //// <summary>
        //// 性能ロガーを取得します。
        //// </summary>
        //// <returns></returns>
        //// <remarks>性能ロガーは、処理時間などの情報を記録します。</remarks>
        //public static ILog GetPerformanceLogger()
        //{
        //    ILog logger = LogManager.GetLogger("PerformanceLogger");
        //    return logger;
        //}

        //// <summary>
        //// 追跡ロガーを取得します。
        //// </summary>
        //// <returns></returns>
        //// <remarks>追跡ロガーは、オブジェクトの状態（プロパティの値など）などの情報を記録します。</remarks>
        //public static ILog GetTraceLogger()
        //{
        //    ILog logger = LogManager.GetLogger("TraceLogger");
        //    return logger;
        //}

        //// <summary>
        //// デバッグロガーを取得します。
        //// </summary>
        //// <returns></returns>
        //// <remarks>デバッグロガーは、デバッグ用の情報を記録します。</remarks>
        //public static ILog GetDebugLogger()
        //{
        //    ILog logger = LogManager.GetLogger("DebugLogger");
        //    return logger;
        //}

        //#region "LogConcurrencyException"

        //// <summary>
        //// 同時実行例外を記録する。
        //// </summary>
        //// <param name="exception">同時実行例外</param>
        //// <remarks></remarks>
        ////public static void LogConcurrencyException(OptimisticConcurrencyException exception) {
        ////    string message = "同時実行例外が投げられました。（{0}）";
        ////    LogUtils.GetErrorLogger().Info(String.Format(message, exception.GetKosinInfoString), exception);
        ////}

        //#endregion

        #region "LogSql"

        // <summary>
        // 正規表現（連続する２つ以上の半角スペースを１つに置換する）を取得または設定します。
        // </summary>
        // <value></value>
        // <returns></returns>
        // <remarks></remarks>
        private static readonly Regex s_regexTrim = new Regex(@"\s{2,}");

        /// <summary>
        /// SQLを記録する。
        /// </summary>
        /// <param name="command">command</param>
        /// <remarks></remarks>
        public static void LogSql(IDbCommand command)
        {
            var builder = new StringBuilder();
            // コマンドテキストを追加する。
            builder.Append(s_regexTrim.Replace(command.CommandText, " ").Trim());
            builder.Append(Environment.NewLine);
            // パラメータを追加する。
            builder.Append(LogUtils.GetParametersString(command.Parameters, Environment.NewLine));
            // SQLを記録する。
            LogUtils.GetSqlLogger().Info(builder.ToString());
        }

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
        private static string GetParametersString(IDataParameterCollection parameterCollection, string delimiter = ", ")
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
                // ２件以降の処理
                while (enumerator.MoveNext())
                {
                    IDataParameter parameter = enumerator.Current;
                    // 区切り文字、パラメータ名、値を追加する。
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
