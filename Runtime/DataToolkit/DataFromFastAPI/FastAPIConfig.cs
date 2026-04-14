
using UnityEngine;
using Cameo;
using System.Threading.Tasks;
using LitJson;
using System.Collections;
namespace Cameo
{
    /// <summary>
    // 設定FastAPI所有ＵＲＬ與funtion name, parameter name
    /// 依據目前運行的url來判斷要使用哪一個api domain,
    /// 藉此區分測試版本與正式版本兩版本不同的api
    /// </summary>
    public class FastAPIConfig : IConfigLoaderWithParams
    {
        [System.Serializable]
        public class UrlDef
        {
            public string APIDomain;  //FastApi 服務主機url
            public string GameDataUrl; //可替換遊戲資料url
            public string LoginUrl; //使用者登入url模組
            public string FileIndex;//所有檔案索引url
            public string GetkeyUrl;//雲端檔案讀取api url   
            public string SetkeyUrl;//雲端檔案寫入api url
        }
        private const string DefaultUrlKey = "Default";

        private const string UrlMapKey = "UrlMap";
        private UrlDef urlDefine = null;
        private string ClientUrl;
        private UrlDef parser(JsonData configJson)
        {
            UrlDef result = new UrlDef();
            if (configJson != null)
            {
                if (configJson.ContainsKey(UrlMapKey))
                {
                    JsonData urlDefJson = configJson[UrlMapKey][DefaultUrlKey];

                    result = JsonMapper.ToObject<UrlDef>(urlDefJson.ToJson());

                    foreach (string urlKey in configJson[UrlMapKey].Keys)
                    {
                        if (ClientUrl.StartsWith(urlKey))
                        {
                            urlDefJson = configJson[UrlMapKey][urlKey];

                            result = JsonMapper.ToObject<UrlDef>(urlDefJson.ToJson());

                            Debug.Log("url key: " + urlKey);

                            break;
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Load Config file failed!");
            }
            return result;
        }
        public IEnumerator InitializeCoroutine(string clientUrl)
        {
            yield return LoadWithParams(clientUrl).AsIEnumerator();
        }

        public async override Task LoadWithParams(object[] info)
        {
            ClientUrl = (string)info[0];
            urlDefine = await CongigTool.LoadConfig<UrlDef>(typeof(UrlDef).Name.ToString(), parser);
            FastAPISettings.BaseAPIUrl = urlDefine.APIDomain;
            FastAPISettings.LoginPageUrl = urlDefine.LoginUrl;
            FastAPISettings.DataIndexSpreadSheet = urlDefine.FileIndex;
            FastAPISettings.key_value_get = urlDefine.GetkeyUrl;
            FastAPISettings.key_value_set = urlDefine.SetkeyUrl;
            FastAPISettings.GameDataUrl = urlDefine.GameDataUrl;
            WebGLPageApiBaseBridge.Publish(FastAPISettings.BaseAPIUrl);
        }
    }

    public static class FastAPISettings
    {  //Api domain
        public static string BaseAPIUrl = "https://falra-band.bowenchiu.repl.co";
        public static string LocalAPIUrl = "http://127.0.0.1:8000";
        //引導玩家登入的Url
        public static string LoginPageUrl = "https://adl.edu.tw/HomePage/login/?sys=planting";
        public static string GameDataUrl = "";

        //fapi
        public static string BaseFapi { get { return BaseAPIUrl + "/fapi/"; } }

        //登入的Url(正式)
        public const string AccountKey = "str_user";
        public const string TokenKey = "str_token";
        public const string FileKey = "str_file";
        public const string ContentKey = "str_content";
        public const string TableKey = "str_table";
        public const string LogKey = "str_log";
        public const string ReadMessageListKey = "lst_message_id_set_true";
        public const string UnreadMessageListKey = "lst_message_id_set_false";

        //檔案索引檔的下載設定
        public static string DataIndexSpreadSheet = "FileIndex";
        public const string DataIndexWorkSheet = "Index";
        public const string DataIndexWorkSheetDevelop = "IndexDevelop";
        public const int DataIndexStartRow = 0;
        //排行榜key
        public const string GameName = "str_game_name";
        public const string RankFile =  "str_rank_file";
        public const string RankScore=  "int_score";
        //下載排行榜資料
        public static string GetRankUrl { get { return BaseAPIUrl + "/rank/get_rank"; } }
        public static string SetRankUrl { get { return BaseAPIUrl + "/rank/set_rank/"; } }
        //下載遊戲資料回傳List of string格式
        public static string BaseListUrl { get { return BaseAPIUrl + "/sheet/get_all_values"; } }

        //下載遊戲資料的Url(from google sheet)
        public static string BaseDataUrl { get { return BaseAPIUrl + "/sheet/get_after_2_rows_v2"; } }
        public const string SpreadSheetKey = "str_spreadsheet";
        public const string WorkSheetKey = "str_worksheet";

        //下載Message
        public static string SetMessageReadUrl { get { return BaseAPIUrl + "/message/set_messages_read/"; } }

        //Log url
        public static string LogUploadUrl { get { return BaseAPIUrl + "/log/web_log/"; } }
        public static string GetRequestUrl { get { return BaseAPIUrl + "/key_value/get"; } }

        //上傳玩家資料的Url
        public static string SetRequestUrl { get { return BaseAPIUrl + "/key_value/set/"; } }
        public static string UploadFileUrl { get { return BaseAPIUrl + "/upload/upload/?str_directory="; } }
        public const string UploadFileKey = "lst_files";
        public static string key_value_get = "/key_value/get";
        public static string key_value_set = "/key_value/set";
    }

}
