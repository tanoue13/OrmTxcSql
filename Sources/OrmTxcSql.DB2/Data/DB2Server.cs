using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using IBM.Data.DB2.iSeries;
using OrmTxcSql.Data;

namespace OrmTxcSql.DB2.Data
{
    /// <summary>
    /// DBMS（DB2）との接続、トランザクション管理を行うクラス。
    /// </summary>
    public class DB2Server : DbServer<iDB2Connection>
    {

        private static IParameterValueConverter ParameterValueConverter { get; set; } = new DB2ParameterValueConverter();

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
            iDB2DbType dbType = iDB2DbType.iDB2VarChar; // default
            if (new Type[] { typeof(decimal), typeof(decimal?) }.Contains(propertyType))
            {
                dbType = iDB2DbType.iDB2Numeric;
            }
            else if (new Type[] { typeof(int), typeof(int?) }.Contains(propertyType))
            {
                dbType = iDB2DbType.iDB2Numeric;
            }
            else
            {
                // fool-proof
                dbType = iDB2DbType.iDB2VarChar;
            }
            // パラメータに設定する値を取得する。
#if NET6_0_OR_GREATER
            object? value = property.GetValue(obj);
#else
            object value = property.GetValue(obj);
#endif
            //
            // nullを考慮し、下のメソッド経由で設定する。
            DB2Server.AddParameterOrReplace(command, parameterName, dbType, value);
        }
        /// <summary>
        /// コマンドにパラメータを追加する。パラメータが存在する場合、置き換える。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="value"></param>
        /// <remarks></remarks>
#if NET6_0_OR_GREATER
        public static void AddParameterOrReplace(IDbCommand command, string parameterName, iDB2DbType dbType, object? value)
#else
        public static void AddParameterOrReplace(IDbCommand command, string parameterName, iDB2DbType dbType, object value)
#endif
        {
            IDataParameter parameter = new iDB2Parameter(parameterName, dbType);
            parameter.Value = DB2Server.ParameterValueConverter.Convert(value, dbType.GetType(), dbType);
            DbServer<iDB2Connection>.AddParameterOrReplace(command, parameter);
        }

        /// <summary>
        /// ［拡張］接続のオープン、クローズのみ管理する。
        /// </summary>
        public void Connect(Action<iDB2Connection> action)
        {
            using (var connection = new iDB2Connection())
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
                catch (DbException ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                    // 例外を投げる。（丸投げ）
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
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
        private void CloseConnection(iDB2Connection connection)
        {
            // 接続を閉じる。
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        #region "更新系処理に関する処理"
        /// <summary>
        /// <see cref="DbServer{TConnection}.ExecuteNonQuery(IDbCommand, bool)"/>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="enableOptimisticConcurrency"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(iDB2Command command, bool enableOptimisticConcurrency = true)
        {
            try
            {
                int count = DbServer<iDB2Connection>.ExecuteNonQuery(command, enableOptimisticConcurrency);
                return count;
            }
            catch (iDB2Exception ex)
            {
                switch (ex.ErrorCode)
                {
                    default:
                        {
                            // 例外を投げる。（丸投げ）
                            throw;
                        }
                }

            }
        }
        #endregion

    }

}
