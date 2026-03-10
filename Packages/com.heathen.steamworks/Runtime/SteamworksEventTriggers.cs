#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Heathen.SteamworksIntegration
{
    [DisallowMultipleComponent]
    public class SteamworksEventTriggers : MonoBehaviour
    {
        [FormerlySerializedAs("mDelegates")] 
        [FormerlySerializedAs("m_Delegates")] 
        [SerializeField]
        private List<SteamEventTriggerType> delegates;

        private void Awake()
        {
            SteamTools.Events.OnSteamInitialised += initSuccess.Invoke;
            SteamTools.Events.OnSteamInitialisationError += initFailed.Invoke;

            SteamTools.Events.OnDlcInstalled += dlcInstalled.Invoke;
            SteamTools.Events.OnNewUrlLaunchParameters += newUrlLaunchParameters.Invoke;
            SteamTools.Events.OnSteamServersDisconnected += serversDisconnected.Invoke;
            SteamTools.Events.OnSteamServerConnectFailure += serversConnectFailure.Invoke;
            SteamTools.Events.OnSteamServersConnected += serversConnected.Invoke;

            SteamTools.Events.OnGamepadTextInputShown += gamepadTextInputShown.Invoke;
            SteamTools.Events.OnGamepadTextInputDismissed += gamepadTextInputDismissed.Invoke;

            SteamTools.Events.OnGameConnectedChatLeave += gameConnectedChatLeave.Invoke;
            SteamTools.Events.OnGameConnectedClanChatMsg += chatMessageReceived.Invoke;
            SteamTools.Events.OnGameConnectedChatJoin += gameConnectedChatJoin.Invoke;

            SteamTools.Events.OnGameConnectedFriendChatMsg += gameConnectedFriendChatMsg.Invoke;
            SteamTools.Events.OnFriendRichPresenceUpdate += friendRichPresenceUpdate.Invoke;
            SteamTools.Events.OnPersonaStateChange += personaStateChange.Invoke;

            SteamTools.Events.OnInputDataChanged += inputDataChanged.Invoke;
            SteamTools.Events.OnControllerConnected += controllerConnected.Invoke;
            SteamTools.Events.OnControllerDisconnected += controllerDisconnected.Invoke;

            SteamTools.Events.OnInventoryDefinitionUpdate += inventoryDefinitionUpdate.Invoke;
            SteamTools.Events.OnInventoryResultReady += inventoryResultReady.Invoke;
            SteamTools.Events.OnMicroTxnAuthorisationResponse += microTransactionAuthorizationResponse.Invoke;

            SteamTools.Events.OnFavoritesListChanged += serverFavoritesListChanged.Invoke;
            SteamTools.Events.OnAskedToLeaveLobby += lobbyAskedToLeave.Invoke;
            SteamTools.Events.OnLobbyAuthentication += lobbyAuthenticationRequest.Invoke;
            SteamTools.Events.OnLobbyChatMsg += lobbyChatMsg.Invoke;
            SteamTools.Events.OnLobbyChatUpdate += lobbyChatUpdate.Invoke;
            SteamTools.Events.OnLobbyDataUpdate += lobbyChatDataUpdate.Invoke;
            SteamTools.Events.OnLobbyEnterFailed += lobbyEnterFailed.Invoke;
            SteamTools.Events.OnLobbyEnterSuccess += lobbyEnterSuccess.Invoke;
            SteamTools.Events.OnLobbyGameServer += lobbyGameCreated.Invoke;
            SteamTools.Events.OnLobbyInvite += lobbyInvite.Invoke;
            SteamTools.Events.OnLobbyLeave += lobbyLeave.Invoke;

            SteamTools.Events.OnLobbyJoinRequested += gameLobbyJoinRequested.Invoke;
            SteamTools.Events.OnRichPresenceJoinRequested += gameRichPresenceJoinRequested.Invoke;
            SteamTools.Events.OnGameServerChangeRequested += gameServerChangeRequested.Invoke;
            SteamTools.Events.OnGameOverlayActivated += gameOverlayActivated.Invoke;

            SteamTools.Events.OnActiveBeaconsUpdated += activeBeaconsUpdated.Invoke;
            SteamTools.Events.OnAvailableBeaconLocationsUpdated += availableBeaconLocationsUpdated.Invoke;
            SteamTools.Events.OnReservationNotification += reservationNotificationCallback.Invoke;

            SteamTools.Events.OnRemotePlaySessionConnected += remotePlaySessionConnected.Invoke;
            SteamTools.Events.OnRemotePlaySessionDisconnected += remotePlaySessionDisconnected.Invoke;
            
            SteamTools.Events.OnRemoteStorageLocalFileChange += remoteStorageFileChange.Invoke;

            SteamTools.Events.OnScreenshotReady += screenshotReady.Invoke;
            SteamTools.Events.OnScreenshotRequested += screenshotRequested.Invoke;

            SteamTools.Events.OnStatsReceived += statsReceived.Invoke;
            SteamTools.Events.OnStatsStored += statsStored.Invoke;
            SteamTools.Events.OnStatsUnloaded += statsUnloaded.Invoke;
            SteamTools.Events.OnUserAchievementStored += achievementStored.Invoke;

            SteamTools.Events.OnAppResumeFromSuspend += appResumeFromSuspend.Invoke;
            SteamTools.Events.OnKeyboardShown += keyboardShown.Invoke;
            SteamTools.Events.OnKeyboardClosed += keyboardClosed.Invoke;
        }

        private void OnDestroy()
        {
            SteamTools.Events.OnSteamInitialised -= initSuccess.Invoke;
            SteamTools.Events.OnSteamInitialisationError -= initFailed.Invoke;

            SteamTools.Events.OnDlcInstalled -= dlcInstalled.Invoke;
            SteamTools.Events.OnNewUrlLaunchParameters -= newUrlLaunchParameters.Invoke;
            SteamTools.Events.OnSteamServersDisconnected -= serversDisconnected.Invoke;
            SteamTools.Events.OnSteamServerConnectFailure -= serversConnectFailure.Invoke;
            SteamTools.Events.OnSteamServersConnected -= serversConnected.Invoke;

            SteamTools.Events.OnGamepadTextInputShown -= gamepadTextInputShown.Invoke;
            SteamTools.Events.OnGamepadTextInputDismissed -= gamepadTextInputDismissed.Invoke;

            SteamTools.Events.OnGameConnectedChatLeave -= gameConnectedChatLeave.Invoke;
            SteamTools.Events.OnGameConnectedClanChatMsg -= chatMessageReceived.Invoke;
            SteamTools.Events.OnGameConnectedChatJoin -= gameConnectedChatJoin.Invoke;

            SteamTools.Events.OnGameConnectedFriendChatMsg -= gameConnectedFriendChatMsg.Invoke;
            SteamTools.Events.OnFriendRichPresenceUpdate -= friendRichPresenceUpdate.Invoke;
            SteamTools.Events.OnPersonaStateChange -= personaStateChange.Invoke;

            SteamTools.Events.OnInputDataChanged -= inputDataChanged.Invoke;
            SteamTools.Events.OnControllerConnected -= controllerConnected.Invoke;
            SteamTools.Events.OnControllerDisconnected -= controllerDisconnected.Invoke;

            SteamTools.Events.OnInventoryDefinitionUpdate -= inventoryDefinitionUpdate.Invoke;
            SteamTools.Events.OnInventoryResultReady -= inventoryResultReady.Invoke;
            SteamTools.Events.OnMicroTxnAuthorisationResponse -= microTransactionAuthorizationResponse.Invoke;
            
            SteamTools.Events.OnFavoritesListChanged -= serverFavoritesListChanged.Invoke;
            SteamTools.Events.OnAskedToLeaveLobby -= lobbyAskedToLeave.Invoke;
            SteamTools.Events.OnLobbyAuthentication -= lobbyAuthenticationRequest.Invoke;
            SteamTools.Events.OnLobbyChatMsg -= lobbyChatMsg.Invoke;
            SteamTools.Events.OnLobbyChatUpdate -= lobbyChatUpdate.Invoke;
            SteamTools.Events.OnLobbyDataUpdate -= lobbyChatDataUpdate.Invoke;
            SteamTools.Events.OnLobbyEnterFailed -= lobbyEnterFailed.Invoke;
            SteamTools.Events.OnLobbyEnterSuccess -= lobbyEnterSuccess.Invoke;
            SteamTools.Events.OnLobbyGameServer -= lobbyGameCreated.Invoke;
            SteamTools.Events.OnLobbyInvite -= lobbyInvite.Invoke;
            SteamTools.Events.OnLobbyLeave -= lobbyLeave.Invoke;

            SteamTools.Events.OnLobbyJoinRequested -= gameLobbyJoinRequested.Invoke;
            SteamTools.Events.OnRichPresenceJoinRequested -= gameRichPresenceJoinRequested.Invoke;
            SteamTools.Events.OnGameServerChangeRequested -= gameServerChangeRequested.Invoke;
            SteamTools.Events.OnGameOverlayActivated -= gameOverlayActivated.Invoke;

            SteamTools.Events.OnActiveBeaconsUpdated -= activeBeaconsUpdated.Invoke;
            SteamTools.Events.OnAvailableBeaconLocationsUpdated -= availableBeaconLocationsUpdated.Invoke;
            SteamTools.Events.OnReservationNotification -= reservationNotificationCallback.Invoke;

            SteamTools.Events.OnRemotePlaySessionConnected -= remotePlaySessionConnected.Invoke;
            SteamTools.Events.OnRemotePlaySessionDisconnected -= remotePlaySessionDisconnected.Invoke;

            SteamTools.Events.OnRemoteStorageLocalFileChange -= remoteStorageFileChange.Invoke;

            SteamTools.Events.OnScreenshotReady -= screenshotReady.Invoke;
            SteamTools.Events.OnScreenshotRequested -= screenshotRequested.Invoke;

            SteamTools.Events.OnStatsReceived -= statsReceived.Invoke;
            SteamTools.Events.OnStatsStored -= statsStored.Invoke;
            SteamTools.Events.OnStatsUnloaded -= statsUnloaded.Invoke;
            SteamTools.Events.OnUserAchievementStored -= achievementStored.Invoke;

            SteamTools.Events.OnAppResumeFromSuspend -= appResumeFromSuspend.Invoke;
            SteamTools.Events.OnKeyboardClosed -= keyboardClosed.Invoke;   
            SteamTools.Events.OnKeyboardShown -= keyboardShown.Invoke;
        }

        public UnityEvent initSuccess;
        public StringEvent initFailed;
        public DlcInstalledEvent dlcInstalled;
        public UnityEvent newUrlLaunchParameters;
        public API.App.Client.UnityEventServersDisconnected serversDisconnected;
        public UnityEvent serversConnected;
        public API.App.Client.UnityEventServersConnectFailure serversConnectFailure;
        public UnityEvent gamepadTextInputShown;
        public UnityEvent<bool, string> gamepadTextInputDismissed;
        public UnityEvent<ChatRoom, UserData, string, EChatEntryType> chatMessageReceived;
        public GameConnectedChatJoinEvent gameConnectedChatJoin;
        public UnityEvent<ChatRoom, UserData, bool, bool> gameConnectedChatLeave;
        public GameConnectedFriendChatMsgEvent gameConnectedFriendChatMsg;
        public UnityEvent<UserData, AppData> friendRichPresenceUpdate;
        public UnityEvent<UserData, EPersonaChange> personaStateChange;
        public ControllerDataEvent inputDataChanged;
        public UnityEvent<InputHandle_t> controllerConnected;
        public UnityEvent<InputHandle_t> controllerDisconnected;
        public SteamInventoryDefinitionUpdateEvent inventoryDefinitionUpdate;
        public SteamInventoryResultReadyEvent inventoryResultReady;
        public UnityEvent<AppData, ulong, bool> microTransactionAuthorizationResponse;
        public FavoritesListChangedEvent serverFavoritesListChanged;
        public LobbyDataEvent lobbyAskedToLeave;
        public LobbyAuthenticationEvent lobbyAuthenticationRequest;
        public LobbyChatMsgEvent lobbyChatMsg;
        public UnityEvent<LobbyData, UserData, EChatMemberStateChange> lobbyChatUpdate;
        public UnityEvent<LobbyData, LobbyMemberData?> lobbyChatDataUpdate;
        public UnityEvent<LobbyData, EChatRoomEnterResponse> lobbyEnterFailed;
        public UnityEvent<LobbyData> lobbyEnterSuccess;
        public UnityEvent<LobbyData, CSteamID, string, ushort> lobbyGameCreated;
        public UnityEvent<UserData, LobbyData, GameData> lobbyInvite;
        public LobbyDataEvent lobbyLeave;
        public GameLobbyJoinRequestedEvent gameLobbyJoinRequested;
        public UnityEvent<bool> gameOverlayActivated;
        public GameRichPresenceJoinRequestedEvent gameRichPresenceJoinRequested;
        public UnityEvent<string,string> gameServerChangeRequested;
        public UnityEvent activeBeaconsUpdated;
        public UnityEvent availableBeaconLocationsUpdated;
        public UnityEvent<UserData, PartyBeaconID_t> reservationNotificationCallback;
        public UnityEvent<RemotePlaySessionID_t> remotePlaySessionConnected;
        public UnityEvent<RemotePlaySessionID_t> remotePlaySessionDisconnected;
        public UnityEvent remoteStorageFileChange;
        public UnityEvent<ScreenshotHandle, EResult> screenshotReady;
        public UnityEvent screenshotRequested;
        public UnityEvent<UserAchievementStoredData> achievementStored;
        public UnityEvent<GameData, EResult, UserData> statsReceived;
        public UnityEvent<GameData, EResult> statsStored;
        public UnityEvent<UserData> statsUnloaded;
        public UnityEvent appResumeFromSuspend;
        public UnityEvent keyboardClosed;
        public UnityEvent keyboardShown;
    }

    public enum SteamEventTriggerType
    {
        AchievementStored,
        ActiveBeaconsUpdated,
        AppResumeFromSuspend,
        AvailableBeaconLocationsUpdated,
        ChatMessageReceived,
        ControllerConnected,
        ControllerDisconnected,
        DlcInstalled,
        FriendRichPresenceUpdate,
        GameConnectedChatJoin,
        GameConnectedChatLeave,
        GameConnectedFriendChatMsg,
        GamepadTextInputDismissed,
        GamepadTextShown,
        InitializationError,
        InitializationSuccess,        
        InputDataChanged,
        InventoryDefinitionUpdate,
        InventoryResultReady,
        KeyboardClosed,
        KeyboardShown,
        LobbyAskedToLeave,
        LobbyAuthenticationRequest,
        LobbyChatMsg,
        LobbyChatUpdate,
        LobbyDataUpdate,
        LobbyEnterFailed,
        LobbyEnterSuccess,
        LobbyGameCreated,
        LobbyInvite,
        LobbyJoinRequested,
        LobbyLeave,
        MicroTransactionAuthorizationResponse,
        NewUrlLaunchParameters,   
        OverlayActivated,
        PersonaStateChange,
        RemotePlaySessionConnected,
        RemotePlaySessionDisconnected,
        RemoteStorageFileChanged,
        ReservationNotificationCallback,
        RichPresenceJoinRequested,
        ScreenshotReady,
        ScreenshotRequested,
        ServerChangeRequested,
        ServersConnectFailure,
        ServersConnected,
        ServersDisconnected,
        ServerFavoritesListChanged,
        StatsReceived,
        StatsStored,
        StatsUnloaded,
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SteamworksEventTriggers), true)]
    public class SteamworksEventTriggersEditor : Editor
    {
        SerializedProperty _mDelegatesProperty;

        GUIContent _mIconToolbarMinus;
        GUIContent _mEventIDName;
        GUIContent[] _mEventTypes;
        GUIContent _mAddButtonContent;

        protected virtual void OnEnable()
        {
            _mDelegatesProperty = serializedObject.FindProperty("delegates");
            _mAddButtonContent = new GUIContent("Add New Event Type");
            _mEventIDName = new GUIContent("");
            // Have to create a copy since otherwise the tooltip will be overwritten.
            _mIconToolbarMinus = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus"))
            {
                tooltip = "Remove all events in this list."
            };

            string[] eventNames = Enum.GetNames(typeof(SteamEventTriggerType));
            _mEventTypes = new GUIContent[eventNames.Length];
            for (int i = 0; i < eventNames.Length; ++i)
            {
                _mEventTypes[i] = new GUIContent(ObjectNames.NicifyVariableName(eventNames[i]));
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            int toBeRemovedEntry = -1;

            EditorGUILayout.Space();

            Vector2 removeButtonSize = GUIStyle.none.CalcSize(_mIconToolbarMinus);

            for (int i = 0; i < _mDelegatesProperty.arraySize; ++i)
            {
                SerializedProperty delegateProperty = _mDelegatesProperty.GetArrayElementAtIndex(i);
                //SerializedProperty eventProperty = delegateProperty.FindPropertyRelative("eventID");
                //SerializedProperty callbacksProperty = delegateProperty.FindPropertyRelative("callback");
                _mEventIDName.text = delegateProperty.enumDisplayNames[delegateProperty.enumValueIndex];

                switch ((SteamEventTriggerType)delegateProperty.enumValueIndex)
                {
                    case SteamEventTriggerType.AchievementStored:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.achievementStored)), _mEventIDName);
                        break;
                    case SteamEventTriggerType.ActiveBeaconsUpdated:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.activeBeaconsUpdated)), _mEventIDName);
                        break;
                    case SteamEventTriggerType.AppResumeFromSuspend:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.appResumeFromSuspend)), _mEventIDName);
                        break;
                    case SteamEventTriggerType.AvailableBeaconLocationsUpdated:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.availableBeaconLocationsUpdated)), _mEventIDName);
                        break;
                    case SteamEventTriggerType.ChatMessageReceived:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.chatMessageReceived)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.ControllerConnected:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.controllerConnected)), _mEventIDName);
                        break;
                    case SteamEventTriggerType.ControllerDisconnected:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.controllerDisconnected)), _mEventIDName);
                        break;
                    case SteamEventTriggerType.DlcInstalled:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.dlcInstalled)), _mEventIDName);
                        break;
                    case SteamEventTriggerType.FriendRichPresenceUpdate:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.friendRichPresenceUpdate)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.GameConnectedChatJoin:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.gameConnectedChatJoin)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.GameConnectedChatLeave:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.gameConnectedChatLeave)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.GameConnectedFriendChatMsg:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.gameConnectedFriendChatMsg)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.GamepadTextInputDismissed:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.gamepadTextInputDismissed)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.GamepadTextShown:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.gamepadTextInputShown)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.InputDataChanged:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.inputDataChanged)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.InventoryDefinitionUpdate:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.inventoryDefinitionUpdate)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.InventoryResultReady:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.inventoryResultReady)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.KeyboardClosed:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.keyboardClosed)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.KeyboardShown:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.keyboardShown)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.LobbyAskedToLeave:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.lobbyAskedToLeave)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.LobbyAuthenticationRequest:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.lobbyAuthenticationRequest)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.LobbyChatMsg:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.lobbyChatMsg)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.LobbyChatUpdate:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.lobbyChatUpdate)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.LobbyDataUpdate:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.lobbyChatDataUpdate)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.LobbyEnterFailed:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.lobbyEnterFailed)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.LobbyEnterSuccess:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.lobbyEnterSuccess)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.LobbyGameCreated:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.lobbyGameCreated)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.LobbyInvite:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.lobbyInvite)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.LobbyJoinRequested:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.gameLobbyJoinRequested)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.LobbyLeave:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.lobbyLeave)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.MicroTransactionAuthorizationResponse:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.microTransactionAuthorizationResponse)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.NewUrlLaunchParameters:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.newUrlLaunchParameters)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.OverlayActivated:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.gameOverlayActivated)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.PersonaStateChange:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.personaStateChange)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.RemotePlaySessionConnected:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.remotePlaySessionConnected)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.RemotePlaySessionDisconnected:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.remotePlaySessionDisconnected)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.RemoteStorageFileChanged:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.remoteStorageFileChange)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.ReservationNotificationCallback:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.reservationNotificationCallback)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.RichPresenceJoinRequested:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.gameRichPresenceJoinRequested)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.ScreenshotReady:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.screenshotReady)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.ScreenshotRequested:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.screenshotRequested)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.ServerChangeRequested:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.gameServerChangeRequested)), _mEventIDName); break;
                    case SteamEventTriggerType.ServerFavoritesListChanged:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.serverFavoritesListChanged)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.ServersConnected:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.serversConnected)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.ServersConnectFailure:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.serversConnectFailure)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.ServersDisconnected:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.serversDisconnected)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.StatsReceived:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.statsReceived)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.StatsStored:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.statsStored)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.StatsUnloaded:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.statsUnloaded)), _mEventIDName); 
                        break;
                    case SteamEventTriggerType.InitializationError:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.initFailed)), _mEventIDName);
                        break;
                        case SteamEventTriggerType.InitializationSuccess:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SteamworksEventTriggers.initSuccess)), _mEventIDName);
                        break;
                }
                
                var callbackRect = GUILayoutUtility.GetLastRect();

                var removeButtonPos = new Rect(callbackRect.xMax - removeButtonSize.x - 8, callbackRect.y + 1, removeButtonSize.x, removeButtonSize.y);
                if (GUI.Button(removeButtonPos, _mIconToolbarMinus, GUIStyle.none))
                {
                    toBeRemovedEntry = i;
                }

                EditorGUILayout.Space();
            }

            if (toBeRemovedEntry > -1)
            {
                RemoveEntry(toBeRemovedEntry);
            }

            Rect btPosition = GUILayoutUtility.GetRect(_mAddButtonContent, GUI.skin.button);
            const float addButtonWidth = 200f;
            btPosition.x = btPosition.x + (btPosition.width - addButtonWidth) / 2;
            btPosition.width = addButtonWidth;
            if (GUI.Button(btPosition, _mAddButtonContent))
            {
                ShowAddTriggerMenu();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void RemoveEntry(int toBeRemovedEntry)
        {
            _mDelegatesProperty.DeleteArrayElementAtIndex(toBeRemovedEntry);
        }

        void ShowAddTriggerMenu()
        {
            // Now create the menu, add items and show it
            GenericMenu menu = new GenericMenu();
            for (int i = 0; i < _mEventTypes.Length; ++i)
            {
                bool active = true;

                // Check if we already have a Entry for the current eventType, if so, disable it
                for (int p = 0; p < _mDelegatesProperty.arraySize; ++p)
                {
                    SerializedProperty delegateEntry = _mDelegatesProperty.GetArrayElementAtIndex(p);
                    //SerializedProperty eventProperty = delegateEntry.FindPropertyRelative("eventID");
                    if (delegateEntry.enumValueIndex == i)
                    {
                        active = false;
                    }
                }
                if (active)
                    menu.AddItem(_mEventTypes[i], false, OnAddNewSelected, i);
                else
                    menu.AddDisabledItem(_mEventTypes[i]);
            }
            menu.ShowAsContext();
            Event.current.Use();
        }

        private void OnAddNewSelected(object index)
        {
            int selected = (int)index;

            _mDelegatesProperty.arraySize += 1;
            SerializedProperty delegateEntry = _mDelegatesProperty.GetArrayElementAtIndex(_mDelegatesProperty.arraySize - 1);
            //SerializedProperty eventProperty = delegateEntry.FindPropertyRelative("eventID");
            delegateEntry.enumValueIndex = selected;
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif