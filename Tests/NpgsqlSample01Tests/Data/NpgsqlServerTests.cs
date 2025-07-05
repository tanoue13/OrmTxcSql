using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NpgsqlSample01.Data;
using OrmTxcSql.Data;
using OrmTxcSql.Npgsql.Data;
using NpgsqlSample01.Daos;
using NpgsqlSample01.Entities;


#if NET462 || NET48
using NUnit.Framework;
#endif

namespace NpgsqlSample01Tests.Data
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

        [Test]
        public void NpgsqlServerTest_NET6_N01()
        {
            //
            HostApplicationBuilder builder = Host.CreateApplicationBuilder();
            builder.Environment.EnvironmentName = nameof(Environments.Development);
            builder.Services.AddSingleton<IDataSource, NpgsqlDataSource>();
            builder.Services.AddTransient<NpgsqlServer>();
            //
            IHost host = builder.Build();
            //
#if NET6_0_OR_GREATER
            NpgsqlServer? target = host.Services.GetService<NpgsqlServer>() as NpgsqlServer;
#else
            NpgsqlServer target = host.Services.GetService<NpgsqlServer>() as NpgsqlServer;
#endif
            Assert.That(target, Is.Not.Null);
            //
            var dao = new BenchmarkDao();
            var entity = new BenchmarkEntity
            {
                Key = 10,
                ColumnString01 = nameof(BenchmarkEntity.ColumnString01),
            };
            //
            target.Execute(dao, tx =>
            {
                dao.Insert(entity);
                //
                dao.SelectByPk(entity);
                //
                //
                tx.Rollback();
            });
        }
    }
}
