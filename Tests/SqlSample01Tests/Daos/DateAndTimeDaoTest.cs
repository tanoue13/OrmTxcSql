using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Npgsql;
using NUnit.Framework;
using OrmTxcSql.Attributes;
using OrmTxcSql.Daos;
using OrmTxcSql.SqlClient.Data;
using OrmTxcSql.Tests.Utils;
using OrmTxcSql.Utils;
using SqlSample01.Daos;
using SqlSample01.Data;
using SqlSample01.Entities;

namespace SqlSample01Tests.Daos
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
            var server = new SqlServer();
            server.DataSource = new SqlDataSource();
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
        /// 正常系：Time
        /// </summary>
        [Test]
        public void InsertTest_Time_N01()
        {
            var baseDateTime = new DateTime(2020, 4, 10, 13, 15, 30, 987, DateTimeKind.Unspecified);
            //
            var entityI0 = new DateAndTimeEntity()
            {
                Key = "A00",
                ColumnTime = TimeSpan.FromTicks(DateTime.Now.TimeOfDay.Ticks),
            };
            var entityI1 = new DateAndTimeEntity()
            {
                Key = "A01",
                ColumnTime = TimeSpan.Parse("12:34:56.1234567"),
            };
            var entityI2 = new DateAndTimeEntity()
            {
                Key = "A02",
                ColumnTime = TimeSpan.FromTicks(baseDateTime.TimeOfDay.Ticks),
            };
            var entityI3 = new DateAndTimeEntity()
            {
                Key = "A03",
                ColumnTime = TimeSpan.FromTicks(DateTime.SpecifyKind(baseDateTime, DateTimeKind.Unspecified).TimeOfDay.Ticks),
            };
            var entityI4 = new DateAndTimeEntity()
            {
                Key = "A04",
                ColumnTime = TimeSpan.FromTicks(DateTime.SpecifyKind(baseDateTime, DateTimeKind.Local).TimeOfDay.Ticks),
            };
            var entityI5 = new DateAndTimeEntity()
            {
                Key = "A05",
                ColumnTime = TimeSpan.FromTicks(DateTime.SpecifyKind(baseDateTime, DateTimeKind.Utc).TimeOfDay.Ticks),
            };
            //
            var dao = new DateAndTimeDaoExt();
            //
            var server = new SqlServer();
            server.DataSource = new SqlDataSource();
            server.Execute(new IDao[] { dao }, tx =>
            {
                dao.Insert(entityI0);
                dao.Insert(entityI1);
                dao.Insert(entityI2);
                dao.Insert(entityI3);
                dao.Insert(entityI4);
                dao.Insert(entityI5);
                var result = dao.SelectAll();
                //
                // ロールバックする。
                tx.Rollback();
                //
                DebugUtils.DumpEntity<DateAndTimeEntity>(result);
            });
        }
        /// <summary>
        /// 正常系：Date
        /// </summary>
        [Test]
        public void InsertTest_Date_N01()
        {
            var baseDateTime = new DateTime(2020, 4, 10, 13, 15, 30, 987, DateTimeKind.Unspecified);
            //
            var entityI0 = new DateAndTimeEntity()
            {
                Key = "A00",
                ColumnDate = DateTime.Now,
            };
            var entityI1 = new DateAndTimeEntity()
            {
                Key = "A01",
                ColumnDate = DateTime.Parse("2020-01-23 13:45:30.1234567"),
            };
            var entityI2 = new DateAndTimeEntity()
            {
                Key = "A02",
                ColumnDate = baseDateTime,
            };
            var entityI3 = new DateAndTimeEntity()
            {
                Key = "A03",
                ColumnDate = DateTime.SpecifyKind(baseDateTime, DateTimeKind.Unspecified),
            };
            var entityI4 = new DateAndTimeEntity()
            {
                Key = "A04",
                ColumnDate = DateTime.SpecifyKind(baseDateTime, DateTimeKind.Local),
            };
            var entityI5 = new DateAndTimeEntity()
            {
                Key = "A05",
                ColumnDate = DateTime.SpecifyKind(baseDateTime, DateTimeKind.Utc),
            };
            //
            var dao = new DateAndTimeDaoExt();
            //
            var server = new SqlServer();
            server.DataSource = new SqlDataSource();
            server.Execute(new IDao[] { dao }, tx =>
            {
                dao.Insert(entityI0);
                dao.Insert(entityI1);
                dao.Insert(entityI2);
                dao.Insert(entityI3);
                dao.Insert(entityI4);
                dao.Insert(entityI5);
                var result = dao.SelectAll();
                //
                // ロールバックする。
                tx.Rollback();
                //
                DebugUtils.DumpEntity<DateAndTimeEntity>(result);
            });
        }
        /// <summary>
        /// 正常系：DateTime
        /// </summary>
        [Test]
        public void InsertTest_DateTime_N01()
        {
            var baseDateTime = new DateTime(2020, 4, 10, 13, 15, 30, 987, DateTimeKind.Unspecified);
            //
            var entityI0 = new DateAndTimeEntity()
            {
                Key = "A00",
                ColumnDateTime = DateTime.Now,
            };
            var entityI1 = new DateAndTimeEntity()
            {
                Key = "A01",
                ColumnDateTime = DateTime.Parse("2020-01-23 13:45:30.1234567"),
            };
            var entityI2 = new DateAndTimeEntity()
            {
                Key = "A02",
                ColumnDateTime = baseDateTime,
            };
            var entityI3 = new DateAndTimeEntity()
            {
                Key = "A03",
                ColumnDateTime = DateTime.SpecifyKind(baseDateTime, DateTimeKind.Unspecified),
            };
            var entityI4 = new DateAndTimeEntity()
            {
                Key = "A04",
                ColumnDateTime = DateTime.SpecifyKind(baseDateTime, DateTimeKind.Local),
            };
            var entityI5 = new DateAndTimeEntity()
            {
                Key = "A05",
                ColumnDateTime = DateTime.SpecifyKind(baseDateTime, DateTimeKind.Utc),
            };
            //
            var dao = new DateAndTimeDaoExt();
            //
            var server = new SqlServer();
            server.DataSource = new SqlDataSource();
            server.Execute(new IDao[] { dao }, tx =>
            {
                dao.Insert(entityI0);
                dao.Insert(entityI1);
                dao.Insert(entityI2);
                dao.Insert(entityI3);
                dao.Insert(entityI4);
                dao.Insert(entityI5);
                var result = dao.SelectAll();
                //
                // ロールバックする。
                tx.Rollback();
                //
                DebugUtils.DumpEntity<DateAndTimeEntity>(result);
            });
        }
        /// <summary>
        /// 正常系：DateTime2
        /// </summary>
        [Test]
        public void InsertTest_DateTime2_N01()
        {
            var baseDateTime = new DateTime(2020, 4, 10, 13, 15, 30, 987, DateTimeKind.Unspecified);
            //
            var entityI0 = new DateAndTimeEntity()
            {
                Key = "A00",
                ColumnDateTime2 = DateTime.Now,
            };
            var entityI1 = new DateAndTimeEntity()
            {
                Key = "A01",
                ColumnDateTime2 = DateTime.Parse("2020-01-23 13:45:30.1234567"),
            };
            var entityI2 = new DateAndTimeEntity()
            {
                Key = "A02",
                ColumnDateTime2 = baseDateTime,
            };
            var entityI3 = new DateAndTimeEntity()
            {
                Key = "A03",
                ColumnDateTime2 = DateTime.SpecifyKind(baseDateTime, DateTimeKind.Unspecified),
            };
            var entityI4 = new DateAndTimeEntity()
            {
                Key = "A04",
                ColumnDateTime2 = DateTime.SpecifyKind(baseDateTime, DateTimeKind.Local),
            };
            var entityI5 = new DateAndTimeEntity()
            {
                Key = "A05",
                ColumnDateTime2 = DateTime.SpecifyKind(baseDateTime, DateTimeKind.Utc),
            };
            //
            var dao = new DateAndTimeDaoExt();
            //
            var server = new SqlServer();
            server.DataSource = new SqlDataSource();
            server.Execute(new IDao[] { dao }, tx =>
            {
                dao.Insert(entityI0);
                dao.Insert(entityI1);
                dao.Insert(entityI2);
                dao.Insert(entityI3);
                dao.Insert(entityI4);
                dao.Insert(entityI5);
                var result = dao.SelectAll();
                //
                // ロールバックする。
                tx.Rollback();
                //
                DebugUtils.DumpEntity<DateAndTimeEntity>(result);
            });
        }
        /// <summary>
        /// 正常系：DateTimeOffset
        /// </summary>
        [Test]
        public void InsertTest_DateTimeOffset_N01()
        {
            var baseDateTime = new DateTime(2020, 4, 10, 13, 15, 30, 987, DateTimeKind.Unspecified);
            //
            var entityI00 = new DateAndTimeEntity()
            {
                Key = "A00",
                ColumnDateTimeOffset = DateTime.Now,
            };
            var entityI01 = new DateAndTimeEntity()
            {
                Key = "A01",
                ColumnDateTimeOffset = DateTime.Parse("2020-01-23 13:45:30.1234567"),
            };
            var entityI02 = new DateAndTimeEntity()
            {
                Key = "A02",
                ColumnDateTimeOffset = baseDateTime,
            };
            var entityI03 = new DateAndTimeEntity()
            {
                Key = "A03",
                ColumnDateTimeOffset = DateTime.SpecifyKind(baseDateTime, DateTimeKind.Unspecified),
            };
            var entityI04 = new DateAndTimeEntity()
            {
                Key = "A04",
                ColumnDateTimeOffset = DateTime.SpecifyKind(baseDateTime, DateTimeKind.Local),
            };
            var entityI05 = new DateAndTimeEntity()
            {
                Key = "A05",
                ColumnDateTimeOffset = DateTime.SpecifyKind(baseDateTime, DateTimeKind.Utc),
            };
            var entityI10 = new DateAndTimeEntity()
            {
                Key = "B00",
                ColumnDateTimeOffset = DateTime.Now.Date,
            };
            var entityI11 = new DateAndTimeEntity()
            {
                Key = "B01",
                ColumnDateTimeOffset = DateTime.Parse("2020-01-23 13:45:30.1234567").Date,
            };
            var entityI12 = new DateAndTimeEntity()
            {
                Key = "B02",
                ColumnDateTimeOffset = baseDateTime.Date,
            };
            var entityI13 = new DateAndTimeEntity()
            {
                Key = "B03",
                ColumnDateTimeOffset = DateTime.SpecifyKind(baseDateTime.Date, DateTimeKind.Unspecified),
            };
            var entityI14 = new DateAndTimeEntity()
            {
                Key = "B04",
                ColumnDateTimeOffset = DateTime.SpecifyKind(baseDateTime.Date, DateTimeKind.Local),
            };
            var entityI15 = new DateAndTimeEntity()
            {
                Key = "B05",
                ColumnDateTimeOffset = DateTime.SpecifyKind(baseDateTime.Date, DateTimeKind.Utc),
            };
            //
            var dao = new DateAndTimeDaoExt();
            //
            var server = new SqlServer();
            server.DataSource = new SqlDataSource();
            server.Execute(new IDao[] { dao }, tx =>
            {
                dao.Insert(entityI00);
                dao.Insert(entityI01);
                dao.Insert(entityI02);
                dao.Insert(entityI03);
                dao.Insert(entityI04);
                dao.Insert(entityI05);
                dao.Insert(entityI10);
                dao.Insert(entityI11);
                dao.Insert(entityI12);
                dao.Insert(entityI13);
                dao.Insert(entityI14);
                dao.Insert(entityI15);
                var result = dao.SelectAll();
                //
                // ロールバックする。
                tx.Rollback();
                //
                DebugUtils.DumpEntity<DateAndTimeEntity>(result, 20);
            });
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// 正常系：Net6Date
        /// </summary>
        [Test]
        public void InsertTest_Net6Date_N01()
        {
            var baseDateTime = new DateTime(2020, 4, 10, 21, 30, 45, 123, DateTimeKind.Unspecified);
            //
            var entityI0 = new DateAndTimeEntity()
            {
                Key = "A00",
                ColumnNet6Date = DateOnly.FromDateTime(DateTime.Now),
            };
            var entityI1 = new DateAndTimeEntity()
            {
                Key = "A01",
                ColumnNet6Date = DateOnly.Parse("2020-01-23"),
            };
            var entityI2 = new DateAndTimeEntity()
            {
                Key = "A02",
                ColumnNet6Date = new DateOnly(2020, 1, 23),
            };
            var entityI3 = new DateAndTimeEntity()
            {
                Key = "A03",
                ColumnNet6Date = DateOnly.FromDateTime(DateTime.SpecifyKind(baseDateTime, DateTimeKind.Unspecified)),
            };
            var entityI4 = new DateAndTimeEntity()
            {
                Key = "A04",
                ColumnNet6Date = DateOnly.FromDateTime(DateTime.SpecifyKind(baseDateTime, DateTimeKind.Local)),
            };
            var entityI5 = new DateAndTimeEntity()
            {
                Key = "A05",
                ColumnNet6Date = DateOnly.FromDateTime(DateTime.SpecifyKind(baseDateTime, DateTimeKind.Utc)),
            };
            //
            var dao = new DateAndTimeDaoExt();
            //
            var server = new SqlServer();
            server.DataSource = new SqlDataSource();
            server.Execute(new IDao[] { dao }, tx =>
            {
                dao.Insert(entityI0);
                dao.Insert(entityI1);
                dao.Insert(entityI2);
                dao.Insert(entityI3);
                dao.Insert(entityI4);
                dao.Insert(entityI5);
                var result = dao.SelectAll();
                //
                // ロールバックする。
                tx.Rollback();
                //
                DebugUtils.DumpEntity<DateAndTimeEntity>(result);
            });
        }
        /// <summary>
        /// 正常系：Net6Time
        /// </summary>
        [Test]
        public void InsertTest_Net6Time_N01()
        {
            var baseDateTime = new DateTime(2020, 4, 10, 21, 30, 45, 123, DateTimeKind.Unspecified);
            //
            var entityI0 = new DateAndTimeEntity()
            {
                Key = "A00",
                ColumnNet6Time = TimeOnly.FromDateTime(DateTime.Now),
            };
            var entityI1 = new DateAndTimeEntity()
            {
                Key = "A01",
                ColumnNet6Time = TimeOnly.Parse("13:15:30.999"),
            };
            var entityI2 = new DateAndTimeEntity()
            {
                Key = "A02",
                ColumnNet6Time = new TimeOnly(13, 15, 30, 999),
            };
            var entityI3 = new DateAndTimeEntity()
            {
                Key = "A03",
                ColumnNet6Time = TimeOnly.FromDateTime(DateTime.SpecifyKind(baseDateTime, DateTimeKind.Unspecified)),
            };
            var entityI4 = new DateAndTimeEntity()
            {
                Key = "A04",
                ColumnNet6Time = TimeOnly.FromDateTime(DateTime.SpecifyKind(baseDateTime, DateTimeKind.Local)),
            };
            var entityI5 = new DateAndTimeEntity()
            {
                Key = "A05",
                ColumnNet6Time = TimeOnly.FromDateTime(DateTime.SpecifyKind(baseDateTime, DateTimeKind.Utc)),
            };
            //
            var dao = new DateAndTimeDaoExt();
            //
            var server = new SqlServer();
            server.DataSource = new SqlDataSource();
            server.Execute(new IDao[] { dao }, tx =>
            {
                dao.Insert(entityI0);
                dao.Insert(entityI1);
                dao.Insert(entityI2);
                dao.Insert(entityI3);
                dao.Insert(entityI4);
                dao.Insert(entityI5);
                var result = dao.SelectAll();
                //
                // ロールバックする。
                tx.Rollback();
                //
                DebugUtils.DumpEntity<DateAndTimeEntity>(result);
            });
        }
#endif

        [Test]
        public void InsertUpdateByPkSelectByPkTest()
        {
            var entityI0 = _entities[0];
            var entityI1 = _entities[1];
            //
            var dao = new DateAndTimeDaoExt();
            //
            var server = new SqlServer();
            server.DataSource = new SqlDataSource();
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
                SqlCommand command = this.Command;
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
            private void BuildSelectAllCommand(SqlCommand command)
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
                builder.Append("   x.column_key");
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
