using System.Data;
using System.Diagnostics;
using SqlSample01.Daos;
using SqlSample01.Entities;
using System;
#if NET462
using NUnit.Framework;
#endif

namespace SqlSample01Tests.Daos
{
    internal class NoPrimaryKeyDaoTest
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

        private NoPrimaryKeyEntity[] _entities;

        [SetUp]
        public void SetUp()
        {
            //
            var entity0 = new NoPrimaryKeyEntity()
            {
                ColumnA = "A",
                ColumnB = "B",
                ColumnC = "C",
            };
            var entity1 = new NoPrimaryKeyEntity()
            {
                ColumnA = "A",
                ColumnB = "B",
                ColumnC = "C",
            };
            //
            _entities = new NoPrimaryKeyEntity[] {
                entity0,
                entity1,
            };
        }

        [Test]
        public void UpdateByPkTest()
        {
            var entity0 = _entities[0];
            var entity1 = _entities[1];
            //
            var dao = new NoPrimaryKeyDao();
            //
            try
            {
                dao.UpdateByPk(entity0);
                dao.UpdateByPk(entity1);
                //
                Assert.Fail();
            }
            catch (MissingPrimaryKeyException e)
            {
                Debug.WriteLine(e);
                Assert.That(e.GetType(), Is.EqualTo(typeof(MissingPrimaryKeyException)));
            }
        }

        [Test]
        public void SelectByPkTest()
        {
            var entity0 = _entities[0];
            var entity1 = _entities[1];
            //
            var dao = new NoPrimaryKeyDao();
            //
            try
            {
                dao.SelectByPk(entity0);
                dao.SelectByPk(entity1);
                //
                Assert.Fail();
            }
            catch (MissingPrimaryKeyException e)
            {
                Debug.WriteLine(e);
                Assert.That(e.GetType(), Is.EqualTo(typeof(MissingPrimaryKeyException)));
            }
        }
    }
}
