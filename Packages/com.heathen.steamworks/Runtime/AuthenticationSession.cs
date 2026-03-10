#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents an authentication session for a Steam user, used for validating and managing user credentials in client or server contexts.
    /// </summary>
    [Serializable]
    public class AuthenticationSession
    {
        /// <summary>
        /// Specifies whether the authentication session is being managed in a client context.
        /// When set to true, the session operates as a client session; otherwise, it operates as a server session.
        /// </summary>
        public bool IsClientSession { get; private set; }

        /// <summary>
        /// Represents the user associated with the authentication session.
        /// This property holds the Steam user data used to validate the credentials
        /// and determine the context of the session.
        /// </summary>
        public UserData User { get; private set; }

        /// <summary>
        /// Represents the owner of the game associated with the current authentication session.
        /// This property identifies the user that holds ownership rights to the game,
        /// which may differ from the current user in cases such as game borrowing.
        /// </summary>
        public UserData GameOwner { get; private set; }

        /// <summary>
        /// Contains raw data associated with the authentication session.
        /// This data is typically used for verifying the credentials or status of the session.
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Represents the response status of the authentication session.
        /// This property indicates the result of the authentication process, such as success,
        /// user not connected, licence expiration, or other status codes defined in the
        /// <c>EAuthSessionResponse</c> enumeration.
        /// </summary>
        public EAuthSessionResponse Response { get; private set; }

        /// <summary>
        /// Indicates whether the game being used in the session is borrowed from another user.
        /// Returns true when the current user is different from the game owner; otherwise, false.
        /// </summary>
        public bool IsBorrowed => User != GameOwner;

        /// <summary>
        /// Defines a callback action invoked when the authentication session is started.
        /// This action receives the current <see cref="AuthenticationSession"/> instance as its parameter,
        /// providing the context for the initiated session.
        /// </summary>
        public Action<AuthenticationSession> OnStartCallback { get; private set; }

        /// <summary>
        /// Represents an authentication session for a Steam user, used for validating and managing user credentials in client or server contexts.
        /// </summary>
        /// <param name="userId">The user the session relates to, specified as a Steam ID</param>
        /// <param name="callback">A delegate to invoke when the session is ready, provided in the form (<see cref="AuthenticationSession"/> session)</param>
        /// <param name="isClient">Indicates whether the session is a client session (default: true) or a server session</param>
        public AuthenticationSession(CSteamID userId, Action<AuthenticationSession> callback, bool isClient = true)
        {
            IsClientSession = isClient;
            User = userId;
            OnStartCallback = callback;
        }

        internal void Authenticate(ValidateAuthTicketResponse_t response)
        {
            Response = response.m_eAuthSessionResponse;
            GameOwner = response.m_OwnerSteamID;
        }

        /// <summary>
        /// Ends the authentication session by notifying the Steamworks API to terminate the session
        /// for the associated user. The method behaves differently depending on whether the session
        /// is a client session or a server session.
        /// </summary>
        public void End()
        {
            if (IsClientSession)
                SteamUser.EndAuthSession(User);
            else
                SteamGameServer.EndAuthSession(User);
        }
    }
}
#endif