namespace OrmTxcSql.Data
{

    /// <summary>
    /// プロパティの接続文字列を戻すデータソース。
    /// </summary>
    public class ConnectionStringDataSource : DataSource
    {

        /// <summary>
        /// 接続文字列を取得または設定します。
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 接続文字列を戻す。
        /// </summary>
        /// <returns></returns>
        public override string GetConnectionString() => this.ConnectionString;

    }

}
