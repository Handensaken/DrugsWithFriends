#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UIElements;

namespace Heathen.SteamworksIntegration.Editors
{
    public class SteamworksSettingsProvider : SettingsProvider
    {
        private SteamToolsSettings _settings;
        private readonly Dictionary<string, bool> _toggles = new();
        private string _newSettingName = string.Empty;

        private bool GetToggle(string name)
        {
            return _toggles.GetValueOrDefault(name, false);
        }

        private void SetToggle(string name, bool value)
        {
            if(!_toggles.TryAdd(name, value))
                _toggles[name] = value;
        }

        private bool this[string name]
        {
            get => GetToggle(name);
            set => SetToggle(name, value);
        }

        class Styles
        {
            public static GUIContent appId = new("Application ID");
            public static GUIContent callbackFrequency = new("Tick (Milliseconds)");
        }

        public SteamworksSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
            : base(path, scope) { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _settings = SteamToolsSettings.GetOrCreate();
        }

        public void UpdateAppDefine()
        {
            if (!_settings.ActiveApp.HasValue)
                return;

            string activeDefine = $"APP{_settings.ActiveApp.Value}";

            var targets = new[]
            {
                NamedBuildTarget.Standalone,
                NamedBuildTarget.Server,
            };

            foreach (var buildTarget in targets)
            {
                string defines = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
                var defineList = new HashSet<string>(defines.Split(';', StringSplitOptions.RemoveEmptyEntries));

                // Remove all APP#### defines except active
                var toRemove = new List<string>();
                bool hasActive = false;

                foreach (var d in defineList)
                {
                    if (d == activeDefine)
                    {
                        hasActive = true;
                        continue;
                    }

                    if (d.StartsWith("APP") &&
                        d.Length > 3 &&
                        uint.TryParse(d.Substring(3), out _) &&
                        d != activeDefine)
                    {
                        toRemove.Add(d);
                    }
                }

                if (hasActive && toRemove.Count == 0)
                    continue;

                foreach (var d in toRemove)
                    defineList.Remove(d);

                // Add active define
                defineList.Add(activeDefine);

                PlayerSettings.SetScriptingDefineSymbols(buildTarget, string.Join(";", defineList));
            }
        }

        public override void OnGUI(string searchContext)
        {
            if(!_settings.ActiveApp.HasValue)
            {
                _settings.activeAppIndex = -1;
                _settings.mainAppSettings ??= SteamToolsSettings.AppSettings.CreateDefault();
            }

            var options = new List<string>();
            var indices = new List<int>();

            // Always add main
            options.Add($"Main ({_settings.mainAppSettings.applicationId})");
            indices.Add(-1);

            // Optional demo
            if (_settings.demoAppSettings != null)
            {
                options.Add($"Demo ({_settings.demoAppSettings.applicationId})");
                indices.Add(-2);
            }

            // Optionally playtests
            if (_settings.playtestSettings != null)
            {
                for (int i = 0; i < _settings.playtestSettings.Count; i++)
                {
                    var app = _settings.playtestSettings[i];
                    options.Add($"{app.editorName} ({app.applicationId})");
                    indices.Add(i);
                }
            }

            // Find the current dropdown index
            int currentIndex = indices.IndexOf(_settings.activeAppIndex);
            if (currentIndex < 0) currentIndex = 0;

            EditorGUI.indentLevel++;
            // Show dropdown
            int newIndex = EditorGUILayout.Popup("Active Application", currentIndex, options.ToArray());
            EditorGUI.indentLevel--;

            // Update activeAppIndex
            int nIndex = indices[newIndex];
            bool needRestart = false;
            if (nIndex != _settings.activeAppIndex)
            {
                EditorUtility.SetDirty(this._settings);
                _settings.activeAppIndex = nIndex;
                needRestart = true;
            }
            
            UpdateAppDefine();

            if (needRestart)
            {
                bool restartNow = EditorUtility.DisplayDialog(
                    "Restart Required",
                    "Changing the active App ID requires restarting Unity. Do you want to restart now?",
                    "Restart Now",
                    "Restart Later"
                );

                if (restartNow)
                {
                    EditorApplication.OpenProject(Environment.CurrentDirectory);
                }
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUIStyle nStyle = new(EditorStyles.boldLabel)
            {
                fontSize = 18
            };
            EditorGUILayout.LabelField(" Global", nStyle);
            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.LinkButton("Knowledge Base"))
            {
                Application.OpenURL("https://kb.heathen.group/steamworks");
            }
            if (EditorGUILayout.LinkButton("Support"))
            {
                Application.OpenURL("https://discord.gg/heathen-group-463483739612381204");
            }
            if (EditorGUILayout.LinkButton("Leave a Review"))
            {
                Application.OpenURL("https://assetstore.unity.com/publishers/5836");
                Application.OpenURL("https://ie.trustpilot.com/review/heathen.group");
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Generate Wrapper", GUILayout.Height(24)))
            {
                _settings.CreateOrUpdateWrapper();
            }
            EditorGUILayout.Space(6);
            EditorGUI.indentLevel++;
            DrawDlcList();
            DrawInventoryArea();
            DrawServerSettings();
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(" Main", nStyle);
            if (EditorGUILayout.LinkButton("Steamworks Portal"))
            {
                Application.OpenURL("https://partner.steamgames.com/apps/landing/" + _settings.mainAppSettings.applicationId.ToString());
            }
                        
            DrawCommonSettings(_settings.mainAppSettings);
            DemoArea();
            PlaytestArea();
        }

        // Register the SettingsProvider
        [SettingsProvider]
        public static SettingsProvider CreateSteamworksSettingsProvider()
        {
            var provider = new SteamworksSettingsProvider("Project/Player/Steamworks")
            {
                // Automatically extract all keywords from the Styles.
                keywords = GetSearchKeywordsFromGUIContentProperties<Styles>()
            };
            return provider;
        }

        private void DemoArea()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            var nStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18
            };
            EditorGUILayout.LabelField(" Demo", nStyle);
            EditorGUILayout.Space();
            if (_settings.demoAppSettings != null)
            {
                if (EditorGUILayout.LinkButton("Steamworks Portal"))
                {
                    Application.OpenURL("https://partner.steamgames.com/apps/landing/" + _settings.demoAppSettings.applicationId.ToString());
                }
                
                DrawCommonSettings(_settings.demoAppSettings);
            }
            else
            {
                if (GUILayout.Button("Create Demo Settings"))
                {
                    GUI.FocusControl(null);

                    _settings.demoAppSettings = new() { editorName = "Demo" };
                }
            }
        }

