#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents an authentication ticket used for verifying the identity of a client or server
    /// in the context of the Steamworks platform. This class encapsulates the ticket's data
    /// and related properties and provides functionality for managing its lifecycle.
    /// </summary>
    [Serializable]
    public class AuthenticationTicket
    {
        /// <summary>
        /// Specifies whether the authentication ticket is associated with a client session,
        /// distinguishing it from tickets generated for server sessions.
        /// </summary>
        public bool IsClientTicket { get; private set; }

        /// <summary>
        /// Represents the handle associated with an authentication ticket, uniquely identifying
        /// the ticket within the Steamworks platform. This property is used to manage and verify
        /// the ticket's lifecycle and validity in client-server interactions.
        /// </summary>
        public HAuthTicket Handle { get; private set; }

        /// <summary>
        /// Contains the raw byte data of the authentication ticket. This data is used to verify
        /// the identity of the client or server within the Steamworks platform. The content and size
        /// of the data may vary depending on the type of ticket issued.
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Indicates whether the authentication ticket has been successfully verified,
        /// confirming its validity and the associated session's authenticity.
        /// </summary>
        public bool Verified { get; private set; }

        /// <summary>
        /// Represents the server-reported real-time timestamp (in seconds since the Unix epoch)
        /// indicating when the authentication ticket was created.
        /// This value is used to track the age of the ticket and manage its lifecycle.
        /// </summary>
        public uint CreatedOn { get; private set; }

        /// <summary>
        /// Represents the result of an authentication operation, providing the status or outcome
        /// as determined by the Steamworks API.
        /// </summary>
        public EResult Result { get; private set; }

        /// <summary>
        /// Represents a callback action that is invoked when the authentication ticket's status changes.
        /// The callback takes two parameters: the current instance of the <see cref="AuthenticationTicket"/>
        /// and a boolean flag indicating specific processing conditions such as error state or validity.
        /// </summary>
        public Action<AuthenticationTicket, bool> Callback { get; private set; }

        /// <summary>
        /// Represents an authentication ticket used for session authorisation between Steam entities.
        /// </summary>
        public AuthenticationTicket(SteamNetworkingIdentity forIdentity, Action<AuthenticationTicket, bool> callback,
            bool isClient = true)
        {
            Callback = callback;
            IsClientTicket = isClient;
            var array = new byte[1024];
            Handle = isClient ? SteamUser.GetAuthSessionTicket(array, 1024, out var mPcbTicket, ref forIdentity) : SteamGameServer.GetAuthSessionTicket(array, 1024, out mPcbTicket, ref forIdentity);
            CreatedOn = SteamUtils.GetServerRealTime();
            Array.Resize(ref array, (int)mPcbTicket);
            Data = array;
        }

        /// <summary>
        /// Represents an authentication ticket used to establish an encrypted session for a Steam application.
        /// </summary>
        public AuthenticationTicket(byte[] dataToInclude, Action<AuthenticationTicket, bool> callback)
        {
            Callback = callback;
            IsClientTicket = true;
            
            var callResult = SteamUser.RequestEncryptedAppTicket(dataToInclude, dataToInclude.Length);
            API.Authentication.MEncryptedAppTicketResponse.Set(callResult, (result, error) =>
            {
                if(!error)
                {
                    if (result.m_eResult == EResult.k_EResultOK)
                    {
                        byte[] array = new byte[1024];
                        if (SteamUser.GetEncryptedAppTicket(array, 1024, out var mPcbTicket))
                        {
                            array = new byte[1024];
                            Array.Resize(ref array, (int)mPcbTicket);
                            Data = array;
                            CreatedOn = SteamUtils.GetServerRealTime();
                            callback?.Invoke(this, false);
                        }
                    }
                    else
                    {
                        Debug.LogError("Invalid encrypted ticket, no action taken.");
                        callback?.Invoke(this, true);
                    }
                }
            });
        }

        /// <summary>
        /// Represents an authentication ticket used to verify the identity of a Steam client or server.
        /// Provides mechanisms for ticket generation, authentication, and lifecycle management.
        /// </summary>
        public AuthenticationTicket(string webIdentity, Action<AuthenticationTicket, bool> callback)
        {
            Callback = callback;
            IsClientTicket = true;
            
            Handle = SteamUser.GetAuthTicketForWebApi(webIdentity);
            CreatedOn = SteamUtils.GetServerRealTime();
            
            Data = null;
        }

        /// <summary>
        /// Handle the Steam native authentication session ticket response
        /// </summary>
        /// <param name="response">The <see cref="Steamworks.GetAuthSessionTicketResponse_t"/></param> from Steam session ticket response
        public void Authenticate(GetAuthSessionTicketResponse_t response)
        {
            if (Handle != default && Handle != HAuthTicket.Invalid
                    && response.m_eResult == EResult.k_EResultOK)
            {
                Result = response.m_eResult;
                Verified = true;
                Callback?.Invoke(this, false);
            }
            else
            {
                Result = response.m_eResult;
                Callback?.Invoke(this, true);
            }
        }

        /// <summary>
        /// Authenticates the provided ticket response and updates the authentication state accordingly.
        /// </summary>
        /// <param name="response">The ticket response received from the Steamworks API that includes the ticket data and associated result information.</param>
        public void Authenticate(GetTicketForWebApiResponse_t response)
        {
            Data = new byte[response.m_cubTicket];
            Array.Copy(response.m_rgubTicket, Data, response.m_cubTicket);

            if (Handle != default && Handle != HAuthTicket.Invalid
                    && response.m_eResult == EResult.k_EResultOK)
            {
                Result = response.m_eResult;
                Verified = true;
                Callback?.Invoke(this, false);
            }
            else
            {
                Result = response.m_eResult;
                Callback?.Invoke(this, true);
            }
        }

        /// <summary>
        /// Gets the duration since the authentication ticket's creation, calculated as a time span.
        /// This property provides a measure of the ticket's age in relation to server real-time.
        /// </summary>
        public TimeSpan Age => new(0, 0, (int)(SteamUtils.GetServerRealTime() - CreatedOn));

        /// <summary>
        /// Cancels the authentication ticket by invalidating it via the associated Steamworks API.
        /// If the ticket belongs to a client, it invokes the `SteamUser.CancelAuthTicket` method;
        /// otherwise, it invokes the `SteamGameServer.CancelAuthTicket` method.
        /// </summary>
        public void Cancel()
        {
            if (IsClientTicket)
                SteamUser.CancelAuthTicket(Handle);
            else
                SteamGameServer.CancelAuthTicket(Handle);
        }
    }
    }
#endif