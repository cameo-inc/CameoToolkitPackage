
using System.Threading.Tasks;
using Cameo;
public class UniversalDataLoader : IDataLoader<string>
{
    /// <summary>
    /// Cameo FastAPI：讀取 Google Sheet 表單資料
    /// </summary>

    //string BaseDataUrl { get { return BaseAPIUrl + AddAPIUrl; } }

    public async Task<string> LoadWithParams(string SheetID, string user, string token)
    {
        DownloadInfo downloadInfo = DownloadInfoManager.Instance.DownloadInfo(SheetID);

        string url = string.Format("{0}/?{1}={2}&{3}={4}&{5}={6}.sheet&{7}={8}", FastAPISettings.BaseDataUrl,
            FastAPISettings.AccountKey, user,
            FastAPISettings.TokenKey, token,
            FastAPISettings.SpreadSheetKey, downloadInfo.SpreadSheet,
            FastAPISettings.WorkSheetKey, downloadInfo.WorkSheet);

        string jsonString = await FileRequestHelper.Instance.LoadJsonString(url);

        return jsonString;
    }
}
