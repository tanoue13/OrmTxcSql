using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OrmTxcSql.Daos;
using OrmTxcSql.Data;
using OrmTxcSql.Utils;

namespace OrmTxcSql.SqlClient.Data
{
    /// <summary>
    /// DBMS（SQL Server）との接続、トランザクション管理を行うクラス。
    /// </summary>
    public class SqlServer : DbServer<SqlConnection>
    {

        private static readonly IParameterValueConverter s_parameterValueConverter = new SqlParameterValueConverter();

        /// <summary>
        /// コマンドにパラメータを追加する。パラメータが存在する場合、置き換える。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterName"></param>
        /// <param name="obj"></param>
        /// <param name="property"></param>
        /// <remarks></remarks>
        public static void AddParameterOrReplace(IDbCommand command, string parameterName, object obj, PropertyInfo property)
        {
            // パラメータのデータ型を取得する。
            Type propertyType = property.PropertyType;
            SqlDbType dbType = SqlDbType.NVarChar; //default
            if (new Type[] { typeof(decimal), typeof(decimal?) }.Contains(propertyType))
            {
                dbType = SqlDbType.Decimal;
            }
            else if (new Type[] { typeof(int), typeof(int?) }.Contains(propertyType))
            {
                dbType = SqlDbType.Decimal;
            }
#if NET6_0_OR_GREATER
            else if (new Type[] { typeof(DateOnly), typeof(DateOnly?) }.Contains(propertyType))
            {
                dbType = SqlDbType.Date;
            }
            else if (new Type[] { typeof(TimeOnly), typeof(TimeOnly?) }.Contains(propertyType))
            {
                dbType = SqlDbType.Time;
            }
#endif
            else if (new Type[] { typeof(DateTime), typeof(DateTime?) }.Contains(propertyType))
            {
                dbType = SqlDbType.DateTime2;
            }
            else if (new Type[] { typeof(TimeSpan), typeof(TimeSpan?) }.Contains(propertyType))
            {
                dbType = SqlDbType.Time;
            }
            else if (new Type[] { typeof(DateTimeOffset), typeof(DateTimeOffset?) }.Contains(propertyType))
            {
                dbType = SqlDbType.DateTimeOffset;
            }
            else
            {
                // fool-proof
                dbType = SqlDbType.NVarChar;
            }
            // パラメータに設定する値を取得する。
#if NET6_0_OR_GREATER
            object? value = property.GetValue(obj);
#else
            object value = property.GetValue(obj);
#endif
            //
            // nullを考慮し、下のメソッド経由で設定する。
            SqlServer.AddParameterOrReplace(command, parameterName, dbType, value);
        }

