#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// Provides functionality to interact with the Steam Remote Play system,
    /// allowing developers to manage and gather information about Remote Play sessions.
    /// </summary>
    public static class RemotePlay
    {
        /// <summary>
        /// Provides a set of methods and events to interact with Steam Remote Play client sessions,
        /// enabling developers to query session details, manage connected users, and send Remote Play invites.
        /// </summary>
        public static class Client
        {
            /// <summary>
            /// Gets the number of currently connected Steam Remote Play sessions.
            /// </summary>
            /// <returns>
            /// The number of active Steam Remote Play sessions.
            /// </returns>
            public static uint GetSessionCount() => SteamRemotePlay.GetSessionCount();

            /// <summary>
            /// Retrieves the Steam Remote Play session ID at the specified index.
            /// </summary>
            /// <param name="index">The zero-based index of the session to retrieve the ID for.</param>
            /// <returns>
            /// The session ID of the Steam Remote Play session at the specified index.
            /// </returns>
            public static RemotePlaySessionID_t GetSessionID(int index) => SteamRemotePlay.GetSessionID(index);

            /// <summary>
            /// Retrieves all currently active Steam Remote Play session identifiers.
            /// </summary>
            /// <returns>
            /// An array of RemotePlaySessionID_t representing all active Remote Play sessions.
            /// </returns>
            public static RemotePlaySessionID_t[] GetSessions()
            {
                var count = SteamRemotePlay.GetSessionCount();
                var results = new RemotePlaySessionID_t[count];
                for (int i = 0; i < count; i++)
                {
                    results[i] = SteamRemotePlay.GetSessionID(i);
                }
                return results;
            }

            /// <summary>
            /// Retrieves the user data of the connected user in the specified Remote Play session.
            /// </summary>
            /// <param name="session">The Remote Play session ID for which to retrieve the user data.</param>
            /// <returns>
            /// The user data associated with the connected user in the specified session.
            /// </returns>
            public static UserData GetSessionUser(RemotePlaySessionID_t session) =>
                SteamRemotePlay.GetSessionSteamID(session);

            /// <summary>
            /// Retrieves the name of the device associated with the specified Steam Remote Play session.
            /// </summary>
            /// <param name="session">The unique identifier of the Remote Play session.</param>
            /// <returns>
            /// The name of the client device for the given Remote Play session, or an empty string if no name is available.
            /// </returns>
            public static string GetSessionClientName(RemotePlaySessionID_t session) =>
                SteamRemotePlay.GetSessionClientName(session);

            /// <summary>
            /// Retrieves the form factor of the client device associated with a specific Remote Play session.
            /// </summary>
            /// <param name="session">The ID of the Remote Play session to query.</param>
            /// <returns>The form factor of the client device participating in the session.</returns>
            public static ESteamDeviceFormFactor GetSessionClientFormFactor(RemotePlaySessionID_t session) =>
                SteamRemotePlay.GetSessionClientFormFactor(session);

            /// <summary>
            /// Gets the resolution, in pixels, of the specified Steam Remote Play session client device.
            /// If the resolution is not available, the result will be 0x0.
            /// </summary>
            /// <param name="session">The ID of the Remote Play session.</param>
            /// <returns>
            /// A Vector2Int representing the resolution of the client device in pixels.
            /// </returns>
            public static Vector2Int GetSessionClientResolution(RemotePlaySessionID_t session)
            {
                SteamRemotePlay.BGetSessionClientResolution(session, out int x, out int y);
                return new Vector2Int(x, y);
            }

            /// <summary>
            /// Sends a Remote Play Together invite to the specified user.
            /// </summary>
            /// <param name="user">
            /// The user data representing the friend to whom the invite will be sent.
            /// </param>
            /// <returns>
            /// Returns true if the invite was successfully sent; otherwise, false.
            /// </returns>
            public static bool SendInvite(UserData user) => SteamRemotePlay.BSendRemotePlayTogetherInvite(user);

#if STEAM_162
            /// <summary>
            /// <para> Make mouse and keyboard input for Remote Play Together sessions available via GetInput() instead of being merged with local input</para>
            /// </summary>
            public static bool EnableRemotePlayTogetherDirectInput() => SteamRemotePlay.BEnableRemotePlayTogetherDirectInput();

            /// <summary>
            /// <para> Merge Remote Play Together mouse and keyboard input with local input</para>
            /// </summary>
            public static void DisableRemotePlayTogetherDirectInput() => SteamRemotePlay.DisableRemotePlayTogetherDirectInput();

            /// <summary>
            /// <para> Get input events from Remote Play Together sessions</para>
            /// <para> This is available after calling BEnableRemotePlayTogetherDirectInput()</para>
            /// <para> pInput is an array of input events that will be filled in by this function, up to unMaxEvents.</para>
            /// <para> This returns the number of events copied to pInput, or the number of events available if pInput is nullptr.</para>
            /// </summary>
            public static uint GetInput(RemotePlayInput_t[] Input, uint MaxEvents) => SteamRemotePlay.GetInput(Input, MaxEvents);

            /// <summary>
            /// <para> Set the mouse cursor visibility for a remote player</para>
            /// <para> This is available after calling BEnableRemotePlayTogetherDirectInput()</para>
            /// </summary>
            public static void SetMouseVisibility(RemotePlaySessionID_t unSessionID, bool bVisible) => SteamRemotePlay.SetMouseVisibility(unSessionID, bVisible);

            /// <summary>
            /// <para> Set the mouse cursor position for a remote player</para>
            /// <para> This is available after calling BEnableRemotePlayTogetherDirectInput()</para>
            /// <para> This is used to warp the cursor to a specific location and isn't needed during normal event processing.</para>
            /// <para> The position is normalized relative to the window, where 0,0 is the upper left, and 1,1 is the lower right.</para>
            /// </summary>
            public static void SetMousePosition(RemotePlaySessionID_t SessionID, float Normalized_X, float Normalized_Y) => SteamRemotePlay.SetMousePosition(SessionID, Normalized_X, Normalized_Y);

            //TODO: Need to update cursor pointer
            /// <summary>
            /// <para> Create a cursor that can be used with SetMouseCursor()</para>
            /// <para> This is available after calling BEnableRemotePlayTogetherDirectInput()</para>
            /// <para> Parameters:</para>
            /// <para> nWidth - The width of the cursor, in pixels</para>
            /// <para> nHeight - The height of the cursor, in pixels</para>
            /// <para> nHotX - The X coordinate of the cursor hot spot in pixels, offset from the left of the cursor</para>
            /// <para> nHotY - The Y coordinate of the cursor hot spot in pixels, offset from the top of the cursor</para>
            /// <para> pBGRA - A pointer to the cursor pixels, with the color channels in red, green, blue, alpha order</para>
            /// <para> nPitch - The distance between pixel rows in bytes, defaults to nWidth * 4</para>
            /// </summary>
            //public static RemotePlayCursorID_t CreateMouseCursor(int nWidth, int nHeight, int nHotX, int nHotY, IntPtr pBGRA, int nPitch = 0) => SteamRemotePlay.CreateMouseCursor(nWidth, nHeight, nHotX, nHotY, pBGRA, nPitch);

            /// <summary>
            /// <para> Set the mouse cursor for a remote player</para>
            /// <para> This is available after calling BEnableRemotePlayTogetherDirectInput()</para>
            /// <para> The cursor ID is a value returned by CreateMouseCursor()</para>
            /// </summary>
            public static void SetMouseCursor(RemotePlaySessionID_t SessionID, RemotePlayCursorID_t CursorID) => SteamRemotePlay.SetMouseCursor(SessionID, CursorID);
#endif
        }
    }
}
#endif