#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Game Server", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyGameServer : MonoBehaviour
    {
        private SteamLobbyData _mInspector;

        private void Awake()
        {
            _mInspector = GetComponent<SteamLobbyData>();
            SteamTools.Events.OnLobbyGameServer += GlobalGameCreated;
        }

        private void OnDestroy()
        {
            SteamTools.Events.OnLobbyGameServer -= GlobalGameCreated;
        }

        private bool EnsureOwner(out LobbyData data)
        {
            data = _mInspector.Data;
            if (!data.IsValid)
            {
                Debug.LogWarning($"[{nameof(SteamLobbyGameServer)}] No lobby to set");
                return false;
            }
            if (!data.IsOwner)
            {
                Debug.LogWarning($"[{nameof(SteamLobbyGameServer)}] Only the owner can set data");
                return false;
            }
            return true;
        }

        private void GlobalGameCreated(LobbyData lobby, CSteamID serverId, string ip, ushort port)
        {
            if (lobby == _mInspector.Data)
            {
                var gameServer = new LobbyGameServer
                {
                    id = serverId,
                    IpAddress = ip,
                    port = port,
                };
            }
        }

        [ContextMenu("Set as Listen Server")]
        public void SetListenServer()
        {
            if (EnsureOwner(out var data))
                _mInspector.Data.SetGameServer();
        }
        public void SetDedicatedSteamGameServer(CSteamID serverId)
        {
            if (EnsureOwner(out var data))
                _mInspector.Data.SetGameServer(serverId);
        }
        public void SetDedicatedGenericServer(string ip, ushort port)
        {
            if (EnsureOwner(out var data))
                _mInspector.Data.SetGameServer(ip, port);
        }
        public void SetGameServer(CSteamID id, string ip, ushort port)
        {
            if (EnsureOwner(out var data))
                _mInspector.Data.SetGameServer(ip, port, id);
        }

        public bool HasGameServer() => _mInspector.Data.IsValid && _mInspector.Data.HasServer;

        public LobbyGameServer? GetGameServer()
        {
            if (_mInspector.Data.IsValid && _mInspector.Data.HasServer)
                return _mInspector.Data.GameServer;
            return null;
        }

        public string GetIdAddress()
        {
            if (_mInspector.Data.IsValid)
            {
                if (_mInspector.Data.HasServer)
                    return _mInspector.Data.GameServer.id.ToString();
                else
                    return string.Empty;
            }
            else
            {
                Debug.LogWarning($"[{nameof(SteamLobbyGameServer)}] No lobby");
                return string.Empty;
            }
        }
        public string GetIpAddress()
        {
            if (_mInspector.Data.IsValid)
            {
                if (_mInspector.Data.HasServer)
                    return _mInspector.Data.GameServer.IpAddress.ToString();
                else
                    return string.Empty;
            }
            else
            {
                Debug.LogWarning($"[{nameof(SteamLobbyGameServer)}] No lobby");
                return string.Empty;
            }
        }
        public ushort GetPort()
        {
            if (_mInspector.Data.IsValid)
            {
                if (_mInspector.Data.HasServer)
                    return _mInspector.Data.GameServer.port;
                else
                    return 0;
            }
            else
            {
                Debug.LogWarning($"[{nameof(SteamLobbyGameServer)}] No lobby");
                return 0;
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyGameServer), true)]
    public class SteamLobbyGameServerEditor : UnityEditor.Editor
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