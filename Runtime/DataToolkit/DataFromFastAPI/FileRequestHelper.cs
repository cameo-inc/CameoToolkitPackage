using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using LitJson;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Net;
#if UNITY_EDITOR
using System.Net.Http;
#endif
namespace Cameo
{
    public class FileRequestHelper : Singleton<FileRequestHelper>
    {
        /// <summary>
        /// 統一記錄請求錯誤，404 時明確顯示哪個 URL 回傳 404
        /// </summary>
        private static void LogRequestError(UnityWebRequest www, string url = null)
        {
            string targetUrl = !string.IsNullOrEmpty(url) ? url : www?.url ?? "";
            long responseCode = www?.responseCode ?? 0;
            if (responseCode == 404)
            {
                Debug.LogError($"404 Not Found: {targetUrl}");
            }
            else if (responseCode > 0)
            {
                Debug.LogError($"Request failed [HTTP {responseCode}]: {targetUrl}\n{www?.error}");
            }
            else
            {
                Debug.LogError($"Request failed: {targetUrl}\n{www?.error}");
            }
        }

        public async Task<T> LoadJson<T>(string url) where T : class
        {
            //Debug.Log("LoadJson: " + url);

            UnityWebRequest www = new UnityWebRequest(url);
#if UNITY_EDITOR
var cert = new ForceAcceptAll();
www.certificateHandler = cert;
#endif
            www.downloadHandler = new DownloadHandlerBuffer();
            www.disposeUploadHandlerOnDispose = true;
            www.disposeDownloadHandlerOnDispose = true;
            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                LogRequestError(www, url);
                www.Dispose();
                return null;
            }
            else
            {
                var data = www.downloadHandler.text;
                www.Dispose();
                try
                {
                    return JsonConvert.DeserializeObject<T>(data); //JsonMapper.ToObject<T>(data);
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Load {0} error: {1},{2}", url, data, e);

                    return null;
                }
            }

        }

        public async Task<T> LoadJson<T>(string url, Func<JsonData, T> parser) where T : class
        {

            //Debug.Log("LoadJson with parser: " + url);

            UnityWebRequest www = new UnityWebRequest(url);
#if UNITY_EDITOR
var cert = new ForceAcceptAll();
www.certificateHandler = cert;
#endif
            www.downloadHandler = new DownloadHandlerBuffer();
            www.disposeUploadHandlerOnDispose = true;
            www.disposeDownloadHandlerOnDispose = true;
            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                LogRequestError(www, url);
                www.Dispose();
                return null;
            }
            else
            {
                //Debug.Log("size: " + (www.downloadHandler.data.Length / 1000).ToString() + "kb");
                //Debug.Log(www.downloadHandler.text);
                var data = www.downloadHandler.text;
                www.Dispose();
                if (data.Contains("No such")){
                    return null;
                }
                return parser(JsonMapper.ToObject(data));
            }
        }

        public async Task<string> LoadJsonString(string url)
        {
#if UNITY_EDITOR
            if (url.Contains("127.0.0.1") || url.Contains("localhost"))
                return await LoadJsonStringLocal(url);
#endif
            //   Debug.Log("01 LoadJsonString create request");
            UnityWebRequest www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
#if UNITY_EDITOR
var cert = new ForceAcceptAll();
www.certificateHandler = cert;
#endif
           // Debug.Log("02 LoadJsonString: created ");
            www.downloadHandler = new DownloadHandlerBuffer();
          //  Debug.Log("03 LoadJsonString: created buffer ");
            www.disposeUploadHandlerOnDispose = true;
            www.disposeDownloadHandlerOnDispose = true;
          //  Debug.Log("04 LoadJsonString: ");
            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                LogRequestError(www, url);
                www.Dispose();
                return null;
            }
            else
            {
                //Debug.Log("size: " + (www.downloadHandler.data.Length / 1000).ToString() + "kb");
            //     Debug.Log("下載成功" + url);
                var data = www.downloadHandler.text;
                www.Dispose();
                return data;
            }
        }
#if UNITY_EDITOR
        public async Task<string> LoadJsonStringLocal(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();
                        Debug.Log("Response: " + responseData);
                        return responseData;
                    }
                    else
                    {
                        Debug.LogError($"{(int)response.StatusCode} {response.StatusCode}: {url}");
                    }
                }
                catch (HttpRequestException e)
                {
                    Debug.Log("Request error: " + e.Message);
                }
            }
            return null;
        }
