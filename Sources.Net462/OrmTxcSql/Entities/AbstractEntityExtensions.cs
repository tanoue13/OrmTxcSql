using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OrmTxcSql.Utils;

namespace OrmTxcSql.Entities
{

    /// <summary>
    /// AbstractEntityの拡張メソッドを定義します。（静的クラス）
    /// </summary>
    public static class AbstractEntityExtensions
    {
        /// <summary>
        /// 等しい属性値を持つかどうかを戻す。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        /// <remarks><see cref="EntityUtils.GetColumnAttributes{TEntity}"/>で取得されるすべてのプロパティを比較します。</remarks>
        public static bool HasEqualPropertyValues<TEntity>(this TEntity obj1, TEntity obj2)
            where TEntity : AbstractEntity
            => HasEqualPropertyValues(obj1, obj2, EntityUtils.GetColumnAttributes<TEntity>().ToArray());
        /// <summary>
        /// 等しい属性値を持つかどうかを戻す。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <param name="properties">比較するプロパティ</param>
        /// <returns></returns>
        /// <remarks><paramref name="properties"/>で与えられたプロパティを比較します。</remarks>
        public static bool HasEqualPropertyValues<TEntity>(this TEntity obj1, TEntity obj2, IEnumerable<PropertyInfo> properties)
            where TEntity : AbstractEntity
        {
            // 比較対象の属性値を取得する。
            IEnumerable<object> values1 = properties.Select(prop => prop.GetValue(obj1));
            IEnumerable<object> values2 = properties.Select(prop => prop.GetValue(obj2));
            // 比較する。
            bool equivalent = Enumerable.SequenceEqual(values1, values2, new AttributeEqualityComparer());
            // 結果を戻す。
            return equivalent;
        }

        /// <summary>
        /// 属性の等価比較を行う IEqualityComparer 。
        /// </summary>
        private class AttributeEqualityComparer : IEqualityComparer<object>
        {
            bool IEqualityComparer<object>.Equals(object x, object y)
            {
                if ((null == x) && (null == y))
                {
                    // どちらも null の場合、「等価」と判定する。
                    return true;
                }
                else if ((null == x) && (null != y))
                {
                    // 「等価」でないと判定する。
                    return false;
                }
                else if ((null != x) && (null == y))
                {
                    // 「等価」でないと判定する。
                    return false;
                }
                else
                {
                    // どちらも null でない場合、比較結果を戻す。
                    return x.Equals(y);
                }
            }
            int IEqualityComparer<object>.GetHashCode(object obj)
            {
                return obj.GetHashCode();
            }
        }
    }

}
