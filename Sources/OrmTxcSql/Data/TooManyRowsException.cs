using System;
using System.Data.Common;
using System.Runtime.Serialization;

namespace OrmTxcSql.Data
{
    /// <summary>
    /// 単一レコードしか許されない部分で複数レコードが戻されている場合に投げられる例外クラス。
    /// </summary>
    /// <see href="https://www.shift-the-oracle.com/plsql/exception/predefined-exception.html"/>
    /// <see href="https://www.ibm.com/support/knowledgecenter/ja/SSEPGG_9.7.0/com.ibm.db2.luw.apdv.plsql.doc/doc/c0053876.html"/>
    public class TooManyRowsException : DbException
    {
        /// <summary>
        /// コンストラクタ.
        /// </summary>
        public TooManyRowsException() : base()
        {
        }
        /// <summary>
        /// コンストラクタ.
        /// </summary>
        /// <param name="message"></param>
        public TooManyRowsException(string message) : base(message)
        {
        }
        /// <summary>
        /// コンストラクタ.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public TooManyRowsException(string message, Exception innerException) : base(message, innerException)
        {
        }
        /// <summary>
        /// コンストラクタ.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
#if NET8_0_OR_GREATER
        [Obsolete]
#endif
        public TooManyRowsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
        /// <summary>
        /// コンストラクタ.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="errorCode"></param>
        public TooManyRowsException(string message, int errorCode) : base(message, errorCode)
        {
        }
    }
}
