using System.Collections.Generic;
using System.Data;
using System.Linq;
using OrmTxcSql.Entities;

namespace OrmTxcSql.Utils
{
    /// <summary>
    /// <see cref="DataTable"/>クラスの拡張メソッドを定義します。<see cref="DataTableExtensions">OrmTxcSql.Utils.DataTableExtensions</see>は静的クラスです。
    /// </summary>
    public static class DataTableExtensions
    {
        /// <summary>
        /// <see cref="IEnumerable{T}"/>オブジェクトを返します。ここで、ジェネリックパラメータ T は<typeparamref name="TEntity"/>です。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <returns>
        /// <see cref="IEnumerable{T}"/>オブジェクトを返します。ここで、ジェネリックパラメータ T は<typeparamref name="TEntity"/>です。
        /// </returns>
        /// <remarks>
        /// <para>
        /// <see cref="DataRow"/>から<typeparamref name="TEntity"/>への変換には、<see cref="EntityUtils.Create{TEntity}(DataRow)"/>が使用されます。
        /// </para>
        /// <para>
        /// Excuse（言い訳）：<br/>
        /// 本来は、OrmTxcSql.Extensions名前空間にOrmTxcSql.Extensions.DataTableExtensions.AsEnumerable&lt;TEntity&gt;(DataTable)として定義したかった。<br/>
        /// しかし、通常はOrmTxcSql.Extensions名前空間のusingが存在しないため、コードにAsEnumerable&lt;TEntity&gt;(DataTable)を記述すると<br/>
        /// <see cref="System.Data"/>名前空間に定義されている<see cref="System.Data.TypedTableBaseExtensions.AsEnumerable{TRow}(TypedTableBase{TRow})"/>と解釈され、<br/>
        /// コンパイルエラーとなってしまう。（&lt;TRow&gt;の部分が定義に合わないことが原因と考えられる。）<br/>
        /// <see cref="AsEnumerable{TEntity}(DataTable)">AsEnumerable&lt;TEntity&gt;(DataTable)</see>を利用する場面では
        /// <see cref="EntityUtils"/>も利用すると想定されるため、<br/>
        /// <see cref="EntityUtils"/>と同じ<see cref="OrmTxcSql.Utils">OrmTxcSql.Utils</see>名前空間に定義し、コンパイルエラーを回避することとした。
        /// </para>
        /// </remarks>
        public static IEnumerable<TEntity> AsEnumerable<TEntity>(this DataTable source)
            where TEntity : AbstractEntity, new()
            => source.AsEnumerable().Select(dataRow => EntityUtils.Create<TEntity>(dataRow));
    }
}
