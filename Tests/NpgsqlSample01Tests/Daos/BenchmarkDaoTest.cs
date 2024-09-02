using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlSample01.Daos;
using NpgsqlSample01.Entities;
using OrmTxcSql.Attributes;
using OrmTxcSql.Daos;
using OrmTxcSql.Npgsql.Data;
using OrmTxcSql.Tests.Utils;
using OrmTxcSql.Utils;
#if NET462
using NUnit.Framework;
#endif

namespace OrmTxcSql.Tests.NpgsqlSample01Tests.Daos
{
    internal class BenchmarkDaoTest
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

        private IEnumerable<BenchmarkEntity> _entities;

        [SetUp]
        public void SetUp()
        {
            string text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            //
            var entity0 = new BenchmarkEntity()
            {
                ColumnString01 = text,
                ColumnString02 = text,
                ColumnString03 = text,
                ColumnString04 = text,
                ColumnString05 = text,
                ColumnString06 = text,
                ColumnString07 = text,
                ColumnString08 = text,
                ColumnString09 = text,
                ColumnString10 = text,
                ColumnString11 = text,
                ColumnString12 = text,
                ColumnString13 = text,
                ColumnString14 = text,
                ColumnString15 = text,
                ColumnString16 = text,
                ColumnString17 = text,
                ColumnString18 = text,
                ColumnString19 = text,
                ColumnString20 = text,
                ColumnString21 = text,
                ColumnString22 = text,
                ColumnString23 = text,
                ColumnString24 = text,
                ColumnString25 = text,
                ColumnString26 = text,
                ColumnString27 = text,
                ColumnString28 = text,
                ColumnString29 = text,
                ColumnString30 = text,
                ColumnString31 = text,
                ColumnString32 = text,
                ColumnString33 = text,
                ColumnString34 = text,
                ColumnString35 = text,
                ColumnString36 = text,
                ColumnString37 = text,
                ColumnString38 = text,
                ColumnString39 = text,
                ColumnString40 = text,
                ColumnString41 = text,
                ColumnString42 = text,
                ColumnString43 = text,
                ColumnString44 = text,
                ColumnString45 = text,
                ColumnString46 = text,
                ColumnString47 = text,
                ColumnString48 = text,
                ColumnString49 = text,
                ColumnString50 = text,
            };
            //
            _entities = Enumerable.Range(0, 50_000).Select(x => new BenchmarkEntity()
            {
                Key = x,
                ColumnString01 = text,
                ColumnString02 = text,
                ColumnString03 = text,
                ColumnString04 = text,
                ColumnString05 = text,
                ColumnString06 = text,
                ColumnString07 = text,
                ColumnString08 = text,
                ColumnString09 = text,
                ColumnString10 = text,
                ColumnString11 = text,
                ColumnString12 = text,
                ColumnString13 = text,
                ColumnString14 = text,
                ColumnString15 = text,
                ColumnString16 = text,
                ColumnString17 = text,
                ColumnString18 = text,
                ColumnString19 = text,
                ColumnString20 = text,
                ColumnString21 = text,
                ColumnString22 = text,
                ColumnString23 = text,
                ColumnString24 = text,
                ColumnString25 = text,
                ColumnString26 = text,
                ColumnString27 = text,
                ColumnString28 = text,
                ColumnString29 = text,
                ColumnString30 = text,
                ColumnString31 = text,
                ColumnString32 = text,
                ColumnString33 = text,
                ColumnString34 = text,
                ColumnString35 = text,
                ColumnString36 = text,
                ColumnString37 = text,
                ColumnString38 = text,
                ColumnString39 = text,
                ColumnString40 = text,
                ColumnString41 = text,
                ColumnString42 = text,
                ColumnString43 = text,
                ColumnString44 = text,
                ColumnString45 = text,
                ColumnString46 = text,
                ColumnString47 = text,
                ColumnString48 = text,
                ColumnString49 = text,
                ColumnString50 = text,
            });
        }

        [Test]
        public void ExecuteTest_N01()
        {
            var dao = new BenchmarkDao();
            //
            var server = new NpgsqlServer();
            server.DataSource = new NpgsqlSample01.Data.NpgsqlDataSource();
            foreach (int _ in Enumerable.Range(0, 10))
            {
                DebugUtils.RunWithTimeMeasurement("async     ", async () =>
                {
                    await server.ExecuteAsync(new IDao[] { dao }, tx => { });
                });
                DebugUtils.RunWithTimeMeasurement("concurrent", () =>
                {
                    server.Execute(new IDao[] { dao }, tx => { });
                });
            }
        }
        [Test]
        public void ExecuteTest_N02()
        {
            var dao = new BenchmarkDao();
            //
            var server = new NpgsqlServer();
            server.DataSource = new NpgsqlSample01.Data.NpgsqlDataSource();
            double case01msec = 0;
            foreach (int _ in Enumerable.Range(0, 10))
            {
                TimeSpan timeSpan = DebugUtils.RunWithTimeMeasurement("async await", async () =>
                {
                    await server.ExecuteAsync(new IDao[] { dao }, tx => { });
                });
                case01msec += timeSpan.TotalSeconds;
            }
            double case02msec = 0;
            foreach (int _ in Enumerable.Range(0, 10))
            {
                TimeSpan timeSpan = DebugUtils.RunWithTimeMeasurement("async      ", () =>
                {
                    ValueTask task = server.ExecuteAsync(new IDao[] { dao }, tx => { });
                });
                case02msec += timeSpan.TotalSeconds;
            }
            double case03msec = 0;
            foreach (int _ in Enumerable.Range(0, 10))
            {
                TimeSpan timeSpan = DebugUtils.RunWithTimeMeasurement("concurrent ", () =>
                {
                    server.Execute(new IDao[] { dao }, tx => { });
                });
                case03msec += timeSpan.TotalSeconds;
            }
            Debug.WriteLine($"---");
            Debug.WriteLine($"(avg) async await: {case01msec / 10,17:##0.000000000000} [sec]");
            Debug.WriteLine($"(avg) async      : {case02msec / 10,17:##0.000000000000} [sec]");
            Debug.WriteLine($"(avg) concurrent : {case03msec / 10,17:##0.000000000000} [sec]");
        }

    }
}
