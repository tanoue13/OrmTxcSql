using System;
using System.Diagnostics;
#if NET462
using NUnit.Framework;
#endif

namespace OrmTxcSqlTests.Utils
{
    internal class DateTimeUtilsTest
    {
        private static readonly TraceListener s_listener = new TextWriterTraceListener(Console.Out);

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // DEBUG出力先にコンソール出力を追加。
            Trace.Listeners.Add(s_listener);
        }
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            // DEBUG出力先からコンソール出力を削除。
            Trace.Listeners.Remove(s_listener);
            s_listener.Dispose();
        }

        private void Dump(DateTime datetime)
            => Debug.WriteLine($"{datetime.Kind,-16}: {datetime:yyyy-MM-dd HH:mm:ss.ffffff}");

        /// <summary>
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        /// <remarks>
        /// <see cref="DateTime.Kind"/>が<see cref="DateTimeKind.Unspecified"/>の場合、ローカル時刻として扱う。
        /// </remarks>
        private static DateTime ConvertToLocalTime(DateTime dateTime)
            => (DateTimeKind.Unspecified == dateTime.Kind)
                ? DateTime.SpecifyKind(dateTime, DateTimeKind.Local).ToLocalTime()
                : dateTime.ToLocalTime();
        /// <summary>
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        /// <remarks>
        /// <see cref="DateTime.Kind"/>が<see cref="DateTimeKind.Unspecified"/>の場合、ローカル時刻として扱う。
        /// </remarks>
        private static DateTime ConvertToUniversalTime(DateTime dateTime)
            => (DateTimeKind.Unspecified == dateTime.Kind)
                ? DateTime.SpecifyKind(dateTime, DateTimeKind.Local).ToUniversalTime()
                : dateTime.ToUniversalTime();

        [Test]
        public void DateTimeKindTest()
        {
            //
            Debug.WriteLine("DateTime.Now");
            {
                DateTime datetime = DateTime.Now;
                this.Dump(datetime);
                this.Dump(datetime.ToUniversalTime());
                this.Dump(datetime.ToLocalTime());
                Debug.WriteLine("DateTime.Now では、DateTime.Kind に DateTimeKind.Local が設定される。");
                Debug.WriteLine("値は、DateTime.ToLocalTime() と等しい。");
            }
            Debug.WriteLine(null);
            //
            Debug.WriteLine("new DateTime(2021, 4, 5, 10, 15, 20, 987)");
            {
                DateTime datetime = new DateTime(2021, 4, 5, 10, 15, 20, 987);
                this.Dump(datetime);
                this.Dump(datetime.ToUniversalTime());
                this.Dump(datetime.ToLocalTime());
                Debug.WriteLine("DateTimeKind を指定しないコンストラクタを使用した場合、DateTime.Kind に DateTimeKind.Unspecified が設定される。");
                Debug.WriteLine("値は、DateTime.ToUniversalTime()、DateTime.ToLocalTime() のいずれとも異なる。");
                this.Dump(ConvertToUniversalTime(datetime));
                this.Dump(ConvertToLocalTime(datetime));
                Debug.WriteLine("値が DateTime.ToLocalTime() と等しくなった。");
            }
            Debug.WriteLine(null);
            //
            Debug.WriteLine("new DateTime(2021, 4, 5, 10, 15, 20, 987, DateTimeKind.Unspecified)");
            {
                DateTime datetime = new DateTime(2021, 4, 5, 10, 15, 20, 987, DateTimeKind.Unspecified);
                this.Dump(datetime);
                this.Dump(datetime.ToUniversalTime());
                this.Dump(datetime.ToLocalTime());
                Debug.WriteLine("コンストラクタの引数で DateTimeKind.Unspecified を指定した場合、DateTime.Kind に DateTimeKind.Unspecified が設定される。");
                Debug.WriteLine("値は、DateTime.ToUniversalTime()、DateTime.ToLocalTime() のいずれとも異なる。");
                this.Dump(ConvertToUniversalTime(datetime));
                this.Dump(ConvertToLocalTime(datetime));
                Debug.WriteLine("値が DateTime.ToLocalTime() と等しくなった。");
            }
            Debug.WriteLine(null);
            //
            Debug.WriteLine("new DateTime(2021, 4, 5, 10, 15, 20, 987, DateTimeKind.Local)");
            {
                DateTime datetime = new DateTime(2021, 4, 5, 10, 15, 20, 987, DateTimeKind.Local);
                this.Dump(datetime);
                this.Dump(datetime.ToUniversalTime());
                this.Dump(datetime.ToLocalTime());
                Debug.WriteLine("コンストラクタの引数で DateTimeKind.Local を指定した場合、DateTime.Kind に DateTimeKind.Local が設定される。");
                Debug.WriteLine("値は、DateTime.ToLocalTime() と等しい。");
            }
            Debug.WriteLine(null);
            //
            Debug.WriteLine("new DateTime(2021, 4, 5, 10, 15, 20, 987, DateTimeKind.Utc)");
            {
                DateTime datetime = new DateTime(2021, 4, 5, 10, 15, 20, 987, DateTimeKind.Utc);
                this.Dump(datetime);
                this.Dump(datetime.ToUniversalTime());
                this.Dump(datetime.ToLocalTime());
                Debug.WriteLine("コンストラクタの引数で DateTimeKind.Unspecified を指定した場合、DateTime.Kind に DateTimeKind.Unspecified が設定される。");
                Debug.WriteLine("値は、DateTime.ToUniversalTime() と等しい。");
            }
            Debug.WriteLine(null);
            //
            //
        }
    }
}
