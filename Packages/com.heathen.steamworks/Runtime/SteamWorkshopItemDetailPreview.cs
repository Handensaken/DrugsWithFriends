#if !DISABLESTEAMWORKS && STEAM_INSTALLED
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Heathen.SteamworksIntegration
{
    [Obsolete("Unity's built in Texture2D LoadImage cant handle many file types and this proves to be more of a liability than an aid." +
              "Recomend the developer use a 3rd party image loader and our WorkshopItemDetail.GetPreviewImage tool to get the file data.")]
    [ModularComponent(typeof(SteamWorkshopItemDetailData), "Preview Images", nameof(image))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamWorkshopItemDetailDataEvents))]
    [RequireComponent(typeof(SteamWorkshopItemDetailData))]
    public class SteamWorkshopItemDetailPreview : MonoBehaviour
    {
        public RawImage image;

        private SteamWorkshopItemDetailData _inspector;
        private SteamWorkshopItemDetailDataEvents _events;

        private void Awake()
        {
            _inspector = GetComponent<SteamWorkshopItemDetailData>();
            _events = GetComponent<SteamWorkshopItemDetailDataEvents>();

            _events.onChange.AddListener(HandleChanged);
            HandleChanged();
        }

        private void HandleChanged()
        {
            if (image == null)
                return;

            if (_inspector.Data == null)
                return;

            _inspector.Data.GetPreviewImage((path, data) =>
            {
                if (data == null || data.Length == 0)
                    return;

                // Create a new Texture2D (size doesn't matter, LoadImage will replace it)
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);

                // Load the byte[] into the texture
                if (tex.LoadImage(data))
                {
                    image.texture = tex;
                    image.SetNativeSize();
                }
                else
                {
                    Debug.LogError($"Failed to load {path}.");
                }
            });
        }
    }
}
#endif