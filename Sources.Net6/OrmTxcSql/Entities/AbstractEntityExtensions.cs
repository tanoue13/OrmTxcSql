using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using OrmTxcSql.Attributes;

namespace OrmTxcSql.Entities
{
    /// <summary>
    /// AbstractEntityの拡張メソッドを定義します。（静的クラス）
    /// </summary>
    public static class AbstractEntityExtensions
    {
        /// <summary>
        /// テーブル名を取得する。
        /// </summary>
        /// <param name="abstractEntity"></param>
        /// <returns></returns>
        public static string GetTableName(this AbstractEntity abstractEntity)
            => AbstractEntityExtensions.GetTableName(abstractEntity.GetType());
        /// <summary>
        /// テーブル名を取得する。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static string GetTableName<TEntity>() where TEntity : AbstractEntity
            => AbstractEntityExtensions.GetTableName(typeof(TEntity));
        /// <summary>
        /// テーブル名を取得する。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string GetTableName(Type type)
        {
            // テーブル名を取得する。
#if NET6_0_OR_GREATER
#nullable disable
#endif
            string tableName = type.GetCustomAttribute<TableAttribute>(false).TableName;
#if NET6_0_OR_GREATER
#nullable restore
#endif
            // 結果を戻す。
            return tableName;
        }

        /// <summary>
        /// カラム属性を取得する。
        /// </summary>
        /// <param name="abstractEntity"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetColumnAttributes(this AbstractEntity abstractEntity)
            => AbstractEntityExtensions.GetColumnAttributes(abstractEntity.GetType());
        /// <summary>
        /// カラム属性を取得する。
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetColumnAttributes<TEntity>() where TEntity : AbstractEntity
            => AbstractEntityExtensions.GetColumnAttributes(typeof(TEntity));
        /// <summary>
        /// カラム属性を取得する。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static IEnumerable<PropertyInfo> GetColumnAttributes(Type type)
        {
            // カラム属性を取得する。
            IEnumerable<PropertyInfo> attributes = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(prop => null != prop.GetCustomAttribute<ColumnAttribute>(false));
            // 結果を戻す。
            return attributes;
        }

        /// <summary>
        /// 主キー属性を取得する。
        /// </summary>
        /// <param name="abstractEntity"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetPrimaryKeyAttributes(this AbstractEntity abstractEntity)
            => AbstractEntityExtensions.GetPrimaryKeyAttributes(abstractEntity.GetType());
        /// <summary>
        /// 主キー属性を取得する。
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetPrimaryKeyAttributes<TEntity>() where TEntity : AbstractEntity
            => AbstractEntityExtensions.GetPrimaryKeyAttributes(typeof(TEntity));
        /// <summary>
        /// 主キー属性を取得する。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static IEnumerable<PropertyInfo> GetPrimaryKeyAttributes(Type type)
        {
            // 主キー属性を取得する。
            IEnumerable<PropertyInfo> attributes = AbstractEntityExtensions.GetColumnAttributes(type)
                .Where(prop => null != prop.GetCustomAttribute<PrimaryKeyAttribute>(false));
            // 結果を戻す。
            return attributes;
        }

        /// <summary>
        /// <paramref name="dataRow"/>の値を使用してEntityを生成する。
        /// </summary>
        /// <typeparam name="TEntity">生成するEntityの型</typeparam>
        /// <param name="dataRow">Entityに設定する値</param>
        /// <returns></returns>
        public static TEntity Create<TEntity>(DataRow dataRow) where TEntity : AbstractEntity, new()
        {
            // インスタンスを生成する。
            TEntity entity = new TEntity();
            // dataRowから値を取得し、エンティティに設定する。
            entity.SetProperties(dataRow);
            // 結果を戻す。
            return entity;
        }


        /// <summary>
        /// 等しい属性値を持つかどうかを戻す。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        /// <remarks><see cref="AbstractEntityExtensions.GetColumnAttributes{TEntity}"/>で取得されるすべてのプロパティを比較します。</remarks>
        public static bool HasEqualPropertyValues<TEntity>(this TEntity obj1, TEntity obj2)
            where TEntity : AbstractEntity
            => HasEqualPropertyValues(obj1, obj2, AbstractEntityExtensions.GetColumnAttributes<TEntity>().ToArray());
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
#if NET6_0_OR_GREATER
            IEnumerable<object?> values1 = properties.Select(prop => prop.GetValue(obj1));
            IEnumerable<object?> values2 = properties.Select(prop => prop.GetValue(obj2));
#else
            IEnumerable<object> values1 = properties.Select(prop => prop.GetValue(obj1));
            IEnumerable<object> values2 = properties.Select(prop => prop.GetValue(obj2));
#endif
            // 比較する。
            bool equivalent = Enumerable.SequenceEqual(values1, values2, new AttributeEqualityComparer());
            // 結果を戻す。
            return equivalent;
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// 属性の等価比較を行う IEqualityComparer 。
        /// </summary>
        private class AttributeEqualityComparer : IEqualityComparer<object?>
        {
            bool IEqualityComparer<object?>.Equals(object? x, object? y)
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
#if NET6_0_OR_GREATER
#nullable disable
#endif
                    return x.Equals(y);
#if NET6_0_OR_GREATER
#nullable restore
#endif
                }
            }
            int IEqualityComparer<object?>.GetHashCode(object obj)
            {
                return obj.GetHashCode();
            }
        }
#else
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
#endif

    }
}