        /// <summary>
        /// コマンドにパラメータを追加する。パラメータが存在する場合、置き換える。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="value"></param>
        /// <remarks>値がnullの場合、DBNull.Valueに変換して設定する。</remarks>
#if NET6_0_OR_GREATER
        public static void AddParameterOrReplace(IDbCommand command, string parameterName, SqlDbType dbType, object? value)
#else
        public static void AddParameterOrReplace(IDbCommand command, string parameterName, SqlDbType dbType, object value)
#endif
        {
            IDataParameter parameter = CreateSqlParameter(parameterName, dbType, value);
            parameter.Value = SqlServer.s_parameterValueConverter.Convert(value, null, null);
            DbServer<SqlConnection>.AddParameterOrReplace(command, parameter);
        }
        /// <summary>
        /// コマンドにパラメータを追加する。パラメータが存在する場合、置き換えない。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="value"></param>
        /// <remarks></remarks>
        public static void AddParameterIfNotExists(IDbCommand command, string parameterName, SqlDbType dbType, object value)
        {
            IDataParameter parameter = CreateSqlParameter(parameterName, dbType, value);
            parameter.Value = SqlServer.s_parameterValueConverter.Convert(value, null, null);
            DbServer<SqlConnection>.AddParameterIfNotExists(command, parameter);
        }
        /// <summary>
        /// 可変長データ型に応じたサイズ（Size, Precision, Scale）が設定済みのパラメータを生成する。
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="dataType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>
        /// SQLServerではPrepareを呼び出す際、可変長データ型のパラメータにおいてはサイズの設定が必要。
        /// <br/>
        /// <seealso href="https://docs.microsoft.com/ja-jp/dotnet/api/system.data.sqlclient.sqlcommand.prepare?view=dotnet-plat-ext-3.1"/>
        /// </remarks>
#if NET6_0_OR_GREATER
        private static SqlParameter CreateSqlParameter(string parameterName, SqlDbType dataType, object? value)
#else
        private static SqlParameter CreateSqlParameter(string parameterName, SqlDbType dataType, object value)
#endif
        {
            // パラメータを生成する。
            SqlParameter parameter = new SqlParameter(parameterName, dataType);
            //
            switch (dataType)
            {
                case SqlDbType.NChar:
                    {
                        // string型に変換する。
#if NET6_0_OR_GREATER
                        string? sValue = value as string;
#else
                        string sValue = value as string;
#endif
                        if (!String.IsNullOrEmpty(sValue))
                        {
                            parameter.Size = sValue.Length;
                        }
                        // nullや長さ0の文字列の場合でも、最低でも1以上のサイズ設定が必要。
                        if (0 == parameter.Size)
                        {
                            parameter.Size = 1;
                        }
                        //
                        break;
                    }
                case SqlDbType.NVarChar:
                    {
                        // string型に変換する。
#if NET6_0_OR_GREATER
                        string? sValue = value as string;
#else
                        string sValue = value as string;
#endif
                        if (!String.IsNullOrEmpty(sValue))
                        {
                            parameter.Size = sValue.Length;
                        }
                        // nullや長さ0の文字列の場合でも、最低でも1以上のサイズ設定が必要。
                        if (0 == parameter.Size)
                        {
                            parameter.Size = 1;
                        }
                        //
                        break;
                    }
                case SqlDbType.Decimal:
                    {
                        // 開発者向けコメント：有効桁数は最大値。小数点以下桁数は値から設定する。
                        // 参考：https://msdn.microsoft.com/ja-jp/library/ms187746(v=sql.120).aspx
                        parameter.Precision = 38;
                        parameter.Scale = (byte)GetScale(value as decimal?);
                        break;
                    }
                case SqlDbType.Time:
                    {
                        // 参考：https://learn.microsoft.com/ja-jp/previous-versions/sql/sql-server-2012/bb677243(v=sql.110)
                        string timeFormat = "hh:mm:ss.nnnnnnn"; // 16桁
                        parameter.Size = timeFormat.Length;
                        break;
                    }
                case SqlDbType.Date:
                    {
                        // 参考：https://learn.microsoft.com/ja-jp/previous-versions/sql/sql-server-2012/bb630352(v=sql.110)
                        string dateFormat = "YYYY-MM-DD"; // 10桁
                        parameter.Size = dateFormat.Length;
                        break;
                    }
                case SqlDbType.DateTime2:
                    {
                        // 参考：https://stackoverflow.com/questions/29699253/trouble-writing-to-sql
                        // 参考：https://learn.microsoft.com/ja-jp/previous-versions/sql/sql-server-2012/bb677335(v=sql.110)
                        string datetime2Format = "YYYY-MM-DD hh:mm:ss.0000000"; // 27桁
                        parameter.Size = datetime2Format.Length;
                        break;
                    }
                case SqlDbType.DateTimeOffset:
                    {
                        // 参考：https://learn.microsoft.com/ja-jp/previous-versions/sql/sql-server-2012/bb630289(v=sql.110)
                        string dateTimeOffsetFormat = "YYYY-MM-DD hh:mm:ss.nnnnnnn +hh:mm"; // 34桁
                        parameter.Size = dateTimeOffsetFormat.Length;
                        break;
                    }
                default:
                    {
                        // fool-proof
                        string message = MessageUtils.GetInvalidEnumArgumentExceptionMessage<SqlDbType>((int)dataType);
                        throw new InvalidEnumArgumentException(message);
                    }
            }
            //
            return parameter;
        }
        private static int GetScale(decimal? value)
            => value.HasValue ? GetScale(value.Value) : 0;
        /// <summary>
        /// Decimal型の数値の小数部分の桁数を取得する。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <seealso href="https://docs.microsoft.com/ja-jp/dotnet/api/system.decimal.getbits?view=netcore-3.1"/>
        /// <seealso href="https://qiita.com/chocolamint/items/80ca5271c6ce1a185430"/>
        private static int GetScale(decimal value)
        {
            int[] bits = Decimal.GetBits(value);
            // 添え字 0 ～ 2 は仮数部領域。添え字 3 の先頭 32 ビット領域が符号と指数部、そして未使用領域。
            int info = bits[3];
            // 下位16ビットは未使用（全部ゼロ）なので捨てる。（符号と指数部を取得）
            int signAndExponent = info >> 16;
            // 下位 8ビットを取得する。（指数部を取得）
            int exponent = signAndExponent & 0x00FF;
            // 結果を戻す。（指数部＝小数部分の桁数）
            return exponent;
        }

