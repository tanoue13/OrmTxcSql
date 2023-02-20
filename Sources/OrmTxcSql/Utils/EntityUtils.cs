using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using OrmTxcSql.Attributes;
using OrmTxcSql.Entities;

namespace OrmTxcSql.Utils
{
    /// <summary>
    /// Entityに関するユーティリティクラス。（AbstractEntityの拡張メソッドを含む）
    /// </summary>
    public static class EntityUtils
    {
        /// <summary>
        /// テーブル名を取得する。
        /// </summary>
        /// <param name="abstractEntity"></param>
        /// <returns></returns>
        public static string GetTableName(this AbstractEntity abstractEntity)
            => EntityUtils.GetTableName(abstractEntity.GetType());
        /// <summary>
        /// テーブル名を取得する。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static string GetTableName<TEntity>() where TEntity : AbstractEntity
            => EntityUtils.GetTableName(typeof(TEntity));
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
            => EntityUtils.GetColumnAttributes(abstractEntity.GetType());
        /// <summary>
        /// カラム属性を取得する。
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetColumnAttributes<TEntity>() where TEntity : AbstractEntity
            => EntityUtils.GetColumnAttributes(typeof(TEntity));
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
            => EntityUtils.GetPrimaryKeyAttributes(abstractEntity.GetType());
        /// <summary>
        /// 主キー属性を取得する。
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetPrimaryKeyAttributes<TEntity>() where TEntity : AbstractEntity
            => EntityUtils.GetPrimaryKeyAttributes(typeof(TEntity));
        /// <summary>
        /// 主キー属性を取得する。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static IEnumerable<PropertyInfo> GetPrimaryKeyAttributes(Type type)
        {
            // 主キー属性を取得する。
            IEnumerable<PropertyInfo> attributes = EntityUtils.GetColumnAttributes(type)
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
    }
}
