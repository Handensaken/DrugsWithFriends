#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Heathen.SteamworksIntegration
{
    [HelpURL("https://kb.heathen.group/steam/features/lobby/unity-lobby/steam-lobby-response-display")]
    public class SteamLobbyResponseDisplay : MonoBehaviour
    {
        [Header("Configuration")]
        public float hideAfterSeconds = 0;

        [FormerlySerializedAs("Success")] public SteamText success = new("Success");
        [FormerlySerializedAs("DoesntExist")] public SteamText doesntExist = new("Chat doesn't exist (probably closed)");
        [FormerlySerializedAs("NotAllowed")] public SteamText notAllowed = new("General Denied - You don't have the permissions needed to join the chat");
        [FormerlySerializedAs("Full")] public SteamText full = new("Chat room has reached its maximum size");
        [FormerlySerializedAs("Error")] public SteamText error = new("Unexpected Error");
        [FormerlySerializedAs("Banned")] public SteamText banned = new("You are banned from this chat room and may not join");
        [FormerlySerializedAs("Limited")] public SteamText limited = new("Joining this chat is not allowed because you are a limited user (no value on account)");
        [FormerlySerializedAs("ClanDisabled")] public SteamText clanDisabled = new("Attempt to join a clan chat when the clan is locked or disabled");
        [FormerlySerializedAs("CommunityBan")] public SteamText communityBan = new("Attempt to join a chat when the user has a community lock on their account");
        [FormerlySerializedAs("MemberBlockedYou")] public SteamText memberBlockedYou = new("Join failed - some member in the chat has blocked you from joining");
        [FormerlySerializedAs("YouBlockedMember")] public SteamText youBlockedMember = new("Join failed - you have blocked some member already in the chat");
        [FormerlySerializedAs("RateLimitExceeded")] public SteamText rateLimitExceeded = new("Join failed - to many join attempts in a very short period of time");

        [Header("Elements")]
        public TMPro.TMP_InputField outputElement;
        public GameObject displayElement;

        [Header("Events")]
        public UnityEvent onDisplay;
        public UnityEvent onHide;


        private void Start()
        {
            if (outputElement != null)
                outputElement.readOnly = true;
        }

        public string GetString(EChatRoomEnterResponse response)
        {
            switch (response)
            {
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseBanned: return banned;
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseClanDisabled: return clanDisabled;
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseCommunityBan: return communityBan;
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseDoesntExist: return doesntExist;
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseError: return error;
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseFull: return full;
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseLimited: return limited;
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseMemberBlockedYou: return memberBlockedYou;
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseNotAllowed: return notAllowed;
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseRatelimitExceeded: return rateLimitExceeded;
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess: return success;
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseYouBlockedMember: return youBlockedMember;
                default: return string.Empty;
            }
        }

        public void DisplayResponse(EChatRoomEnterResponse response)
        {
            if (outputElement != null)
                outputElement.text = GetString(response);

            if (displayElement != null)
                displayElement.SetActive(true);

            onDisplay?.Invoke();

            if (hideAfterSeconds > 0)
            {
                CancelInvoke("HideDisplay");
                Invoke("HideDisplay", hideAfterSeconds);
            }
        }

        public void HideDisplay()
        {
            if (displayElement != null)
                displayElement.SetActive(false);

            onHide?.Invoke();
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyResponseDisplay), true)]
    public class SteamLobbyResponseDisplayEditor : UnityEditor.Editor
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