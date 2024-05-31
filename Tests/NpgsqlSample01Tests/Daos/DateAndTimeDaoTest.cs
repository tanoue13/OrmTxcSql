using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Npgsql;
using NpgsqlSample01.Daos;
using NpgsqlSample01.Data;
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
    internal class DateAndTimeDaoTest
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

        private DateAndTimeEntity[] _entities;

        [SetUp]
        public void SetUp()
        {
            //
            var entity0 = new DateAndTimeEntity()
            {
                Key = "Z00",
                Description = "abcdefg",
            };
            var entity1 = new DateAndTimeEntity()
            {
                Key = "Z01",
                Description = "hijklmn",
            };
            //
            _entities = new DateAndTimeEntity[] {
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
            var dao = new DateAndTimeDaoExt();
            //
            var server = new NpgsqlServer();
            server.DataSource = new NpgsqlDataSource();
            server.Execute(new IDao[] { dao }, tx =>
            {
                dao.Insert(entityI0);
                dao.Insert(entityI1);
                var result = dao.SelectAll();
                //
                // ロールバックする。
                tx.Rollback();
                //
                DebugUtils.DumpEntity<DateAndTimeEntity>(result);
            });
        }

        /// <summary>
        /// 正常系：TimestampWithoutTimeZone
        /// </summary>
        [Test]
        public void InsertTest_N01()
        {
            var entityI0 = new DateAndTimeEntity()
            {
                Key = "A00",
                ColumnTimestampWithoutTimeZone = DateTime.Now,
            };
            var entityI1 = new DateAndTimeEntity()
            {
                Key = "A01",
                ColumnTimestampWithoutTimeZone = new DateTime(2020, 4, 10, 13, 15, 30),
            };
            var entityI2 = new DateAndTimeEntity()
            {
                Key = "A02",
                ColumnTimestampWithoutTimeZone = new DateTime(2020, 4, 10, 13, 15, 30, DateTimeKind.Unspecified),
            };
            var entityI3 = new DateAndTimeEntity()
            {
                Key = "A03",
                ColumnTimestampWithoutTimeZone = new DateTime(2020, 4, 10, 13, 15, 30, DateTimeKind.Local),
            };
            var entityI4 = new DateAndTimeEntity()
            {
                Key = "A04",
                ColumnTimestampWithoutTimeZone = new DateTime(2020, 4, 10, 13, 15, 30, DateTimeKind.Utc),
            };
            //
            var dao = new DateAndTimeDaoExt();
            //
            var server = new NpgsqlServer();
            server.DataSource = new NpgsqlDataSource();
            server.Execute(new IDao[] { dao }, tx =>
            {
                dao.Insert(entityI0);
                dao.Insert(entityI1);
                dao.Insert(entityI2);
                dao.Insert(entityI3);
                dao.Insert(entityI4);
                var result = dao.SelectAll();
                //
                // ロールバックする。
                tx.Rollback();
                //
                DebugUtils.DumpEntity<DateAndTimeEntity>(result);
            });
        }

        /// <summary>
        /// 正常系：ColumnTimestampWithTimeZone
        /// </summary>
        [Test]
        public void InsertTest_N02()
        {
            var entityI0 = new DateAndTimeEntity()
            {
                Key = "A00",
                ColumnTimestampWithTimeZone = DateTime.Now,
            };
            var entityI1 = new DateAndTimeEntity()
            {
                Key = "A01",
                ColumnTimestampWithTimeZone = new DateTime(2020, 4, 10, 13, 15, 30),
            };
            var entityI2 = new DateAndTimeEntity()
            {
                Key = "A02",
                ColumnTimestampWithTimeZone = new DateTime(2020, 4, 10, 13, 15, 30, DateTimeKind.Unspecified),
            };
            var entityI3 = new DateAndTimeEntity()
            {
                Key = "A03",
                ColumnTimestampWithTimeZone = new DateTime(2020, 4, 10, 13, 15, 30, DateTimeKind.Local),
            };
            var entityI4 = new DateAndTimeEntity()
            {
                Key = "A04",
                ColumnTimestampWithTimeZone = new DateTime(2020, 4, 10, 13, 15, 30, DateTimeKind.Utc),
            };
            //
            var dao = new DateAndTimeDaoExt();
            //
            var server = new NpgsqlServer();
            server.DataSource = new NpgsqlDataSource();
            server.Execute(new IDao[] { dao }, tx =>
            {
                dao.Insert(entityI0);
                dao.Insert(entityI1);
                dao.Insert(entityI2);
                dao.Insert(entityI3);
                dao.Insert(entityI4);
                var result = dao.SelectAll();
                //
                // ロールバックする。
                tx.Rollback();
                //
                DebugUtils.DumpEntity<DateAndTimeEntity>(result);
            });
        }

        /// <summary>
        /// 正常系：ColumnTimestampWithTimeZone
        /// </summary>
        [Test]
        public void InsertTest_N03()
        {
            var entityI00 = new DateAndTimeEntity()
            {
                Key = "A00",
                ColumnDate = DateTime.Now,
            };
            var entityI01 = new DateAndTimeEntity()
            {
                Key = "A01",
                ColumnDate = new DateTime(2020, 4, 10, 13, 15, 30),
            };
            var entityI02 = new DateAndTimeEntity()
            {
                Key = "A02",
                ColumnDate = new DateTime(2020, 4, 10, 13, 15, 30, DateTimeKind.Unspecified),
            };
            var entityI03 = new DateAndTimeEntity()
            {
                Key = "A03",
                ColumnDate = new DateTime(2020, 4, 10, 13, 15, 30, DateTimeKind.Local),
            };
            var entityI04 = new DateAndTimeEntity()
            {
                Key = "A04",
                ColumnDate = new DateTime(2020, 4, 10, 13, 15, 30, DateTimeKind.Utc),
            };
            var entityI10 = new DateAndTimeEntity()
            {
                Key = "B00",
                ColumnDate = DateTime.Now.Date,
            };
            var entityI11 = new DateAndTimeEntity()
            {
                Key = "B01",
                ColumnDate = new DateTime(2021, 5, 15, 0, 0, 0).Date,
            };
            var entityI12 = new DateAndTimeEntity()
            {
                Key = "B02",
                ColumnDate = new DateTime(2021, 5, 15, 0, 0, 0, DateTimeKind.Unspecified).Date,
            };
            var entityI13 = new DateAndTimeEntity()
            {
                Key = "B03",
                ColumnDate = new DateTime(2021, 5, 15, 0, 0, 0, DateTimeKind.Local).Date,
            };
            var entityI14 = new DateAndTimeEntity()
            {
                Key = "B04",
                ColumnDate = new DateTime(2021, 5, 15, 0, 0, 0, DateTimeKind.Utc).Date,
            };
            //
            var dao = new DateAndTimeDaoExt();
            //
            var server = new NpgsqlServer();
            server.DataSource = new NpgsqlDataSource();
            server.Execute(new IDao[] { dao }, tx =>
            {
                dao.Insert(entityI00);
                dao.Insert(entityI01);
                dao.Insert(entityI02);
                dao.Insert(entityI03);
                dao.Insert(entityI04);
                dao.Insert(entityI10);
                dao.Insert(entityI11);
                dao.Insert(entityI12);
                dao.Insert(entityI13);
                dao.Insert(entityI14);
                var result = dao.SelectAll();
                //
                // ロールバックする。
                tx.Rollback();
                //
                DebugUtils.DumpEntity<DateAndTimeEntity>(result);
            });
        }

        [Test]
        public void InsertUpdateByPkSelectByPkTest()
        {
            var entityI0 = _entities[0];
            var entityI1 = _entities[1];
            //
            var dao = new DateAndTimeDaoExt();
            //
            var server = new NpgsqlServer();
            server.DataSource = new NpgsqlDataSource();
            server.Execute(new IDao[] { dao }, tx =>
            {
                dao.Insert(entityI0);
                dao.Insert(entityI1);
                //
                var result00 = dao.SelectAll();
                DebugUtils.DumpEntity<DateAndTimeEntity>(result00);
                //
                var entityU0 = result00.First();
                entityU0.Description = entityU0.Description?.ToUpperInvariant();
                //
                dao.UpdateByPk(entityU0);
                //
                var result01 = dao.SelectAll();
                DebugUtils.DumpEntity<DateAndTimeEntity>(result01);
                //
                //
                // ロールバックする。
                tx.Rollback();
            });
        }

        internal class DateAndTimeDaoExt : DateAndTimeDao
        {
            internal IEnumerable<DateAndTimeEntity> SelectAll()
            {
                //
                // コマンドの準備に必要なオブジェクトを生成する。
                NpgsqlCommand command = this.Command;
                //
                // コマンドを準備する。
                this.BuildSelectAllCommand(command);
                //
                // コマンドを実行する。
                DataTable dataTable = this.GetDataTable(command);
                //
                // DataTableをオブジェクトに変換する。
                DateAndTimeEntity[] result = dataTable.AsEnumerable<DateAndTimeEntity>().ToArray();
                //
                // 結果を戻す。
                return result;
            }
            private void BuildSelectAllCommand(NpgsqlCommand command)
            {
                // テーブル名を取得する。
                string tableName = EntityUtils.GetTableName<DateAndTimeEntity>();
                // カラム名を取得する。
                string[] columnNames = EntityUtils.GetColumnAttributes<DateAndTimeEntity>()
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
                builder.Append("   x.key");
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