        /// <summary>
        /// ［拡張］接続のオープン、クローズのみ管理する。
        /// </summary>
        public void Connect(Action<SqlConnection> action)
        {
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = this.DataSource.GetConnectionString();
                connection.Open();
                //
                // メイン処理
                try
                {
                    // メイン処理を実行する。
                    action(connection);
                    //
                }
                catch (SqlException)
                {
                    // 例外を投げる。（丸投げ）
                    throw;
                }
                catch (Exception)
                {
                    // 例外を投げる。（丸投げ）
                    throw;
                }
                // 接続を閉じる。
                this.CloseConnection(connection);
            }
        }
        /// <summary>
        /// 接続を閉じる。
        /// </summary>
        /// <param name="connection"></param>
        private void CloseConnection(SqlConnection connection)
        {
            // 接続を閉じる。
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        /// <summary>
        /// トランザクション管理下において、<paramref name="action"/>を実行する。（非同期）
        /// </summary>
        /// <param name="daos">トランザクションに参加する<see cref="IDao"/>のコレクション</param>
        /// <param name="action">トランザクション管理下で実行される処理</param>
        [Obsolete("プロバイダーによる実装ではないため、通常の Execute(IEnumerable<IDao> daos, Action<IDbTransaction> action) を使用ください。")]
        public override async ValueTask ExecuteAsync(IEnumerable<IDao> daos, Action<IDbTransaction> action)
        {
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = this.DataSource.GetConnectionString();
                //
                LogUtils.GetDataLogger().Debug("Connection is being opened.");
                await connection.OpenAsync();
                LogUtils.GetDataLogger().Debug("Connection has been opened.");
                //
                LogUtils.GetDataLogger().Debug("Transaction is starting.");
                using (SqlTransaction tx = connection.BeginTransaction())
                {
                    //LogUtils.GetDataLogger().Debug("Transaction has started.");
                    //
                    // 前処理：コマンドに接続とトランザクションを設定する。
                    foreach (IDao dao in daos)
                    {
                        IEnumerable<IDbCommand> commands = dao.Commands ?? Enumerable.Empty<IDbCommand>();
                        foreach (IDbCommand command in commands)
                        {
                            // 接続を設定する。
                            command.Connection = connection;
                            // トランザクションを設定する。
                            command.Transaction = tx;
                        }
                    }
                    //
                    // メイン処理：実装クラスのexecute()を実行する。
                    try
                    {
                        // メイン処理を実行する。
                        action(tx);
                        //
                        // トランザクションをコミットする。
#if NET6_0_OR_GREATER
                        await this.CommitAsync(tx);
#else
                        this.Commit(tx);
#endif
                    }
                    catch (DbException ex)
                    {
                        LogUtils.GetDataLogger().Error(ex);
                        LogUtils.GetErrorLogger().Error(ex);
                        // トランザクションをロールバックする。
#if NET6_0_OR_GREATER
                        await this.RollbackAsync(tx);
#else
                        this.Rollback(tx);
#endif
                        //
                        // 例外を投げる。（丸投げ）
                        throw;
                    }
                    catch (Exception ex)
                    {
                        LogUtils.GetDataLogger().Error(ex);
                        LogUtils.GetErrorLogger().Error(ex);
                        // トランザクションをロールバックする。
#if NET6_0_OR_GREATER
                        await this.RollbackAsync(tx);
#else
                        this.Rollback(tx);
#endif
                        //
                        // 例外を投げる。（丸投げ）
                        throw;
                    }
                }
                //
                // 接続を閉じる。
                this.CloseConnection(connection);
            }
        }
        /// <summary>
        /// トランザクションをコミットする。
        /// </summary>
        /// <param name="tx"></param>
        private void Commit(SqlTransaction tx)
        {
            try
            {
                // トランザクションをコミットする。
                tx.Commit();
            }
            catch (InvalidOperationException ex)
            {
                // トランザクションは、既にコミットまたはロールバックされています。
                // または、接続が切断されています。
                LogUtils.GetErrorLogger().Error(ex);
            }
            catch (Exception ex)
            {
                // トランザクションのコミット中にエラーが発生しました。
                LogUtils.GetErrorLogger().Error(ex);
            }
        }
        /// <summary>
        /// トランザクションをロールバックする。
        /// </summary>
        /// <param name="tx"></param>
        private void Rollback(SqlTransaction tx)
        {
            try
            {
                // トランザクションをロールバックする。
                tx.Rollback();
            }
            catch (InvalidOperationException ex)
            {
                // トランザクションは、既にコミットまたはロールバックされています。
                // または、接続が切断されています。
                LogUtils.GetErrorLogger().Error(ex);
            }
            catch (Exception ex)
            {
                // トランザクションのロールバック中にエラーが発生しました。
                LogUtils.GetErrorLogger().Error(ex);
            }
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// トランザクションをコミットする。（非同期）
        /// </summary>
        /// <param name="tx"></param>
        private async ValueTask CommitAsync(SqlTransaction tx)
        {
            try
            {
                // トランザクションをコミットする。
                await tx.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                // トランザクションは、既にコミットまたはロールバックされています。
                // または、接続が切断されています。
                LogUtils.GetErrorLogger().Error(ex);
            }
            catch (Exception ex)
            {
                // トランザクションのコミット中にエラーが発生しました。
                LogUtils.GetErrorLogger().Error(ex);
            }
        }
        /// <summary>
        /// トランザクションをロールバックする。（非同期）
        /// </summary>
        /// <param name="tx"></param>
        private async ValueTask RollbackAsync(SqlTransaction tx)
        {
            try
            {
                // トランザクションをロールバックする。
                await tx.RollbackAsync();
            }
            catch (InvalidOperationException ex)
            {
                // トランザクションは、既にコミットまたはロールバックされています。
                // または、接続が切断されています。
                LogUtils.GetErrorLogger().Error(ex);
            }
            catch (Exception ex)
            {
                // トランザクションのロールバック中にエラーが発生しました。
                LogUtils.GetErrorLogger().Error(ex);
            }
        }
        /// <summary>
        /// 接続を閉じる。（非同期）
        /// </summary>
        /// <param name="connection"></param>
        private async ValueTask CloseConnectionAsync(SqlConnection connection)
        {
            // 接続を閉じる。
            //LogUtils.GetDataLogger().Debug($"Connection is being closed. [ConnectionState: {Enum.GetName(typeof(ConnectionState), connection.State)}]");
            await connection.CloseAsync();
            //LogUtils.GetDataLogger().Debug($"Connection has been closed.");
        }
#endif

        #region "更新系処理に関する処理"
        /// <summary>
        /// <see cref="DbServer{TConnection}.ExecuteNonQuery(IDbCommand, bool)"/>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="enableOptimisticConcurrency"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(SqlCommand command, bool enableOptimisticConcurrency = true)
        {
            try
            {
                int count = DbServer<SqlConnection>.ExecuteNonQuery(command, enableOptimisticConcurrency);
                return count;
            }
            catch (SqlException ex)
            {
                switch (ex.Number)
                {
                    case SqlErrorUniqueKeyViolation:
                        {
                            // 一意性インデックス違反
                            string message = DbServer<SqlConnection>.GetDBConcurrencyExceptionMessage(command);
                            // 例外を投げる。
                            var exception = new DBConcurrencyException(message);
                            throw exception;
                        }
                    case SqlErrorUniqueConstraintViolation:
                        {
                            // 一意性制約違反
                            string message = DbServer<SqlConnection>.GetDBConcurrencyExceptionMessage(command);
                            // 例外を投げる。
                            var exception = new DBConcurrencyException(message);
                            throw exception;
                        }
                    default:
                        {
                            // 例外を投げる。（丸投げ）
                            throw;
                        }
                }
            }
        }
        #endregion

        #region "SQLServer SQL ERROR NO"

        ///＜参考＞
        /// URL: http://msdn.microsoft.com/ja-jp/library/cc645603.aspx
        ///
        ///select m.*
        ///from sys.messages m
        ///  inner join sys.syslanguages l
        ///    on  m.language_id = l.msglangid
        ///    and l.alias = 'Japanese'

        /// <summary>タイムアウト</summary>
        private const int SqlErrorTimeout = -2;

        /// <summary>一意性インデックス違反</summary>
        private const int SqlErrorUniqueKeyViolation = 2601;
        /// <summary>一意性制約違反</summary>
        private const int SqlErrorUniqueConstraintViolation = 2627;

        /// <summary>接続エラー</summary>
        private const int SqlErrorUnableToEstablishTheConnection = 53;

        /// <summary>ログイン失敗</summary>
        private const int SqlErrorLoginFailed = 18456;

        #endregion

    }
}
