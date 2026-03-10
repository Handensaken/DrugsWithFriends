#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
#if UNITY_EDITOR
#endif
using UnityEngine;
using Heathen.SteamworksIntegration.API;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Helper component to get an authentication ticket.
    /// </summary>
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamAuthenticationData))]
    public class SteamAuthenticationGetTicket : MonoBehaviour
    {
        /// <summary>
        /// The last ticket received.
        /// </summary>
        public AuthenticationTicket Data
        {
            get => _mData;
            private set
            {
                _mData = value;
            }
        }
        private AuthenticationTicket _mData;

        /// <summary>
        /// Gets a ticket for the lobby's game server.
        /// </summary>
        /// <param name="lobby">The lobby to get the ticket for.</param>
        public void GetTicketForLobbyServer(SteamLobbyData lobby)
        {
            if (!lobby.Data.IsValid)
                return;

            var serverData = lobby.Data.GameServer;
            if (serverData.id.IsValid())
            {
                Authentication.GetAuthSessionTicket(serverData.id, HandleTicketCallback);
            }
        }

        /// <summary>
        /// Gets a ticket for the lobby's owner.
        /// </summary>
        /// <param name="lobby">The lobby to get the ticket for.</param>
        public void GetTicketForLobbyOwner(SteamLobbyData lobby)
        {
            if (lobby.Data.IsValid)
            {
                Authentication.GetAuthSessionTicket(lobby.Data.Owner.user, HandleTicketCallback);
            }
        }

        /// <summary>
        /// Gets a ticket for a specific user.
        /// </summary>
        /// <param name="user">The user to get the ticket for.</param>
        public void GetTicketForUser(SteamUserData user)
        {
            if (user.Data.IsValid)
            {
                Authentication.GetAuthSessionTicket(user.Data, HandleTicketCallback);
            }
        }

        /// <summary>
        /// Gets a ticket for a specific game server.
        /// </summary>
        /// <param name="server">The game server to get the ticket for.</param>
        public void GetTicketForGameServer(SteamGameServerData server)
        {
            if(server.Data.SteamId.IsValid())
            {
                Authentication.GetAuthSessionTicket(server.Data.SteamId, HandleTicketCallback);
            }
        }

        /// <summary>
        /// Gets a ticket for use with a Web API.
        /// </summary>
        /// <param name="identity">The identity to get the ticket for.</param>
        public void GetTicketForWebAPI(string identity)
        {
            Authentication.GetWebAuthSessionTicket(identity, HandleTicketCallback);
        }

        private void HandleTicketCallback(AuthenticationTicket ticket, bool ioError)
        {
            Data = ticket;
        }
    }
}
#endif