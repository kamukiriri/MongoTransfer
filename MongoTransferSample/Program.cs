using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Driver;
using MongoDB.Bson;

using MongoUtil;

namespace MongoTransferSample
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("処理開始");

                //MongoDBのサーバーオブジェクトを事前に用意します
                MongoServer server = MongoServer.Create("mongodb://localhost");
                
                //MongoDBのデータベースオブジェクトを渡して、データ転送クラスのインスタンスを作成します
                MongoTransfer transfer = new MongoTransfer(server.GetDatabase("Test"));

                //CSVファイルのパスと対応するクラスのTypeを指定してデータを取り込みます
                string path = @".\ImportSample.csv";
                CsvImportResult result = transfer.ImportCsv(path, typeof(Document));
                
                //ログを出力します
                //処理結果には成功件数と取り込みに失敗した行の情報が保存されています
                WriteLog(result);

                Console.WriteLine("処理終了");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.ReadKey();
        }

        /// <summary>
        /// ログ出力
        /// </summary>
        /// <param name="result"></param>
        static void WriteLog(CsvImportResult result)
        {
            var log = new StringBuilder();
            if (result.Successed == 0)
            {
                log.AppendLine("全件失敗");
            }
            else
            {
                log.Append("成功：").Append(result.Successed);
                log.AppendLine();
            }
            if (result.FailedInfo.Length > 0)
            {
                log.Append("失敗：").Append(result.FailedInfo.Length);
                log.AppendLine();
                foreach (var failed in result.FailedInfo)
                {
                    log.Append(failed.LineNo).Append("行目：").Append(failed.Message);
                    log.AppendLine();
                }
            }
            Console.WriteLine(log);
        }
    }

    /// <summary>
    /// サンプルクラス
    /// </summary>
    public class Document
    {
        public ObjectId _id
        {
            get;
            set;
        }

        public int No
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public DateTime EntryDate
        {
            get;
            set;
        }
    }
}
