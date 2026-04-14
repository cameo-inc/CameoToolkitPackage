using System.Runtime.InteropServices;

namespace Cameo
{
    /// <summary>
    /// WebGL：將 FastAPI 選到的 APIDomain 寫入網頁 window.__CAMEO_API_BASE__，供模板 JS 與 UrlDef 一致。
    /// </summary>
    internal static class WebGLPageApiBaseBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void CameoInjectWebPageApiBase(string baseUrl);
#endif

        internal static void Publish(string baseUrl)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (string.IsNullOrEmpty(baseUrl))
            {
                return;
            }
            CameoInjectWebPageApiBase(baseUrl);
#endif
        }
    }
}
