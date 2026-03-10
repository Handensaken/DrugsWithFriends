#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    public class UgcQuery : IDisposable
    {
        public UGCQueryHandle_t Handle;
        public uint MatchedRecordCount = 0;
        public uint PageCount = 1;
        private bool _isAllQuery = false;
        private bool _isUserQuery = false;
        private EUserUGCList _listType;
        private EUGCQuery _queryType;
        private EUGCMatchingUGCType _matchingType;
        private EUserUGCListSortOrder _sortOrder;
        private AppId_t _creatorApp;
        private AppId_t _consumerApp;
        private AccountID_t _account;
        private uint _page = 1;
        public uint Page { get { return _page; } set { SetPage(value); } }
        private UnityAction<UgcQuery> _callback;

        public CallResult<SteamUGCQueryCompleted_t> MSteamUgcQueryCompleted;

        public List<WorkshopItemDetails> ResultsList = new List<WorkshopItemDetails>();

        private UgcQuery()
        {
            MSteamUgcQueryCompleted = CallResult<SteamUGCQueryCompleted_t>.Create(HandleQueryCompleted);
        }

        public static UgcQuery Get(EUGCQuery queryType, EUGCMatchingUGCType matchingType, AppId_t creatorApp, AppId_t consumerApp)
        {
            UgcQuery nQuery = new UgcQuery
            {
                MatchedRecordCount = 0,
                PageCount = 1,
                _isAllQuery = true,
                _isUserQuery = false,
                _queryType = queryType,
                _matchingType = matchingType,
                _creatorApp = creatorApp,
                _consumerApp = consumerApp,
                Page = 1,
                Handle = API.UserGeneratedContent.Client.CreateQueryAllRequest(queryType, matchingType, creatorApp, consumerApp, 1)
            };

            return nQuery;
        }

        public static UgcQuery Get(params PublishedFileId_t[] fileIds)
        {
            if (fileIds == null
                || fileIds.Length < 1)
                return null;

            UgcQuery nQuery = new UgcQuery
            {
                MatchedRecordCount = 0,
                PageCount = 1,
                _isAllQuery = true,
                _isUserQuery = false,
                Page = 1,
                Handle = API.UserGeneratedContent.Client.CreateQueryDetailsRequest(fileIds)
            };

            return nQuery;
        }

        public static UgcQuery Get(IEnumerable<PublishedFileId_t> fileIds)
        {
            var list = new List<PublishedFileId_t>(fileIds);
            UgcQuery nQuery = new UgcQuery
            {
                MatchedRecordCount = 0,
                PageCount = 1,
                _isAllQuery = true,
                _isUserQuery = false,
                Page = 1,
                Handle = API.UserGeneratedContent.Client.CreateQueryDetailsRequest(list.ToArray())
            };

            return nQuery;
        }

        public static UgcQuery Get(AccountID_t account, EUserUGCList listType, EUGCMatchingUGCType matchingType, EUserUGCListSortOrder sortOrder, AppId_t creatorApp, AppId_t consumerApp)
        {
            UgcQuery nQuery = new UgcQuery
            {
                MatchedRecordCount = 0,
                PageCount = 1,
                _isAllQuery = false,
                _isUserQuery = true,
                _listType = listType,
                _sortOrder = sortOrder,
                _matchingType = matchingType,
                _creatorApp = creatorApp,
                _consumerApp = consumerApp,
                _account = account,
                Page = 1,
                Handle = API.UserGeneratedContent.Client.CreateQueryUserRequest(account, listType, matchingType, sortOrder, creatorApp, consumerApp, 1)
            };

            return nQuery;
        }

        public static UgcQuery Get(UserData user, EUserUGCList listType, EUGCMatchingUGCType matchingType, EUserUGCListSortOrder sortOrder, AppId_t creatorApp, AppId_t consumerApp)
        {
            UgcQuery nQuery = new UgcQuery
            {
                MatchedRecordCount = 0,
                PageCount = 1,
                _isAllQuery = false,
                _isUserQuery = true,
                _listType = listType,
                _sortOrder = sortOrder,
                _matchingType = matchingType,
                _creatorApp = creatorApp,
                _consumerApp = consumerApp,
                _account = user.AccountId,
                Page = 1,
                Handle = API.UserGeneratedContent.Client.CreateQueryUserRequest(user.AccountId, listType, matchingType, sortOrder, creatorApp, consumerApp, 1)
            };

            return nQuery;
        }

        public static UgcQuery GetMyPublished()
        {
            var query = Get(UserData.Me, EUserUGCList.k_EUserUGCList_Published, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderDesc, AppData.Me, AppData.Me);
            query.SetReturnLongDescription(true);
            query.SetReturnMetadata(true);
            return query;
        }

        public static UgcQuery GetMyPublished(AppData creatorApp, AppData consumerApp)
        {
            var query = Get(UserData.Me, EUserUGCList.k_EUserUGCList_Published, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderDesc, creatorApp, consumerApp);
            query.SetReturnLongDescription(true);
            query.SetReturnMetadata(true);
            return query;
        }

        public static UgcQuery GetSubscribed(bool withLongDescription, bool withMetadata, bool withKeyValueTags, bool withAdditionalPreviews, uint withPlayTimeStatsInDays)
        {
            var query = Get(API.UserGeneratedContent.Client.GetSubscribedItems());
            if (query != null)
            {
                query.SetReturnLongDescription(withLongDescription);
                query.SetReturnMetadata(withMetadata);
                query.SetReturnKeyValueTags(withKeyValueTags);
                if (withPlayTimeStatsInDays > 0)
                    query.SetReturnPlaytimeStats(withPlayTimeStatsInDays);
                query.SetReturnAdditionalPreviews(withAdditionalPreviews);
            }
            return query;
        }

        public static UgcQuery GetPlayed()
        {
            var query = Get(UserData.Me, EUserUGCList.k_EUserUGCList_UsedOrPlayed, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, EUserUGCListSortOrder.k_EUserUGCListSortOrder_LastUpdatedDesc, AppData.Me, AppData.Me);
            query.SetReturnLongDescription(true);
            query.SetReturnMetadata(true);
            return query;
        }

        public static UgcQuery GetPlayed(AppData creatorApp, AppData consumerApp)
        {
            var query = Get(UserData.Me, EUserUGCList.k_EUserUGCList_UsedOrPlayed, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, EUserUGCListSortOrder.k_EUserUGCListSortOrder_LastUpdatedDesc, creatorApp, consumerApp);
            query.SetReturnLongDescription(true);
            query.SetReturnMetadata(true);
            return query;
        }

        /// <summary>
        /// Adds a excluded tag to a pending UGC Query. This will only return UGC without the specified tag.
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public bool AddExcludedTag(string tagName) => API.UserGeneratedContent.Client.AddExcludedTag(Handle, tagName);
        /// <summary>
        /// Adds a required key-value tag to a pending UGC Query. This will only return workshop items that have a key = pKey and a value = pValue.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool AddRequiredKeyValueTag(string key, string value) => API.UserGeneratedContent.Client.AddRequiredKeyValueTag(Handle, key, value);
        /// <summary>
        /// Adds a required tag to a pending UGC Query. This will only return UGC with the specified tag.
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public bool AddRequiredTag(string tagName) => API.UserGeneratedContent.Client.AddRequiredTag(Handle, tagName);
        /// <summary>
        /// Set allow cached response
        /// </summary>
        /// <param name="maxAgeSeconds"></param>
        /// <returns></returns>
        public bool SetAllowCachedResponse(uint maxAgeSeconds) => API.UserGeneratedContent.Client.SetAllowCachedResponse(Handle, maxAgeSeconds);
        /// <summary>
        /// Set cloud file name filter
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool SetCloudFileNameFilter(string fileName) => API.UserGeneratedContent.Client.SetCloudFileNameFilter(Handle, fileName);
        /// <summary>
        /// Set item langauge
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public bool SetLanguage(string language) => API.UserGeneratedContent.Client.SetLanguage(Handle, language);

        /// <summary>
        /// Set match any tag
        /// </summary>
        /// <param name="anyTag"></param>
        /// <returns></returns>
        public bool SetMatchAnyTag(bool anyTag) => API.UserGeneratedContent.Client.SetMatchAnyTag(Handle, anyTag);

        /// <summary>
        /// Set ranked by trend days
        /// </summary>
        /// <param name="days"></param>
        /// <returns></returns>
        public bool SetRankedByTrendDays(uint days) => API.UserGeneratedContent.Client.SetRankedByTrendDays(Handle, days);

        /// <summary>
        /// Set return additional previews
        /// </summary>
        /// <param name="additionalPreviews"></param>
        /// <returns></returns>
        public bool SetReturnAdditionalPreviews(bool additionalPreviews) => API.UserGeneratedContent.Client.SetReturnAdditionalPreviews(Handle, additionalPreviews);

        /// <summary>
        /// Set return childre
        /// </summary>
        /// <param name="returnChildren"></param>
        /// <returns></returns>
        public bool SetReturnChildren(bool returnChildren) => API.UserGeneratedContent.Client.SetReturnChildren(Handle, returnChildren);

        /// <summary>
        /// Set return key value tags
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public bool SetReturnKeyValueTags(bool tags) => API.UserGeneratedContent.Client.SetReturnKeyValueTags(Handle, tags);

        /// <summary>
        /// Set return long description
        /// </summary>
        /// <param name="longDescription"></param>
        /// <returns></returns>
        public bool SetReturnLongDescription(bool longDescription) =>  API.UserGeneratedContent.Client.SetReturnLongDescription(Handle, longDescription);

        /// <summary>
        /// Set return metadata
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public bool SetReturnMetadata(bool metadata) =>  API.UserGeneratedContent.Client.SetReturnMetadata(Handle, metadata);

        /// <summary>
        /// Set return IDs only
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="onlyIds"></param>
        /// <returns></returns>
        public bool SetReturnOnlyIDs(bool onlyIds) => API.UserGeneratedContent.Client.SetReturnOnlyIDs(Handle, onlyIds);

        /// <summary>
        /// Set return playtime stats
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="days"></param>
        /// <returns></returns>
        public bool SetReturnPlaytimeStats(uint days) => API.UserGeneratedContent.Client.SetReturnPlaytimeStats(Handle, days);

        /// <summary>
        /// Set return total only
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="totalOnly"></param>
        /// <returns></returns>
        public bool SetReturnTotalOnly(bool totalOnly) => API.UserGeneratedContent.Client.SetReturnTotalOnly(Handle, totalOnly);

        /// <summary>
        /// Set search text
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool SetSearchText(string text) => API.UserGeneratedContent.Client.SetSearchText(Handle, text);

        public bool SetNextPage() =>  SetPage((uint)Mathf.Clamp((int)_page + 1, 1, PageCount));

        public bool SetPreviousPage() => SetPage((uint)Mathf.Clamp((int)_page - 1, 1, PageCount));

        public bool SetPage(uint page)
        {
            _page = page > 0 ? page : 1;
            if (_isAllQuery)
            {
                ReleaseHandle();
                Handle = API.UserGeneratedContent.Client.CreateQueryAllRequest(_queryType, _matchingType, _creatorApp, _consumerApp, Page);
                MatchedRecordCount = 0;
                return true;
            }
            else if (_isUserQuery)
            {
                ReleaseHandle();
                Handle = API.UserGeneratedContent.Client.CreateQueryUserRequest(_account, _listType, _matchingType, _sortOrder, _creatorApp, _consumerApp, Page);
                MatchedRecordCount = 0;
                return true;
            }
            else
            {
                Debug.LogError("Pages are not supported by detail queries e.g. searching for specific file Ids");
                return false;
            }
        }

        public bool Execute(UnityAction<UgcQuery> callback)
        {
            if(Handle == UGCQueryHandle_t.Invalid)
            {
                Debug.LogError("Invalid handle, you must call CreateAll");
                return false;
            }

            ResultsList.Clear();
            _callback = callback;
            API.UserGeneratedContent.Client.SendQueryUgcRequest(Handle, HandleQueryCompleted);

            return true;
        }

        private void HandleQueryCompleted(SteamUGCQueryCompleted_t param, bool bIOFailure)
        {
            if (!bIOFailure)
            {
                if (param.m_eResult == EResult.k_EResultOK)
                {
                    MatchedRecordCount = param.m_unTotalMatchingResults;

                    PageCount = (uint)Mathf.Clamp((int)MatchedRecordCount / 50, 1, int.MaxValue);
                    if (PageCount * 50 < MatchedRecordCount)
                        PageCount++;

                    for (int i = 0; i < param.m_unNumResultsReturned; i++)
                    {
                        SteamUGCDetails_t details;
                        API.UserGeneratedContent.Client.GetQueryResult(param.m_handle, (uint)i, out details);
                        var nRecord = new WorkshopItemDetails(details);
                        API.UserGeneratedContent.Client.GetQueryMetadata(param.m_handle, (uint)i, out nRecord.metadata, Constants.k_cchDeveloperMetadataMax);
                        nRecord.metadata?.Trim();

                        var tagCount = API.UserGeneratedContent.Client.GetQueryNumKeyValueTags(param.m_handle, (uint)i);
                        nRecord.keyValueTags = new StringKeyValuePair[tagCount];
                        for(int tagI = 0; tagI < tagCount; tagI++)
                        {
                            API.UserGeneratedContent.Client.GetQueryKeyValueTag(param.m_handle, (uint)i, (uint)tagI, out string key, 255, out string value, 255);
                            nRecord.keyValueTags[tagI].key = key?.Trim();
                            nRecord.keyValueTags[tagI].value = value?.Trim();
                        }

                        ResultsList.Add(nRecord);
                    }
                    ReleaseHandle();
                    if (_callback != null)
                        _callback.Invoke(this);
                }
                else
                {
                    Debug.LogError("HeathenWorkitemQuery|HandleQueryCompleted Unexpected results, state = " + param.m_eResult.ToString());
                }
            }
            else
            {
                Debug.LogError("HeathenWorkitemQuery|HandleQueryCompleted failed.");
            }
        }

        public void ReleaseHandle()
        {
            if (Handle != UGCQueryHandle_t.Invalid)
            {
                API.UserGeneratedContent.Client.ReleaseQueryRequest(Handle);
                Handle = UGCQueryHandle_t.Invalid;
            }
        }

        public void Dispose()
        {
            try
            {
                ReleaseHandle();
            }
            catch { }
        }

#if STEAM_LEGACY || STEAM_161
        public static UgcQuery GetSubscribed() => Get(API.UserGeneratedContent.Client.GetSubscribedItems());
#elif STEAM_162 || STEAM_163
        public static UgcQuery GetSubscribed(bool IncludeLocallyDisabled = false) => Get(API.UserGeneratedContent.Client.GetSubscribedItems(IncludeLocallyDisabled));
#endif
    }
}
#endif