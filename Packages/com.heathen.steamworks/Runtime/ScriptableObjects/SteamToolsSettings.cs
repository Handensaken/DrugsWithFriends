#if UNITY_EDITOR && !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    public class SteamToolsSettings : ScriptableObject
    {
        [Serializable]
        public struct NameAndID : IEquatable<NameAndID>, IComparable<NameAndID>
        {
            public string name;
            public int id;

            public int CompareTo(NameAndID other)
            {
                return string.Compare(name, other.name, StringComparison.Ordinal);
            }

            public bool Equals(NameAndID other)
            {
                // HashSet uniqueness is based on 'name' only
                return string.Equals(name, other.name);
            }

            public override bool Equals(object obj)
            {
                return obj is NameAndID other && Equals(other);
            }

            public override int GetHashCode()
            {
                // Only use 'name' for hash code, so HashSet considers it unique per name
                return name != null ? name.GetHashCode() : 0;
            }
        }

        [Serializable]
        public class AppSettings
        {
            public string editorName;
            public uint applicationId;
            public List<LeaderboardData.GetAllRequest> leaderboards = new();
            public List<StatData> stats = new();
            public List<AchievementData> achievements = new();
            public List<string> actionSets = new();
            public List<string> actionSetLayers = new();
            public List<InputActionData> actions = new();

            public static AppSettings CreateDefault()
            {
                var app = new AppSettings
                {
                    editorName = "Main",
                    applicationId = 480
                };
                app.leaderboards.Add(new LeaderboardData.GetAllRequest { create = false, name = "Feet Traveled", sort = ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending, type = ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric });
                app.stats.Add("AverageSpeed");
                app.stats.Add("FeetTraveled");
                app.stats.Add("MaxFeetTraveled");
                app.stats.Add("NumGames");
                app.stats.Add("NumLosses");
                app.stats.Add("NumWins");
                app.stats.Add("Unused2");
                app.achievements.Add("ACH_TRAVEL_FAR_ACCUM");
                app.achievements.Add("ACH_TRAVEL_FAR_SINGLE");
                app.achievements.Add("ACH_WIN_100_GAMES");
                app.achievements.Add("ACH_WIN_ONE_GAME");
                app.achievements.Add("NEW_ACHIEVEMENT_0_4");
                app.actionSets.Add("menu_controls");
                app.actionSets.Add("ship_controls");
                app.actionSetLayers.Add("thrust_action_layer");
                app.actions.Add(new("analog_controls", InputActionType.Analog));
                app.actions.Add(new("backward_thrust", InputActionType.Digital));
                app.actions.Add(new("fire_lasers", InputActionType.Digital));
                app.actions.Add(new("forward_thrust", InputActionType.Digital));
                app.actions.Add(new("menu_cancel", InputActionType.Digital));
                app.actions.Add(new("menu_down", InputActionType.Digital));
                app.actions.Add(new("menu_left", InputActionType.Digital));
                app.actions.Add(new("menu_right", InputActionType.Digital));
                app.actions.Add(new("menu_select", InputActionType.Digital));
                app.actions.Add(new("menu_up", InputActionType.Digital));
                app.actions.Add(new("pause_menu", InputActionType.Digital));
                app.actions.Add(new("turn_left", InputActionType.Digital));
                app.actions.Add(new("turn_right", InputActionType.Digital));

                return app;
            }
            public static AppSettings CreateEmpty()
            {
                var app = new AppSettings();
                app.editorName = "Empty";

                return app;
            }

            public void Clear()
            {
                leaderboards.Clear();
                stats.Clear();
                achievements.Clear();
                actionSets.Clear();
                actions.Clear();
                actionSetLayers.Clear();
            }

            public void SetDefault()
            {
                leaderboards.Clear();
                stats.Clear();
                achievements.Clear();
                actionSets.Clear();
                actions.Clear();
                actionSetLayers.Clear();

                applicationId = 480;
                leaderboards.Add(new LeaderboardData.GetAllRequest { create = false, name = "Feet Traveled", sort = ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending, type = ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric });
                stats.Add("AverageSpeed");
                stats.Add("FeetTraveled");
                stats.Add("MaxFeetTraveled");
                stats.Add("NumGames");
                stats.Add("NumLosses");
                stats.Add("NumWins");
                stats.Add("Unused2");
                achievements.Add("ACH_TRAVEL_FAR_ACCUM");
                achievements.Add("ACH_TRAVEL_FAR_SINGLE");
                achievements.Add("ACH_WIN_100_GAMES");
                achievements.Add("ACH_WIN_ONE_GAME");
                achievements.Add("NEW_ACHIEVEMENT_0_4");
                actionSets.Add("menu_controls");
                actionSets.Add("ship_controls");
                actionSetLayers.Add("thrust_action_layer");
                actions.Add(new("analog_controls", InputActionType.Analog));
                actions.Add(new("backward_thrust", InputActionType.Digital));
                actions.Add(new("fire_lasers", InputActionType.Digital));
                actions.Add(new("forward_thrust", InputActionType.Digital));
                actions.Add(new("menu_cancel", InputActionType.Digital));
                actions.Add(new("menu_down", InputActionType.Digital));
                actions.Add(new("menu_left", InputActionType.Digital));
                actions.Add(new("menu_right", InputActionType.Digital));
                actions.Add(new("menu_select", InputActionType.Digital));
                actions.Add(new("menu_up", InputActionType.Digital));
                actions.Add(new("pause_menu", InputActionType.Digital));
                actions.Add(new("turn_left", InputActionType.Digital));
                actions.Add(new("turn_right", InputActionType.Digital));
            }
        }

        public uint? ActiveApp
        {
            get
            {
                if (activeAppIndex == -1) return mainAppSettings?.applicationId;
                if (activeAppIndex == -2) return demoAppSettings?.applicationId;
                if (activeAppIndex >= 0 && activeAppIndex < playtestSettings.Count)
                    return playtestSettings[activeAppIndex]?.applicationId;
                return null;
            }
        }

        public DateTime LastGenerated;
        [HideInInspector]
        public int activeAppIndex = -1;
        [HideInInspector]
        public AppSettings mainAppSettings = AppSettings.CreateDefault();
        [HideInInspector]
        public AppSettings demoAppSettings;
        [HideInInspector]
        public List<string> dlcNames = new();
        [HideInInspector]
        public List<uint> dlc = new();
        [HideInInspector]
        public SteamGameServerConfiguration defaultServerSettings;
        [HideInInspector]
        public InventorySettings inventorySettings = new();
        [HideInInspector]
        public List<AppSettings> playtestSettings = new();
        [HideInInspector]
        public List<uint> appIds = new();
        [HideInInspector]
        public List<string> leaderboards = new();
        [HideInInspector]
        public List<string> stats = new();
        [HideInInspector]
        public List<string> achievements = new();
        [HideInInspector]
        public List<string> inputSets = new();
        [HideInInspector]
        public List<string> inputLayers = new();
        [HideInInspector]
        public List<string> inputActions = new();
        [HideInInspector]
        public List<NameAndID> items = new();
        [HideInInspector]
        public bool isDirty = true;

#if UNITY_EDITOR
        //private AppSettings current = null;

        public AppSettings Get(uint appId)
        {
            if (mainAppSettings.applicationId == appId)
                return mainAppSettings;
            else if (demoAppSettings?.applicationId == appId)
                return demoAppSettings;
            else
            {
                foreach (var playtest in playtestSettings)
                {
                    if (playtest?.applicationId == appId)
                        return playtest;
                }
            }

            return null;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static SteamToolsSettings GetOrCreate()
        {
            // Search for any existing SteamToolsSettings asset in the project
            var guids = UnityEditor.AssetDatabase.FindAssets("t:SteamToolsSettings");
            SteamToolsSettings asset = null;

            if (guids != null && guids.Length > 0)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                asset = UnityEditor.AssetDatabase.LoadAssetAtPath<SteamToolsSettings>(path);
            }

            // If none found, create a new one at a sensible default location
            if (asset) 
                return asset;
            
            const string defaultPath = "Assets/Settings/SteamToolsSettings.asset";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(defaultPath) ?? string.Empty);

            asset = CreateInstance<SteamToolsSettings>();
            UnityEditor.AssetDatabase.CreateAsset(asset, defaultPath);
            UnityEditor.AssetDatabase.SaveAssets();

            Debug.Log("[SteamToolsSettings] Created new settings asset at " + defaultPath);

            return asset;
        }

        public void CollectUniqueData()
        {
            // Clear existing lists
            appIds.Clear();
            leaderboards.Clear();
            stats.Clear();
            achievements.Clear();
            inputSets.Clear();
            inputLayers.Clear();
            inputActions.Clear();
            items.Clear();

            var uniqueAppIds = new HashSet<uint>();
            var uniqueLeaderboards = new HashSet<string>();
            var uniqueStats = new HashSet<string>();
            var uniqueAchievements = new HashSet<string>();
            var uniqueInputSets = new HashSet<string>();
            var uniqueInputLayers = new HashSet<string>();
            var uniqueInputActions = new HashSet<string>();
            var uniqueItems = new HashSet<NameAndID>();

            if (inventorySettings?.items != null)
            {
                foreach (var item in inventorySettings.items.Where(item => !string.IsNullOrEmpty(item?.itemName.GetSimpleValue())))
                    uniqueItems.Add(new() { name = item.itemName.GetSimpleValue(), id = item.id });
            }

            // Collect from all apps
            CollectFromApp(mainAppSettings);
            CollectFromApp(demoAppSettings);
            foreach (var app in playtestSettings)
                CollectFromApp(app);

            // Assign to public lists
            appIds.AddRange(uniqueAppIds);
            leaderboards.AddRange(uniqueLeaderboards);
            stats.AddRange(uniqueStats);
            achievements.AddRange(uniqueAchievements);
            inputSets.AddRange(uniqueInputSets);
            inputLayers.AddRange(uniqueInputLayers);
            inputActions.AddRange(uniqueInputActions);
            items.AddRange(uniqueItems);

            // Sort all lists
            leaderboards.Sort();
            stats.Sort();
            achievements.Sort();
            inputSets.Sort();
            inputLayers.Sort();
            inputActions.Sort();
            items.Sort();
            dlcNames.Sort();

            isDirty = true; // mark dirty since lists changed
            return;

            // Helper to walk one AppSettings
            void CollectFromApp(AppSettings app)
            {
                if (app == null) return;

                uniqueAppIds.Add(app.applicationId);

                if (app.leaderboards != null)
                {
                    foreach (var lb in app.leaderboards.Where(lb => !string.IsNullOrEmpty(lb.name)))
                        uniqueLeaderboards.Add(lb.name);
                }

                if (app.stats != null)
                {
                    foreach (var s in app.stats.Where(s => !string.IsNullOrEmpty(s)))
                        uniqueStats.Add(s);
                }

                if (app.achievements != null)
                {
                    foreach (var a in app.achievements.Where(a => !string.IsNullOrEmpty(a)))
                        uniqueAchievements.Add(a);
                }

                if (app.actionSets != null)
                {
                    foreach (var set in app.actionSets.Where(set => !string.IsNullOrEmpty(set)))
                        uniqueInputSets.Add(set);
                }

                if (app.actionSetLayers != null)
                {
                    foreach (var layer in app.actionSetLayers.Where(layer => !string.IsNullOrEmpty(layer)))
                        uniqueInputLayers.Add(layer);
                }

                if (app.actions != null)
                {
                    foreach (var act in app.actions.Where(act => !string.IsNullOrEmpty(act.Name)))
                        uniqueInputActions.Add(act.Name);
                }
            }
        }

        public void CreateOrUpdateWrapper()
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("SteamTools.Game t:Script");
            var path = (from guid in guids select UnityEditor.AssetDatabase.GUIDToAssetPath(guid) into p let text = System.IO.File.ReadAllText(p) where text.Contains("namespace Heathen.SteamTools") select p).FirstOrDefault();

            if (string.IsNullOrEmpty(path))
                path = "Assets/Scripts/Generated/SteamTools.Game.cs";

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path) ?? string.Empty);

            CollectUniqueData();
            var code = GenerateWrapperCode();
            System.IO.File.WriteAllText(path, code);
            UnityEditor.AssetDatabase.Refresh();
            LastGenerated = DateTime.Now;
        }

        private static string MakeValidIdentifier(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "_unknown";

            // 1. Replace any non-letter/digit/_ with _
            // This handles spaces, hyphens, and special characters
            var identifier = Regex.Replace(name, @"[^a-zA-Z0-9_]", "_");

            // 2. Collapse multiple underscores into one for cleaner names (e.g., "My Variable" -> "My_Variable")
            identifier = Regex.Replace(identifier, @"_+", "_");

            // 3. Trim leading/trailing underscores
            identifier = identifier.Trim('_');

            // 4. If it's now empty (e.g. input was "!@#"), give it a default
            if (string.IsNullOrEmpty(identifier))
            {
                var hexPart = string.Join("", name.Select(c => ((int)c).ToString("X2")));
                return $"_Hex_{hexPart}";
            }

            // 5. C# identifiers cannot start with a digit
            if (char.IsDigit(identifier[0]))
                identifier = "_" + identifier;

            // 6. Handle C# Reserved Keywords
            // If the name is "class" or "event", we prepend @ or _ 
            // Prepending @ allows it to be used as a literal identifier
            if (IsReservedKeyword(identifier))
                identifier = "@" + identifier;

            return identifier;
        }
        
        private static bool IsReservedKeyword(string word)
        {
            var keywords = new HashSet<string> {
                "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
                "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
                "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
                "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
                "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
                "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
                "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw",
                "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using",
                "virtual", "void", "volatile", "while"
            };
            return keywords.Contains(word);
        }

        private string GenerateWrapperCode()
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("using Heathen.SteamworksIntegration;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine("namespace SteamTools");
            sb.AppendLine("{");
            sb.AppendLine("    public static class Game");
            sb.AppendLine("    {");
            sb.AppendLine("        // Generated by SteamToolsSettings");
            sb.AppendLine();

            //-------------------------------------------------
            // AppId section
            //-------------------------------------------------
            for (var i = 0; i < appIds.Count; i++)
            {
                var id = appIds[i];
                sb.AppendLine(i == 0 ? $"#if APP{id}" : $"#elif APP{id}");
                sb.AppendLine($"        public const uint AppId = {id};");
            }
            sb.AppendLine("#else");
            sb.AppendLine("        public const uint AppId = 0;");
            sb.AppendLine("#endif");

            //-------------------------------------------------
            // DLC section
            //-------------------------------------------------
            if (dlcNames.Count > 0)
            {
                bool wroteDlc = false;

                if (!wroteDlc)
                {
                    sb.AppendLine("        public static class DLC");
                    sb.AppendLine("        {");
                    wroteDlc = true;
                }

                for (var ii = 0; ii < dlcNames.Count; ii++)
                {
                    var foundItem = dlc[ii];
                    string safeName = MakeValidIdentifier(dlcNames[ii]);
                    sb.AppendLine($"            public static DlcData {safeName} = {foundItem};");
                }

                if (wroteDlc)
                {
                    sb.AppendLine("        }");
                    sb.AppendLine();
                }
            }

            //-------------------------------------------------
            // Server Config section
            //-------------------------------------------------
            sb.AppendLine("        // Default server configuration");
            sb.AppendLine("        public static SteamGameServerConfiguration ServerConfiguration = new()");
            sb.AppendLine("        {");
            sb.AppendLine($"            autoInitialise = {defaultServerSettings.autoInitialise.ToString().ToLower()},");
            sb.AppendLine($"            autoLogon = {defaultServerSettings.autoLogon.ToString().ToLower()},");
            sb.AppendLine($"            ip = {defaultServerSettings.ip},");
            sb.AppendLine($"            gamePort = {defaultServerSettings.gamePort},");
            sb.AppendLine($"            queryPort = {defaultServerSettings.queryPort},");
            sb.AppendLine($"            spectatorPort = {defaultServerSettings.spectatorPort},");
            sb.AppendLine($"            serverVersion = \"{defaultServerSettings.serverVersion}\",");
            sb.AppendLine($"            usingGameServerAuthApi = {defaultServerSettings.usingGameServerAuthApi.ToString().ToLower()},");
            sb.AppendLine($"            enableHeartbeats = {defaultServerSettings.enableHeartbeats.ToString().ToLower()},");
            sb.AppendLine($"            supportSpectators = {defaultServerSettings.supportSpectators.ToString().ToLower()},");
            sb.AppendLine($"            spectatorServerName = \"{defaultServerSettings.spectatorServerName}\",");
            sb.AppendLine($"            anonymousServerLogin = {defaultServerSettings.anonymousServerLogin.ToString().ToLower()},");
            sb.AppendLine($"            gameServerToken = \"{defaultServerSettings.gameServerToken}\",");
            sb.AppendLine($"            isPasswordProtected = {defaultServerSettings.isPasswordProtected.ToString().ToLower()},");
            sb.AppendLine($"            serverName = \"{defaultServerSettings.serverName}\",");
            sb.AppendLine($"            gameDescription = \"{defaultServerSettings.gameDescription}\",");
            sb.AppendLine($"            gameDirectory = \"{defaultServerSettings.gameDirectory}\",");
            sb.AppendLine($"            isDedicated = {defaultServerSettings.isDedicated.ToString().ToLower()},");
            sb.AppendLine($"            maxPlayerCount = {defaultServerSettings.maxPlayerCount},");
            sb.AppendLine($"            botPlayerCount = {defaultServerSettings.botPlayerCount},");
            sb.AppendLine($"            mapName = \"{defaultServerSettings.mapName}\",");
            sb.AppendLine($"            gameData = \"{defaultServerSettings.gameData}\",");
            sb.AppendLine($"            rulePairs = null");
            sb.AppendLine("        };");
            sb.AppendLine();
            sb.AppendLine("        public static void ServerConfigFromIni(string iniData) => ServerConfiguration = SteamGameServerConfiguration.ParseIniString(iniData);");
            sb.AppendLine("        public static void ServerConfigFromJson(string jsonData) => ServerConfiguration = JsonUtility.FromJson<SteamGameServerConfiguration>(jsonData);");

            //-------------------------------------------------
            // Game.Initialise section
            //-------------------------------------------------
            sb.AppendLine();
            sb.AppendLine("        public static void Initialise()");
            sb.AppendLine("        {");
            sb.AppendLine($"              Debug.Log($\"Initialising for app {{AppId}}\");");
            sb.AppendLine($"              {typeof(SteamTools.Events)}.{nameof(SteamTools.Events.OnSteamInitialised)} += HandleInitialised;");
            sb.AppendLine("#if UNITY_SERVER");
            sb.AppendLine($"              {typeof(API.App.Server).FullName.Replace('+', '.')}.{nameof(API.App.Server.Initialise)}(AppId, ServerConfiguration);");
            sb.AppendLine("#else");
            sb.AppendLine("             List<InputActionData> actions = new();");
            sb.AppendLine();
            for (var i = 0; i < appIds.Count; i++)
            {
                var appId = appIds[i];
                var appSettings = Get(appId);

                sb.AppendLine(i == 0 ? $"#if APP{appId}" : $"#elif APP{appId}");
                foreach (var actions in appSettings.actions)
                {
                    var type = "InputActionType.Digital";
                    if (actions.Type == InputActionType.Analog)
                        type = "InputActionType.Analog";
                    sb.AppendLine($"                actions.Add(new(\"{actions.Name}\", {type}));");
                }
            }
            sb.AppendLine($"#endif");
            sb.AppendLine();
            sb.AppendLine($"             {typeof(API.App.Client).FullName.Replace('+', '.')}.{nameof(API.App.Client.Initialise)}(AppId, actions.ToArray());");
            sb.AppendLine($"#endif");
            sb.AppendLine("        }");

            //-------------------------------------------------
            // Game.HandleInitialised section
            //-------------------------------------------------
            sb.AppendLine();
            sb.AppendLine("        private static void HandleInitialised()");
            sb.AppendLine("        {");
            if (inputSets.Count > 0)
                sb.AppendLine("            Inputs.Sets.Initialise();");

            if (leaderboards.Count > 0)
            {
                for (var i = 0; i < appIds.Count; i++)
                {
                    var appId = appIds[i];
                    var appSettings = Get(appId);
                    sb.AppendLine(i == 0 ? $"#if APP{appId}" : $"#elif APP{appId}");
                    if (appSettings.leaderboards.Count > 0)
                    {
                        sb.AppendLine($"            int boardCount = {appSettings.leaderboards.Count};");
                        sb.AppendLine($"            int returnedBoards = 0;");
                        foreach (var leaderboard in appSettings.leaderboards)
                        {
                            var display = leaderboard.type switch
                            {
                                ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeSeconds =>
                                    "ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeSeconds",
                                ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeMilliSeconds =>
                                    "ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeMilliSeconds",
                                _ => "ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric"
                            };

                            var sort = leaderboard.sort == ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending ? "ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending" : "ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending";

                            if (leaderboard.create)
                            {
                                sb.AppendLine($"            LeaderboardData.GetOrCreate(\"{leaderboard.name}\", {display}, {sort}, (result, ioError) =>");
                                sb.AppendLine($"            {{");
                                sb.AppendLine($"                if(!ioError)");
                                sb.AppendLine($"                    Leaderboards.{MakeValidIdentifier(leaderboard.name)} = result;");
                                sb.AppendLine();
                                sb.AppendLine($"                returnedBoards++;");
                                sb.AppendLine($"                if(returnedBoards >= boardCount)");
                                sb.AppendLine($"                {{");
                                sb.AppendLine(leaderboards.Count > 0
                                    ? $"                    Dictionary<string, LeaderboardData> boardMap = Leaderboards.GetMap();"
                                    : $"                    Dictionary<string, LeaderboardData> boardMap = new();");

                                sb.AppendLine(inputSets.Count > 0
                                    ? $"                    Dictionary<string, InputActionSetData> setMap = Inputs.Sets.GetMap();"
                                    : $"                    Dictionary<string, InputActionSetData> setMap = new();");

                                sb.AppendLine(inputActions.Count > 0
                                    ? $"                    Dictionary<string, InputActionData> actionMap = Inputs.Actions.GetMap();"
                                    : $"                    Dictionary<string, InputActionData> actionMap = new();");

                                sb.AppendLine($"                    Interface.RaiseOnReady(boardMap, setMap, actionMap);");
                                sb.AppendLine($"                }}");
                                sb.AppendLine($"            }});");

                            }
                            else
                            {
                                sb.AppendLine($"            LeaderboardData.Get(\"{leaderboard.name}\", (result, ioError) =>");
                                sb.AppendLine($"            {{");
                                sb.AppendLine($"                if(!ioError)");
                                sb.AppendLine($"                    Leaderboards.{MakeValidIdentifier(leaderboard.name)} = result;");
                                sb.AppendLine();
                                sb.AppendLine($"                returnedBoards++;");
                                sb.AppendLine($"                if(returnedBoards >= boardCount)");
                                sb.AppendLine($"                {{");
                                sb.AppendLine(leaderboards.Count > 0
                                    ? $"                    Dictionary<string, LeaderboardData> boardMap = Leaderboards.GetMap();"
                                    : $"                    Dictionary<string, LeaderboardData> boardMap = new();");

                                sb.AppendLine(inputSets.Count > 0
                                    ? $"                    Dictionary<string, InputActionSetData> setMap = Inputs.Sets.GetMap();"
                                    : $"                    Dictionary<string, InputActionSetData> setMap = new();");

                                sb.AppendLine(inputActions.Count > 0
                                    ? $"                    Dictionary<string, InputActionData> actionMap = Inputs.Actions.GetMap();"
                                    : $"                    Dictionary<string, InputActionData> actionMap = new();");

                                sb.AppendLine("                    Interface.RaiseOnReady(boardMap, setMap, actionMap);");
                                sb.AppendLine($"                }}");
                                sb.AppendLine($"            }});");
                            }
                        }
                    }
                    else
                    {
                        sb.AppendLine(leaderboards.Count > 0
                            ? $"                Dictionary<string, LeaderboardData> boardMap = Leaderboards.GetMap();"
                            : $"                Dictionary<string, LeaderboardData> boardMap = new();");

                        sb.AppendLine(inputSets.Count > 0
                            ? $"                Dictionary<string, InputActionSetData> setMap = Inputs.Sets.GetMap();"
                            : $"                Dictionary<string, InputActionSetData> setMap = new();");

                        sb.AppendLine(inputActions.Count > 0
                            ? $"                Dictionary<string, InputActionData> actionMap = Inputs.Actions.GetMap();"
                            : $"                Dictionary<string, InputActionData> actionMap = new();");

                        sb.AppendLine("                Interface.RaiseOnReady(boardMap, setMap, actionMap);");
                    }
                }
                sb.AppendLine($"#endif");
            }
            else
            {
                sb.AppendLine(leaderboards.Count > 0
                    ? $"                Dictionary<string, LeaderboardData> boardMap = Leaderboards.GetMap();"
                    : $"                Dictionary<string, LeaderboardData> boardMap = new();");

                sb.AppendLine(inputSets.Count > 0
                    ? $"                Dictionary<string, InputActionSetData> setMap = Inputs.Sets.GetMap();"
                    : $"                Dictionary<string, InputActionSetData> setMap = new();");

                sb.AppendLine(inputActions.Count > 0
                    ? $"                Dictionary<string, InputActionData> actionMap = Inputs.Actions.GetMap();"
                    : $"                Dictionary<string, InputActionData> actionMap = new();");

                sb.AppendLine("                Interface.RaiseOnReady(boardMap, setMap, actionMap);");
            }

            sb.AppendLine("        }");

            //-------------------------------------------------
            // Stats section
            //-------------------------------------------------
            if (stats.Count > 0)
            {
                sb.AppendLine("        public static class Stats");
                sb.AppendLine("        {");

                for (var i = 0; i < appIds.Count; i++)
                {
                    var appId = appIds[i];
                    var appSettings = Get(appId);

                    sb.AppendLine(i == 0 ? $"#if APP{appId}" : $"#elif APP{appId}");
                    foreach (var statName in stats)
                    {
                        var matchStat = appSettings.stats.Find(s => s.ApiName == statName);
                        sb.AppendLine(string.IsNullOrEmpty(matchStat)
                            ? $"            public static StatData {MakeValidIdentifier(statName)};"
                            : $"            public static StatData {MakeValidIdentifier(statName)} = \"{statName}\";");
                    }
                }

                sb.AppendLine("#else");
                foreach (var statName in stats)
                {
                    sb.AppendLine($"            public static StatData {MakeValidIdentifier(statName)};");
                }
                sb.AppendLine("#endif");
                
                sb.AppendLine();
                sb.AppendLine("            public static Dictionary<string, StatData> GetMap() => new()");
                sb.AppendLine("            {");
                foreach (var statName in stats)
                {
                    sb.AppendLine($"                {{ \"{statName}\", {MakeValidIdentifier(statName)} }},");
                }
                sb.AppendLine("            };");
                sb.AppendLine("        }");
                sb.AppendLine();
            }

            //-------------------------------------------------
            // Achievements section
            //-------------------------------------------------
            if (achievements.Count > 0)
            {
                sb.AppendLine("        public static class Achievements");
                sb.AppendLine("        {");

                for (var i = 0; i < appIds.Count; i++)
                {
                    var appId = appIds[i];
                    var appSettings = Get(appId);

                    sb.AppendLine(i == 0 ? $"#if APP{appId}" : $"#elif APP{appId}");
                    foreach (var achievementName in achievements)
                    {
                        var matchApiName = appSettings.achievements.Find(p => p.ApiName == achievementName);
                        sb.AppendLine(string.IsNullOrEmpty(matchApiName)
                            ? $"            public static AchievementData {MakeValidIdentifier(achievementName)};"
                            : $"            public static AchievementData {MakeValidIdentifier(achievementName)} = \"{achievementName}\";");
                    }
                }

                sb.AppendLine("#else");
                foreach (var achievementName in achievements)
                    sb.AppendLine($"            public static AchievementData {MakeValidIdentifier(achievementName)};");
                sb.AppendLine("#endif");

                sb.AppendLine();
                sb.AppendLine("            public static Dictionary<string, AchievementData> GetMap() => new()");
                sb.AppendLine("            {");
                foreach (var achievementName in achievements)
                {
                    sb.AppendLine($"                {{ \"{achievementName}\", {MakeValidIdentifier(achievementName)} }},");
                }
                sb.AppendLine("            };");
                sb.AppendLine("        }");
                sb.AppendLine();
            }

            //-------------------------------------------------
            // Leaderboards section
            //-------------------------------------------------
            if (leaderboards.Count > 0)
            {
                sb.AppendLine("        public static class Leaderboards");
                sb.AppendLine("        {");
                foreach (var leaderboardName in leaderboards)
                {
                    sb.AppendLine($"            public static LeaderboardData {MakeValidIdentifier(leaderboardName)};");
                }
                sb.AppendLine();
                sb.AppendLine("            public static Dictionary<string, LeaderboardData> GetMap() => new()");
                sb.AppendLine("            {");
                foreach (var leaderboardName in leaderboards)
                {
                    sb.AppendLine($"                {{ \"{leaderboardName}\", {MakeValidIdentifier(leaderboardName)} }},");
                }
                sb.AppendLine("            };");
                sb.AppendLine("        }");
                sb.AppendLine();
            }

            //-------------------------------------------------
            // Inputs section
            //-------------------------------------------------
            if (inputSets.Count > 0)
            {
                sb.AppendLine("        public static class Inputs");
                sb.AppendLine("        {");                

                // ---- Sets ----
                if (inputSets.Count > 0)
                {
                    sb.AppendLine("            public static class Sets");
                    sb.AppendLine("            {");
                    sb.AppendLine();
                    sb.AppendLine("                public static Dictionary<string, InputActionSetData> GetMap() => new()");
                    sb.AppendLine("                {");
                    foreach (var setName in inputSets)
                    {
                        sb.AppendLine($"                    {{ \"{setName}\", {MakeValidIdentifier(setName)} }},");
                    }
                    sb.AppendLine("                };");
                    sb.AppendLine();
                    sb.AppendLine("                public static void Initialise()");
                    sb.AppendLine("                {");
                    sb.AppendLine("#if UNITY_SERVER");
                    sb.AppendLine("                    return;");
                    sb.AppendLine("#endif");

                    for (var i = 0; i < appIds.Count; i++)
                    {
                        var appId = appIds[i];
                        var appSettings = Get(appId);

                        sb.AppendLine(i == 0 ? $"#if APP{appId}" : $"#elif APP{appId}");
                        foreach (var setName in appSettings.actionSets)
                        {
                            sb.AppendLine($"                    {MakeValidIdentifier(setName)} = InputActionSetData.Get(\"{setName}\");");
                        }
                    }
                    sb.AppendLine("#endif");
                    sb.AppendLine("                }");

                    foreach (var setName in inputSets)
                    {
                        sb.AppendLine($"                public static InputActionSetData {MakeValidIdentifier(setName)};");
                    }
                    sb.AppendLine("            }");
                    sb.AppendLine();
                }

                // ---- Layers ----
                if (inputLayers.Count > 0)
                {
                    sb.AppendLine("            public static class Layers");
                    sb.AppendLine("            {");

                    for (var i = 0; i < appIds.Count; i++)
                    {
                        var appId = appIds[i];
                        var appSettings = Get(appId);
                        sb.AppendLine(i == 0 ? $"#if APP{appId}" : $"#elif APP{appId}");
                        foreach (var layerName in inputLayers)
                        {
                            var exists = appSettings.actionSetLayers.Contains(layerName);
                            sb.AppendLine(exists
                                ? $"                public static InputActionSetLayerData {MakeValidIdentifier(layerName)} = new() {{ LayerName = \"{layerName}\" }};"
                                : $"                public static InputActionSetLayerData {MakeValidIdentifier(layerName)};");
                        }
                    }
                    sb.AppendLine("#else");
                    foreach (var layerName in inputLayers)
                    {
                        sb.AppendLine($"                public static InputActionSetLayerData {MakeValidIdentifier(layerName)};");
                    }
                    sb.AppendLine("#endif");

                    sb.AppendLine();
                    sb.AppendLine("                public static Dictionary<string, InputActionSetLayerData> GetMap() => new()");
                    sb.AppendLine("                {");
                    foreach (var layerName in inputLayers)
                    {
                        sb.AppendLine($"                    {{ \"{layerName}\", {MakeValidIdentifier(layerName)} }},");
                    }
                    sb.AppendLine("                };");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                }

                // ---- Actions ----
                if (inputActions.Count > 0)
                {
                    sb.AppendLine("            public static class Actions");
                    sb.AppendLine("            {");
                    sb.AppendLine();

                    for (int i = 0; i < appIds.Count; i++)
                    {
                        var appId = appIds[i];
                        var appSettings = Get(appId);
                        sb.AppendLine(i == 0 ? $"#if APP{appId}" : $"#elif APP{appId}");
                        foreach (var actionName in inputActions)
                        {
                            var match = appSettings.actions.Find(a => a.Name == actionName);
                            if (!string.IsNullOrEmpty(match.Name))
                            {
                                sb.AppendLine($"                public static InputActionData {MakeValidIdentifier(actionName)} = new(\"{match.Name}\", InputActionType.{match.Type});");
                            }
                            else
                            {
                                sb.AppendLine($"                public static InputActionData {MakeValidIdentifier(actionName)};");
                            }
                        }
                    }
                    sb.AppendLine("#else");
                    foreach (var actionName in inputActions)
                    {
                        sb.AppendLine($"                public static InputActionData {MakeValidIdentifier(actionName)};");
                    }
                    sb.AppendLine("#endif");

                    sb.AppendLine();
                    sb.AppendLine("                public static Dictionary<string, InputActionData> GetMap() => new()");
                    sb.AppendLine("                {");
                    foreach (var actionName in inputActions)
                    {
                        sb.AppendLine($"                    {{ \"{actionName}\", {MakeValidIdentifier(actionName)} }},");
                    }
                    sb.AppendLine("                };");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                }

                sb.AppendLine("        }");
                sb.AppendLine();
            }

            //-------------------------------------------------
            // Inventory section
            //-------------------------------------------------
            if (items.Count > 0)
            {
                sb.AppendLine("        public static class Inventory");
                sb.AppendLine("        {");

                foreach (var item in items)
                {
                    sb.AppendLine($"            public static ItemData {MakeValidIdentifier(item.name)} = {item.id};");
                }

                sb.AppendLine();
                sb.AppendLine("            public static Dictionary<int, ItemData> GetMap() => new()");
                sb.AppendLine("            {");
                foreach (var item in items)
                {
                    sb.AppendLine($"                {{ {item.id}, {MakeValidIdentifier(item.name)} }},");
                }
                sb.AppendLine("            };");
                sb.AppendLine("        }");
                sb.AppendLine();
            }
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }
#endif
    }
}
#endif