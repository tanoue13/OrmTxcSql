using System;

namespace OrmTxcSql.Attributes
{

    /// <summary>
    /// UID（unique identifier）属性
    /// </summary>
    /// <remarks>
    /// 値の設定（採番を含む）はDBMS側の機能で実施し、プログラム側では参照のみのカラムに設定する。
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class UIDAttribute : Attribute
    {
        /// <summary>
        /// コンストラクタ.
        /// </summary>
        public UIDAttribute()
        {
        }
    }

}
