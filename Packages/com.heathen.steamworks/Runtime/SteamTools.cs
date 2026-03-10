#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Heathen.SteamworksIntegration;
using Heathen.SteamworksIntegration.API;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if UNITASK_INSTALLED
using Cysharp.Threading.Tasks;
#endif
using UnityEngine;
// ReSharper disable NotAccessedField.Local

namespace SteamTools
{
    /// <summary>
    /// Provides a static interface for interacting with Steam tools and utilities.
    /// </summary>
    public static class Interface
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            IsReady = false;
            _boards = new();
            _sets = new();
            _actions = new();
            _whenReadyCalls = new();
        }

        /// <summary>
        /// A property indicating whether the Steam integration has been successfully initialised.
        /// This property returns <c>true</c> if the initialisation process is complete and
        /// all necessary systems are ready for interaction, otherwise <c>false</c>.
        /// </summary>
        public static bool IsInitialised => App.Initialised;

        /// <summary>
        /// A property that controls whether debugging mode is enabled for the Steam integration system.
        /// When set to <c>true</c>, additional diagnostic information may be logged during execution
        /// to assist in debugging and development processes. Setting it to <c>false</c> disables
        /// these debugging features.
        /// </summary>
        public static bool IsDebugging
        {
            get => App.IsDebugging;
            set => App.IsDebugging = value;
        }

        /// <summary>
        /// A property indicating whether the SteamTools interface is fully initialised and ready for use.
        /// This property returns <c>true</c> if all required components have been successfully set up
        /// and the system is prepared to process interactions, otherwise <c>false</c>.
        /// </summary>
        public static bool IsReady { get; private set; }

        /// <summary>
        /// An event that is triggered when the Steam integration process has successfully completed its initialisation
        /// and is ready for interaction. Subscribers to this event will be notified once the system is fully prepared
        /// with all necessary data and components set up.
        /// </summary>
        public static event Action OnReady;

        /// <summary>
        /// An event that is triggered when an error occurs during the Steam integration initialisation process.
        /// This event provides a string argument containing a detailed error message
        /// to help identify the cause of the initialisation failure.
        /// </summary>
        public static event Action<string> OnInitialisationError;

        private static Dictionary<string, LeaderboardData> _boards = new();
        private static Dictionary<string, InputActionSetData> _sets = new();
        private static Dictionary<string, InputActionData> _actions = new();
        private static List<Action> _whenReadyCalls = new();

        /// <summary>
        /// Initialises the SteamTools interface by attempting to reflect the
        /// required Game.AppId for initialisation. This method locates and executes
        /// the static "Initialise" method on the "SteamTools.Game" class, if available.
        /// </summary>
        /// <remarks>
        /// If initialisation fails due to missing or inaccessible "SteamTools.Game",
        /// an error is logged and the <see cref="OnInitialisationError"/> event is triggered.
        /// This method also subscribes to the <see cref="Events.OnSteamInitialisationError"/> event
        /// for handling errors during the initialisation process.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown if an exception occurs while reflecting and invoking the "SteamTools.Game.Initialise" method.
        /// The exception details are also passed to the <see cref="OnInitialisationError"/> event.
        /// </exception>
        /// <example>
        /// This method is called during the Unity lifecycle by the <c>InitializeSteamworks</c> MonoBehaviour.
        /// </example>
        public static void Initialise()
        {
            Events.Initialise();
            Events.OnSteamInitialisationError += HandleInitialisedError;
            try
            {
                // Try to reflect Game.App.AppId
                Type gameType = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    gameType = asm.GetType("SteamTools.Game");
                    if (gameType == null) continue;
                    Debug.Log($"Found SteamTools.Game in assembly: {asm.GetName().Name}");
                    break;
                }

                if (gameType != null)
                {

                    var initMethod = gameType.GetMethod("Initialise", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (initMethod != null)
                        initMethod.Invoke(null, null);
                    else
                        Debug.LogError("Unable to locate SteamTools.Game.Initialise method make sure your generate wrapper before attempting to initialise.");
                }
                else
                {
                    Debug.LogError("Unable to locate SteamTools.Game class make sure your generate wrapper before attempting to initialise.");
                    OnInitialisationError?.Invoke("Unable to locate SteamTools.Game class");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error reflecting Game.AppId: {e.Message}");
                OnInitialisationError?.Invoke($"Error reflecting Game.AppId: {e.Message}");
            }
        }

        /// <summary>
        /// Registers a callback to be executed when the SteamTools interface is ready for use.
        /// If the interface is already ready at the time of calling, the callback is invoked immediately;
        /// otherwise, it is queued to be called when the <see cref="OnReady"/> event is raised.
        /// </summary>
        /// <param name="callback">
        /// The callback action to execute when the SteamTools interface is ready. If null, the method does nothing.
        /// </param>
        /// <remarks>
        /// This method ensures that dependent operations or components can safely execute their logic only
        /// after the readiness of the SteamTools interface has been confirmed. Callbacks are executed
        /// in the order they are registered (FIFO order).
        /// </remarks>
        public static void WhenReady(Action callback)
        {
            if (callback == null)
                return;

            if (IsReady)
                callback.Invoke();
            else
                _whenReadyCalls.Add(callback);
        }

        private static void HandleInitialisedError(string arg0)
        {
            OnInitialisationError?.Invoke(arg0);
        }

        /// <summary>
        /// Marks the SteamTools interface as ready and triggers the <see cref="OnReady"/> event.
        /// This method also assigns the specified maps for leaderboards, input action sets, and input actions,
        /// and processes any pending callbacks registered with <see cref="WhenReady(Action)"/>.
        /// </summary>
        /// <param name="boardMap">A dictionary mapping leaderboard names to their respective <see cref="LeaderboardData"/>.</param>
        /// <param name="setMap">A dictionary mapping input action set names to their respective <see cref="InputActionSetData"/>.</param>
        /// <param name="actionMap">A dictionary mapping input action names to their respective <see cref="InputActionData"/>.</param>
        /// <remarks>
        /// This method is typically called after successful initialisation of SteamTools.Game.
        /// It clears the list of pending callbacks once they have been executed.
        /// </remarks>
        public static void RaiseOnReady(Dictionary<string, LeaderboardData> boardMap,
                                        Dictionary<string, InputActionSetData> setMap,
                                        Dictionary<string, InputActionData> actionMap)
        {
            _boards = boardMap;
            _sets = setMap;
            _actions = actionMap;
            IsReady = true;
            OnReady?.Invoke();
            foreach (var call in _whenReadyCalls)
            {
                try
                {
                    call?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            _whenReadyCalls.Clear();
        }

        /// <summary>
        /// Adds a new leaderboard to the internal collection of boards maintained by the SteamTools interface.
        /// This method ensures that the leaderboard is registered using its API name as the key.
        /// </summary>
        /// <param name="board">
        /// The <see cref="LeaderboardData"/> object representing the leaderboard to be added.
        /// The <c>apiName</c> property of the <paramref name="board"/> is used as a key for registration.
        /// </param>
        /// <remarks>
        /// If a leaderboard with the same <c>apiName</c> already exists in the internal collection,
        /// it remains unmodified, and the method safely returns without an error.
        /// This method facilitates quick access to leaderboards by their API names for later operations.
        /// </remarks>
        public static void AddBoard(LeaderboardData board)
        {
            _boards.TryAdd(board.apiName, board);
        }

        /// <summary>
        /// Retrieves the leaderboard data associated with the specified name.
        /// </summary>
        /// <param name="name">
        /// The name of the leaderboard to retrieve. This corresponds to the API name of the leaderboard.
        /// </param>
        /// <returns>
        /// A <see cref="LeaderboardData"/> structure representing the leaderboard with the specified name,
        /// or <c>null</c> if no leaderboard with the given name exists.
        /// </returns>
        public static LeaderboardData GetBoard(string name)
        {
            return _boards.GetValueOrDefault(name);
        }

        /// <summary>
        /// Retrieves all registered leaderboard data as an array of <see cref="LeaderboardData"/> objects.
        /// </summary>
        /// <remarks>
        /// This method accesses the internal dictionary of leaderboards and returns all entries as an array.
        /// It is useful for scenarios where batch processing or enumeration of all leaderboards is required.
        /// </remarks>
        /// <returns>
        /// An array of <see cref="LeaderboardData"/> containing all leaderboards currently registered
        /// in the interface.
        /// </returns>
        public static LeaderboardData[] GetBoards() => _boards.Values.ToArray();

        /// <summary>
        /// Retrieves an input action set by its name from the Steam input action sets.
        /// </summary>
        /// <param name="name">
        /// The name of the input action set to retrieve.
        /// </param>
        /// <returns>
        /// The <see cref="InputActionSetData"/> associated with the specified name if the name exists within the collection;
        /// otherwise, returns null.
        /// </returns>
        public static InputActionSetData GetSet(string name)
        {
            return _sets.GetValueOrDefault(name);
        }

        /// <summary>
        /// Retrieves the input action data associated with the specified action name.
        /// </summary>
        /// <param name="name">The name of the input action to retrieve.</param>
        /// <returns>
        /// An <see cref="InputActionData"/> instance corresponding to the specified action name,
        /// or <c>null</c> if no matching action is found.
        /// </returns>
        public static InputActionData GetAction(string name)
        {
            return _actions.GetValueOrDefault(name);
        }
    }

    /// <summary>
    /// Provides a collection of predefined colour values for use with Steam-related utilities and editors.
    /// </summary>
    public static class Colors
    {
        /// <summary>
        /// A predefined colour value representing the signature blue often associated with Steam branding.
        /// This colour is primarily used for UI elements and components to maintain visual consistency
        /// within Steam-related utilities and editors. The RGBA value is approximately (0.2, 0.6, 0.93, 1).
        /// </summary>
        public static Color SteamBlue = new(0.2f, 0.60f, 0.93f, 1f);

        /// <summary>
        /// A predefined colour value representing a green shade often associated with Steam branding.
        /// This colour is primarily used for UI elements and components to convey a sense of success,
        /// confirmation, or environmental themes within Steam-related utilities and editors. The RGBA
        /// value is approximately (0.2, 0.42, 0.2, 1).
        /// </summary>
        public static Color SteamGreen = new(0.2f, 0.42f, 0.2f, 1f);

        /// <summary>
        /// A predefined colour representing a bright green shade with RGBA values (0.4, 0.84, 0.4, 1.0).
        /// Typically used in the SteamTools namespace for UI elements requiring a vibrant green accent.
        /// </summary>
        public static Color BrightGreen = new(0.4f, 0.84f, 0.4f, 1f);

        /// <summary>
        /// A predefined colour value representing a semi-transparent white with 50% opacity.
        /// This colour is commonly used to overlay visual elements or to create subtle highlights
        /// in Steam-related utilities and editors. The RGBA value is (1, 1, 1, 0.5).
        /// </summary>
        public static Color HalfAlpha = new(1f, 1f, 1f, 0.5f);

        /// <summary>
        /// Represents a colour with prominent red and light shading, commonly used to
        /// indicate errors or warnings in the user interface.
        /// This colour has the RGBA values (1, 0.5, 0.5, 1), producing a soft red tone
        /// with full opacity.
        /// </summary>
        public static Color ErrorRed = new(1, 0.5f, 0.5f, 1);
    }

    /// <summary>
    /// Provides static methods to manage Steam authentication sessions and integrations.
    /// </summary>
    public static class Authenticate
    {
        /// <summary>
        /// Initiates a new Steam authentication session for the provided user using the specified authentication ticket.
        /// This method validates the ticket and begins the session if the ticket is valid.
        /// </summary>
        /// <param name="user">
        /// The user for whom the authentication session is being initiated. This is represented as a <see cref="UserData"/> object.
        /// </param>
        /// <param name="ticket">
        /// The byte array representing the authentication ticket for the session.
        /// </param>
        /// <param name="callback">
        /// A callback function to handle the result of the authentication session initiation. The callback provides the result
        /// as an <see cref="EBeginAuthSessionResult"/> and an optional <see cref="AuthenticationSession"/> object when successful.
        /// </param>
        /// <remarks>
        /// If the ticket is invalid or there are any issues beginning the session, the callback is invoked with the
        /// respective <see cref="EBeginAuthSessionResult"/> value and a null session.
        /// </remarks>
        public static void BeginSession(UserData user, byte[] ticket, BeginSessionResult callback)
        {
            var result = Authentication.BeginAuthSession(ticket, user, session =>
            {
                callback?.Invoke(EBeginAuthSessionResult.k_EBeginAuthSessionResultOK, session);
            });

            if (result != EBeginAuthSessionResult.k_EBeginAuthSessionResultOK)
                callback?.Invoke(result, null);
        }
        
        /// <summary>
        /// Initiates a new Steam authentication session for the provided user using the specified authentication ticket.
        /// This method validates the ticket and begins the session if the ticket is valid.
        /// </summary>
        /// <param name="user">
        /// The user for whom the authentication session is being initiated. This is represented as a <see cref="UserData"/> object.
        /// </param>
        /// <param name="ticket">
        /// The byte array representing the authentication ticket for the session.
        /// </param>
        /// <returns>
        /// A Task that resolves when the authentication session initiation completes.
        /// The Task result contains a tuple:
        /// <list type="bullet">
        ///     <item><description><see cref="EBeginAuthSessionResult"/> indicating the result of the operation.</description></item>
        ///     <item><description><see cref="AuthenticationSession"/> representing the session if successful; otherwise null.</description></item>
        /// </list>
        /// </returns>
        public static Task<(EBeginAuthSessionResult result, AuthenticationSession session)> BeginSessionTask(UserData user, byte[] ticket)
        {
            var tcs = new TaskCompletionSource<(EBeginAuthSessionResult, AuthenticationSession)>();

            var result = Authentication.BeginAuthSession(ticket, user, session =>
            {
                tcs.TrySetResult((EBeginAuthSessionResult.k_EBeginAuthSessionResultOK, session));
            });

            if (result != EBeginAuthSessionResult.k_EBeginAuthSessionResultOK)
                tcs.TrySetResult((result, null));

            return tcs.Task;
        }
        
#if UNITASK_INSTALLED
        /// <summary>
        /// Initiates a new Steam authentication session for the provided user using the specified authentication ticket.
        /// This method validates the ticket and begins the session if the ticket is valid.
        /// </summary>
        /// <param name="user">
        /// The user for whom the authentication session is being initiated. This is represented as a <see cref="UserData"/> object.
        /// </param>
        /// <param name="ticket">
        /// The byte array representing the authentication ticket for the session.
        /// </param>
        /// <returns>
        /// A UniTask that resolves when the authentication session initiation completes.
        /// The UniTask result contains a tuple:
        /// <list type="bullet">
        ///     <item><description><see cref="EBeginAuthSessionResult"/> indicating the result of the operation.</description></item>
        ///     <item><description><see cref="AuthenticationSession"/> representing the session if successful; otherwise null.</description></item>
        /// </list>
        /// </returns>
        public static UniTask<(EBeginAuthSessionResult result, AuthenticationSession session)> BeginSessionUniTask(UserData user, byte[] ticket)
        {
            var tcs = new UniTaskCompletionSource<(EBeginAuthSessionResult, AuthenticationSession)>();

            var result = Authentication.BeginAuthSession(ticket, user, session =>
            {
                tcs.TrySetResult((EBeginAuthSessionResult.k_EBeginAuthSessionResultOK, session));
            });

            if (result != EBeginAuthSessionResult.k_EBeginAuthSessionResultOK)
                tcs.TrySetResult((result, null));

            return tcs.Task;
        }
#endif

        /// <summary>
        /// Ends the authentication session for the specified user.
        /// </summary>
        /// <param name="user">
        /// The user whose authentication session should be ended. This is used to terminate
        /// the validation session initiated for the specified <see cref="UserData"/>.
        /// </param>
        /// <remarks>
        /// This method invokes the <see cref="Authentication.EndAuthSession"/> API to properly close
        /// the active authentication session for the provided user. It is essential to call this method
        /// when the authentication session is no longer needed to maintain proper session management.
        /// </remarks>
        public static void EndSession(UserData user) => Authentication.EndAuthSession(user);

        /// <summary>
        /// Ends all active authentication sessions by invoking the <see cref="Authentication.EndAllSessions"/> method.
        /// </summary>
        /// <remarks>
        /// This method clears all currently tracked authentication sessions in the underlying Steamworks API.
        /// It is commonly used to ensure that no lingering sessions remain, especially during application shutdown
        /// or user logout scenarios.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown if an unexpected error occurs while processing active sessions.
        /// The specific exceptions depend on the behaviour of the underlying Steamworks implementation.
        /// </exception>
        public static void EndAllSessions() => Authentication.EndAllSessions();

        /// <summary>
        /// Attempts to retrieve an authentication session ticket for the specified server ID and invokes
        /// the provided callback with the generated ticket data once the ticket is ready. Handles errors during
        /// the authentication process by invoking the provided error callback.
        /// </summary>
        /// <param name="serverId">
        /// The Steam server ID for which the authentication session ticket is to be generated.
        /// </param>
        /// <param name="serverRpcDelegate">
        /// A callback method to execute upon successfully getting the authentication session ticket.
        /// The callback receives the user ID and the authentication ticket data.
        /// </param>
        /// <param name="onResult">
        /// A callback method to execute if the authentication process fails or if an error occurs while generating
        /// the ticket. The callback receives the error result denoting the type of failure.
        /// </param>
        /// <remarks>
        /// This method makes use of the Steamworks API to generate an authentication session ticket for
        /// authentication against a game server. If the ticket generation fails or encounters an I/O error,
        /// the error callback is invoked with the corresponding <see cref="EResult"/> error code.
        /// </remarks>
        public static void SendToRpcWhenReady(ulong serverId, SendGameServerAuthentication serverRpcDelegate, Action<AuthenticationTicket, EResult> onResult)
        {
            if (serverRpcDelegate == null)
            {
                var nId = new SteamNetworkingIdentity()
                { 
                    m_eType = ESteamNetworkingIdentityType.k_ESteamNetworkingIdentityType_SteamID 
                };
                nId.SetSteamID(new CSteamID(serverId));
                var authTicket = new AuthenticationTicket(nId, null);
                onResult?.Invoke(authTicket, EResult.k_EResultInvalidParam);
                return;
            }
            Authentication.GetAuthSessionTicket(new CSteamID(serverId), (ticket, ioError) =>
            {
                if (ioError || ticket.Result != EResult.k_EResultOK)
                    onResult?.Invoke(ticket, ticket.Result);
                else
                {
                    serverRpcDelegate.Invoke(UserData.Me, ticket.Data);
                    onResult?.Invoke(ticket, ticket.Result);
                }
            });
        }

        /// <summary>
        /// Attempts to retrieve an authentication session ticket for the specified server ID and invokes
        /// the provided RPC delegate with the generated ticket data once the ticket is ready.
        /// </summary>
        /// <param name="serverId">
        /// The Steam server ID for which the authentication session ticket is to be generated.
        /// </param>
        /// <param name="serverRpcDelegate">
        /// A delegate to execute upon successfully getting the authentication session ticket.
        /// The delegate receives the user ID and the authentication ticket data.
        /// </param>
        /// <returns>
        /// A Task that resolves when the authentication process completes.
        /// The Task result contains a tuple:
        /// <list type="bullet">
        ///     <item><description><see cref="AuthenticationTicket"/> containing ticket data and identity.</description></item>
        ///     <item><description><see cref="EResult"/> result of the operation.</description></item>
        /// </list>
        /// </returns>
        public static Task<(AuthenticationTicket ticket, EResult result)> SendToRpcWhenReadyTask(
            ulong serverId,
            SendGameServerAuthentication serverRpcDelegate)
        {
            var tcs = new TaskCompletionSource<(AuthenticationTicket, EResult)>();

            if (serverRpcDelegate == null)
            {
                var nId = new SteamNetworkingIdentity()
                {
                    m_eType = ESteamNetworkingIdentityType.k_ESteamNetworkingIdentityType_SteamID
                };
                nId.SetSteamID(new CSteamID(serverId));
                var authTicket = new AuthenticationTicket(nId, null);
                tcs.TrySetResult((authTicket, EResult.k_EResultInvalidParam));
                return tcs.Task;
            }

            Authentication.GetAuthSessionTicket(new CSteamID(serverId), (ticket, ioError) =>
            {
                if (ioError || ticket.Result != EResult.k_EResultOK)
                {
                    tcs.TrySetResult((ticket, ticket.Result));
                }
                else
                {
                    serverRpcDelegate.Invoke(UserData.Me, ticket.Data);
                    tcs.TrySetResult((ticket, ticket.Result));
                }
            });

            return tcs.Task;
        }

#if UNITASK_INSTALLED
        /// <summary>
        /// Attempts to retrieve an authentication session ticket for the specified server ID and invokes
        /// the provided RPC delegate with the generated ticket data once the ticket is ready.
        /// </summary>
        /// <param name="serverId">
        /// The Steam server ID for which the authentication session ticket is to be generated.
        /// </param>
        /// <param name="serverRpcDelegate">
        /// A delegate to execute upon successfully getting the authentication session ticket.
        /// The delegate receives the user ID and the authentication ticket data.
        /// </param>
        /// <returns>
        /// A UniTask that resolves when the authentication process completes.
        /// The UniTask result contains a tuple:
        /// <list type="bullet">
        ///     <item><description><see cref="AuthenticationTicket"/> containing ticket data and identity.</description></item>
        ///     <item><description><see cref="EResult"/> result of the operation.</description></item>
        /// </list>
        /// </returns>
        public static UniTask<(AuthenticationTicket ticket, EResult result)> SendToRpcWhenReadyUniTask(
            ulong serverId,
            SendGameServerAuthentication serverRpcDelegate)
        {
            var tcs = new UniTaskCompletionSource<(AuthenticationTicket, EResult)>();

            if (serverRpcDelegate == null)
            {
                var nId = new SteamNetworkingIdentity()
                {
                    m_eType = ESteamNetworkingIdentityType.k_ESteamNetworkingIdentityType_SteamID
                };
                nId.SetSteamID(new CSteamID(serverId));
                var authTicket = new AuthenticationTicket(nId, null);
                tcs.TrySetResult((authTicket, EResult.k_EResultInvalidParam));
                return tcs.Task;
            }

            Authentication.GetAuthSessionTicket(new CSteamID(serverId), (ticket, ioError) =>
            {
                if (ioError || ticket.Result != EResult.k_EResultOK)
                {
                    tcs.TrySetResult((ticket, ticket.Result));
                }
                else
                {
                    serverRpcDelegate.Invoke(UserData.Me, ticket.Data);
                    tcs.TrySetResult((ticket, ticket.Result));
                }
            });

            return tcs.Task;
        }
#endif

        /// <summary>
        /// Sends an authentication payload from the current user to the lobby owner
        /// when ready, including an authentication session ticket and optional inventory data.
        /// </summary>
        /// <param name="lobby">The lobby data containing the owner's information to whom the authentication payload is to be sent.</param>
        /// <param name="onResult">
        /// A callback invoked with the result of the authentication process.
        /// The result indicates whether the session ticket was successfully generated
        /// and sent to the lobby owner.
        /// </param>
        /// <remarks>
        /// This method retrieves an authentication session ticket for the current user
        /// and sends it to the lobby owner through the <c>Authenticate</c> method of the provided <c>LobbyData</c>.
        /// If an error occurs during ticket generation or validation, the <paramref name="onResult"/> callback
        /// is invoked with the corresponding error result.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Raised if the user is not properly authenticated or if there were issues with the session ticket generation process.
        /// </exception>
        public static void SendToLobbyOwnerWhenReady(LobbyData lobby, Action<AuthenticationTicket, EResult> onResult)
        {
            Authentication.GetAuthSessionTicket(lobby.Owner.user, (ticket, ioError) =>
            {
                if (ioError || ticket.Result != EResult.k_EResultOK)
                    onResult?.Invoke(ticket, ticket.Result);
                else
                {
                    lobby.Authenticate(new LobbyMessagePayload()
                    {
                        id = UserData.Me,
                        data = ticket.Data,
                        inventory = null
                    });

                    onResult?.Invoke(ticket, ticket.Result);
                }
            });
        }
        
        /// <summary>
        /// Sends an authentication payload from the current user to the lobby owner
        /// when ready, including an authentication session ticket and optional inventory data.
        /// </summary>
        /// <param name="lobby">The lobby data containing the owner's information to whom the authentication payload is to be sent.</param>
        /// <returns>
        /// A Task that resolves when the authentication process completes.
        /// The Task result contains a tuple:
        /// <list type="bullet">
        ///     <item><description><see cref="AuthenticationTicket"/> containing ticket data and identity.</description></item>
        ///     <item><description><see cref="EResult"/> result of the operation.</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method retrieves an authentication session ticket for the current user
        /// and sends it to the lobby owner through the <c>Authenticate</c> method of the provided <c>LobbyData</c>.
        /// If an error occurs during ticket generation or validation, the returned result
        /// will contain the corresponding error code.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Raised if the user is not properly authenticated or if there were issues with the session ticket generation process.
        /// </exception>
        public static Task<(AuthenticationTicket ticket, EResult result)> SendToLobbyOwnerWhenReadyTask(LobbyData lobby)
        {
            var tcs = new TaskCompletionSource<(AuthenticationTicket, EResult)>();

            Authentication.GetAuthSessionTicket(lobby.Owner.user, (ticket, ioError) =>
            {
                if (ioError || ticket.Result != EResult.k_EResultOK)
                {
                    tcs.TrySetResult((ticket, ticket.Result));
                }
                else
                {
                    lobby.Authenticate(new LobbyMessagePayload()
                    {
                        id = UserData.Me,
                        data = ticket.Data,
                        inventory = null
                    });

                    tcs.TrySetResult((ticket, ticket.Result));
                }
            });

            return tcs.Task;
        }
        
#if UNITASK_INSTALLED
        /// <summary>
        /// Sends an authentication payload from the current user to the lobby owner
        /// when ready, including an authentication session ticket and optional inventory data.
        /// </summary>
        /// <param name="lobby">The lobby data containing the owner's information to whom the authentication payload is to be sent.</param>
        /// <returns>
        /// A UniTask that resolves when the authentication process completes.
        /// The UniTask result contains a tuple:
        /// <list type="bullet">
        ///     <item><description><see cref="AuthenticationTicket"/> containing ticket data and identity.</description></item>
        ///     <item><description><see cref="EResult"/> result of the operation.</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method retrieves an authentication session ticket for the current user
        /// and sends it to the lobby owner through the <c>Authenticate</c> method of the provided <c>LobbyData</c>.
        /// If an error occurs during ticket generation or validation, the returned result
        /// will contain the corresponding error code.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Raised if the user is not properly authenticated or if there were issues with the session ticket generation process.
        /// </exception>
        public static UniTask<(AuthenticationTicket ticket, EResult result)> SendToLobbyOwnerWhenReadyUniTask(
            LobbyData lobby)
        {
            var tcs = new UniTaskCompletionSource<(AuthenticationTicket, EResult)>();

            Authentication.GetAuthSessionTicket(lobby.Owner.user, (ticket, ioError) =>
            {
                if (ioError || ticket.Result != EResult.k_EResultOK)
                {
                    tcs.TrySetResult((ticket, ticket.Result));
                }
                else
                {
                    lobby.Authenticate(new LobbyMessagePayload()
                    {
                        id = UserData.Me,
                        data = ticket.Data,
                        inventory = null
                    });

                    tcs.TrySetResult((ticket, ticket.Result));
                }
            });

            return tcs.Task;
        }
#endif

        /// <summary>
        /// Attempts to establish a provisional connection between the current application
        /// and Discord by getting and using a Steam-based provisional token for authentication.
        /// </summary>
        /// <remarks>
        /// This method interacts with the Heathen Discord Social Integration API to acquire a
        /// provisional token from Discord. If successful, it connects to Discord using the provided token.
        /// If the required integration is not present or accessible, an error message is logged.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown if an unexpected issue arises during the interaction with the Discord Social Integration API.
        /// Details of the exception, if any, will depend on the behaviour of the integration.
        /// </exception>
        public static void DiscordConnectProvisional()
        {
#if DISCORD //|| true
            Heathen.DiscordSocialIntegration.API.DiscordSocialApp.GetSteamProvisionalToken(
                (result, token, refreshToken, tokenType, expiresIn, scope) =>
                {
                    if (result.Successful())
                        Heathen.DiscordSocialIntegration.API.DiscordSocialApp.Connect(token, DateTime.UtcNow.AddSeconds(expiresIn), refreshToken);
                });
#else
            Debug.LogError($"[{nameof(DiscordConnectProvisional)}]: Heathen.DiscordSocialIntegration not found.");
#endif
        }

    }

    /// <summary>
    /// Provides a centralised static class for managing and dispatching Steam-specific events.
    /// </summary>
    /// <remarks>
    /// This class includes event definitions and internal methods for handling and raising events
    /// related to Steam initialisation, server connections, DLC installations, and other
    /// Steam API functionalities. External systems can subscribe to these events to react
    /// to critical updates in the Steam integration workflow.
    /// </remarks>
    public static class Events
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            // Reset all static events
            OnSteamInitialised = null;
            OnSteamInitialisationError = null;
            OnDlcInstalled = null;
            OnNewUrlLaunchParameters = null;
            OnSteamServersConnected = null;
            OnSteamServerConnectFailure = null;
            OnSteamServersDisconnected = null;
            OnGamepadTextInputShown = null;
            OnGamepadTextInputDismissed = null;
            OnGameConnectedChatLeave = null;
            OnGameConnectedClanChatMsg = null;
            OnGameConnectedChatJoin = null;
            OnGameConnectedFriendChatMsg = null;
            OnFriendRichPresenceUpdate = null;
            OnPersonaStateChange = null;
            OnInputDataChanged = null;
            OnControllerConnected = null;
            OnControllerDisconnected = null;
            OnInventoryDefinitionUpdate = null;
            OnInventoryResultReady = null;
            OnMicroTxnAuthorisationResponse = null;
            OnLobbyEnterSuccess = null;
            OnLobbyEnterFailed = null;
            OnLobbyDataUpdate = null;
            OnLobbyChatMsg = null;
            OnLobbyAuthentication = null;
            OnAskedToLeaveLobby = null;
            OnLobbyChatUpdate = null;
            OnLobbyGameServer = null;
            OnLobbyLeave = null;
            OnLobbyInvite = null;
            OnFavoritesListChanged = null;
            OnLobbyJoinRequested = null;
            OnGameOverlayActivated = null;
            OnGameServerChangeRequested = null;
            OnRichPresenceJoinRequested = null;
            OnReservationNotification = null;
            OnActiveBeaconsUpdated = null;
            OnAvailableBeaconLocationsUpdated = null;
            OnRemotePlaySessionConnected = null;
            OnRemotePlaySessionDisconnected = null;
            OnRemoteStorageLocalFileChange = null;
            OnScreenshotReady = null;
            OnScreenshotRequested = null;
            OnStatsReceived = null;
            OnStatsUnloaded = null;
            OnUserAchievementStored = null;
            OnStatsStored = null;
            OnAppResumeFromSuspend = null;
            OnKeyboardShown = null;
            OnKeyboardClosed = null;

            // Reset all static callbacks
            DisposeCallbacks();
        }
        
        #region Client Events

        /// <summary>
        /// An event that is triggered when Steam has been initialised successfully.
        /// This event can be subscribed to in order to perform actions or update dependent systems
        /// once the initial Steam integration setup is complete.
        /// </summary>
        public static event Action OnSteamInitialised;

        /// <summary>
        /// An event that is triggered when an error occurs during the Steam initialisation process.
        /// This event can be subscribed to in order to handle or log errors encountered
        /// while setting up the Steam integration, providing a string message with error details.
        /// </summary>
        public static event StringDelegate OnSteamInitialisationError;

        /// <summary>
        /// An event that is triggered when a piece of downloadable content (DLC) is installed
        /// and ownership of the DLC is gained by the user. This event provides information about
        /// the installed DLC, which can be used to enable specific content or update game systems accordingly.
        /// </summary>
        public static event DlcDataDelegate OnDlcInstalled;

        /// <summary>
        /// An event that is triggered when new URL launch parameters are detected.
        /// This event can be subscribed to in order to handle or process the parameters
        /// passed during the URL launch, enabling custom behaviour or configurations as needed.
        /// </summary>
        public static event Action OnNewUrlLaunchParameters;

        /// <summary>
        /// An event that is triggered when a successful connection to the Steam servers is established.
        /// This event can be subscribed to in order to perform actions dependent on a stable connection
        /// with the Steam servers, such as validating user sessions or syncing game data.
        /// </summary>
        public static event Action OnSteamServersConnected;

        /// <summary>
        /// An event that is triggered when a connection attempt to the Steam server has failed.
        /// This event occurs periodically if the Steam client is unable to establish a connection
        /// and retries have been unsuccessful. Subscribers can use this event to handle scenarios
        /// where a connection failure impacts functionality or requires user intervention.
        /// </summary>
        public static event SteamServerConnectFailureDelegate OnSteamServerConnectFailure;

        /// <summary>
        /// An event that is triggered when the client has lost connection to the Steam servers.
        /// This event provides the result of the disconnection as an <see cref="EResult"/> value,
        /// which can be used to determine the reason for the disconnection.
        /// Real-time services will remain disabled until a SteamServersConnected event is received.
        /// </summary>
        public static event EResultDelegate OnSteamServersDisconnected;

        /// <summary>
        /// An event triggered when the gamepad-based text input interface is displayed.
        /// This event enables subscribers to respond to the activation of the gamepad text entry UI,
        /// allowing for necessary adjustments or updates within the application.
        /// </summary>
        public static event Action OnGamepadTextInputShown;

        /// <summary>
        /// An event triggered when the gamepad text input interface is dismissed.
        /// This event provides a callback to indicate whether the input was confirmed
        /// or cancelled and includes the associated text input value if confirmed.
        /// </summary>
        public static event SteamTextInputDelegate OnGamepadTextInputDismissed;

        /// <summary>
        /// An event triggered when a player leaves a game-connected chat room.
        /// This event provides details about the departure, including the chat room, the user who left,
        /// and whether the user was kicked or disconnected unexpectedly.
        /// </summary>
        public static event SteamUserLeaveDataDelegate OnGameConnectedChatLeave;

        /// <summary>
        /// Event triggered when a message is received in a clan chat room that the current user is connected to.
        /// This event provides details about the chat room, the user who sent the message, the message content,
        /// and the type of message received.
        /// </summary>
        public static event SteamClanChatMsgDelegate OnGameConnectedClanChatMsg;

        /// <summary>
        /// An event triggered when a user joins a game-connected chat room.
        /// This event provides information about the chat room and the user who joined,
        /// allowing subscribed handlers to respond to the event accordingly.
        /// </summary>
        public static event SteamClanChatJoinDelegate OnGameConnectedChatJoin;

        /// <summary>
        /// An event triggered when a chat message from a connected friend is received during gameplay.
        /// This event provides details about the sender, the message content, and the type of chat entry.
        /// Subscribers can use this event to process or respond to friend chat messages in real-time.
        /// </summary>
        public static event SteamFriendChatMsgDelegate OnGameConnectedFriendChatMsg;

        /// <summary>
        /// An event triggered when a friend's rich presence data is updated.
        /// This event is invoked whenever the rich presence information of a friend changes,
        /// such as a friend's status or activity within a game or application.
        /// </summary>
        public static event SteamFriendRichPresenceUpdateDelegate OnFriendRichPresenceUpdate;

        /// <summary>
        /// An event that is triggered when a change occurs in the persona state of a Steam user.
        /// This includes updates to user information such as username, avatar, online status, or other relevant persona attributes.
        /// Subscribers can handle this event to respond to changes in Steam friend data.
        /// </summary>
        public static event PersonaStateChangeEvent OnPersonaStateChange;

        /// <summary>
        /// An event that is invoked whenever the input data state changes.
        /// This event notifies subscribers of updates to the state of input controllers,
        /// such as changes in button presses, joystick movements, or other input data.
        /// </summary>
        public static event InputControllerStateDataDelegate OnInputDataChanged;

        /// <summary>
        /// An event that triggers when a new controller is connected to the system through Steam Input.
        /// Subscribers to this event will receive the corresponding input handle, allowing them
        /// to perform further actions or configurations for the connected controller.
        /// </summary>
        public static event SteamInputHandleDelegate OnControllerConnected;

        /// <summary>
        /// An event triggered when a controller disconnects during runtime.
        /// This event allows subscribers to respond to controller disconnection
        /// events by providing the relevant input handle associated with the disconnected controller.
        /// </summary>
        public static event SteamInputHandleDelegate OnControllerDisconnected;

        /// <summary>
        /// An event that is triggered when the inventory item definitions are updated.
        /// This event is invoked internally in response to changes in the Steam inventory system.
        /// Subscribers can attach to this event to handle custom logic or perform actions
        /// that depend on updated inventory definitions.
        /// </summary>
        public static event Action OnInventoryDefinitionUpdate;

        /// <summary>
        /// An event that is triggered when a Steam inventory result is ready.
        /// This event occurs after a Steam inventory operation, such as item generation or exchange,
        /// has completed, providing the resulting data for further processing or display.
        /// </summary>
        public static event SteamInventoryResultReadyDelegate OnInventoryResultReady;

        /// <summary>
        /// An event triggered upon receiving a microtransaction authorisation response from Steam.
        /// This event provides details about the transaction, including the application ID,
        /// order ID, and whether the transaction was authorised successfully.
        /// </summary>
        public static event SteamMtxTranAuthDelegate OnMicroTxnAuthorisationResponse;

        /// <summary>
        /// An event triggered when a lobby is successfully entered in the Steam matchmaking system.
        /// This event provides details about the entered lobby using a Steam Lobby ID.
        /// Subscribing to this event allows handling custom logic upon successful lobby entry.
        /// </summary>
        public static event SteamLobbyEnterSuccessDelegate OnLobbyEnterSuccess;

        /// <summary>
        /// An event triggered when the attempt to enter a Steam lobby fails.
        /// This event is invoked alongside relevant details such as the lobby ID
        /// and the response code indicating the reason for failure.
        /// </summary>
        public static event SteamLobbyEnterFailedDelegate OnLobbyEnterFailed;

        /// <summary>
        /// An event triggered when there is an update to the metadata or state of a Steam lobby.
        /// This event is invoked in scenarios such as changes to lobby configuration, member updates,
        /// or metadata alterations. Listeners of this event can use the provided data to handle
        /// or respond to the updated state of the lobby.
        /// </summary>
        public static event SteamLobbyDataUpdateDelegate OnLobbyDataUpdate;

        /// <summary>
        /// An event that triggers when a chat message is received within a Steam Lobby.
        /// This event is invoked for general chat messages that are not associated with a specific
        /// authentication request or other contextual data.
        /// </summary>
        public static event SteamLobbyChatMsgDelegate OnLobbyChatMsg;

        /// <summary>
        /// An event triggered when a lobby authentication message is received.
        /// This event provides information related to the authentication process within a Steam lobby,
        /// including details about the lobby, the sender, and any associated data or inventory
        /// relevant to the authentication context.
        /// </summary>
        public static event SteamLobbyAuthDelegate OnLobbyAuthentication;

        /// <summary>
        /// An event that is triggered when the current user is asked to leave a lobby.
        /// This typically occurs when the lobby's metadata changes to indicate the user should
        /// no longer remain in the lobby, such as in scenarios involving user kicks.
        /// </summary>
        public static event SteamLobbyDataDelegate OnAskedToLeaveLobby;

        /// <summary>
        /// An event that triggers when there is an update to the chat activity within a Steam lobby.
        /// This event is invoked to reflect changes such as members joining, leaving, being muted, or other chat-related activity.
        /// </summary>
        public static event SteamLobbyChatUpdateDelegate OnLobbyChatUpdate;

        /// <summary>
        /// An event triggered when a lobby game server is created within the Steamworks environment.
        /// Subscribers to this event will receive details about the created lobby game server,
        /// including the Steam ID of the lobby, Steam ID of the game server, its IP address, and port.
        /// </summary>
        public static event SteamLobbyGameServerDelegate OnLobbyGameServer;

        /// <summary>
        /// An event triggered when a user leaves a Steam lobby. This event provides
        /// the lobby data associated with the lobby that was left, allowing systems
        /// to respond accordingly to the lobby exit.
        /// </summary>
        public static event SteamLobbyDataDelegate OnLobbyLeave;

        /// <summary>
        /// An event that is triggered when the user receives a lobby invitation from another Steam user.
        /// This event allows subscribers to handle lobby invitations and perform actions, such as
        /// displaying a notification or joining the lobby, when the event is fired.
        /// </summary>
        public static event SteamLobbyInviteDelegate OnLobbyInvite;

        /// <summary>
        /// An event triggered whenever the favorites list in the Steam integration changes.
        /// This event allows subscribers to respond to updates such as additions, removals,
        /// or modifications in the favorites list.
        /// </summary>
        public static event SteamFavoritesListChangeDelegate OnFavoritesListChanged;

        /// <summary>
        /// An event triggered when a request to join a lobby is received.
        /// This event allows subscribers to handle logic when a user is invited to
        /// or attempts to join a lobby, typically integrating with Steamworks functionality.
        /// </summary>
        public static event SteamLobbyJoinRequestDelegate OnLobbyJoinRequested;

        /// <summary>
        /// An event triggered when the Steam in-game overlay is activated or deactivated.
        /// This event provides a boolean parameter that indicates the activation state,
        /// where <c>true</c> represents that the overlay is active and <c>false</c> indicates
        /// it has been deactivated.
        /// </summary>
        public static event BoolDelegate OnGameOverlayActivated;

        /// <summary>
        /// An event that is triggered when a request to change the current game server is made.
        /// This event allows subscribers to handle the process of transitioning to a new server
        /// based on the request details provided during invocation.
        /// </summary>
        public static event SteamGameServerChangeRequestedDelegate OnGameServerChangeRequested;

        /// <summary>
        /// An event that triggers when a rich presence join request is received.
        /// This event is invoked with the details of the join request, allowing the
        /// application to process or respond to the request as needed.
        /// </summary>
        public static event SteamRichPresenceJoinRequestedDelegate OnRichPresenceJoinRequested;

        /// <summary>
        /// An event triggered when a reservation notification is received.
        /// This event provides information about the Steam ID of the joiner and the associated beacon ID
        /// for a reservation on a Steam lobby or game session.
        /// </summary>
        public static event SteamReservationNotificationDelegate OnReservationNotification;

        /// <summary>
        /// An event triggered whenever the list of active Steam beacons is updated.
        /// This event provides a mechanism to notify subscribers about changes in
        /// the state or availability of active beacons.
        /// </summary>
        public static event Action OnActiveBeaconsUpdated;

        /// <summary>
        /// An event triggered when the available beacon locations are updated.
        /// Subscribers to this event are notified whenever there is a change in
        /// the set of available locations for creating or joining a beacon.
        /// </summary>
        public static event Action OnAvailableBeaconLocationsUpdated;

        /// <summary>
        /// An event triggered when a remote play session is successfully connected.
        /// This event notifies subscribers that a new remote play session has been established,
        /// providing the necessary context for handling the connection in the application.
        /// </summary>
        public static event SteamRemotePlaySessionIdDelegate OnRemotePlaySessionConnected;

        /// <summary>
        /// An event triggered when a remote play session is disconnected.
        /// This event notifies subscribers that a previously active remote play session
        /// has been terminated, allowing for the appropriate handling of session disconnection scenarios.
        /// </summary>
        public static event SteamRemotePlaySessionIdDelegate OnRemotePlaySessionDisconnected;

        /// <summary>
        /// An event that triggers when a file in the local Steam Remote Storage directory is changed.
        /// This event allows subscribers to handle changes such as updates, creations, or deletions
        /// of files that are mirrored in Steam's cloud storage.
        /// </summary>
        public static event Action OnRemoteStorageLocalFileChange;

        /// <summary>
        /// An event triggered when a screenshot has been successfully processed and is ready.
        /// This event provides data about the processed screenshot, including its handle
        /// and the result status of the operation.
        /// </summary>
        public static event SteamScreenshotReadyDelegate OnScreenshotReady;

        /// <summary>
        /// An event triggered when a screenshot request is made.
        /// This event can be used to handle custom logic or actions
        /// associated with capturing or processing a screenshot request.
        /// </summary>
        public static event Action OnScreenshotRequested;

        /// <summary>
        /// An event triggered when the stats for a game are received from Steam's backend services.
        /// This event provides information about the game ID, the result of the stats retrieval operation,
        /// and the Steam ID of the user for whom the stats were requested.
        /// </summary>
        public static event SteamStatsReceivedDelegate OnStatsReceived;

        /// <summary>
        /// An event triggered when user statistics data for a specific Steam user is unloaded.
        /// This can indicate that data previously loaded for a Steam user is no longer available
        /// or is being cleared from memory. Typically used to perform any necessary clean-up
        /// or update systems dependent on that data.
        /// </summary>
        public static event SteamUserStatsUnloadedDelegate OnStatsUnloaded;

        /// <summary>
        /// An event that triggers when a user's achievement is stored.
        /// This event provides information about the game ID, whether the achievement
        /// is group-based, the name of the achievement, the current progress made,
        /// and the maximum progress required to complete the achievement.
        /// </summary>
        public static event SteamUserAchievementStoredDelegate OnUserAchievementStored;

        /// <summary>
        /// An event triggered when statistics for the game have been stored on Steam.
        /// This event provides the game's unique identifier and the result of the operation,
        /// indicating whether the storage process was successful or encountered an error.
        /// </summary>
        public static event SteamStatsStoredDelegate OnStatsStored;

        /// <summary>
        /// An event that is triggered when the application resumes operation after being suspended.
        /// This event can be used to execute logic required to handle state restoration or other
        /// operations needed when the app transitions back to an active state.
        /// </summary>
        public static event Action OnAppResumeFromSuspend;

        /// <summary>
        /// An event triggered when the on-screen keyboard is shown.
        /// Subscribers to this event will be notified whenever the keyboard becomes visible,
        /// allowing actions to be taken in response to the keyboard's appearance.
        /// </summary>
        public static event Action OnKeyboardShown;

        /// <summary>
        /// An event that is triggered when the virtual or physical keyboard is closed.
        /// Subscribing to this event allows the execution of custom logic in response
        /// to the keyboard closure action.
        /// </summary>
        public static event Action OnKeyboardClosed;
        #endregion

        #region Callbacks
        private static Callback<DlcInstalled_t> _dlcInstalled;
        private static Callback<NewUrlLaunchParameters_t> _newUrlLaunchParameters;
        private static Callback<SteamServerConnectFailure_t> _steamServerConnectFailure;
        private static Callback<SteamServersConnected_t> _steamServersConnected;
        private static Callback<SteamServersDisconnected_t> _steamServersDisconnected;
        private static Callback<GamepadTextInputDismissed_t> _gamepadTextInputDismissed;
        private static Callback<GameConnectedChatLeave_t> _gameConnectedChatLeave;
        private static Callback<GameConnectedClanChatMsg_t> _gameConnectedClanChatMsg;
        private static Callback<GameConnectedChatJoin_t> _gameConnectedChatJoin;
        private static Callback<GameConnectedFriendChatMsg_t> _gameConnectedFriendChatMsg;
        private static Callback<FriendRichPresenceUpdate_t> _friendRichPresenceUpdate;
        private static Callback<PersonaStateChange_t> _personaStateChange;
        private static Callback<SteamInventoryDefinitionUpdate_t> _steamInventoryDefinitionUpdate;
        private static Callback<SteamInventoryResultReady_t> _steamInventoryResultReady;
        private static Callback<MicroTxnAuthorizationResponse_t> _microTxnAuthorizationResponse;
        private static Callback<LobbyEnter_t> _lobbyEnter;
        private static Callback<LobbyDataUpdate_t> _lobbyDataUpdate;
        private static Callback<LobbyChatMsg_t> _lobbyChatMsg;
        private static Callback<LobbyChatUpdate_t> _lobbyChatUpdate;
        private static Callback<LobbyGameCreated_t> _lobbyGameCreated;
        private static Callback<LobbyInvite_t> _lobbyInvite;
        private static Callback<FavoritesListChanged_t> _favoritesListChanged;
        private static Callback<GameLobbyJoinRequested_t> _gameLobbyJoinRequested;
        private static Callback<GameOverlayActivated_t> _gameOverlayActivated;
        private static Callback<GameServerChangeRequested_t> _gameServerChangeRequested;
        private static Callback<GameRichPresenceJoinRequested_t> _gameRichPresenceJoinRequested;
        private static Callback<ReservationNotificationCallback_t> _reservationNotificationCallback;
        private static Callback<ActiveBeaconsUpdated_t> _activeBeaconsUpdated;
        private static Callback<AvailableBeaconLocationsUpdated_t> _availableBeaconLocationsUpdated;
        private static Callback<SteamRemotePlaySessionConnected_t> _remotePlaySessionConnected;
        private static Callback<SteamRemotePlaySessionDisconnected_t> _remotePlaySessionDisconnected;
        private static Callback<RemoteStorageLocalFileChange_t> _remoteStorageLocalFileChange;
        private static Callback<ScreenshotReady_t> _screenshotReady;
        private static Callback<ScreenshotRequested_t> _screenshotRequested;
        private static Callback<UserStatsReceived_t> _userStatsReceived;
        private static Callback<UserStatsUnloaded_t> _userStatsUnloaded;
        private static Callback<UserStatsStored_t> _userStatsStored;
        private static Callback<UserAchievementStored_t> _userAchievementStored;
        private static Callback<AppResumingFromSuspend_t> _appResumeFromSuspend;
        private static Callback<FloatingGamepadTextInputDismissed_t> _floatingGamepadTextInputDismissed;
        #endregion

        private static void DisposeCallbacks()
        {
            _dlcInstalled?.Dispose();
            _dlcInstalled = null;
            _newUrlLaunchParameters?.Dispose();
            _newUrlLaunchParameters = null;
            _steamServerConnectFailure?.Dispose();
            _steamServerConnectFailure = null;
            _steamServersConnected?.Dispose();
            _steamServersConnected = null;
            _steamServersDisconnected?.Dispose();
            _steamServersDisconnected = null;
            _gamepadTextInputDismissed?.Dispose();
            _gamepadTextInputDismissed = null;
            _gameConnectedChatLeave?.Dispose();
            _gameConnectedChatLeave = null;
            _gameConnectedClanChatMsg?.Dispose();
            _gameConnectedClanChatMsg = null;
            _gameConnectedChatJoin?.Dispose();
            _gameConnectedChatJoin = null;
            _gameConnectedFriendChatMsg?.Dispose();
            _gameConnectedFriendChatMsg = null;
            _friendRichPresenceUpdate?.Dispose();
            _friendRichPresenceUpdate = null;
            _personaStateChange?.Dispose();
            _personaStateChange = null;
            _steamInventoryDefinitionUpdate?.Dispose();
            _steamInventoryDefinitionUpdate = null;
            _steamInventoryResultReady?.Dispose();
            _steamInventoryResultReady = null;
            _microTxnAuthorizationResponse?.Dispose();
            _microTxnAuthorizationResponse = null;
            _lobbyEnter?.Dispose();
            _lobbyEnter = null;
            _lobbyDataUpdate?.Dispose();
            _lobbyDataUpdate = null;
            _lobbyChatMsg?.Dispose();
            _lobbyChatMsg = null;
            _lobbyChatUpdate?.Dispose();
            _lobbyChatUpdate = null;
            _lobbyGameCreated?.Dispose();
            _lobbyGameCreated = null;
            _lobbyInvite?.Dispose();
            _lobbyInvite = null;
            _favoritesListChanged?.Dispose();
            _favoritesListChanged = null;
            _gameLobbyJoinRequested?.Dispose();
            _gameLobbyJoinRequested = null;
            _gameOverlayActivated?.Dispose();
            _gameOverlayActivated = null;
            _gameServerChangeRequested?.Dispose();
            _gameServerChangeRequested = null;
            _gameRichPresenceJoinRequested?.Dispose();
            _gameRichPresenceJoinRequested = null;
            _reservationNotificationCallback?.Dispose();
            _reservationNotificationCallback = null;
            _activeBeaconsUpdated?.Dispose();
            _activeBeaconsUpdated = null;
            _availableBeaconLocationsUpdated?.Dispose();
            _availableBeaconLocationsUpdated = null;
            _remotePlaySessionConnected?.Dispose();
            _remotePlaySessionConnected = null;
            _remotePlaySessionDisconnected?.Dispose();
            _remotePlaySessionDisconnected = null;
            _remoteStorageLocalFileChange?.Dispose();
            _remoteStorageLocalFileChange = null;
            _screenshotReady?.Dispose();
            _screenshotReady = null;
            _screenshotRequested?.Dispose();
            _screenshotRequested = null;
            _userStatsReceived?.Dispose();
            _userStatsReceived = null;
            _userStatsUnloaded?.Dispose();
            _userStatsUnloaded = null;
            _userStatsStored?.Dispose();
            _userStatsStored = null;
            _userAchievementStored?.Dispose();
            _userAchievementStored = null;
            _appResumeFromSuspend?.Dispose();
            _appResumeFromSuspend = null;
            _floatingGamepadTextInputDismissed?.Dispose();
            _floatingGamepadTextInputDismissed = null;
        }

        internal static void Initialise()
        {
#if !UNITY_SERVER
            _dlcInstalled = Callback<DlcInstalled_t>.Create(OnDlcInstalledCallback);
            _newUrlLaunchParameters = Callback<NewUrlLaunchParameters_t>.Create(OnNewUrlLaunchParametersCallback);
            _steamServerConnectFailure = Callback<SteamServerConnectFailure_t>.Create(OnSteamServerConnectFailureCallback);
            _steamServersConnected = Callback<SteamServersConnected_t>.Create(OnSteamServersConnectedCallback);
            _steamServersDisconnected = Callback<SteamServersDisconnected_t>.Create(OnSteamServersDisconnectedCallback);
            _gamepadTextInputDismissed = Callback<GamepadTextInputDismissed_t>.Create(OnGamepadTextInputDismissedCallback);
            _gameConnectedChatLeave = Callback<GameConnectedChatLeave_t>.Create(OnGameConnectedChatLeaveCallback);
            _gameConnectedClanChatMsg = Callback<GameConnectedClanChatMsg_t>.Create(OnGameConnectedClanChatMsgCallback);
            _gameConnectedChatJoin = Callback<GameConnectedChatJoin_t>.Create(OnGameConnectedChatJoinCallback);
            _gameConnectedFriendChatMsg = Callback<GameConnectedFriendChatMsg_t>.Create(OnGameConnectedFriendChatMsgCallback);
            _friendRichPresenceUpdate = Callback<FriendRichPresenceUpdate_t>.Create(OnFriendRichPresenceUpdateCallback);
            _personaStateChange = Callback<PersonaStateChange_t>.Create(OnPersonaStateChangeCallback);
            _steamInventoryDefinitionUpdate = Callback<SteamInventoryDefinitionUpdate_t>.Create(OnSteamInventoryDefinitionUpdateCallback);
            _steamInventoryResultReady = Callback<SteamInventoryResultReady_t>.Create(OnSteamInventoryResultReadyCallback);
            _microTxnAuthorizationResponse = Callback<MicroTxnAuthorizationResponse_t>.Create(OnMicroTxnAuthorizationResponseCallback);
            _lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEnterCallback);
            _lobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdateCallback);
            _lobbyChatMsg = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMsgCallback);
            _lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdateCallback);
            _lobbyGameCreated = Callback<LobbyGameCreated_t>.Create(OnLobbyGameCreated);
            _lobbyInvite = Callback<LobbyInvite_t>.Create(OnLobbyInviteCallback);
            _favoritesListChanged = Callback<FavoritesListChanged_t>.Create(OnFavoritesListChangedCallback);
            _gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequestedCallback); 
            _gameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivatedCallback);
            _gameServerChangeRequested = Callback<GameServerChangeRequested_t>.Create(OnGameServerChangeRequestedCallback); 
            _gameRichPresenceJoinRequested = Callback<GameRichPresenceJoinRequested_t>.Create(OnGameRichPresenceJoinRequestedCallback); 
            _reservationNotificationCallback = Callback<ReservationNotificationCallback_t>.Create(OnReservationNotificationCallback);
            _activeBeaconsUpdated = Callback<ActiveBeaconsUpdated_t>.Create(OnActiveBeaconsUpdatedCallback);
            _availableBeaconLocationsUpdated = Callback<AvailableBeaconLocationsUpdated_t>.Create(OnAvailableBeaconLocationsUpdatedCallback);
            _remotePlaySessionConnected = Callback<SteamRemotePlaySessionConnected_t>.Create(OnRemotePlaySessionConnectedCallback);
            _remotePlaySessionDisconnected = Callback<SteamRemotePlaySessionDisconnected_t>.Create(OnRemotePlaySessionDisconnectedCallback);
            _remoteStorageLocalFileChange = Callback<RemoteStorageLocalFileChange_t>.Create(OnRemoteStorageLocalFileChangeCallback);
            _screenshotReady = Callback<ScreenshotReady_t>.Create(OnScreenshotReadyCallback);
            _screenshotRequested = Callback<ScreenshotRequested_t>.Create(OnScreenshotRequestedCallback);
            _userStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceivedCallback);
            _userStatsUnloaded = Callback<UserStatsUnloaded_t>.Create(OnUserStatsUnloadedCallback);
            _userStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStoredCallback);
            _userAchievementStored = Callback<UserAchievementStored_t>.Create(OnUserAchievementStoredCallback);
            _appResumeFromSuspend = Callback<AppResumingFromSuspend_t>.Create(OnAppResumeFromSuspendCallback);
            _floatingGamepadTextInputDismissed = Callback<FloatingGamepadTextInputDismissed_t>.Create(OnFloatingGamepadTextInputDismissedCallback);
