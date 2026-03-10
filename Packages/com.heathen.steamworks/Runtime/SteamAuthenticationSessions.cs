#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
#if UNITY_EDITOR
#endif
using UnityEngine;
using Heathen.SteamworksIntegration.API;
using System;
using System.Collections.Generic;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Manages Steam authentication sessions.
    /// </summary>
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamAuthenticationEvents))]
    public class SteamAuthenticationSessions : MonoBehaviour
    {
        /// <summary>
        /// A bitmask of response codes that are considered "accepted" by the session manager.
        /// </summary>
        [Flags]
        public enum AuthSessionResponseMask
        {
            None = 0,
            Ok = 1 << 0,
            NotConnectedToSteam = 1 << 1,
            NoLicenseOrExpired = 1 << 2,
            VacBanned = 1 << 3,
            LoggedInElseWhere = 1 << 4,
            VacCheckTimedOut = 1 << 5,
            Canceled = 1 << 6,
            AlreadyUsed = 1 << 7,
            Invalid = 1 << 8,
            PublisherIssuedBan = 1 << 9,
            IdentityFailure = 1 << 10,
        }

        /// <summary>
        /// The responses that will be accepted; responses not in this mask will be rejected and the session will be ended.
        /// </summary>
        [SettingsField]
        public AuthSessionResponseMask acceptedResponses;
        /// <summary>
        /// A list of all active authentication sessions.
        /// </summary>
        public List<AuthenticationSession> Sessions => Authentication.ActiveSessions;

        private SteamAuthenticationEvents _mEvents;

        private void Awake()
        {
            _mEvents = GetComponent<SteamAuthenticationEvents>();
        }

        /// <summary>
        /// Begins a new authentication session for a given user and ticket.
        /// </summary>
        /// <param name="user">The Steam ID of the user.</param>
        /// <param name="ticket">The authentication ticket data.</param>
        public void Begin(ulong user, byte[] ticket)
        {
            var result = Authentication.BeginAuthSession(ticket, user, session =>
            {
                // Map Steam response to mask
                AuthSessionResponseMask mask = session.Response switch
                {
                    Steamworks.EAuthSessionResponse.k_EAuthSessionResponseOK => AuthSessionResponseMask.Ok,
                    Steamworks.EAuthSessionResponse.k_EAuthSessionResponseUserNotConnectedToSteam => AuthSessionResponseMask.NotConnectedToSteam,
                    Steamworks.EAuthSessionResponse.k_EAuthSessionResponseNoLicenseOrExpired => AuthSessionResponseMask.NoLicenseOrExpired,
                    Steamworks.EAuthSessionResponse.k_EAuthSessionResponseVACBanned => AuthSessionResponseMask.VacBanned,
                    Steamworks.EAuthSessionResponse.k_EAuthSessionResponseLoggedInElseWhere => AuthSessionResponseMask.LoggedInElseWhere,
                    Steamworks.EAuthSessionResponse.k_EAuthSessionResponseVACCheckTimedOut => AuthSessionResponseMask.VacCheckTimedOut,
                    Steamworks.EAuthSessionResponse.k_EAuthSessionResponseAuthTicketCanceled => AuthSessionResponseMask.Canceled,
                    Steamworks.EAuthSessionResponse.k_EAuthSessionResponseAuthTicketInvalidAlreadyUsed => AuthSessionResponseMask.AlreadyUsed,
                    Steamworks.EAuthSessionResponse.k_EAuthSessionResponseAuthTicketInvalid => AuthSessionResponseMask.Invalid,
                    Steamworks.EAuthSessionResponse.k_EAuthSessionResponsePublisherIssuedBan => AuthSessionResponseMask.PublisherIssuedBan,
                    Steamworks.EAuthSessionResponse.k_EAuthSessionResponseAuthTicketNetworkIdentityFailure => AuthSessionResponseMask.IdentityFailure,
                    _ => 0
                };

                if ((acceptedResponses & mask) != 0)
                    _mEvents.onSessionStart?.Invoke(session.Response);
                else
                {
                    session.End();
                    _mEvents.onInvalidSession?.Invoke(session.Response);
                }
            });

            if (result != Steamworks.EBeginAuthSessionResult.k_EBeginAuthSessionResultOK)
                _mEvents.onInvalidTicket?.Invoke(result);
        }
        /// <summary>
        /// Ends an authentication session for a given user.
        /// </summary>
        /// <param name="user">The Steam ID of the user.</param>
        public void End(ulong user) => Authentication.EndAuthSession(user);
        /// <summary>
        /// Ends an authentication session for a given user.
        /// </summary>
        /// <param name="user">The user data object.</param>
        public void End(SteamUserData user)
        {
            if (user != null && user.Data.IsValid)
                Authentication.EndAuthSession(user.Data);
        }
        /// <summary>
        /// Ends all active authentication sessions.
        /// </summary>
        public void EndAll() => Authentication.EndAllSessions();
    }
}
#endif