        private void PlaytestArea()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUIStyle nStyle = new(EditorStyles.boldLabel)
            {
                fontSize = 18
            };
            EditorGUILayout.LabelField(" Playtests", nStyle);

            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            _newSettingName = EditorGUILayout.TextField("Playtest Name", _newSettingName);
            if (GUILayout.Button("Create Playtest Settings") && !string.IsNullOrEmpty(_newSettingName))
            {
                GUI.FocusControl(null);

                _settings.playtestSettings.Add(new()
                {
                    editorName = _newSettingName,
                    applicationId = 0
                });

                this._settings.isDirty = true;
                EditorUtility.SetDirty(this._settings);

                _newSettingName = string.Empty;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
            SteamToolsSettings.AppSettings settingsToRemove = null;
            foreach (var playtest in _settings.playtestSettings)
            {
                if (!PlaytestArea(playtest))
                {
                    settingsToRemove = playtest;
                    break;
                }
            }

            if(settingsToRemove != null)
                _settings.playtestSettings.Remove(settingsToRemove);
        }

        private bool PlaytestArea(SteamToolsSettings.AppSettings settings)
        {
            EditorGUILayout.Space();
            var nStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16
            };
            EditorGUILayout.LabelField(" " + settings.editorName, nStyle);
            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.LinkButton("Steamworks Portal"))
            {
                Application.OpenURL("https://partner.steamgames.com/apps/landing/" + settings.applicationId.ToString());
            }
            if (EditorGUILayout.LinkButton("Remove"))
            {
                return false;
            }
            EditorGUILayout.EndHorizontal();

            DrawCommonSettings(settings);
            return true;
        }

        private void DrawCommonSettings(SteamToolsSettings.AppSettings settings)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Debug Window"))
            {
                GUI.FocusControl(null);

                SteamInspector_Code.ShowExample();
            }

            if (GUILayout.Button("Clear All"))
            {
                GUI.FocusControl(null);

                settings.Clear();
                this._settings.isDirty = true;
                EditorUtility.SetDirty(this._settings);
            }

            if (GUILayout.Button("Set Test Values"))
            {
                GUI.FocusControl(null);
                settings.SetDefault();
                this._settings.isDirty = true;
                EditorUtility.SetDirty(this._settings);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel++;
            var id = EditorGUILayout.TextField("Application Id", settings.applicationId.ToString());
            if (uint.TryParse(id, out var buffer))
            {
                if (buffer != settings.applicationId)
                {
                    Undo.RecordObject(this._settings, "editor");
                    settings.applicationId = buffer;
                    this._settings.isDirty = true;
                    EditorUtility.SetDirty(this._settings);
                }
            }
            
            this[settings.editorName + "artifactFoldout"] = EditorGUILayout.Foldout(this[settings.editorName + "artifactFoldout"], "Artifacts");

            if (this[settings.editorName + "artifactFoldout"])
            {
                int il = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;

                DrawInputArea(settings);
                DrawStatsList(settings);
                DrawLeaderboardList(settings);
                DrawAchievementList(settings);

                EditorGUI.indentLevel = il;
            }
            EditorGUI.indentLevel--;
        }

        private void DrawServerSettings()
        {
            this["sgsFoldout"] = EditorGUILayout.Foldout(this["sgsFoldout"], "Steam Game Server Configuration");

            if (this["sgsFoldout"])
            {
                EditorGUILayout.BeginHorizontal();
                if (EditorGUILayout.LinkButton("Default"))
                {
                    _settings.defaultServerSettings = SteamGameServerConfiguration.Default;
                }
                if (EditorGUILayout.LinkButton("Clear"))
                {
                    _settings.defaultServerSettings = new();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                DrawServerToggleSettings();
                EditorGUILayout.Space();
                DrawConnectionSettings();
                EditorGUILayout.Space();
                DrawServerGeneralSettings();
            }
        }

        private void DrawServerGeneralSettings()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("General", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            if (!_settings.defaultServerSettings.anonymousServerLogin)
            {
                EditorGUILayout.HelpBox("If anonymous server login is not enabled then you must provide a game server token.", MessageType.Info);

                var token = EditorGUILayout.TextField("Token", _settings.defaultServerSettings.gameServerToken);

                if (token != _settings.defaultServerSettings.gameServerToken)
                {
                    Undo.RecordObject(_settings, "editor");
                    _settings.defaultServerSettings.gameServerToken = token;
                    _settings.isDirty = true;
                    EditorUtility.SetDirty(_settings);
                }
            }

            var serverName = EditorGUILayout.TextField("Server Name", _settings.defaultServerSettings.serverName);

            if (serverName != _settings.defaultServerSettings.serverName)
            {
                Undo.RecordObject(_settings, "editor");
                _settings.defaultServerSettings.serverName = serverName;
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }

            var serverVersion = EditorGUILayout.TextField("Server Version", _settings.defaultServerSettings.serverVersion);

            if (serverVersion != _settings.defaultServerSettings.serverVersion)
            {
                Undo.RecordObject(_settings, "editor");
                _settings.defaultServerSettings.serverVersion = serverVersion;
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }

            if (_settings.defaultServerSettings.supportSpectators)
            {
                serverName = EditorGUILayout.TextField("Spectator Name", _settings.defaultServerSettings.spectatorServerName);

                if (serverName != _settings.defaultServerSettings.spectatorServerName)
                {
                    Undo.RecordObject(_settings, "editor");
                    _settings.defaultServerSettings.spectatorServerName = serverName;
                    _settings.isDirty = true;
                    EditorUtility.SetDirty(_settings);
                }
            }

            serverName = EditorGUILayout.TextField("Description", _settings.defaultServerSettings.gameDescription);

            if (serverName != _settings.defaultServerSettings.gameDescription)
            {
                Undo.RecordObject(_settings, "editor");
                _settings.defaultServerSettings.gameDescription = serverName;
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }

            serverName = EditorGUILayout.TextField("Directory", _settings.defaultServerSettings.gameDirectory);

            if (serverName != _settings.defaultServerSettings.gameDirectory)
            {
                Undo.RecordObject(_settings, "editor");
                _settings.defaultServerSettings.gameDirectory = serverName;
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }

            serverName = EditorGUILayout.TextField("Map Name", _settings.defaultServerSettings.mapName);

            if (serverName != _settings.defaultServerSettings.mapName)
            {
                Undo.RecordObject(_settings, "editor");
                _settings.defaultServerSettings.mapName = serverName;
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }

            serverName = EditorGUILayout.TextField("Game Metadata", _settings.defaultServerSettings.gameData);

            if (serverName != _settings.defaultServerSettings.gameData)
            {
                Undo.RecordObject(_settings, "editor");
                _settings.defaultServerSettings.gameData = serverName;
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }

            var count = EditorGUILayout.TextField("Max Player Count", _settings.defaultServerSettings.maxPlayerCount.ToString());
            if (int.TryParse(count, out var buffer) && buffer != _settings.defaultServerSettings.maxPlayerCount)
            {
                Undo.RecordObject(_settings, "editor");
                _settings.defaultServerSettings.maxPlayerCount = buffer;
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }

            count = EditorGUILayout.TextField("Bot Player Count", _settings.defaultServerSettings.botPlayerCount.ToString());

            if (int.TryParse(count, out buffer) && buffer != _settings.defaultServerSettings.botPlayerCount)
            {
                Undo.RecordObject(_settings, "editor");
                _settings.defaultServerSettings.botPlayerCount = buffer;
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }
        }

        private void DrawConnectionSettings()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Connection", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            var address = API.Utilities.IPUintToString(_settings.defaultServerSettings.ip);
            var nAddress = EditorGUILayout.TextField("IP Address", address);

            if (address != nAddress)
            {
                try
                {
                    var nip = API.Utilities.IPStringToUint(nAddress);
                    if (nip != _settings.defaultServerSettings.ip)
                    {
                        Undo.RecordObject(_settings, "editor");
                        _settings.defaultServerSettings.ip = nip;
                        _settings.isDirty = true;
                        EditorUtility.SetDirty(_settings);
                    }
                }
                catch
                {
                    // ignored
                }
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Ports ");
            EditorGUILayout.EndHorizontal();

            var port = EditorGUILayout.TextField(new GUIContent("Game", "The port that clients will connect to for gameplay.  You will usually open up your own socket bound to this port."), _settings.defaultServerSettings.gamePort.ToString());
            ushort nPort;

            if (ushort.TryParse(port, out nPort) && nPort != _settings.defaultServerSettings.gamePort)
            {
                Undo.RecordObject(_settings, "editor");
                _settings.defaultServerSettings.gamePort = nPort;
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }

            port = EditorGUILayout.TextField(new GUIContent("Query", "The port that will manage server browser related duties and info pings from clients.\nIf you pass MASTERSERVERUPDATERPORT_USEGAMESOCKETSHARE (65535) for QueryPort, then it will use 'GameSocketShare' mode, which means that the game is responsible for sending and receiving UDP packets for the master server updater. See references to GameSocketShare in isteamgameserver.h"), _settings.defaultServerSettings.queryPort.ToString());

            if (ushort.TryParse(port, out nPort) && nPort != _settings.defaultServerSettings.queryPort)
            {
                Undo.RecordObject(_settings, "editor");
                _settings.defaultServerSettings.queryPort = nPort;
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }

            port = EditorGUILayout.TextField("Spectator", _settings.defaultServerSettings.spectatorPort.ToString());

            if (ushort.TryParse(port, out nPort) && nPort != _settings.defaultServerSettings.spectatorPort)
            {
                Undo.RecordObject(_settings, "editor");
                _settings.defaultServerSettings.spectatorPort = nPort;
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }
        }

        private void DrawServerToggleSettings()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Features", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            var autoInt = GUILayout.Toggle(_settings.defaultServerSettings.autoInitialise, (_settings.defaultServerSettings.autoInitialise ? "Disable" : "Enable") + " Auto-Initialise", EditorStyles.toolbarButton);
            var autoLog = GUILayout.Toggle(_settings.defaultServerSettings.autoLogon, (_settings.defaultServerSettings.autoLogon ? "Disable" : "Enable") + " Auto-Logon", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            var heart = GUILayout.Toggle(_settings.defaultServerSettings.enableHeartbeats, (_settings.defaultServerSettings.enableHeartbeats ? "Disable" : "Enable") + " Server Heartbeat", EditorStyles.toolbarButton);
            var anon = GUILayout.Toggle(_settings.defaultServerSettings.anonymousServerLogin, (_settings.defaultServerSettings.anonymousServerLogin ? "Disable" : "Enable") + " Anonymous Server Login", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            var gsAuth = GUILayout.Toggle(_settings.defaultServerSettings.usingGameServerAuthApi, (_settings.defaultServerSettings.usingGameServerAuthApi ? "Disable" : "Enable") + " Game Server Auth API", EditorStyles.toolbarButton);
            var pass = GUILayout.Toggle(_settings.defaultServerSettings.isPasswordProtected, (_settings.defaultServerSettings.isPasswordProtected ? "Disable" : "Enable") + " Password Protected", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            var dedicated = GUILayout.Toggle(_settings.defaultServerSettings.isDedicated, (_settings.defaultServerSettings.isDedicated ? "Disable" : "Enable") + " Dedicated Server", EditorStyles.toolbarButton);
            var spec = GUILayout.Toggle(_settings.defaultServerSettings.supportSpectators, (_settings.defaultServerSettings.supportSpectators ? "Disable" : "Enable") + " Spectator Support", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();
            
            if (autoInt != _settings.defaultServerSettings.autoInitialise)
            {
                Undo.RecordObject(_settings, "editor");
                _settings.defaultServerSettings.autoInitialise = autoInt;
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }

            if (heart != _settings.defaultServerSettings.enableHeartbeats)
            {
                Undo.RecordObject(_settings, "editor");
                _settings.defaultServerSettings.enableHeartbeats = heart;
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }

            if (spec != _settings.defaultServerSettings.supportSpectators)
            {
                Undo.RecordObject(_settings, "editor");
                _settings.defaultServerSettings.supportSpectators = spec;
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }

            if (anon != _settings.defaultServerSettings.anonymousServerLogin)
            {
                Undo.RecordObject(_settings, "editor");
                _settings.defaultServerSettings.anonymousServerLogin = anon;
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }

            if (gsAuth != _settings.defaultServerSettings.usingGameServerAuthApi)
            {
                Undo.RecordObject(_settings, "editor");
                _settings.defaultServerSettings.usingGameServerAuthApi = gsAuth;
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }

            if (pass != _settings.defaultServerSettings.isPasswordProtected)
            {
                Undo.RecordObject(_settings, "editor");
                _settings.defaultServerSettings.isPasswordProtected = pass;
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }

            if (dedicated != _settings.defaultServerSettings.isDedicated)
            {
                Undo.RecordObject(_settings, "editor");
                _settings.defaultServerSettings.isDedicated = dedicated;
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }

            if (autoLog != _settings.defaultServerSettings.autoLogon)
            {
                Undo.RecordObject(_settings, "editor");
                _settings.defaultServerSettings.autoLogon = autoLog;
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }
        }

        private void DrawStatsList(SteamToolsSettings.AppSettings settings)
        {
            this[settings.editorName + "statsFoldout"] = EditorGUILayout.Foldout(this[settings.editorName + "statsFoldout"], "Stats: " + settings.stats.Count);

            if (this[settings.editorName + "statsFoldout"])
            {
                int mil = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;

                var color = GUI.contentColor;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                GUI.contentColor = SteamTools.Colors.BrightGreen;
                if (GUILayout.Button("+ Int", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    GUI.FocusControl(null);

                    StatData nStat = "New_Int_Stat";
                    settings.stats.Add(nStat);
                    this._settings.isDirty = true;
                    EditorUtility.SetDirty(this._settings);
                }
                if (GUILayout.Button("+ Float", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    GUI.FocusControl(null);

                    StatData nStat = "New_Float_Stat";
                    settings.stats.Add(nStat);
                    this._settings.isDirty = true;
                    EditorUtility.SetDirty(this._settings);
                }
                if (GUILayout.Button("+ Avg Rate", EditorStyles.toolbarButton, GUILayout.Width(75)))
                {
                    GUI.FocusControl(null);

                    StatData nStat = "New_AvgRat_Stat";
                    settings.stats.Add(nStat);
                    this._settings.isDirty = true;
                    EditorUtility.SetDirty(this._settings);
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();

                int il = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;

                for (int i = 0; i < settings.stats.Count; i++)
                {
                    var target = settings.stats[i];

                    var sC = GUI.backgroundColor;

                    GUI.backgroundColor = sC;
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    var newName = EditorGUILayout.TextField(target);
                    if (!string.IsNullOrEmpty(newName) && newName != target)
                    {
                        Undo.RecordObject(this._settings, "name change");
                        settings.stats[i] = newName;
                        this._settings.isDirty = true;
                        EditorUtility.SetDirty(this._settings);
                    }

                    var terminate = false;
                    GUI.contentColor = SteamTools.Colors.ErrorRed;
                    if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                    {
                        settings.stats.RemoveAt(i);
                        terminate = true;
                        EditorUtility.SetDirty(this._settings);
                    }
                    GUI.contentColor = color;
                    EditorGUILayout.EndHorizontal();

                    if (terminate)
                    {
                        break;
                    }
                }
                EditorGUI.indentLevel = il;

                EditorGUI.indentLevel = mil;
            }
        }

        private void DrawAchievementList(SteamToolsSettings.AppSettings settings)
        {
            this[settings.editorName + "achievements"] = EditorGUILayout.Foldout(this[settings.editorName + "achievements"], "Achievements: " + settings.achievements.Count);

            if (this[settings.editorName + "achievements"])
            {
                var color = GUI.contentColor;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                GUI.contentColor = color;
                if (GUILayout.Button("Import", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    if (!API.App.Initialised)
                    {
                        Debug.Log("You cannot import data before initializing Steam API.");
                        return;
                    }

                    if (!SteamUser.BLoggedOn())
                    {
                        Debug.Log("You cannot import data when the Steam client is in offline mode.");
                        return;
                    }

                    try
                    {
                        GUI.FocusControl(null);

                        var names = API.StatsAndAchievements.Client.GetAchievementNames();
                        settings.achievements.Clear();
                        foreach (var name in names)
                            settings.achievements.Add(name);
                        EditorUtility.SetDirty(this._settings);
                    }
                    catch
                    {
                        Debug.LogWarning("Achievements can only be imported while the simulation is running, press play and try again to import.");
                    }
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();

                int il = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;

                for (int i = 0; i < settings.achievements.Count; i++)
                {
                    var target = settings.achievements[i];

                    var sC = GUI.backgroundColor;

                    GUI.backgroundColor = sC;
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

                    EditorGUILayout.LabelField(target.ApiName);
                    if (Application.isPlaying && API.App.Initialised)
                        EditorGUILayout.LabelField(target.IsAchieved ? "Unlocked" : "Locked");

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel = il;
            }
        }

        private void DrawLeaderboardList(SteamToolsSettings.AppSettings settings)
        {
            this[settings.editorName + "leaderboardFoldout"] = EditorGUILayout.Foldout(this[settings.editorName + "leaderboardFoldout"], "Leaderboards: " + settings.leaderboards.Count);

            if (this[settings.editorName + "leaderboardFoldout"])
            {
                var color = GUI.contentColor;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                GUI.contentColor = SteamTools.Colors.BrightGreen;
                if (GUILayout.Button("+ New", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    GUI.FocusControl(null);
                    Undo.RecordObject(this._settings, "editor");
                    LeaderboardData.GetAllRequest nStat = new()
                    {
                        name = "New Leaderboard",
                        create = false,
                        sort = ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending,
                        type = ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric,
                    };

                    settings.leaderboards.Add(nStat);
                    this._settings.isDirty = true;
                    EditorUtility.SetDirty(this._settings);
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();

                var bgColor = GUI.backgroundColor;

                for (int i = 0; i < settings.leaderboards.Count; i++)
                {
                    var item = settings.leaderboards[i];

                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

                    if (GUILayout.Button(new GUIContent(item.create ? "✓" : "-", "Create if missing?"), EditorStyles.toolbarButton, GUILayout.Width(20)))
                    {
                        GUI.FocusControl(null);
                        item.create = !item.create;
                        settings.leaderboards[i] = item;
                        
                        EditorUtility.SetDirty(this._settings);
                    }

                    var nVal = EditorGUILayout.TextField(item.name);
                    if (nVal != item.name)
                    {
                        item.name = nVal;
                        settings.leaderboards[i] = item;
                        var boards = settings.leaderboards.ToArray();
                        settings.leaderboards.Clear();
                        settings.leaderboards.AddRange(boards);
                        EditorUtility.SetDirty(this._settings);
                    }

                    GUI.contentColor = SteamTools.Colors.ErrorRed;
                    if (GUILayout.Button(new GUIContent("X", "Remove the object"), EditorStyles.toolbarButton, GUILayout.Width(25)))
                    {
                        GUI.FocusControl(null);
                        Undo.RecordObject(this._settings, "editor");
                        settings.leaderboards.RemoveAt(i);
                        this._settings.isDirty = true;
                        EditorUtility.SetDirty(this._settings);
                        return;
                    }
                    GUI.contentColor = color;
                    EditorGUILayout.EndHorizontal();
                }
                
                GUI.backgroundColor = bgColor;
            }
        }

        private void DrawDlcList()
        {
            this["dlcFoldout"] = EditorGUILayout.Foldout(this["dlcFoldout"], "Downloadable Content: " + _settings.dlc.Count);

            if (this["dlcFoldout"])
            {
                var color = GUI.contentColor;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

                if (GUILayout.Button("Import", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    if (!API.App.Initialised)
                    {
                        Debug.Log("You cannot import data before initializing Steam API.");
                        return;
                    }

                    if (!SteamUser.BLoggedOn())
                    {
                        Debug.Log("You cannot import data when the Steam client is in offline mode.");
                        return;
                    }

                    GUI.FocusControl(null);
                    try
                    {
                        var dlc = API.App.Client.Dlc;
                        _settings.dlcNames.Clear();
                        _settings.dlc.Clear();

                        foreach(var data in dlc)
                        {
                            _settings.dlc.Add(data);
                            _settings.dlcNames.Add(data.Name);
                        }

                        this._settings.isDirty = true;
                        EditorUtility.SetDirty(this._settings);
                    }
                    catch
                    {
                        Debug.LogWarning("DLC can only be imported while the simulation is running, press play and try again to import.");
                    }
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();

                var bgColor = GUI.backgroundColor;

                for (int i = 0; i < _settings.dlcNames.Count; i++)
                {
                    var item = _settings.dlcNames[i];

                    EditorGUILayout.LabelField(item);
                }

                GUI.backgroundColor = bgColor;
            }
        }

        private void DrawInventoryArea()
        {
            _settings.inventorySettings.items.RemoveAll(p => p == null);

            this["inventoryFoldout"] = EditorGUILayout.Foldout(this["inventoryFoldout"], "Inventory: " + _settings.inventorySettings.items.Count);

            if (this["inventoryFoldout"])
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                var color = GUI.contentColor;
                if (GUILayout.Button("Import", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    if (!API.App.Initialised)
                    {
                        Debug.Log("You cannot import data before initializing Steam API.");
                        return;
                    }

                    if (!SteamUser.BLoggedOn())
                    {
                        Debug.Log("You cannot import data when the Steam client is in offline mode.");
                        return;
                    }

                    GUI.FocusControl(null);

                    try
                    {
                        Debug.Log("Processing inventory item definition cache!");
                        _settings.inventorySettings.items.Clear();
                        _settings.inventorySettings.UpdateItemDefinitions(true);
                        Debug.Log("Requesting Refresh of Steam Inventory Item Definitions");

                        SteamTools.Events.OnInventoryDefinitionUpdate -= _settings.inventorySettings.HandleSettingsInventoryDefinitionUpdate;
                        SteamTools.Events.OnInventoryDefinitionUpdate += _settings.inventorySettings.HandleSettingsInventoryDefinitionUpdate;
                        API.Inventory.Client.LoadItemDefinitions();
                    }
                    catch
                    {
                        Debug.LogWarning("Failed to import data from Steam, make sure you have simulated/ran at least once in order to engage the Steam API.");
                    }

                    this._settings.isDirty = true;
                    EditorUtility.SetDirty(this._settings);
                    GUI.FocusControl(null);
                }
                GUI.contentColor = color;


                EditorGUILayout.EndHorizontal();

                this["itemsFoldout"] = EditorGUILayout.Foldout(this["itemsFoldout"], "Items: " + _settings.inventorySettings.items.Where(p => p.Type == InventoryItemType.Item).Count());

                if (this["itemsFoldout"])
                {
                    _settings.inventorySettings.items.Sort((a, b) => a.Id.m_SteamItemDef.CompareTo(b.Id.m_SteamItemDef));

                    for (int i = 0; i < _settings.inventorySettings.items.Count; i++)
                    {
                        var item = _settings.inventorySettings.items[i];

                        if (item.Type == InventoryItemType.Item)
                        {
                            if (DrawItem(item))
                                break;
                        }
                    }
                }

                this["bundlesFoldout"] = EditorGUILayout.Foldout(this["bundlesFoldout"], "Bundles: " + _settings.inventorySettings.items.Where(p => p.Type == InventoryItemType.Bundle).Count());

                if (this["bundlesFoldout"])
                {
                    _settings.inventorySettings.items.RemoveAll(p => p == null);
                    _settings.inventorySettings.items.Sort((a, b) => a.Id.m_SteamItemDef.CompareTo(b.Id.m_SteamItemDef));

                    for (int i = 0; i < _settings.inventorySettings.items.Count; i++)
                    {
                        var item = _settings.inventorySettings.items[i];

                        if (item.Type == InventoryItemType.Bundle)
                        {
                            if (DrawItem(item))
                                break;
                        }
                    }
                }

                this["generatorFoldout"] = EditorGUILayout.Foldout(this["generatorFoldout"], "Generators: " + _settings.inventorySettings.items.Where(p => p.Type == InventoryItemType.Generator).Count());

                if (this["generatorFoldout"])
                {
                    _settings.inventorySettings.items.RemoveAll(p => p == null);
                    _settings.inventorySettings.items.Sort((a, b) => a.Id.m_SteamItemDef.CompareTo(b.Id.m_SteamItemDef));

                    for (int i = 0; i < _settings.inventorySettings.items.Count; i++)
                    {
                        var item = _settings.inventorySettings.items[i];

                        if (item.Type == InventoryItemType.Generator)
                        {
                            if (DrawItem(item))
                                break;
                        }
                    }
                }

                this["playtimegeneratorFoldout"] = EditorGUILayout.Foldout(this["playtimegeneratorFoldout"], "Playtime Generators: " + _settings.inventorySettings.items.Where(p => p.Type == InventoryItemType.Playtimegenerator).Count());

                if (this["playtimegeneratorFoldout"])
                {
                    _settings.inventorySettings.items.RemoveAll(p => p == null);
                    _settings.inventorySettings.items.Sort((a, b) => a.Id.m_SteamItemDef.CompareTo(b.Id.m_SteamItemDef));

                    for (int i = 0; i < _settings.inventorySettings.items.Count; i++)
                    {
                        var item = _settings.inventorySettings.items[i];

                        if (item.Type == InventoryItemType.Playtimegenerator)
                        {
                            if (DrawItem(item))
                                break;
                        }
                    }
                }

                this["taggeneratorFoldout"] = EditorGUILayout.Foldout(this["taggeneratorFoldout"], "Tag Generators: " + _settings.inventorySettings.items.Where(p => p.Type == InventoryItemType.TagGenerator).Count());

                if (this["taggeneratorFoldout"])
                {
                    _settings.inventorySettings.items.RemoveAll(p => p == null);
                    _settings.inventorySettings.items.Sort((a, b) => a.Id.m_SteamItemDef.CompareTo(b.Id.m_SteamItemDef));

                    for (int i = 0; i < _settings.inventorySettings.items.Count; i++)
                    {
                        var item = _settings.inventorySettings.items[i];

                        if (item.Type == InventoryItemType.TagGenerator)
                        {
                            if (DrawItem(item))
                                break;
                        }
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        private void DrawInputArea(SteamToolsSettings.AppSettings settings)
        {
            //this[settings.name + "inputFoldout"]
            this[settings.editorName + "inputFoldout"] = EditorGUILayout.Foldout(this[settings.editorName + "inputFoldout"], "Input: " + (settings.actions.Count + settings.actionSets.Count + settings.actionSetLayers.Count).ToString());

            if (this[settings.editorName + "inputFoldout"])
            {

                EditorGUI.indentLevel++;

                this[settings.editorName + "inputActionSetFoldout"] = EditorGUILayout.Foldout(this[settings.editorName + "inputActionSetFoldout"], "Action Sets: " + settings.actionSets.Count.ToString());

                if (this[settings.editorName + "inputActionSetFoldout"])
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    var color = GUI.contentColor;
                    GUI.contentColor = SteamTools.Colors.BrightGreen;
                    if (GUILayout.Button("+ New", EditorStyles.toolbarButton, GUILayout.Width(50)))
                    {
                        GUI.FocusControl(null);

                        var nItem = "action_set";

                        settings.actionSets.Add(nItem);
                        this._settings.isDirty = true;
                        EditorUtility.SetDirty(this._settings);

                        GUI.FocusControl(null);
                    }
                    GUI.contentColor = color;
                    EditorGUILayout.EndHorizontal();

                    for (int i = 0; i < settings.actionSets.Count; i++)
                    {
                        if (DrawActionSet(settings, i))
                            break;
                    }
                }

                this[settings.editorName + "inputActionSetLayerFoldout"] = EditorGUILayout.Foldout(this[settings.editorName + "inputActionSetLayerFoldout"], "Action Set Layers: " + settings.actionSetLayers.Count.ToString());

                if (this[settings.editorName + "inputActionSetLayerFoldout"])
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    var color = GUI.contentColor;
                    GUI.contentColor = SteamTools.Colors.BrightGreen;
                    if (GUILayout.Button("+ New", EditorStyles.toolbarButton, GUILayout.Width(50)))
                    {
                        GUI.FocusControl(null);

                        string nItem = "action_set_layer";

                        settings.actionSetLayers.Add(nItem);
                        this._settings.isDirty = true;
                        EditorUtility.SetDirty(this._settings);

                        GUI.FocusControl(null);
                    }
                    GUI.contentColor = color;
                    EditorGUILayout.EndHorizontal();

                    for (int i = 0; i < settings.actionSetLayers.Count; i++)
                    {
                        if (DrawActionSetLayer(settings, i))
                            break;
                    }
                }

                this[settings.editorName + "inputActionFoldout"] = EditorGUILayout.Foldout(this[settings.editorName + "inputActionFoldout"], "Actions: " + settings.actions.Count.ToString());

                if (this[settings.editorName + "inputActionFoldout"])
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    var color = GUI.contentColor;
                    GUI.contentColor = SteamTools.Colors.BrightGreen;
                    if (GUILayout.Button("+ New", EditorStyles.toolbarButton, GUILayout.Width(50)))
                    {
                        GUI.FocusControl(null);

                        InputActionData nItem = new("action", InputActionType.Digital);

                        settings.actions.Add(nItem);
                        this._settings.isDirty = true;
                        EditorUtility.SetDirty(this._settings);

                        GUI.FocusControl(null);
                    }
                    GUI.contentColor = color;
                    EditorGUILayout.EndHorizontal();

                    for (int i = 0; i < settings.actions.Count; i++)
                    {
                        if (DrawAction(settings, i))
                            break;
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        private static bool DrawItem(ItemDefinitionSettings item)
        {
            var color = GUI.contentColor;

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

            // Draw the item name as a label
            EditorGUILayout.LabelField(item.Name, EditorStyles.boldLabel);

            GUI.contentColor = color;
            EditorGUILayout.EndHorizontal();
            return false;
        }

        private bool DrawActionSet(SteamToolsSettings.AppSettings settings, int setIndex)
        {
            var color = GUI.contentColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            var result = EditorGUILayout.TextField(settings.actionSets[setIndex]);

            if (result != settings.actionSets[setIndex])
            {
                settings.actionSets[setIndex] = result;
                this._settings.isDirty = true;
                EditorUtility.SetDirty(this._settings);
            }

            GUI.contentColor = SteamTools.Colors.ErrorRed;
            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                GUI.FocusControl(null);
                settings.actionSets.RemoveAt(setIndex);
                this._settings.isDirty = true;
                EditorUtility.SetDirty(this._settings);
                return true;
            }
            GUI.contentColor = color;
            EditorGUILayout.EndHorizontal();
            return false;
        }

        private bool DrawActionSetLayer(SteamToolsSettings.AppSettings settings, int index)
        {
            var color = GUI.contentColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            var result = EditorGUILayout.TextField(settings.actionSetLayers[index]);

            if (result != settings.actionSetLayers[index])
            {
                settings.actionSetLayers[index] = result;
                this._settings.isDirty = true;
                EditorUtility.SetDirty(this._settings);
            }

            GUI.contentColor = SteamTools.Colors.ErrorRed;
            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                GUI.FocusControl(null);
                settings.actionSetLayers.RemoveAt(index);
                this._settings.isDirty = true;
                EditorUtility.SetDirty(this._settings);
                return true;
            }
            GUI.contentColor = color;
            EditorGUILayout.EndHorizontal();
            return false;
        }

        private bool DrawAction(SteamToolsSettings.AppSettings settings, int index)
        {
            var color = GUI.contentColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            var item = settings.actions[index];
            if (item.Type == InputActionType.Digital)
            {
                if (GUILayout.Button(new GUIContent("DI", "Click to make this an analog action."), EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    item = new(item.Name, InputActionType.Analog);
                    settings.actions[index] = item;
                    GUI.FocusControl(null);
                    this._settings.isDirty = true;
                    EditorUtility.SetDirty(this._settings);
                }
            }
            else
            {
                if (GUILayout.Button(new GUIContent("AI", "Click to make this a digital action."), EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    item = new(item.Name, InputActionType.Digital);
                    settings.actions[index] = item;
                    GUI.FocusControl(null);
                    this._settings.isDirty = true;
                    EditorUtility.SetDirty(this._settings);
                }
            }

            var result = EditorGUILayout.TextField(item.Name);

            if (result != item.Name)
            {
                item = new(result, item.Type);
                settings.actions[index] = item;
                GUI.FocusControl(null);
                this._settings.isDirty = true;
                EditorUtility.SetDirty(this._settings);
            }

            GUI.contentColor = SteamTools.Colors.ErrorRed;
            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                GUI.FocusControl(null);
                settings.actions.RemoveAt(index);
                this._settings.isDirty = true;
                EditorUtility.SetDirty(this._settings);
                return true;
            }
            GUI.contentColor = color;
            EditorGUILayout.EndHorizontal();
            return false;
        }
    }
}
#endif