#endif

        public static string param_to_url(string key, string value)
        {
            return string.Format("&{0}={1}", key, value);
        }
        //多用：通用的基本的get api, API="/xx/xx/", str_param="&a=1&b=2"
        public async Task<string> FalraGetAPI(string API, string  user, string token,string str_param)
        {
            //Debug.Log("request api : "+FastAPISettings.BaseAPIUrl+API);
            string url = string.Format("{0}/?{1}={2}&{3}={4}{5}", FastAPISettings.BaseAPIUrl+API,
                FastAPISettings.AccountKey, user,
                FastAPISettings.TokenKey, token,
                str_param);
            Debug.Log("api url:"+url);
            string result = await LoadJsonString(url);
            return result;
           
        }
        
        public async Task InvokeAPI(string url)
        {
            //Debug.Log(url);

            UnityWebRequest www = new UnityWebRequest(url);
            #if UNITY_EDITOR
var cert = new ForceAcceptAll();
www.certificateHandler = cert;
#endif
            www.disposeUploadHandlerOnDispose = true;
            www.disposeDownloadHandlerOnDispose = true;
            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                LogRequestError(www, url);
            }
            www.Dispose();
        }

        public async Task<RequestResult> InvokeAPI<T>(string url, Func<JsonData, T> parser) where T : class
        {
            //Debug.Log(url);

            UnityWebRequest www = new UnityWebRequest(url);
#if UNITY_EDITOR
var cert = new ForceAcceptAll();
www.certificateHandler = cert;
#endif
            www.downloadHandler = new DownloadHandlerBuffer();
            www.disposeUploadHandlerOnDispose = true;
            www.disposeDownloadHandlerOnDispose = true;
            await www.SendWebRequest();

            RequestResult result = new RequestResult();

            if (www.result != UnityWebRequest.Result.Success)
            {
                result.Content = null;
                result.ErrorMsg = www.error;
                LogRequestError(www, url);
            }
            else
            {
                result.Content = parser(JsonMapper.ToObject(www.downloadHandler.text));

                result.ErrorMsg = "";
            }
            www.Dispose();
            return result;
        }

        //以JsonObject Array方式載入sheet資料，(回傳資料包含Key名稱)
        public async Task<T[]> LoadArray<T>(string spreadSheet, string workSheet, int index, Func<JsonData, T> parser, string userAccount, string token) where T : class
        {
            T[] returnArray = null;

            string url = string.Format("{0}/?{1}={2}&{3}={4}&{5}={6}.sheet&{7}={8}", FastAPISettings.BaseDataUrl,
                FastAPISettings.AccountKey, userAccount,
                FastAPISettings.TokenKey, token,
                FastAPISettings.SpreadSheetKey, spreadSheet,
                FastAPISettings.WorkSheetKey, workSheet);

            UnityWebRequest www = new UnityWebRequest(url);
#if UNITY_EDITOR
var cert = new ForceAcceptAll();
www.certificateHandler = cert;
#endif
            www.downloadHandler = new DownloadHandlerBuffer();
            www.disposeUploadHandlerOnDispose = true;
            www.disposeDownloadHandlerOnDispose = true;
            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                LogRequestError(www, url);
                Debug.Log("可能是user id與user token驗證失敗");
            }
            else
            {
                //Debug.Log("size: " + (www.downloadHandler.data.Length / 1000).ToString() + "kb");
try{
                JsonData jsonData = JsonMapper.ToObject(www.downloadHandler.text);

                returnArray = new T[jsonData.Count - index];

                for (int i = index; i < jsonData.Count; i++)
                {
                    T obj = parser(jsonData[i]);
                    returnArray[i - index] = obj;
                }
}
catch(Exception e){
    Debug.Log("下載資料錯誤error:"+e);
    Debug.Log("url:"+url);
    Debug.Log("data:"+www.downloadHandler.text);
}
            }
            www.Dispose();
            return returnArray;
        }
        //未來盡量都用這個，sheet會從第三行開始讀取
        public async Task<List<T>> LoadSheetAsList<T>(string spreadSheet, string workSheet, string UserAccount, string Token) where T : class
        {
            List<T> dataArray = new List<T>();
            string url = string.Format("{0}/?{1}={2}&{3}={4}&{5}={6}.sheet&{7}={8}", FastAPISettings.BaseDataUrl,
                FastAPISettings.AccountKey, UserAccount,
                FastAPISettings.TokenKey, Token,
                FastAPISettings.SpreadSheetKey, spreadSheet,
                FastAPISettings.WorkSheetKey, workSheet);

            UnityWebRequest www = new UnityWebRequest(url);
#if UNITY_EDITOR
var cert = new ForceAcceptAll();
www.certificateHandler = cert;
#endif
            www.downloadHandler = new DownloadHandlerBuffer();
            www.disposeUploadHandlerOnDispose = true;
            www.disposeDownloadHandlerOnDispose = true;
            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                LogRequestError(www, url);
            }
            else
            { 
                dataArray = JsonConvert.DeserializeObject<List<T>>(www.downloadHandler.text);// JsonMapper.ToObject<string[][]>(www.downloadHandler.text);
               
                GC.Collect();
            }
            www.Dispose();
            return dataArray;//dataArray;
        }
        //以每row一個array的方式載入(不含Key，較省資源，但是必須知道一個物件的variable來自array第幾個元素)
        public async Task<T[]> LoadArrayFromStringArray<T>(string spreadSheet, string workSheet, int index, Func<string[], T> parser, string UserAccount, string Token) where T : class
        {
            T[] returnArray = null;
            List<T> dataArray = new List<T>();
            string url = string.Format("{0}/?{1}={2}&{3}={4}&{5}={6}.sheet&{7}={8}", FastAPISettings.BaseListUrl,
                FastAPISettings.AccountKey, UserAccount,
                FastAPISettings.TokenKey, Token,
                FastAPISettings.SpreadSheetKey, spreadSheet,
                FastAPISettings.WorkSheetKey, workSheet);

            //  Debug.Log(url);

            UnityWebRequest www = new UnityWebRequest(url);
#if UNITY_EDITOR
var cert = new ForceAcceptAll();
www.certificateHandler = cert;
#endif
            www.downloadHandler = new DownloadHandlerBuffer();
            www.disposeUploadHandlerOnDispose = true;
            www.disposeDownloadHandlerOnDispose = true;
            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                LogRequestError(www, url);
                if (!string.IsNullOrEmpty(www.downloadHandler?.text))
                    Debug.Log("Response: " + www.downloadHandler.text);
            }
            else
            {
                //Debug.Log("size: " + (www.downloadHandler.data.Length / 1000).ToString() + "kb");

                string[][] data = JsonConvert.DeserializeObject<string[][]>(www.downloadHandler.text);// JsonMapper.ToObject<string[][]>(www.downloadHandler.text);

                returnArray = new T[data.Length - index];

                for (int i = index; i < data.Length; i++)
                {
                    try
                    {
                        T obj = parser(data[i]);
                        returnArray[i - index] = obj;
                        dataArray.Add(obj);
                    }
                    catch (System.Exception e)
                    {
                        Debug.Log(url);
                        Debug.Log(www.downloadHandler.text);
                        Debug.LogError("表單格式錯誤：" + e.Message);
                        Debug.LogError("表單格式錯誤");
                        for (int ii = 0; ii < data.Length; ii++)
                        {
                            Debug.Log(data[ii]);
                        }
                    }
                }
                data = null;
                GC.Collect();
            }
            returnArray = dataArray.ToArray();
            www.Dispose();
            return returnArray;
        }

        public async Task<string> UploadGameData(string fileName, object data, string UserAccount, string Token)
        {
            UnityWebRequest www = new UnityWebRequest(FastAPISettings.SetRequestUrl, "POST");
#if UNITY_EDITOR
var cert = new ForceAcceptAll();
www.certificateHandler = cert;
#endif
            Dictionary<string, string> requestBody = new Dictionary<string, string>();
            requestBody[FastAPISettings.AccountKey] = UserAccount;
            requestBody[FastAPISettings.TokenKey] = Token;
            requestBody[FastAPISettings.FileKey] = fileName;
            requestBody[FastAPISettings.ContentKey] = JsonConvert.SerializeObject(data);// JsonMapper.ToJson(data);

            string jsonStr = JsonConvert.SerializeObject(requestBody);// JsonMapper.ToJson(requestBody);

            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonStr);

            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.disposeUploadHandlerOnDispose = true;
            www.disposeDownloadHandlerOnDispose = true;
            await www.SendWebRequest();
            jsonToSend = null;
            GC.Collect();
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error While Sending: " + www.error);
                var outData = www.error;
                www.Dispose();
                return outData;
            }
            else
            {
                www.Dispose();
                //Debug.Log("Received: " + www.downloadHandler.text);
                return "";
            }

        }

        public async Task<string> UploadMessageData(List<string> readedMessageIDs, List<string> unreadedMessageIDs, string UserAccount, string Token)
        {
            UnityWebRequest www = new UnityWebRequest(FastAPISettings.SetMessageReadUrl, "POST");
#if UNITY_EDITOR
var cert = new ForceAcceptAll();
www.certificateHandler = cert;
#endif
            Dictionary<string, object> requestBody = new Dictionary<string, object>();
            requestBody[FastAPISettings.AccountKey] = UserAccount;
            requestBody[FastAPISettings.TokenKey] = Token;
            requestBody[FastAPISettings.ReadMessageListKey] = readedMessageIDs;
            requestBody[FastAPISettings.UnreadMessageListKey] = unreadedMessageIDs;

            string jsonStr = JsonConvert.SerializeObject(requestBody);// JsonMapper.ToJson(requestBody);

            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonStr);

            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.disposeUploadHandlerOnDispose = true;
            www.disposeDownloadHandlerOnDispose = true;
            await www.SendWebRequest();
            jsonToSend = null;
            GC.Collect();
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error While Sending: " + www.error);
                var outData = www.error;
                www.Dispose();
                return outData;
            }
            else
            {
                //Debug.Log("Received: " + www.downloadHandler.text);
                return "";
            }
        }
        public static async Task<string> UploadRankScore(string url, string userAccount, string token, string gameName, string rankFileName, string userName, int score)
        {
            using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
            {
                Dictionary<string, object> requestBody = new Dictionary<string, object>();
                requestBody[FastAPISettings.AccountKey] =userAccount;
                requestBody[FastAPISettings.TokenKey] = token;
                requestBody[FastAPISettings.GameName] = gameName;
                     requestBody[FastAPISettings.RankFile] =rankFileName;
                            requestBody["str_name"] = userName;
                            requestBody[FastAPISettings.RankScore] = score;
                string jsonStr = JsonConvert.SerializeObject(requestBody);//JsonMapper.ToJson(requestBody);
#if UNITY_EDITOR
var cert = new ForceAcceptAll();
www.certificateHandler = cert;
#endif
                //byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonStr);
                www.uploadHandler = (UploadHandler)new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonStr));
                www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.disposeUploadHandlerOnDispose = true;
                www.disposeDownloadHandlerOnDispose = true;
                await www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log("Error While Sending: " + www.error);
                    var outData = www.error;
                    www.Dispose();
                    return outData;
                }
                else
                {
                   // Debug.Log("Received: " + www.downloadHandler.text);
                    return "";
                }
            }
        }
        public static async Task<string> UploadNewRankScore(string url, string userAccount, string token, string userName, int score)
        {
            //Debug.Log(userName + " " + score.ToString());
#if UNITY_EDITOR
            if (url.Contains("127.0.0.1") || url.Contains("localhost"))
            {
                Debug.Log($"{url}/?str_user={userAccount}&int_score={score}&str_name={userName}");
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        Dictionary<string, object> requestBody = new Dictionary<string, object>();
                        requestBody[FastAPISettings.TokenKey] = token;
                        requestBody[FastAPISettings.AccountKey] = userAccount;
                        requestBody["int_score"] = score;
                        requestBody["str_name"] = userName;
                        string jsonStr = JsonConvert.SerializeObject(requestBody);
                        StringContent httpContent = new StringContent(jsonStr, System.Text.Encoding.UTF8, "application/json");
                        HttpResponseMessage response = await client.PutAsync(url, httpContent);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            Debug.Log("");
                        }
                        else
                            Debug.Log($"Error: {response.StatusCode}");
                        
                    }
                    catch (HttpRequestException e)
                    {
                        Debug.Log("Request error: " + e.Message);
                    }
                }
                return "";
            }