#else
            _steamServerConnectFailure =
 Callback<SteamServerConnectFailure_t>.CreateGameServer(OnSteamServerConnectFailureCallback);
            _steamServersConnected =
 Callback<SteamServersConnected_t>.CreateGameServer(OnSteamServersConnectedCallback);
            _steamServersDisconnected =
 Callback<SteamServersDisconnected_t>.CreateGameServer(OnSteamServersDisconnectedCallback);
#endif
        }

        private static void OnFloatingGamepadTextInputDismissedCallback(FloatingGamepadTextInputDismissed_t param)
        {
            OnKeyboardClosed?.Invoke();
        }

        private static void OnAppResumeFromSuspendCallback(AppResumingFromSuspend_t param)
        {
            OnAppResumeFromSuspend?.Invoke();
        }

        private static void OnUserAchievementStoredCallback(UserAchievementStored_t param)
        {
            OnUserAchievementStored?.Invoke(new(param));
        }

        private static void OnUserStatsStoredCallback(UserStatsStored_t param)
        {
            OnStatsStored?.Invoke(param.m_nGameID, param.m_eResult);
        }

        private static void OnUserStatsUnloadedCallback(UserStatsUnloaded_t param)
        {
            OnStatsUnloaded?.Invoke(param.m_steamIDUser);
        }

        private static void OnUserStatsReceivedCallback(UserStatsReceived_t param)
        {
            OnStatsReceived?.Invoke(param.m_nGameID, param.m_eResult, param.m_steamIDUser);
        }

        private static void OnScreenshotRequestedCallback(ScreenshotRequested_t param)
        {
            OnScreenshotRequested?.Invoke();
        }

        private static void OnScreenshotReadyCallback(ScreenshotReady_t param)
        {
            OnScreenshotReady?.Invoke(param.m_hLocal, param.m_eResult);
        }

        private static void OnRemoteStorageLocalFileChangeCallback(RemoteStorageLocalFileChange_t param)
        {
            OnRemoteStorageLocalFileChange?.Invoke();
        }

        private static void OnRemotePlaySessionDisconnectedCallback(SteamRemotePlaySessionDisconnected_t param)
        {
            OnRemotePlaySessionDisconnected?.Invoke(param.m_unSessionID);
        }

        private static void OnRemotePlaySessionConnectedCallback(SteamRemotePlaySessionConnected_t param)
        {
            OnRemotePlaySessionConnected?.Invoke(param.m_unSessionID);
        }

        private static void OnAvailableBeaconLocationsUpdatedCallback(AvailableBeaconLocationsUpdated_t param)
        {
            OnAvailableBeaconLocationsUpdated?.Invoke();
        }

        private static void OnActiveBeaconsUpdatedCallback(ActiveBeaconsUpdated_t param)
        {
            OnActiveBeaconsUpdated?.Invoke();
        }

        private static void OnReservationNotificationCallback(ReservationNotificationCallback_t param)
        {
            Parties.Client.ReservationList ??= new List<ReservationNotificationCallback_t>();
            Parties.Client.ReservationList.Add(param);
            OnReservationNotification?.Invoke(param.m_steamIDJoiner, param.m_ulBeaconID);
        }

        private static void OnGameRichPresenceJoinRequestedCallback(GameRichPresenceJoinRequested_t param)
        {
            OnRichPresenceJoinRequested?.Invoke(param.m_steamIDFriend, param.m_rgchConnect);
        }

        private static void OnGameServerChangeRequestedCallback(GameServerChangeRequested_t param)
        {
            OnGameServerChangeRequested?.Invoke(param.m_rgchServer, param.m_rgchPassword);
        }

        private static void OnGameOverlayActivatedCallback(GameOverlayActivated_t param)
        {
            Overlay.Client.IsShowing = param.m_bActive == 1;
            OnGameOverlayActivated?.Invoke(param.m_bActive == 1);
        }

        private static void OnGameLobbyJoinRequestedCallback(GameLobbyJoinRequested_t param)
        {
            OnLobbyJoinRequested?.Invoke(param.m_steamIDLobby, param.m_steamIDFriend);
        }

        private static void OnFavoritesListChangedCallback(FavoritesListChanged_t param)
        {
            OnFavoritesListChanged?.Invoke(new(Utilities.IPUintToString(param.m_nIP), param.m_nQueryPort, param.m_nConnPort, param.m_nAppID, param.m_nFlags, param.m_bAdd, param.m_unAccountId));
        }

        private static void OnLobbyInviteCallback(LobbyInvite_t param)
        {
            OnLobbyInvite?.Invoke(param.m_ulSteamIDUser, param.m_ulSteamIDLobby, param.m_ulGameID);
        }

        private static void OnLobbyGameCreated(LobbyGameCreated_t param)
        {
            OnLobbyGameServer?.Invoke(param.m_ulSteamIDLobby, new CSteamID(param.m_ulSteamIDGameServer), Utilities.IPUintToString(param.m_unIP), param.m_usPort);
        }

        private static void OnLobbyChatUpdateCallback(LobbyChatUpdate_t param)
        {
            OnLobbyChatUpdate?.Invoke(param.m_ulSteamIDLobby, param.m_ulSteamIDUserChanged, (EChatMemberStateChange)param.m_rgfChatMemberStateChange);
        }

        private static void OnLobbyChatMsgCallback(LobbyChatMsg_t param)
        {
            var data = new byte[4096];
            var lobby = new CSteamID(param.m_ulSteamIDLobby);
            var ret = SteamMatchmaking.GetLobbyChatEntry(lobby, (int)param.m_iChatID, out var user, data, data.Length, out var chatEntryType);
            Array.Resize(ref data, ret);

            var chatMsg = new LobbyChatMsg
            {
                lobby = lobby,
                type = chatEntryType,
                data = data,
                ReceivedTime = DateTime.Now,
                sender = user,
            };

            //If this is a chat message
            //and is not from me,
            //and I am the owner of the lobby its from
            if (chatMsg.type == EChatEntryType.k_EChatEntryTypeChatMsg
                && chatMsg.sender != UserData.Me
                && chatMsg.lobby.IsOwner)
            {
                // Handle auth request and chat independently
                if (chatMsg.TryFromJson<LobbyMessagePayload>(out var authRequest)
                    && authRequest.data != null)
                    OnLobbyAuthentication?.Invoke(chatMsg.lobby, chatMsg.sender, authRequest.data, authRequest.inventory);
                else
                    OnLobbyChatMsg?.Invoke(chatMsg);
            }
            else
            {
                // We only handle chat in this case
                if (!chatMsg.TryFromJson<LobbyMessagePayload>(out var authRequest)
                    || authRequest.data == null)
                    OnLobbyChatMsg?.Invoke(chatMsg);
            }
        }

        private static void OnLobbyDataUpdateCallback(LobbyDataUpdate_t param)
        {
            LobbyData lobby = param.m_ulSteamIDLobby;
            if (param.m_ulSteamIDLobby == param.m_ulSteamIDMember)
            {
                //This is a metadata change for the lobby ... check for kick update
                if (lobby[LobbyData.DataKick].Contains("[" + User.Client.Id.ToString() + "]"))
                    OnAskedToLeaveLobby?.Invoke(lobby);
                
                OnLobbyDataUpdate?.Invoke(lobby, null);
            }
            else
            {
                OnLobbyDataUpdate?.Invoke(lobby, new LobbyMemberData { lobby = lobby, user = param.m_ulSteamIDMember});
            }
        }

        private static void OnLobbyEnterCallback(LobbyEnter_t param)
        {
            var responseCode = (EChatRoomEnterResponse)param.m_EChatRoomEnterResponse;
            
            if (responseCode == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
            {
                if (Matchmaking.Client.MemberOfLobbies.All(p => p != param.m_ulSteamIDLobby))
                    Matchmaking.Client.MemberOfLobbies.Add(new CSteamID(param.m_ulSteamIDLobby));
                
                OnLobbyEnterSuccess?.Invoke(param.m_ulSteamIDLobby);
            }
            else
            {
                if (Interface.IsDebugging || Application.isEditor)
                {
                    if (responseCode == EChatRoomEnterResponse.k_EChatRoomEnterResponseLimited)
                    {
                        Debug.LogWarning("This user is limited and cannot fully join a Steam Lobby! metadata and lobby chat will not work for this user though they may appear in the members list.");
                    }
                    else
                    {
                        Debug.LogWarning("Detected a Failed lobby enter attempt (" + (LobbyData)(param.m_ulSteamIDLobby) + ":" + responseCode + ")");
                    }
                }

                Matchmaking.Client.LeaveLobby(param.m_ulSteamIDLobby);
                OnLobbyEnterFailed?.Invoke(param.m_ulSteamIDLobby, responseCode);
            }
        }

        private static void OnMicroTxnAuthorizationResponseCallback(MicroTxnAuthorizationResponse_t param)
        {
            OnMicroTxnAuthorisationResponse?.Invoke(param.m_unAppID, param.m_ulOrderID, param.m_bAuthorized == 1);
        }

        private static void OnSteamInventoryResultReadyCallback(SteamInventoryResultReady_t param)
        {
            Inventory.Client.HandleInventoryResults(param);
        }

        private static void OnSteamInventoryDefinitionUpdateCallback(SteamInventoryDefinitionUpdate_t param)
        {
            OnInventoryDefinitionUpdate?.Invoke();
        }

        private static void OnPersonaStateChangeCallback(PersonaStateChange_t param)
        {
            // Let the friends API update cash images if any
            Friends.Client.HandlePersonaStateChange(param);
            OnPersonaStateChange?.Invoke(param.m_ulSteamID, param.m_nChangeFlags);
        }

        private static void OnFriendRichPresenceUpdateCallback(FriendRichPresenceUpdate_t param)
        {
            OnFriendRichPresenceUpdate?.Invoke(param.m_steamIDFriend, param.m_nAppID);
        }

        private static void OnGameConnectedFriendChatMsgCallback(GameConnectedFriendChatMsg_t param)
        {
            var count = SteamFriends.GetFriendMessage(param.m_steamIDUser, param.m_iMessageID, out var message, 8193, out var type);
            if (count > 0)
                OnGameConnectedFriendChatMsg?.Invoke(param.m_steamIDUser, message, type);
        }

        private static void OnGameConnectedChatJoinCallback(GameConnectedChatJoin_t param)
        {
            var room = Clans.Client.JoinedRooms.FirstOrDefault(p => p.id == param.m_steamIDClanChat);

            if (room.clan == default(ClanData))
            {
                room.id = param.m_steamIDClanChat;
                room.enterResponse = EChatRoomEnterResponse.k_EChatRoomEnterResponseError;

                if (App.IsDebugging)
                    Debug.LogWarning("Received a chat join event from chat room: " + room.id + ", no such room is known!");
            }

            OnGameConnectedChatJoin?.Invoke(room, param.m_steamIDUser);
        }

        private static void OnGameConnectedClanChatMsgCallback(GameConnectedClanChatMsg_t param)
        {
            var room = Clans.Client.JoinedRooms.FirstOrDefault(p => p.id == param.m_steamIDClanChat);

            if (room.clan == default(ClanData))
            {
                room.id = param.m_steamIDClanChat;
                room.enterResponse = EChatRoomEnterResponse.k_EChatRoomEnterResponseError;

                if (App.IsDebugging)
                    Debug.LogWarning("Received a message from chat room: " + room.id + ", no such room is known!");
            }

            var message = Clans.Client.GetChatMessage(param.m_steamIDClanChat, param.m_iMessageID, out var type,
                out var chatter);
            OnGameConnectedClanChatMsg?.Invoke(room, chatter, message, type);
        }

        private static void OnGameConnectedChatLeaveCallback(GameConnectedChatLeave_t param)
        {
            var room = Clans.Client.JoinedRooms.FirstOrDefault(p => p.id == param.m_steamIDClanChat);

            if (room.clan == default(ClanData))
            {
                room.id = param.m_steamIDClanChat;
                room.enterResponse = EChatRoomEnterResponse.k_EChatRoomEnterResponseError;

                if (Interface.IsDebugging) Debug.LogWarning("Received a chat leave event from chat room: " + room.id + ", no such room is known!");
            }
            
            OnGameConnectedChatLeave?.Invoke(room, param.m_steamIDUser, param.m_bKicked, param.m_bDropped);
        }

        private static void OnGamepadTextInputDismissedCallback(GamepadTextInputDismissed_t param)
        {
            if (param.m_bSubmitted)
            {
                if (SteamUtils.GetEnteredGamepadTextInput(out var textValue, param.m_unSubmittedText))
                {
                    OnGamepadTextInputDismissed?.Invoke(true, textValue);
                }
            }
            else
            {
                OnGamepadTextInputDismissed?.Invoke(false, string.Empty);
            }
        }

        private static void OnSteamServerConnectFailureCallback(SteamServerConnectFailure_t param)
        {
            OnSteamServerConnectFailure?.Invoke(param.m_eResult, param.m_bStillRetrying);
        }

        private static void OnSteamServersConnectedCallback(SteamServersConnected_t _)
        {
            OnSteamServersConnected?.Invoke();
        }

        internal static void InvokeOnSteamInitialised()
        {
            OnSteamInitialised?.Invoke();
        }

        internal static void InvokeOnSteamInitialisationError(string message)
        {
            OnSteamInitialisationError?.Invoke(message);
        }

        internal static void OnDlcInstalledCallback(DlcInstalled_t data)
        {
            OnDlcInstalled?.Invoke(data.m_nAppID);
        }

        internal static void OnNewUrlLaunchParametersCallback(NewUrlLaunchParameters_t _)
        {
            OnNewUrlLaunchParameters?.Invoke();
        }
        
        internal static void OnSteamServersDisconnectedCallback(SteamServersDisconnected_t param)
        {
            OnSteamServersDisconnected?.Invoke(param.m_eResult);
        }

        internal static void InvokeOnGamepadTextInputShown()
        {
            OnGamepadTextInputShown?.Invoke();
        }

        internal static void InvokeOnInputDataChanged(InputControllerStateData data)
        {
            OnInputDataChanged?.Invoke(data);
        }

        internal static void InvokeOnControllerConnected(InputHandle_t handle)
        {
            OnControllerConnected?.Invoke(handle);
        }

        internal static void InvokeOnControllerDisconnected(InputHandle_t handle)
        {
            OnControllerDisconnected?.Invoke(handle);
        }

        internal static void InvokeOnInventoryResultReady(InventoryResult result)
        {
            OnInventoryResultReady?.Invoke(result);
        }

        internal static void InvokeOnLobbyLeave(LobbyData lobby)
        {
            OnLobbyLeave?.Invoke(lobby);
        }

        internal static void InvokeOnKeyboardShown()
        {
            OnKeyboardShown?.Invoke();
        }
    }

    /// <summary>
    /// Represents the callback method used to send game server authentication data.
    /// </summary>
    /// <param name="userId">The unique identifier of the user being authenticated.</param>
    /// <param name="ticket">The authentication ticket for the user.</param>
    public delegate void SendGameServerAuthentication(ulong userId, byte[] ticket);

    /// <summary>
    /// Represents a callback delegate for handling the result of a session initiation process
    /// during user authentication with the Steamworks API.
    /// </summary>
    /// <param name="requestResult">
    /// Specifies the result of the authentication request, as defined by the <see cref="EBeginAuthSessionResult"/> enum.
    /// </param>
    /// <param name="session">
    /// Contains details about the authentication session, represented by an instance of the <see cref="AuthenticationSession"/> class.
    /// </param>
    public delegate void BeginSessionResult(EBeginAuthSessionResult requestResult, AuthenticationSession session);

    /// <summary>
    /// Delegate representing the method to handle events related to Steam server connection failures.
    /// </summary>
    /// <param name="result">The result code indicating the reason for the connection failure.</param>
    /// <param name="retrying">A flag indicating whether the connection attempt will be retried.</param>
    public delegate void SteamServerConnectFailureDelegate(EResult result, bool retrying);

    /// <summary>
    /// Represents a method that handles data related to downloadable content (DLC) in the Steamworks integration framework.
    /// </summary>
    /// <param name="data">An instance of <see cref="DlcData"/> containing information about the DLC event.</param>
    public delegate void DlcDataDelegate(DlcData data);

    /// <summary>
    /// Represents a delegate that processes or handles a string message.
    /// </summary>
    /// <param name="message">The string message to be processed or handled.</param>
    public delegate void StringDelegate(string message);

    /// <summary>
    /// Represents a delegate for handling operations that process an EResult outcome.
    /// </summary>
    public delegate void EResultDelegate(EResult result);

    /// <summary>
    /// Represents a delegate for handling Steam gamepad text input events.
    /// Provides information on whether the input was submitted and the associated text value.
    /// </summary>
    /// <param name="submitted">Indicates whether the user submitted the text input.</param>
    /// <param name="textValue">The text input value provided by the user.</param>
    public delegate void SteamTextInputDelegate(bool submitted, string textValue);

    /// <summary>
    /// Represents a delegate for handling events when a user leaves a Steam chat room.
    /// </summary>
    /// <param name="room">The chat room the user left.</param>
    /// <param name="user">The user who left the chat room.</param>
    /// <param name="wasKicked">Indicates whether the user was kicked from the chat room.</param>
    /// <param name="wasDropped">Indicates whether the user was dropped from the chat room.</param>
    public delegate void SteamUserLeaveDataDelegate(ChatRoom room, UserData user, bool wasKicked, bool wasDropped);

    /// <summary>
    /// Represents a delegate for handling Steam Clan Chat messages.
    /// </summary>
    /// <param name="room">The chat room associated with the message.</param>
    /// <param name="user">The user who sent the message.</param>
    /// <param name="message">The content of the message.</param>
    /// <param name="type">The type of chat entry, such as a message or notification.</param>
    public delegate void SteamClanChatMsgDelegate(ChatRoom room, UserData user, string message, EChatEntryType type);

    /// <summary>
    /// Represents a delegate type for handling events when a user joins a Steam clan chat room.
    /// </summary>
    /// <param name="room">The chat room the user is joining.</param>
    /// <param name="user">Information about the user joining the chat.</param>
    public delegate void SteamClanChatJoinDelegate(ChatRoom room, UserData user);

    /// <summary>
    /// Represents a delegate for handling friend chat messages in the Steam platform.
    /// </summary>
    /// <param name="user">The user who sent the message, encapsulated in a UserData structure.</param>
    /// <param name="message">The content of the chat message.</param>
    /// <param name="type">The type of the chat message, represented by the EChatEntryType enumeration.</param>
    public delegate void SteamFriendChatMsgDelegate(UserData user, string message, EChatEntryType type);

    /// <summary>
    /// Represents a delegate to handle updates to a friend's rich presence in Steam.
    /// </summary>
    /// <param name="user">The user whose rich presence has been updated.</param>
    /// <param name="app">The application associated with the rich presence update.</param>
    public delegate void SteamFriendRichPresenceUpdateDelegate(UserData user, AppData app);

    /// <summary>
    /// Represents a delegate for handling events where a user's persona state changes.
    /// </summary>
    /// <param name="user">The user whose persona state has changed.</param>
    /// <param name="changeFlag">The specific changes in the user's persona, represented by a combination of <see cref="EPersonaChange"/> flags.</param>
    public delegate void PersonaStateChangeEvent(UserData user, EPersonaChange changeFlag);

    /// <summary>
    /// Represents a delegate for handling Steam Input events with input handle parameters.
    /// </summary>
    /// <param name="handle">The input handle associated with the Steam Input event.</param>
    public delegate void SteamInputHandleDelegate(InputHandle_t handle);

    /// <summary>
    /// Represents a delegate invoked when input controller state data is updated or modified.
    /// </summary>
    /// <param name="data">The updated state data of the input controller.</param>
    public delegate void InputControllerStateDataDelegate(InputControllerStateData data);

    /// <summary>
    /// Represents a delegate invoked when a Steam inventory result is ready.
    /// </summary>
    /// <param name="result">The inventory result data associated with the event.</param>
    public delegate void SteamInventoryResultReadyDelegate(InventoryResult result);

    /// <summary>
    /// Represents a delegate for handling microtransaction authorisation responses
    /// from the Steam platform.
    /// </summary>
    /// <param name="app">The application data associated with the request.</param>
    /// <param name="orderId">The unique identifier for the order being authorised.</param>
    /// <param name="authorised">Indicates whether the transaction was authorised.</param>
    public delegate void SteamMtxTranAuthDelegate(AppData app, ulong orderId, bool authorised);

    /// <summary>
    /// Represents a delegate that handles the successful entry into a Steam lobby.
    /// </summary>
    /// <param name="lobby">
    /// The data associated with the Steam lobby that was successfully entered.
    /// </param>
    public delegate void SteamLobbyEnterSuccessDelegate(LobbyData lobby);

    /// <summary>
    /// Represents a delegate that handles the result of attempting to enter a Steam lobby.
    /// </summary>
    /// <param name="lobby">The data associated with the lobby being entered.</param>
    /// <param name="result">The result of the attempt to enter the lobby, represented as an <see cref="EChatRoomEnterResponse"/> value.</param>
    public delegate void SteamLobbyEnterFailedDelegate(LobbyData lobby, EChatRoomEnterResponse result);

    /// <summary>
    /// Represents a delegate for handling updates to Steam lobby data.
    /// </summary>
    /// <param name="lobby">The lobby whose data has been updated.</param>
    /// <param name="member">Optional member data associated with the lobby, if applicable.</param>
    public delegate void SteamLobbyDataUpdateDelegate(LobbyData lobby, LobbyMemberData? member);

    /// <summary>
    /// Represents a delegate for handling lobby chat messages in the Steamworks integration.
    /// </summary>
    /// <param name="lobbyChatMsg">The lobby chat message data received from the Steamworks API.</param>
    public delegate void SteamLobbyChatMsgDelegate(LobbyChatMsg lobbyChatMsg);

    /// <summary>
    /// Represents a delegate for handling lobby authentication events in the Steamworks integration.
    /// Provides the necessary data for authenticating a user within a specific lobby, including lobby details,
    /// user information, ticket data, and inventory data.
    /// </summary>
    /// <param name="lobby">The lobby data associated with the current authentication operation.</param>
    /// <param name="user">The user data associated with the current authentication operation.</param>
    /// <param name="ticketData">The authentication ticket data for validating the user in the lobby.</param>
    /// <param name="inventoryData">Data related to the user's inventory for additional validation or context.</param>
    public delegate void SteamLobbyAuthDelegate(LobbyData lobby, UserData user, byte[] ticketData,
        byte[] inventoryData);

    /// <summary>
    /// Represents a delegate for handling events related to Steam lobby data.
    /// </summary>
    /// <param name="lobby">An object containing data about the Steam lobby.</param>
    public delegate void SteamLobbyDataDelegate(LobbyData lobby);

    /// <summary>
    /// Represents a delegate for handling updates to a Steam lobby's chat state.
    /// </summary>
    /// <param name="lobby">An object representing the lobby where the chat update occurred.</param>
    /// <param name="user">An object representing the user associated with the lobby chat update.</param>
    /// <param name="changes">An indicator of the type of state change that occurred for the chat member.</param>
    public delegate void SteamLobbyChatUpdateDelegate(LobbyData lobby, UserData user, EChatMemberStateChange changes);

    /// <summary>
    /// Represents the delegate used to handle scenarios involving a Steam Lobby connected to a game server.
    /// </summary>
    /// <param name="lobby">The data associated with the Steam Lobby.</param>
    /// <param name="server">The unique identifier of the connected game server.</param>
    /// <param name="ip">The IP address of the connected game server.</param>
    /// <param name="port">The port number of the connected game server.</param>
    public delegate void SteamLobbyGameServerDelegate(LobbyData lobby, CSteamID server, string ip, ushort port);

    /// <summary>
    /// Represents a delegate used to handle Steam Lobby Invite events.
    /// </summary>
    /// <param name="fromUser">The user who has sent the lobby invite.</param>
    /// <param name="forLobby">The lobby associated with the invitation.</param>
    /// <param name="inGame">The game context in which the invitation is sent.</param>
    public delegate void SteamLobbyInviteDelegate(UserData fromUser, LobbyData forLobby, GameData inGame);

    /// <summary>
    /// Represents a delegate that handles events related to changes in the Steam favorites list.
    /// </summary>
    /// <param name="data">An object of type <see cref="FavoritesListChanged"/> containing information about the favorites list change.</param>
    public delegate void SteamFavoritesListChangeDelegate(FavoritesListChanged data);

    /// <summary>
    /// Represents a delegate that handles requests to join a Steam lobby.
    /// </summary>
    /// <param name="lobby">The lobby data associated with the join request.</param>
    /// <param name="user">The user data of the player requesting to join the lobby.</param>
    public delegate void SteamLobbyJoinRequestDelegate(LobbyData lobby, UserData user);

    /// <summary>
    /// Represents a method that handles an input boolean value as its parameter.
    /// </summary>
    /// <param name="value">A boolean value that provides the input to the delegate.</param>
    public delegate void BoolDelegate(bool value);

    /// <summary>
    /// Represents a delegate for handling requests to change the game server,
    /// including the server address and an optional password.
    /// </summary>
    /// <param name="server">The address of the server to switch to.</param>
    /// <param name="password">The password required to access the specified server, if applicable.</param>
    public delegate void SteamGameServerChangeRequestedDelegate(string server, string password);

    /// <summary>
    /// Represents a delegate used to handle events when a Steam rich presence join request is received.
    /// </summary>
    /// <param name="user">The user who sent the join request, represented as a <see cref="Heathen.SteamworksIntegration.UserData"/> object.</param>
    /// <param name="connectionString">The connection string provided by the sender for joining their session.</param>
    public delegate void SteamRichPresenceJoinRequestedDelegate(UserData user, string connectionString);

    /// <summary>
    /// Represents the method signature for handling notifications related to Steam reservation updates.
    /// </summary>
    /// <param name="user">The user data associated with the reservation notification.</param>
    /// <param name="party">The party beacon ID associated with the reservation notification.</param>
    public delegate void SteamReservationNotificationDelegate(UserData user, PartyBeaconID_t party);

    /// <summary>
    /// Represents a delegate for handling events related to a Steam Remote Play session identified by a unique session ID.
    /// </summary>
    /// <param name="sessionId">The unique identifier for the specific Remote Play session.</param>
    public delegate void SteamRemotePlaySessionIdDelegate(RemotePlaySessionID_t sessionId);

    /// <summary>
    /// Represents a delegate invoked when a screenshot operation is completed in the Steam client.
    /// </summary>
    /// <param name="screenshotHandle">The handle identifying the completed screenshot operation.</param>
    /// <param name="result">The result of the screenshot operation, indicating success or failure.</param>
    public delegate void SteamScreenshotReadyDelegate(ScreenshotHandle screenshotHandle, EResult result);

    /// <summary>
    /// Represents a delegate used to handle the event of Steam stats being received for a specified user and game.
    /// </summary>
    /// <param name="game">The game related to the received stats, represented as a <see cref="GameData"/> object.</param>
    /// <param name="result">The result of the stats request, represented as an <see cref="EResult"/> enum value.</param>
    /// <param name="user">The user associated with the received stats, represented as a <see cref="UserData"/> object.</param>
    public delegate void SteamStatsReceivedDelegate(GameData game, EResult result, UserData user);

    /// <summary>
    /// Represents a delegate type used to handle callbacks or events related to storage or updates
    /// of Steam statistics for a specific game. Invoked with the associated game data and the result
    /// of the operation.
    /// </summary>
    /// <param name="game">The game data associated with the operation.</param>
    /// <param name="result">The result of the operation represented as an <see cref="EResult"/> value.</param>
    public delegate void SteamStatsStoredDelegate(GameData game, EResult result);

    /// <summary>
    /// Represents a delegate for handling events related to the unloading of Steam user statistics.
    /// </summary>
    /// <param name="user">The user data associated with the unloaded statistics.</param>
    public delegate void SteamUserStatsUnloadedDelegate(UserData user);

    /// <summary>
    /// Represents a delegate for handling events related to achieving and storing user achievements on Steam.
    /// </summary>
    /// <param name="data">The data associated with the user's achievement storage, encapsulated in a <see cref="UserAchievementStoredData"/> struct.</param>
    public delegate void SteamUserAchievementStoredDelegate(UserAchievementStoredData data);
}
#endif