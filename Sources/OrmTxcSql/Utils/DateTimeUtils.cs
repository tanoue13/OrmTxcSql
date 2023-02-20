using System;

namespace OrmTxcSql.Utils
{
    /// <summary>
    /// <see cref="DateTime"/>に関するユーティリティクラス。
    /// </summary>
    /// <remarks>
    /// ・このユーティリティクラスでは、<see cref="DateTime.Kind"/>が<see cref="DateTimeKind.Unspecified"/>の場合はローカル時刻として扱います。
    /// </remarks>
    public class DateTimeUtils
    {
        /// <summary>
        /// <see cref="DateTime.Kind"/>を考慮しローカル時刻に変換します。
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime ToLocalTime(DateTime dateTime)
            => (DateTimeKind.Unspecified == dateTime.Kind)
                ? DateTime.SpecifyKind(dateTime, DateTimeKind.Local).ToLocalTime()
                : dateTime.ToLocalTime();
        /// <summary>
        /// <see cref="DateTime.Kind"/>を考慮しUTCに変換します。
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime ToUniversalTime(DateTime dateTime)
            => (DateTimeKind.Unspecified == dateTime.Kind)
                ? DateTime.SpecifyKind(dateTime, DateTimeKind.Local).ToUniversalTime()
                : dateTime.ToUniversalTime();
    }
}
