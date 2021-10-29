using System.Collections.Generic;
using System.Data;
using OrmTxcSql.Entities;
using OrmTxcSql.Utils;

namespace OrmTxcSql.Extensions
{

    public static class DataTableExtensions
    {

        /// <summary>
        /// <see cref="IEnumerable&lt;out T&gt;" />オブジェクトを返します。ここで、ジェネリックパラメータ T は<typeparamref name="TEntity"/>です。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <remarks><see cref="DataRow"/>から<typeparamref name="TEntity"/>への変換には、<see cref="EntityUtils.Create{TEntity}(DataRow)"/>が使用されます。</remarks>
        /// <returns>
        /// <see cref="IEnumerable&lt;out T&gt;" />オブジェクトを返します。ここで、ジェネリックパラメータ T は<typeparamref name="TEntity"/>です。
        /// </returns>
        public static IEnumerable<TEntity> AsEnumerable<TEntity>(this DataTable source)
            where TEntity : AbstractEntity, new()
            => source.AsEnumerable().Select(dataRow => EntityUtils.Create<TEntity>(dataRow));

    }

}
