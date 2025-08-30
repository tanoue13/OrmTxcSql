using System;
using System.Diagnostics;
using System.Linq;
using NpgsqlSample01.Daos;
using NpgsqlSample01.Entities;
using OrmTxcSql.Daos;
using OrmTxcSql.Npgsql.Data;
using OrmTxcSql.Tests.Utils;
using System.Data;

#if NET462 || NET48
using NUnit.Framework;
#endif

namespace OrmTxcSql.Tests.NpgsqlSample01Tests.Daos
{
    internal class ProductDaoTest
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

        private ProductEntity[] _entities;

        [SetUp]
        public void SetUp()
        {
            //
            var entity0 = new ProductEntity()
            {
                ProductCode = "ABCDE12345",
                Description = "あいうえおかきくけこさしすせそたちつてとなにぬねの",
            };
            var entity1 = new ProductEntity()
            {
                ProductCode = "ABCDE00001",
                Description = "あいうえおかきくけこさしすせそたちつてとなにぬねの",
            };
            var entity2 = new ProductEntity()
            {
                ProductCode = "ABCDE00002",
                Description = "あいうえおかきくけこさしすせそたちつてとなにぬねの",
            };
            var entity3 = new ProductEntity()
            {
                ProductCode = "ABCDE00003",
                Description = "あいうえおかきくけこさしすせそたちつてとなにぬねの",
            };
            //
            _entities = new ProductEntity[] {
                entity0,
                entity1,
            };
        }

        [Test]
        public void InsertTest()
        {
            var entityI0 = _entities[0];
            var entityI1 = _entities[1];
            //
            var dao = new ProductDao();
            //
            var server = new NpgsqlServer();
            server.DataSource = new NpgsqlSample01.Data.NpgsqlDataSource();
            server.Execute(new IDao[] { dao }, tx =>
            {
                dao.Insert(entityI0);
                dao.Insert(entityI1);
                var result = dao.Select(new ProductEntity());
                //
                // ロールバックする。
                tx.Rollback();
                //
                DebugUtils.Dump<ProductEntity>(result);
            });
        }

        [Test]
        public void InsertUpdateByPkSelectByPkTest()
        {
            var entityI0 = _entities[0];
            var entityI1 = _entities[1];
            //
            var dao = new ProductDao();
            //
            var server = new NpgsqlServer();
            server.DataSource = new NpgsqlSample01.Data.NpgsqlDataSource();
            server.Execute(new IDao[] { dao }, tx =>
            {
                dao.Insert(entityI0);
                dao.Insert(entityI1);
                //
                var result00 = dao.Select(new ProductEntity());
                DebugUtils.Dump<ProductEntity>(result00);
                //
                var entityU0 = result00.First();
                entityU0.Description = entityU0.Description?.ToUpperInvariant();
                //
                dao.UpdateByPk(entityU0);
                //
                var result01 = dao.Select(new ProductEntity());
                DebugUtils.Dump<ProductEntity>(result01);
                //
                // ロールバックする。
                tx.Rollback();
            });
        }

        [Test]
        public void DeleteByPkTest()
        {
            var entityI0 = _entities[0];
            var entityI1 = _entities[1];
            //
            var dao = new ProductDao();
            //
            var server = new NpgsqlServer();
            server.DataSource = new NpgsqlSample01.Data.NpgsqlDataSource();
            server.Execute(new IDao[] { dao }, tx =>
            {
                foreach (ProductEntity entity in _entities)
                {
                    dao.Insert(entity);
                }
                //
                var result00 = dao.Select(new ProductEntity());
                DebugUtils.Dump<ProductEntity>(result00);
                //
                var entityU0 = result00.First();
                entityU0.Description = entityU0.Description?.ToUpperInvariant();
                //
                dao.DeleteByPk(entityU0);
                //
                var result01 = dao.Select(new ProductEntity());
                DebugUtils.Dump<ProductEntity>(result01);
                //
                // ロールバックする。
                tx.Rollback();
            });
        }

        [Test]
        public void DeleteByPkTest_E01()
        {
            var entityI0 = _entities[0];
            var entityI1 = _entities[1];
            //
            var dao = new ProductDao();
            //
            var server = new NpgsqlServer();
            server.DataSource = new NpgsqlSample01.Data.NpgsqlDataSource();
            server.Execute(new IDao[] { dao }, tx =>
            {
                foreach (ProductEntity entity in _entities)
                {
                    dao.Insert(entity);
                }
                //
                var result00 = dao.Select(new ProductEntity());
                DebugUtils.Dump<ProductEntity>(result00);
                //
                var entityU0 = result00.First();
                entityU0.Description = entityU0.Description?.ToUpperInvariant();
                //
                dao.UpdateByPk(entityU0);
                var result01 = dao.Select(new ProductEntity());
                DebugUtils.Dump<ProductEntity>(result01);
                //
                Assert.Throws<DBConcurrencyException>(() =>
                {
                    // 主キーで削除する：
                    // →versionNo の相違により、更新対象が０健となり、 DBConcurrencyException が発生する。
                    dao.DeleteByPk(entityU0);
                    // ２回目の削除は、主キーに該当する行が存在しないため、DBConcurrencyExceptionが発生する。
                    //                    dao.DeleteByPk(entityU0);
                });
                //
                entityU0.VersionNo = 1;
                dao.DeleteByPk(entityU0);
                Assert.Throws<DBConcurrencyException>(() =>
                {
                    // 再度の削除する：
                    // →主キーに該当する行が存在しないため、DBConcurrencyExceptionが発生する。
                    dao.DeleteByPk(entityU0);
                });
                //
                //
                // ロールバックする。
                tx.Rollback();
            });
        }
    }
}
