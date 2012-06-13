using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoUtil
{
    /// <summary>
    /// 処理結果
    /// </summary>
    public class Result<TFailed>
    {
        private readonly int _successed;
        /// <summary>
        /// 成功した処理の総数
        /// </summary>
        public int Successed
        {
            get
            {
                return _successed;
            }
        }

        private readonly TFailed[] _failedInfo;
        /// <summary>
        /// 失敗した処理の情報
        /// </summary>
        public TFailed[] FailedInfo
        {
            get
            {
                return (TFailed[])_failedInfo.Clone();
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name='successd'>
        /// 成功した処理の総数
        /// </param>
        /// <param name='failedInfo'>
        /// 失敗した処理の情報
        /// </param>
        public Result(int successd, TFailed[] failedInfo)
        {
            _successed = successd;
            _failedInfo = (TFailed[])failedInfo.Clone();
        }
    }

    /// <summary>
    /// 処理失敗行の情報
    /// </summary>
    public class FailedLineInfo
    {
        private readonly int _lineNo;
        /// <summary>
        /// 行番号
        /// </summary>
        public int LineNo
        {
            get
            {
                return _lineNo;
            }
        }

        private readonly string _message;
        /// <summary>
        /// メッセージ
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name='lineNo'>
        /// 行番号
        /// </param>
        /// <param name='data'>
        /// 元データ
        /// </param>
        /// <param name='message'>
        /// メッセージ
        /// </param>
        public FailedLineInfo(int lineNo, string message)
        {
            _lineNo = lineNo;
            _message = message;
        }
    }

    /// <summary>
    /// CSVインポート結果
    /// </summary>
    public class CsvImportResult : Result<FailedLineInfo>
    {
        public CsvImportResult(int successd, FailedLineInfo[] failedInfo)
            : base(successd, failedInfo)
        {
        }
    }
}
