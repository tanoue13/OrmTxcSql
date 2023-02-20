using System;

namespace OrmTxcSql.Attributes
{
    /// <summary>
    /// カラム属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        /// カラム名を取得します。
        /// </summary>
        public string ColumnName { get; private set; }
        /// <summary>
        /// コンストラクタ.
        /// </summary>
        /// <param name="name">カラム名</param>
        public ColumnAttribute(string name)
        {
            this.ColumnName = name;
        }
    }
}
