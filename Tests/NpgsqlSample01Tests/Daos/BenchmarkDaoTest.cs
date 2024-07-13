using System;
using System.Collections.Generic;
using System.Data;
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
        public void InsertTest()
        {
            var dao = new BenchmarkDaoExt();
            //
            var server = new NpgsqlServer();
            server.DataSource = new NpgsqlSample01.Data.NpgsqlDataSource();
            server.Execute(new IDao[] { dao }, tx =>
            {
                foreach (var entity in _entities)
                {
                    dao.Insert(entity);
                }
                //
                foreach (int _ in Enumerable.Range(0, 3))
                {
                    DebugUtils.RunWithTimeMeasurement("concurrent", () =>
                    {
                        IEnumerable<BenchmarkEntity> result = dao.SelectAll();
                    });
                    DebugUtils.RunWithTimeMeasurement("async", async () =>
                    {
                        IEnumerable<BenchmarkEntity> result = await dao.SelectAllAsync();
                    });
                }
                //
                // ロールバックする。
                tx.Rollback();
            });
        }

        internal class BenchmarkDaoExt : BenchmarkDao
        {
            internal async Task<IEnumerable<BenchmarkEntity>> SelectAllAsync()
            {
                //
                // コマンドの準備に必要なオブジェクトを生成する。
                NpgsqlCommand command = this.Command;
                //
                // コマンドを準備する。
                this.BuildSelectAllCommand(command);
                //
                // コマンドを実行する。
                DataTable dataTable = await this.GetDataTableAsync(command);
                //
                // DataTableをオブジェクトに変換する。
                BenchmarkEntity[] result = dataTable.AsEnumerable<BenchmarkEntity>().ToArray();
                //
                // 結果を戻す。
                return result;
            }
            internal IEnumerable<BenchmarkEntity> SelectAll()
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
                BenchmarkEntity[] result = dataTable.AsEnumerable<BenchmarkEntity>().ToArray();
                //
                // 結果を戻す。
                return result;
            }
            private void BuildSelectAllCommand(NpgsqlCommand command)
            {
                // テーブル名を取得する。
                string tableName = EntityUtils.GetTableName<BenchmarkEntity>();
                // カラム名を取得する。
                string[] columnNames = EntityUtils.GetColumnAttributes<BenchmarkEntity>()
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
