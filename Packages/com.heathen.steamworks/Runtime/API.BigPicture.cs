#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration.API
{
    public static class BigPicture
    {
        public static class Client
        {
            /// <summary>
            /// Indicates whether Steam is currently running in Big Picture mode.
            /// </summary>
            public static bool IsInBigPicture => SteamUtils.IsSteamInBigPictureMode();

            /// <summary>
            /// Indicates whether the application is currently running on a Steam Deck device.
            /// </summary>
            public static bool IsRunningOnDeck => SteamUtils.IsSteamRunningOnSteamDeck();

            /// <summary>
            /// Displays the Big Picture mode text input dialogue, designed for gamepad input only.
            /// </summary>
            /// <param name="inputMode">Specifies the input mode to use, such as Normal or Password (hidden text).</param>
            /// <param name="lineMode">Determines whether single or multi-line input is allowed.</param>
            /// <param name="description">A description intended to inform the user about the purpose of the input dialogue.</param>
            /// <param name="maxLength">Defines the maximum character count allowed in the input field.</param>
            /// <param name="currentText">The initial pre-filled text that the user may edit.</param>
            /// <returns>Returns true if the Big Picture overlay is active; otherwise, false.</returns>
            public static bool ShowTextInput(EGamepadTextInputMode inputMode, EGamepadTextInputLineMode lineMode,
                string description, uint maxLength, string currentText)
            {
                if (SteamUtils.ShowGamepadTextInput(inputMode, lineMode, description, maxLength, currentText))
                {
                    SteamTools.Events.InvokeOnGamepadTextInputShown();
                    return true;
                }
                else
                    return false;
            }

            /// <summary>
            /// Activates the Big Picture text input dialogue which only supports gamepad input.
            /// </summary>
            /// <param name="inputMode">Selects the input mode to use, either Normal or Password (hidden text)</param>
            /// <param name="lineMode">Controls whether to use single or multi-line input.</param>
            /// <param name="description">Sets the description that should inform the user what the input dialogue is for.</param>
            /// <param name="maxLength">The maximum number of characters that the user can input.</param>
            /// <param name="currentText">Sets the preexisting text which the user can edit.</param>
            /// <returns>True if the big picture overlay is running; otherwise, false.</returns>
            public static bool ShowTextInput(EGamepadTextInputMode inputMode, EGamepadTextInputLineMode lineMode, string description, int maxLength, string currentText)
            {
                if(SteamUtils.ShowGamepadTextInput(inputMode, lineMode, description, System.Convert.ToUInt32(maxLength), currentText))
                {
                    SteamTools.Events.InvokeOnGamepadTextInputShown();
                    return true;
                }
                else
                    return false;
            }
        }
    }
}
#endif