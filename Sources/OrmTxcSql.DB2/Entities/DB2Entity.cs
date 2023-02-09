using OrmTxcSql.Entities;

namespace OrmTxcSql.DB2.Entities
{
    /// <summary>
    /// DB2用のエンティティクラス。
    /// </summary>
    public abstract class DB2Entity : AbstractEntity
    {

        /// <summary>
        /// テーブル内の相対行番号（Relative Record Number）を取得する際のカラム名です。
        /// </summary>
        internal static string RRNColumnName
        {
            get => DB2Entity.s_rrnColumnName;
        }
        private readonly static string s_rrnColumnName = "relative_record_number";

        /// <summary>
        /// テーブル内の相対行番号（Relative Record Number）を取得または設定します。
        /// </summary>
        public int? RelativeRecordNumber
        {
            get => this._relativeRecordNumber;
            set => this._relativeRecordNumber = value;
        }
        private int? _relativeRecordNumber;

    }
}