#endif
            using (UnityWebRequest www = new UnityWebRequest(url, "PUT"))
            {
                Dictionary<string, object> requestBody = new Dictionary<string, object>();
                requestBody[FastAPISettings.TokenKey] = token;
                requestBody[FastAPISettings.AccountKey] = userAccount;
                requestBody["int_score"] = score;
                requestBody["str_name"] = userName;
                string jsonStr = JsonConvert.SerializeObject(requestBody);//JsonMapper.ToJson(requestBody);
#if UNITY_EDITOR
var cert = new ForceAcceptAll();
www.certificateHandler = cert;
#endif
                //byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonStr);
                www.uploadHandler = (UploadHandler)new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonStr));
                www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.disposeUploadHandlerOnDispose = true;
                www.disposeDownloadHandlerOnDispose = true;
                await www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log("Error While Sending: " + www.error);
                    var outData = www.error;
                    www.Dispose();
                    return outData;
                }
                else
                {
                    return "";
                }
            }
        }
        public async Task<string> UploadLog(string logTable, string logs, string UserAccount, string Token)
        {
            using (UnityWebRequest www = new UnityWebRequest(FastAPISettings.LogUploadUrl, "POST"))
            {
                Dictionary<string, object> requestBody = new Dictionary<string, object>();
                requestBody[FastAPISettings.AccountKey] = UserAccount;
                requestBody[FastAPISettings.TokenKey] = Token;
                requestBody[FastAPISettings.TableKey] = logTable;
                requestBody[FastAPISettings.LogKey] = logs;

                string jsonStr = JsonConvert.SerializeObject(requestBody);//JsonMapper.ToJson(requestBody);
#if UNITY_EDITOR
var cert = new ForceAcceptAll();
www.certificateHandler = cert;
#endif
                //byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonStr);
                www.uploadHandler = (UploadHandler)new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonStr));
                www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.disposeUploadHandlerOnDispose = true;
                www.disposeDownloadHandlerOnDispose = true;
                await www.SendWebRequest();
                GC.Collect();
                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log("Error While Sending: " + www.error);
                    var outData = www.error;
                    www.Dispose();
                    return outData;
                }
                else
                {
                    //Debug.Log("Received: " + www.downloadHandler.text);
                    return "";
                }
            }
        }

        public async Task<string> UploadImage(byte[] file, string folder, string fileName, string strUser = null, string strToken = null)
        {
            string url = string.Format("{0}{1}/", FastAPISettings.UploadFileUrl, folder);

            WWWForm form = new WWWForm();

            form.AddBinaryData(FastAPISettings.UploadFileKey, file, fileName, "image/jpeg");

            if (!string.IsNullOrEmpty(strUser) && !string.IsNullOrEmpty(strToken))
            {
                form.AddField(FastAPISettings.AccountKey, strUser);
                form.AddField(FastAPISettings.TokenKey, strToken);
            }

            UnityWebRequest www = UnityWebRequest.Post(url, form);
        #if UNITY_EDITOR
var cert = new ForceAcceptAll();
www.certificateHandler = cert;
#endif
            www.disposeUploadHandlerOnDispose = true;
            www.disposeDownloadHandlerOnDispose = true;
            await www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error While Sending: " + www.error);
                var outData = www.error;
                www.Dispose();
                return outData;
            }
            else
            {
                //Debug.Log("Received: " + www.downloadHandler.text);
                return "";
            }
        }

        public async Task<Texture2D> DownloadImage(string url)
        {
            UnityWebRequest www = new UnityWebRequest(url, "GET");
#if UNITY_EDITOR
var cert = new ForceAcceptAll();
www.certificateHandler = cert;
#endif
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            www.SetRequestHeader("Content-Type", "image/jpeg");
            www.disposeUploadHandlerOnDispose = true;
            www.disposeDownloadHandlerOnDispose = true;
            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                LogRequestError(www, url);
                www.Dispose();
                return null;
            }
            else if (www.downloadHandler.text == "Not Found")
            {
                Debug.LogError($"404 Not Found: {url}");
                www.Dispose();
                return null;
            }
            else
            {
                //Debug.Log("Received: " + www.downloadHandler.text);

                Texture2D tex = new Texture2D(2, 2);

                tex.LoadImage(www.downloadHandler.data, false);
                www.Dispose();
                return tex;
            }
        }

        #region FAPI

        /// <summary>
        /// 取得連續Json檔案
        /// </summary>

        public async Task<T[]> LoadJsonList<T>(string[] paths, Func<JsonData, T> parser) where T : class
        {
            T[] returnArray = null;

            UnityWebRequest www = new UnityWebRequest(FastAPISettings.BaseFapi, "POST");
#if UNITY_EDITOR
var cert = new ForceAcceptAll();
www.certificateHandler = cert;
#endif
            Dictionary<string, object> requestBody = new Dictionary<string, object>();
            requestBody["command"] = "file";
            requestBody["action"] = "read-files-not-null";
            requestBody["paths"] = paths;

            string jsonStr = JsonConvert.SerializeObject(requestBody);//JsonMapper.ToJson(requestBody, false);

            Debug.Log(www.url);
            Debug.Log(jsonStr);
            Debug.Log($"Request Body: {JsonConvert.SerializeObject(requestBody, Formatting.Indented)}");
            www.disposeUploadHandlerOnDispose = true;
            www.disposeDownloadHandlerOnDispose = true;
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonStr);

            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                LogRequestError(www, "paths: " + string.Join(", ", paths));
            }
            else if (www.downloadHandler.text == "null")
            {
                Debug.Log("Get null from server");
            }
            else
            {
                //Debug.Log("size: " + (www.downloadHandler.data.Length / 1000).ToString() + "kb");

                JsonData jsonData = JsonMapper.ToObject(www.downloadHandler.text);

                returnArray = new T[paths.Length];

                for (int i = 0; i < paths.Length; ++i)
                {
                    returnArray[i] = parser(JsonMapper.ToObject(jsonData[i].ToString()));
                }
            }
            www.Dispose();
            return returnArray;
        }

        /// <summary>
        /// 取得不重複key資料的最後一筆
        /// </summary>
        public async Task<T[]> LoadArrayGroupLast<T>(string path, string[] columns, Func<string[], T> parser) where T : class
        {
            T[] returnArray = null;

            UnityWebRequest www = new UnityWebRequest(FastAPISettings.BaseFapi, "POST");
#if UNITY_EDITOR
var cert = new ForceAcceptAll();
www.certificateHandler = cert;
#endif
            Dictionary<string, object> requestBody = new Dictionary<string, object>();
            requestBody["command"] = "csv";
            requestBody["action"] = "group-last";
            requestBody["path"] = path;
            requestBody["columns"] = columns;

            string jsonStr = JsonConvert.SerializeObject(requestBody);// JsonMapper.ToJson(requestBody, false);

            Debug.Log(www.url);
            Debug.Log(jsonStr);

            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonStr);

            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.disposeUploadHandlerOnDispose = true;
            www.disposeDownloadHandlerOnDispose = true;
            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                LogRequestError(www, "path: " + path);
            }
            else
            {
                //Debug.Log("size: " + (www.downloadHandler.data.Length / 1000).ToString() + "kb");

                JsonData jsonData = JsonMapper.ToObject(www.downloadHandler.text);

                //Debug.Log(www.downloadHandler.text);

                returnArray = new T[jsonData.Count];

                for (int i = 0; i < returnArray.Length; ++i)
                {
                    Debug.Log($"第 {i+1} 筆資料: {string.Join(", ", JsonMapper.ToObject<string[]>(jsonData[i].ToJson()))}");
                    returnArray[i] = parser(JsonMapper.ToObject<string[]>(jsonData[i].ToJson()));
                }
            }
            www.Dispose();
            return returnArray;
        }

        /// <summary>
        /// 針對csv寫入一筆資料
        /// </summary>
        /// <param name="path">csv path</param>
        /// <param name="columns">columns名稱</param>
        /// <param name="values">對應column的值</param>
        /// <returns></returns>
        public async Task<string> AppendAndCreate(string path, string[] columns, string[] values)
        {
            UnityWebRequest www = new UnityWebRequest(FastAPISettings.BaseFapi, "POST");
#if UNITY_EDITOR
var cert = new ForceAcceptAll();
www.certificateHandler = cert;
#endif
            Dictionary<string, object> requestBody = new Dictionary<string, object>();
            requestBody["command"] = "csv";
            requestBody["action"] = "create-append";
            requestBody["path"] = path;
            requestBody["columns"] = columns;
            requestBody["values"] = values;

            string jsonStr = JsonConvert.SerializeObject(requestBody);// JsonMapper.ToJson(requestBody, false);

            Debug.Log(www.url);
            Debug.Log(jsonStr);

            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonStr);

            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);

            www.SetRequestHeader("Content-Type", "application/json");

            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                LogRequestError(www, "path: " + path);
                return www.error;
            }
            else
            {
                return null;
            }
        }
        #endregion
    }

    public class RequestResult
    {
        public object Content;

        public string ErrorMsg;

        public bool IsSuccess
        {
            get
            {
                return string.IsNullOrEmpty(ErrorMsg);
            }
        }
    }
}
