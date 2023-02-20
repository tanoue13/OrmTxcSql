using System;

namespace OrmTxcSql.Attributes
{
    /// <summary>
    /// 主キー属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PrimaryKeyAttribute : Attribute
    {
        /// <summary>
        /// コンストラクタ.
        /// </summary>
        public PrimaryKeyAttribute()
        {
        }
    }
}
