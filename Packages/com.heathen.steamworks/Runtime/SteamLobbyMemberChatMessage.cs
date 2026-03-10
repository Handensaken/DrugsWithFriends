#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// A simple chat message UI behaviour
    /// </summary>
    [AddComponentMenu("")]
    [ModularComponent(typeof(SteamLobbyMemberData), "Chat Message", "")]
    [RequireComponent(typeof(SteamLobbyMemberData))]
    [HelpURL("https://heathen.group/kb/lobby/#chat")]
    public class SteamLobbyMemberChatMessage : MonoBehaviour
    {
        [ElementField("Chat Message")]
        public GameObject expansionPanel;
        [SettingsField(0,false, "Chat Display")]
        [SerializeField]
        private string dateTimeFormat = "HH:mm:ss";
        [ElementField("Chat Message")]
        [SerializeField]
        private TMPro.TextMeshProUGUI datetime;
        [ElementField("Chat Message")]
        [SerializeField]
        private TMPro.TextMeshProUGUI message;

        public UserData User => _mData.Data.user;
        public byte[] Data { get; private set; }
        public string Message { get; private set; }
        public DateTime ReceivedAt { get; private set; }
        public EChatEntryType Type { get; private set; }
        public bool IsExpanded
        {
            get
            {
                if (expansionPanel != null)
                    return expansionPanel.activeSelf;
                else
                    return false;
            }
            set
            {
                if (expansionPanel != null)
                    expansionPanel.SetActive(value);
            }
        }

        private SteamLobbyMemberData _mData;

        private void Awake()
        {
            _mData = GetComponent<SteamLobbyMemberData>();
        }

        /// <summary>
        /// Initialise the message given a source <see cref="LobbyChatMsg" />
        /// </summary>
        /// <param name="chatMessage">The message to initialise</param>
        public void Initialise(LobbyChatMsg chatMessage)
        {
            _mData.Data = new() { lobby = chatMessage.lobby, user = chatMessage.sender };
            Type = chatMessage.type;
            Message = chatMessage.Message;
            Data = chatMessage.data;
            ReceivedAt = DateTime.Now;

            message.text = Message;

            if (datetime != null)
                datetime.text = ReceivedAt.ToString(dateTimeFormat);
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyMemberChatMessage), true)]
    public class BasicChatMessageEditor : UnityEditor.Editor
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