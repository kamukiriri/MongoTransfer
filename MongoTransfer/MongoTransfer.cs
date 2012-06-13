using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualBasic.FileIO;

using MongoDB.Driver;
using MongoDB.Bson;

namespace MongoUtil
{
    /// <summary>
    /// MongoDBデータ転送クラス
    /// </summary>
    public class MongoTransfer
    {
        /// <summary>
        /// 操作対象のMongoDBデータベース
        /// </summary>
        public MongoDatabase DataBase {
            get;
            private set;
        }

        /// <summary>
        /// MongoTransferのインスタンスを初期化します
        /// </summary>
        /// <param name="db"></param>
        public MongoTransfer(MongoDatabase db)
        {
            DataBase = db;
        }

        /// <summary>
        /// CSVファイルからインポートする
        /// </summary>
        /// <returns>
        /// 処理結果
        /// </returns>
        /// <param name='csvPath'>
        /// CSVファイルのパス
        /// </param>
		/// <para name='documentType'>
		/// ドキュメントに対応するクラス
		/// </param>
        /// </typeparam>
        public CsvImportResult ImportCsv(string csvPath, Type documentType)
        {		
            //ドキュメントオブジェクトのプロパティに値をセットするメソッドを作成
            var setterList = CreateSetter(documentType);
            if (setterList.Count == 0)
            {
                return new CsvImportResult(0, new []{new FailedLineInfo(-1, "指定された型はドキュメントとして使用できません")});
            }

            //コレクション取得
            var collection = DataBase.GetCollection(documentType.Name);

            //読み込み結果保存用
            var successed = 0;
            var failedList = new List<FailedLineInfo>();
			
			//CSV読み込み
			string[] columnNames = null; 
			var parser = new TextFieldParser(csvPath);
			parser.Delimiters = new string[]{","};
			parser.TrimWhiteSpace = false;
			
			//ヘッダ(フィールド名)行取得
			if(!parser.EndOfData)
			{
				columnNames = parser.ReadFields();				
			}
			
			//データ行取得
			while(!parser.EndOfData)
			{
				try
				{
					//ドキュメントオブジェクトを作成し、コレクションにインサートする
					var fields = parser.ReadFields();
					var document = Activator.CreateInstance(documentType);
					
					for(var index = 0; index < fields.Length; index++)
					{
						//プロパティに値をセット
						string column = columnNames[index];
						string value = fields[index];
						
						if (setterList.ContainsKey(column))
						{
							setterList[column](document, value);
						}
					}
					
					collection.Insert(documentType, document);
					
					successed++;
				}
				catch(Exception ex)
				{
                    failedList.Add(new FailedLineInfo((int)parser.ErrorLineNumber, ex.GetType().Name + ":" + ex.Message));
				}
			}
			
			//結果オブジェクト作成
            var result = new CsvImportResult(successed, failedList.ToArray());
            return result;
        }

        /// <summary>
        /// 指定されたTypeのインスタンスのプロパティに値をセットするメソッドを作成する
        /// </summary>
        /// <param name="type">対象のType</param>
        /// <returns>セッターメソッドのディクショナリ</returns>
        private static Dictionary<string, Action<dynamic, string>> CreateSetter(Type type)
        {
            var setterList = new Dictionary<string, Action<dynamic, string>>();

            //型検証
            if (type.IsAbstract
                || type.GetConstructor(Type.EmptyTypes) == null)
            {
                return setterList;
            }

            //プロパティのSetterを取得する
            PropertyInfo[] properties = type.GetProperties();
            var propQuery = from prop in properties
                            let setter = prop.GetSetMethod()
                            where prop.CanWrite
                                    && prop.PropertyType != typeof(ObjectId)
                                    && setter.GetParameters().Length == 1
                            select new { Name = prop.Name, Setter = setter };

            foreach (var prop in propQuery)
            {
                //Setterメソッドのキャッシュを作成する
                var paramType = prop.Setter.GetParameters()[0].ParameterType;
                var setterDelegateType = typeof(Action<,>).MakeGenericType(type, paramType);
                //Delegate型を使用してもDelegate.DynamicInvokeの実行に失敗する為、dynamicを使用する
                dynamic setter = Delegate.CreateDelegate(setterDelegateType, prop.Setter);

                Action<dynamic, string> setValue = (obj, valueText) =>
                {
                    dynamic value = Convert.ChangeType(valueText, paramType);
                    setter.Invoke(obj, value);
                };

                setterList.Add(prop.Name, setValue);
            }
            return setterList;
        }

    }


}

