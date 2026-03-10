#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Members", nameof(attributes))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyMembers : MonoBehaviour
    {
        [Serializable]
        public class Attributes
        {
            [Header("Configuration")]
            [Tooltip("If true the local user's display will be shown otherwise we skip the local user")]
            public bool showSelf = false;
            [Header("Elements")]
            [Tooltip("This game object will be instantiated for each member that joins and managed by the component")]
            public SteamLobbyMemberData template;
            [Tooltip("The container where member templates will be spawned as members join or removed from when members leave.")]
            public Transform content;
        }

        public Attributes attributes = new();

        //[Header("Configuration")]
        //[Tooltip("If true the local user's display will be shown otherwise we skip the local user")]
        //public bool showSelf = false;
        //[Header("Elements")]
        //[Tooltip("This game object will be instantiated for each member that joins and managed by the component")]
        //public SteamLobbyMemberData template;
        //[Tooltip("The container where member templates will be spawned as members join or removed from when members leave.")]
        //public Transform content;

        private SteamLobbyData _mInspector;
        private List<SteamLobbyMemberData> _mSpawnedMembers = new();

        private void Awake()
        {
            _mInspector = GetComponent<SteamLobbyData>();
            _mInspector.onChanged.AddListener(HandleLobbyChanged);
            
            SteamTools.Events.OnLobbyChatUpdate += GlobalChatUpdate;
        }

        private void OnDestroy()
        {
            _mInspector?.onChanged.RemoveListener(HandleLobbyChanged);
            SteamTools.Events.OnLobbyChatUpdate -= GlobalChatUpdate;
        }

        private void HandleLobbyChanged(LobbyData arg0)
        {
            foreach(var member in _mSpawnedMembers)
                Destroy(member.gameObject);

            _mSpawnedMembers.Clear();

            if (_mInspector.Data.IsValid)
            {
                foreach (var member in _mInspector.Data.Members)
                {
                    AddMember(member);
                }
            }
        }

        private void GlobalChatUpdate(LobbyData lobby, UserData user, EChatMemberStateChange state)
        {
            if (lobby == _mInspector.Data)
            {
                if (state == EChatMemberStateChange.k_EChatMemberStateChangeEntered)
                    AddMember(LobbyMemberData.Get(_mInspector.Data, user));
                else
                    RemoveMember(new UserLobbyLeaveData { user = user, state = state });
            }
        }

        private void AddMember(LobbyMemberData data)
        {
            if (!attributes.showSelf && data.user == UserData.Me)
                return;

            if(attributes.template != null && attributes.content != null)
            {
                var go = Instantiate(attributes.template, attributes.content);
                var comp = go.GetComponent<SteamLobbyMemberData>();
                comp.Data = data;
                _mSpawnedMembers.Add(comp);
            }
        }

        private void RemoveMember(UserLobbyLeaveData data)
        {
            var target = _mSpawnedMembers.Find((p) => { return p.Data.user == data.user; });
            if(target != null)
            {
                _mSpawnedMembers.Remove(target);
                Destroy(target.gameObject);
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyMembers), true)]
    public class SteamLobbyMembersEditor : UnityEditor.Editor
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