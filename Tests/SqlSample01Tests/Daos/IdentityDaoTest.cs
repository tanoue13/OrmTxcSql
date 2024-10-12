using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using OrmTxcSql.Attributes;
using OrmTxcSql.Daos;
using OrmTxcSql.SqlClient.Data;
using OrmTxcSql.Tests.Utils;
using OrmTxcSql.Utils;
using SqlSample01.Daos;
using SqlSample01.Data;
using SqlSample01.Entities;
#if NET462 || NET48
using NUnit.Framework;
#endif

namespace SqlSample01Tests.Daos
{
    internal class IdentityDaoTest
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

        private IdentityEntity[] _entities;

        [SetUp]
        public void SetUp()
        {
            //
            var entity0 = new IdentityEntity()
            {
                Description = "abcdefg",
            };
            var entity1 = new IdentityEntity()
            {
                Description = "hijklmn",
            };
            //
            _entities = new IdentityEntity[] {
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
            var dao = new IdentityDaoExt();
            //
            var server = new SqlServer();
            server.DataSource = new SqlDataSource();
            server.Execute(new IDao[] { dao }, tx =>
            {
                foreach (int index in Enumerable.Repeat(0, 3))
                {
                    dao.Insert(entityI0);
                }
                foreach (int index in Enumerable.Repeat(0, 3))
                {
                    dao.Insert(entityI1);
                }
                var result = dao.SelectAll();
                //
                // ロールバックする。
                tx.Rollback();
                //
                DebugUtils.Dump<IdentityEntity>(result);
            });
        }

        [Test]
        public void InsertUpdateByPkSelectByPkTest()
        {
            var entityI0 = _entities[0];
            var entityI1 = _entities[1];
            //
            var dao = new IdentityDaoExt();
            //
            var server = new SqlServer();
            server.DataSource = new SqlDataSource();
            server.Execute(new IDao[] { dao }, tx =>
            {
                dao.Insert(entityI0);
                dao.Insert(entityI1);
                //
                var result00 = dao.SelectAll();
                DebugUtils.Dump<IdentityEntity>(result00);
                //
                var entityU0 = result00.First();
                entityU0.Description = entityU0.Description?.ToUpperInvariant();
                //
                dao.UpdateByPk(entityU0);
                //
                var result01 = dao.SelectAll();
                DebugUtils.Dump<IdentityEntity>(result01);
                //
                //
                // ロールバックする。
                tx.Rollback();
            });
        }

        internal class IdentityDaoExt : IdentityDao
        {
            internal IEnumerable<IdentityEntity> SelectAll()
            {
                //
                // コマンドの準備に必要なオブジェクトを生成する。
                SqlCommand command = this.Command;
                //
                // コマンドを準備する。
                this.BuildSelectAllCommand(command);
                //
                // コマンドを実行する。
                DataTable dataTable = this.GetDataTable(command);
                //
                // DataTableをオブジェクトに変換する。
                IdentityEntity[] result = dataTable.AsEnumerable<IdentityEntity>().ToArray();
                //
                // 結果を戻す。
                return result;
            }
            private void BuildSelectAllCommand(SqlCommand command)
            {
                // テーブル名を取得する。
                string tableName = EntityUtils.GetTableName<IdentityEntity>();
                // カラム名を取得する。
                string[] columnNames = EntityUtils.GetColumnAttributes<IdentityEntity>()
                    .Select(prop => new
                    {
                        PropertyInfo = prop,
                        ColumnName = prop.GetCustomAttribute<ColumnAttribute>(false)?.ColumnName ?? String.Empty
                    })
                    .Where(src => !String.IsNullOrEmpty(src.ColumnName))
                    .Select(src => src.ColumnName)
                    .ToArray();
                //
                // コマンドテキストを生成する。
                // 開発者向けコメント：（fool-proof：条件の前に、空白を１つはさむ）
                // OK: select a, b, c from table_name where x = @x
                // NG: select a, b, c from table_namewhere x = @x
                var builder = new StringBuilder();
                builder.Append(" select");
                builder.Append(String.Join(",", columnNames.Select(columnName => String.Format(" x.{0}", columnName))));
                builder.Append(this.GetCommonFieldForSelect("x"));
                builder.Append(" from ").Append(tableName).Append(" as x");
                builder.Append(" "); // fool-proof
                builder.Append(" order by");
                builder.Append("   x.uid");
                //
                //
                // コマンドテキストを設定する。
                command.CommandText = builder.ToString();
                // データソースにコマンドを準備する。
                command.Prepare();
                //
                this.GetDataTable(command);
            }
        }
    }
}
