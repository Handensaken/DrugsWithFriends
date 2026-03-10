#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Displays the controller button reported for this specific action
    /// </summary>
    [ModularComponent(typeof(SteamInputActionData), "Glyphs", nameof(image))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamInputActionData))]
    public class SteamInputActionGlyph : MonoBehaviour
    {
        /// <summary>
        /// The image the icon will be displayed with
        /// </summary>
        public UnityEngine.UI.RawImage image;

        private SteamInputActionData _mInspector;

        private void Awake()
        {
            _mInspector = GetComponent<SteamInputActionData>();

            if (!SteamTools.Interface.IsReady)
                SteamTools.Interface.OnReady += HandleInitialization;
            else
                HandleInitialization();
        }

        private void HandleInitialization()
        {
            SteamTools.Interface.OnReady -= HandleInitialization;

            RefreshImage();
        }

        private void OnEnable()
        {
            RefreshImage();
        }
        /// <summary>
        /// Refresh the image
        /// </summary>
        public void RefreshImage()
        {
            if (_mInspector != null && !string.IsNullOrEmpty(_mInspector.Action.Name) && image != null)
            {
                if (_mInspector.Set.Handle > 0)
                {
                    if (API.Input.Client.ConnectedControllers.Count > 0)
                    {
                        var textures = _mInspector.Action.GetInputGlyphs(API.Input.Client.ConnectedControllers[0], _mInspector.Set);
                        if (textures.Length > 0)
                        {
                            image.texture = textures[0];
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(_mInspector.Layer.LayerName))
                {
                    if (API.Input.Client.ConnectedControllers.Count > 0)
                    {
                        var textures = _mInspector.Action.GetInputGlyphs(API.Input.Client.ConnectedControllers[0], _mInspector.Layer);
                        if (textures.Length > 0)
                        {
                            image.texture = textures[0];
                        }
                    }
                }
            }
        }
    }
}
#endif