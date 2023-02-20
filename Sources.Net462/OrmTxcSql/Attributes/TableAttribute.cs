using System;

namespace OrmTxcSql.Attributes
{

    /// <summary>
    /// テーブル属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TableAttribute : Attribute
    {

        /// <summary>
        /// テーブル名を取得します。
        /// </summary>
        public string TableName { get; private set; }

        /// <summary>
        /// コンストラクタ.
        /// </summary>
        /// <param name="name"></param>
        public TableAttribute(string name)
        {
            this.TableName = name;
        }

    }

}
