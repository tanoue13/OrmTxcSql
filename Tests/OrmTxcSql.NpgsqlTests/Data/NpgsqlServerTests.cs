using System;
using System.Diagnostics;
#if NET462 || NET48
using NUnit.Framework;
#endif
using OrmTxcSql.Npgsql.Data;

namespace OrmTxcSql.NpgsqlTests.Data
{
    internal class NpgsqlServerTests
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

#if NET6_0_OR_GREATER
        [Test]
        public void DbServerTest_NET6_E01()
        {
            NpgsqlServer server = new();
            //
            Assert.That(server, Is.TypeOf<NpgsqlServer>());
            Assert.That(() =>
            {
                // DataSource プロパティに null を設定すると例外が発生することを確認
#pragma warning disable CS8625 // null リテラルを null 非許容参照型に変換できません。
                server.DataSource = null;
#pragma warning restore CS8625 // null リテラルを null 非許容参照型に変換できません。
            }, Throws.ArgumentNullException.With.Message.Contains("DataSource cannot be null."));
        }
        [Test]
        public void DbServerTest_NET6_E02()
        {
            Assert.That(() =>
            {
                // 引数に null を渡すと例外が発生することを確認
#pragma warning disable CS8625 // null リテラルを null 非許容参照型に変換できません。
                var server = new NpgsqlServer(null);
#pragma warning restore CS8625 // null リテラルを null 非許容参照型に変換できません。
            }, Throws.ArgumentNullException.With.Message.Contains("DataSource cannot be null."));
        }
#else
        [Test]
        public void NpgsqlServerTest_NET462_E01()
        {
            var server = new NpgsqlServer();
            //
            Assert.That(server, Is.TypeOf<NpgsqlServer>());
            Assert.That(() =>
            {
                // DataSource プロパティに null を設定すると例外が発生することを確認
                server.DataSource = null;
            }, Throws.ArgumentNullException.With.Message.Contains("DataSource cannot be null."));
        }
        [Test]
        public void NpgsqlServerTest_NET462_E02()
        {
            Assert.That(() =>
            {
                // 引数に null を渡すと例外が発生することを確認
                var server = new NpgsqlServer(null);
            }, Throws.ArgumentNullException.With.Message.Contains("DataSource cannot be null."));
        }

#endif
    }
}
