#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;
using Steamworks;
using FriendsAPI = Heathen.SteamworksIntegration.API.Friends.Client;
using UnityEngine.UI;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Applies the avatar of the indicated user to the attached RawImage
    /// </summary>
    [ModularComponent(typeof(SteamUserData), "Avatars", nameof(image))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamUserData))]
    public class SteamUserAvatar : MonoBehaviour
    {
        public RawImage image;

        private SteamUserData _inspector;

        private void Awake()
        {
            _inspector = GetComponent<SteamUserData>();
            _inspector.onChanged.AddListener(HandlePersonaStateChange);            
        }

        private void Start()
        {
            if (_inspector.Data.IsValid)
            {
                SteamTools.Interface.WhenReady(HandleSteamInitialized);
            }
        }

        private void HandleSteamInitialized()
        {
            LoadAvatar(_inspector.Data);
        }

        private void HandlePersonaStateChange(UserData user, EPersonaChange flag)
        {
            if (FriendsAPI.PersonaChangeHasFlag(flag, EPersonaChange.k_EPersonaChangeAvatar))
            {
                _inspector.Data.LoadAvatar(AvatarLoaded);
            }
        }

        public void LoadAvatar(UserData user) => user.LoadAvatar(AvatarLoaded);

        public void LoadAvatar(CSteamID user) => UserData.Get(user).LoadAvatar((r) =>
        {
            if (!image)
                return;

            image.texture = r;
        });

        public void LoadAvatar(ulong user) => UserData.Get(user).LoadAvatar(AvatarLoaded);

        private void AvatarLoaded(Texture2D texture)
        {
            if (texture && image)
                image.texture = texture;
        }
    }
}
#endif