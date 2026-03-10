#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Chat", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyChatUI : MonoBehaviour
    {
        [SettingsField(0, false, "Chat UI")]
        [SerializeField]
        private int maxMessages = 200;

        [ElementField("Chat UI")]
        [SerializeField]
        private GameObject chatPanel;
        [ElementField("Chat UI")]
        [SerializeField]
        private TMPro.TMP_InputField inputField;
        [ElementField("Chat UI")]
        [SerializeField]
        private UnityEngine.UI.ScrollRect scrollView;
        [ElementField("Chat UI")]
        [SerializeField]
        private Transform messageRoot;

        [TemplateField("Chat UI")]
        [SerializeField]
        private GameObject myChatTemplate;
        [TemplateField("Chat UI")]
        [SerializeField]
        private GameObject theirChatTemplate;

        private SteamLobbyData _mInspector;
        private readonly List<SteamLobbyMemberChatMessage> _chatMessages = new();

        private void Start()
        {
            _mInspector = GetComponent<SteamLobbyData>();
            _mInspector.onChanged.AddListener(HandleOnChanged);
            SteamTools.Events.OnLobbyChatMsg += HandleChatMessage;
        }

        private void HandleOnChanged(LobbyData arg0)
        {
            if (!arg0.IsValid)
                Clear();
        }

        private void OnDestroy()
        {
            SteamTools.Events.OnLobbyChatMsg -= HandleChatMessage;
        }

        private void Update()
        {
            if (!chatPanel || !inputField)
                return;

            //Show or hide the chat panel based on rather or not we have a lobby
            if (_mInspector.Data.IsValid
                && !chatPanel.activeSelf)
                chatPanel.SetActive(true);
            else if (!_mInspector.Data.IsValid
                && chatPanel.activeSelf)
                chatPanel.SetActive(false);

            if (EventSystem.current.currentSelectedGameObject == inputField.gameObject
#if ENABLE_INPUT_SYSTEM
                && (UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame
                || UnityEngine.InputSystem.Keyboard.current.numpadEnterKey.wasPressedThisFrame)
#else
                && (Input.GetKeyDown(KeyCode.Return)
                    || Input.GetKeyDown(KeyCode.KeypadEnter))
#endif
                )
            {
                SendMessage();
            }
        }

        public void Clear()
        {
            if (_chatMessages != null)
            {
                foreach(var message in _chatMessages)
                {
                    Destroy(message.gameObject);
                }
                _chatMessages.Clear();
            }
        }

        private void HandleChatMessage(LobbyChatMsg message)
        {
            if (message.lobby == _mInspector.Data
                && message.type == Steamworks.EChatEntryType.k_EChatEntryTypeChatMsg)
            {
                if (_chatMessages != null)
                {
                    if (_chatMessages.Count == maxMessages)
                    {
                        Destroy(_chatMessages[0].gameObject);
                        _chatMessages.RemoveAt(0);
                    }

                    if (message.sender == UserData.Me)
                    {
                        var go = Instantiate(myChatTemplate, messageRoot);
                        go.transform.SetAsLastSibling();
                        var cmsg = go.GetComponent<SteamLobbyMemberChatMessage>();
                        if (cmsg != null)
                        {
                            cmsg.Initialise(message);
                            if (_chatMessages.Count > 0
                                && _chatMessages[^1].User == cmsg.User)
                                cmsg.IsExpanded = false;

                            _chatMessages.Add(cmsg);
                        }
                    }
                    else
                    {
                        var go = Instantiate(theirChatTemplate, messageRoot);
                        go.transform.SetAsLastSibling();
                        var cmsg = go.GetComponent<SteamLobbyMemberChatMessage>();
                        if (cmsg != null)
                        {
                            cmsg.Initialise(message);
                            if (_chatMessages[^1].User == cmsg.User)
                                cmsg.IsExpanded = false;

                            _chatMessages.Add(cmsg);
                        }
                    }

                    StartCoroutine(ForceScrollDown());
                }
            }
        }

        public void SendMessage()
        {
            if (_mInspector.Data.IsValid
                && !string.IsNullOrEmpty(inputField.text))
            {
                _mInspector.Data.SendChatMessage(inputField.text);
                inputField.text = string.Empty;
                StartCoroutine(SelectInputField());
            }
        }

        IEnumerator SelectInputField()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            inputField.ActivateInputField();
        }
        /// <summary>
        /// Called when a new chat message is added to the UI to force the system to scroll to the bottom
        /// </summary>
        /// <returns></returns>
        IEnumerator ForceScrollDown()
        {
            // Wait for end of frame AND force update all canvases before setting to bottom.
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            scrollView.verticalNormalizedPosition = 0f;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyChatUI), true)]
    public class SteamLobbyChatUIEditor : UnityEditor.Editor
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