#if !DISABLESTEAMWORKS && STEAM_INSTALLED
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("Steamworks/Workshop Item Search")]
    public class SteamWorkshopItemSearch : MonoBehaviour
    {
        [Header("Elements")]
        public TMPro.TMP_InputField searchText;
        public TMPro.TextMeshProUGUI currentPageLabel;
        public TMPro.TextMeshProUGUI pageCountLabel;
        public SteamWorkshopItemDetailData template;
        public Transform content;

        public UgcQuery ActiveQuery;
        public int CurrentFrom
        {
            get
            {
                if (ActiveQuery != null)
                {
                    var maxItemIndex = (int)(ActiveQuery.Page * 50);
                    if (maxItemIndex < ActiveQuery.MatchedRecordCount)
                    {
                        return maxItemIndex - 49;
                    }
                    else
                    {
                        var remainder = (int)(ActiveQuery.MatchedRecordCount % 50);
                        maxItemIndex = maxItemIndex - 50 + remainder;
                        return maxItemIndex - remainder + 1;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }
        public int CurrentTo
        {
            get
            {
                if (ActiveQuery != null)
                {
                    var maxItemIndex = (int)(ActiveQuery.Page * 50);
                    if (maxItemIndex < ActiveQuery.MatchedRecordCount)
                    {
                        return maxItemIndex;
                    }
                    else
                    {
                        var remainder = (int)(ActiveQuery.MatchedRecordCount % 50);
                        maxItemIndex = maxItemIndex - 50 + remainder;
                        return maxItemIndex;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }
        public int TotalCount => ActiveQuery != null ? (int)ActiveQuery.MatchedRecordCount : 0;
        public int CurrentPage => ActiveQuery != null ? (int)ActiveQuery.Page : 0;

        [Header("Events")]
        public UnityEvent<UgcQuery> onResultsReady;
        public UnityEvent<UgcQuery> onQueryUpdated;

        private readonly List<SteamWorkshopItemDetailData> _currentRecords = new();
        private string _lastSearchString = "";

        private string GetSearchString()
        {
            if (searchText != null)
                return searchText.text;
            else
                return string.Empty;
        }

        public void SearchMyPublished()
        {
            var filter = GetSearchString();
            _lastSearchString = filter;
            ActiveQuery = UgcQuery.GetMyPublished();            
            if (!string.IsNullOrEmpty(filter))
            {
                API.UserGeneratedContent.Client.SetSearchText(ActiveQuery.Handle, filter);
            }

            if (ActiveQuery.Handle != UGCQueryHandle_t.Invalid)
                ActiveQuery.Execute(HandleResults);
            else
                Debug.LogError("Steam was unable to create a query handle for this argument. Check your App ID and the App ID in the query manager.");
        }

        public void SearchAll()
        {
            var filter = GetSearchString();
            _lastSearchString = filter;
            ActiveQuery = UgcQuery.Get(EUGCQuery.k_EUGCQuery_RankedByTrend, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, new(0), AppData.Me);
            if (!string.IsNullOrEmpty(filter))
            {
                API.UserGeneratedContent.Client.SetSearchText(ActiveQuery.Handle, filter);
            }

            if (ActiveQuery.Handle != UGCQueryHandle_t.Invalid)
                ActiveQuery.Execute(HandleResults);
            else
                Debug.LogError("Steam was unable to create a query handle for this argument. Check your App ID and the App ID in the query manager.");
        }

        public void SearchSubscribed()
        {
            var filter = GetSearchString();
            _lastSearchString = filter;
            ActiveQuery = UgcQuery.GetSubscribed();
            if (!string.IsNullOrEmpty(filter))
            {
                API.UserGeneratedContent.Client.SetSearchText(ActiveQuery.Handle, filter);
            }

            if (ActiveQuery.Handle != UGCQueryHandle_t.Invalid)
                ActiveQuery.Execute(HandleResults);
            else
                Debug.LogError("Steam was unable to create a query handle for this argument. Check your App ID and the App ID in the query manager.");
        }

        public void PrepareSearchAll()
        {
            var filter = GetSearchString();
            _lastSearchString = filter;
            ActiveQuery = UgcQuery.Get(EUGCQuery.k_EUGCQuery_RankedByTrend, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, new(0), AppData.Me);
            if (!string.IsNullOrEmpty(filter))
            {
                API.UserGeneratedContent.Client.SetSearchText(ActiveQuery.Handle, filter);
            }

            if (ActiveQuery.Handle != UGCQueryHandle_t.Invalid)
                onQueryUpdated.Invoke(ActiveQuery);
            else
                Debug.LogError("Steam was unable to create a query handle for this argument. Check your App ID and the App ID in the query manager.");
        }

        public void SearchFavorites()
        {
            var filter = GetSearchString();
            _lastSearchString = filter;
            ActiveQuery = UgcQuery.Get(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Favorited, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, EUserUGCListSortOrder.k_EUserUGCListSortOrder_TitleAsc, new(0), AppData.Me);
            if (!string.IsNullOrEmpty(filter))
            {
                API.UserGeneratedContent.Client.SetSearchText(ActiveQuery.Handle, filter);
            }

            if (ActiveQuery.Handle != UGCQueryHandle_t.Invalid)
                ActiveQuery.Execute(HandleResults);
            else
                Debug.LogError("Steam was unable to create a query handle for this argument. Check your App ID and the App ID in the query manager.");
        }

        public void PrepareSearchFavorites()
        {
            var filter = GetSearchString();
            _lastSearchString = filter;
            ActiveQuery = UgcQuery.Get(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Favorited, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, EUserUGCListSortOrder.k_EUserUGCListSortOrder_TitleAsc, new(0), AppData.Me);
            if (!string.IsNullOrEmpty(filter))
            {
                API.UserGeneratedContent.Client.SetSearchText(ActiveQuery.Handle, filter);
            }

            if (ActiveQuery.Handle != UGCQueryHandle_t.Invalid)
                onQueryUpdated.Invoke(ActiveQuery);
            else
                Debug.LogError("Steam was unable to create a query handle for this argument. Check your App ID and the App ID in the query manager.");
        }

        public void SearchFollowed()
        {
            var filter = GetSearchString();
            _lastSearchString = filter;
            ActiveQuery = UgcQuery.Get(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Followed, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, EUserUGCListSortOrder.k_EUserUGCListSortOrder_TitleAsc, new(0), AppData.Me);
            if (!string.IsNullOrEmpty(filter))
            {
                API.UserGeneratedContent.Client.SetSearchText(ActiveQuery.Handle, filter);
            }

            if (ActiveQuery.Handle != UGCQueryHandle_t.Invalid)
                ActiveQuery.Execute(HandleResults);
            else
                Debug.LogError("Steam was unable to create a query handle for this argument. Check your App ID and the App ID in the query manager.");
        }

        public void PrepareSearchFollowed()
        {
            var filter = GetSearchString();
            _lastSearchString = filter;
            ActiveQuery = UgcQuery.Get(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Followed, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, EUserUGCListSortOrder.k_EUserUGCListSortOrder_TitleAsc, new(0), AppData.Me);
            if (!string.IsNullOrEmpty(filter))
            {
                API.UserGeneratedContent.Client.SetSearchText(ActiveQuery.Handle, filter);
            }
            onQueryUpdated.Invoke(ActiveQuery);
        }

        public void ExecuteSearch()
        {
            if (ActiveQuery != null && ActiveQuery.Handle != UGCQueryHandle_t.Invalid)
                ActiveQuery.Execute(HandleResults);
            else
                Debug.LogError("Attempted to execute a query with an invalid query handle.");
        }

        public void SetNextSearchPage()
        {
            if (ActiveQuery != null)
            {
                ActiveQuery.SetNextPage();

                if (!string.IsNullOrEmpty(_lastSearchString))
                    API.UserGeneratedContent.Client.SetSearchText(ActiveQuery.Handle, _lastSearchString);

                ActiveQuery.Execute(HandleResults);
            }
            else
            {
                Debug.LogWarning("No active query or the query handle is invalid, you must call a Search or Prepare Search method before iterating over the pages");
            }
        }

        public void SetPreviousSearchPage()
        {
            if (ActiveQuery != null)
            {
                ActiveQuery.SetPreviousPage();

                if (!string.IsNullOrEmpty(_lastSearchString))
                    API.UserGeneratedContent.Client.SetSearchText(ActiveQuery.Handle, _lastSearchString);

                ActiveQuery.Execute(HandleResults);
            }
            else
            {
                Debug.LogWarning("No active query or the query handle is invalid, you must call a Search or Prepare Search method before iterating over the pages");
            }
        }

        public void SetSearchPage(uint page)
        {
            if(ActiveQuery != null)
            {
                ActiveQuery.SetPage(page);

                if (!string.IsNullOrEmpty(_lastSearchString))
                    API.UserGeneratedContent.Client.SetSearchText(ActiveQuery.Handle, _lastSearchString);

                ActiveQuery.Execute(HandleResults);
            }
            else
            {
                Debug.LogWarning("No active query or the query handle is invalid, you must call a Search or Prepare Search method before iterating over the pages");
            }
        }

        private void HandleResults(UgcQuery query)
        {
            if(currentPageLabel != null)
            currentPageLabel.text = ActiveQuery.Page.ToString();
            if(pageCountLabel != null)
            pageCountLabel.text = ActiveQuery.PageCount.ToString();

            if (template != null
                && content != null)
            {
                foreach (var comp in _currentRecords)
                    Destroy(comp.gameObject);

                _currentRecords.Clear();

                foreach (var result in query.ResultsList)
                {
                    var comp = Instantiate(template, content);
                    _currentRecords.Add(comp);

                    comp.Data = result;
                    comp.LoadPreview();
                }
            }
            onResultsReady?.Invoke(ActiveQuery);
        }
    }
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamWorkshopItemSearch), true)]
    public class SteamWorkshopItemSearchEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif