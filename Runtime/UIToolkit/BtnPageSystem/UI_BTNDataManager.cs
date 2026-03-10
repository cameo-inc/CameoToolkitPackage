using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cameo;
using Cameo.UI;
using Sirenix.OdinInspector;
public class UI_BTNDataManager : Singleton<UI_BTNDataManager>
{
    public List<string> preloaderIndexNames;
    public List<UI_BTNPageDataLoader> preloaders;
    public List<UI_BTNPageDataLoader> DynamicLoaders;
    private Dictionary<string, UI_BTNPageDataLoader> _loaderCheckers= new Dictionary<string, UI_BTNPageDataLoader>();
    private Dictionary<string, bool> _hasLoaded = new Dictionary<string, bool>();
    public GameObject BTNPageDataLoaderPrefab;

    private void Awake()
    {
        for (int i = 0; i < preloaders.Count; i++)
        {
            _loaderCheckers.Add(preloaders[i].BTNMenuUniqueID, preloaders[i]);
            _hasLoaded.Add(preloaders[i].BTNMenuUniqueID, false);
        }
        for (int i = 0; i < DynamicLoaders.Count; i++)
        {
            _loaderCheckers.Add(DynamicLoaders[i].BTNMenuUniqueID, DynamicLoaders[i]);
            _hasLoaded.Add(DynamicLoaders[i].BTNMenuUniqueID, false);
        }
    }
    [Button]
    public void CreatePreloaderByName()
    {
        //preloaders = new List<UI_BTNPageDataLoader>();
        foreach (var name in preloaderIndexNames)
        {
            var preloader = Instantiate(BTNPageDataLoaderPrefab).GetComponent<UI_BTNPageDataLoader>();
            preloader.BTNMenuUniqueID= name;
            preloader.name = name;
            preloader.transform.SetParent(transform);
            preloaders.Add(preloader);
        }
    }

    public IEnumerator WaitForloading(string BTNMenuUniqueID)
    {
        var loader = GetPreloader(BTNMenuUniqueID);
        if(loader)
        yield return null;
    }
    public string GetStateFileName(string BTNMenuUniqueID)
    {
        return GetPreloader(BTNMenuUniqueID)?.StateFileName;
    }
    public string GetSheetID(string BTNMenuUniqueID)
    {
        return GetPreloader(BTNMenuUniqueID)?.BTNDtataSheetID;
    }
    public IEnumerator Preload(string BTNMenuUniqueID)
    {
         yield return GetPreloader(BTNMenuUniqueID)?.InitializeCoroutine();
    }
    public List<BTNData> GetBTNData(string BTNMenuUniqueID)
    {
        return GetPreloader(BTNMenuUniqueID)?.BtnDatas;
    }
    public MissionBTNState GetMissionData(string BTNMenuUniqueID, string BTNID)
    {
        var pageData = GetPreloader(BTNMenuUniqueID);
        if(pageData==null)
        {
            Debug.LogError("找不到MissionData，可能沒有下載或是沒有建立：" + BTNMenuUniqueID);
            return null;
        }
        var datalist = pageData.MissionData;
        foreach(var obj in datalist)
        {
            if (obj.BTNID == BTNID)
                return obj;
        }
        return null;
    }
    public List<MissionBTNState> GetMissionData(string BTNMenuUniqueID)
    {
        return GetPreloader(BTNMenuUniqueID)?.MissionData;
    }
    public List<MissionBTNState> SetMissionData(string BTNMenuUniqueID, List<MissionBTNState>  data)
    {
        return GetPreloader(BTNMenuUniqueID).MissionData= data;
    }
    
    public UI_BTNPageDataLoader GetPreloader(string BTNMenuUniqueID)
    {
        if (!(_loaderCheckers.ContainsKey(BTNMenuUniqueID)))
        {
#if UNITY_EDITOR
                Debug.LogError($"找不到對應的 BTN Loader, 請設定場景中{gameObject.name}, 對應ID:{BTNMenuUniqueID}");
#endif
            return null;
        }
        if (!_hasLoaded[BTNMenuUniqueID])
        {
#if UNITY_EDITOR
                Debug.LogError($"Get 前未先 Load, 對應ID:{BTNMenuUniqueID}");
#endif
            return null;
        }
        return _loaderCheckers[BTNMenuUniqueID];
    }
    public IEnumerator LoadAll()
    {
        // 並行載入所有 preloader，減少 API 等待時間
        if (preloaders.Count > 0)
        {
            int completedCount = 0;
            foreach (var loader in preloaders)
            {
                var loaderCopy = loader;
                StartCoroutine(LoadSingleThenCallback(loaderCopy, () =>
                {
                    _hasLoaded[loaderCopy.BTNMenuUniqueID] = true;
                    completedCount++;
                }));
            }
            yield return new WaitUntil(() => completedCount >= preloaders.Count);
        }
        for (int i = 0; i < DynamicLoaders.Count; i++)
        {
            preloaders.Add(DynamicLoaders[i]);
        }
    }

    private IEnumerator LoadSingleThenCallback(UI_BTNPageDataLoader loader, System.Action onComplete)
    {
        yield return loader.InitializeCoroutine();
        onComplete?.Invoke();
    }
    public IEnumerator Load(string btnID)
    {
        if (!(_loaderCheckers.ContainsKey(btnID) && _hasLoaded.ContainsKey(btnID)))
        {
#if UNITY_EDITOR
                Debug.LogError($"找不到對應的 BTN Dynamic Loader, 請設定場景中{gameObject.name}, 對應ID:{btnID}");
#endif
            yield break;
        }
        if (_hasLoaded[btnID])
            yield return null;
        else
        {
            yield return StartCoroutine(_loaderCheckers[btnID].InitializeCoroutine());
            _hasLoaded[btnID] = true;
        }
    }
    public IEnumerator Load(List<string> btnIDs)
    {
        foreach (string btnID in btnIDs)
            yield return StartCoroutine(Load(btnID));
    }
}
