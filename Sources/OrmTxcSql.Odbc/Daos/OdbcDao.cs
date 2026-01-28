using System.Data.Odbc;
using OrmTxcSql.Daos;
using OrmTxcSql.Odbc.Data;
using OrmTxcSql.Odbc.Entities;

namespace OrmTxcSql.Odbc.Daos
{
    /// <summary>
    /// DB2用のdao。BaseEntityのサブクラスに対して Insert, UpdateByPk, DeleteByPk, SelectByPk を実装済み。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <remarks>
    /// 同時実行排他制御による更新時の異常等はDB2Server#ExecuteNonQuery()で処理し、
    /// このクラスでは例外処理等は実施しない。
    /// </remarks>
    public abstract class OdbcDao<TEntity> : AbstractDao<TEntity, OdbcCommand, OdbcDataAdapter>
        where TEntity : OdbcEntity, new()
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="entity"></param>
        /// <param name="enableOptimisticConcurrency"></param>
        /// <returns></returns>
        protected override int ExecuteNonQuery(OdbcCommand command, TEntity entity, bool enableOptimisticConcurrency = true)
            => OdbcServer.ExecuteNonQuery(command, enableOptimisticConcurrency);
    }
}
