#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// Provides access to Steamworks Party and Beacon functionalities for managing and interacting with game parties.
    /// </summary>
    public static class Parties
    {
        /// <summary>
        /// Provides functionalities for managing Steam Party beacons, including creating, joining, and modifying beacons, retrieving details about beacons, handling reservations, and accessing beacon location data.
        /// </summary>
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                _createBeaconCallbackT = null;
                _changeNumOpenSlotsCallbackT = null;
                _joinPartyCallbackT = null;
                ReservationList = null;
                _createdBeacons = null;
            }

            /// <summary>
            /// An array of beacons created by this user in this session.
            /// </summary>
            public static PartyBeaconID_t[] MyBeacons => _createdBeacons?.ToArray();

            /// <summary>
            /// The array of reservations you have been notified of but have not yet completed.
            /// </summary>
            public static ReservationNotificationCallback_t[] Reservations => ReservationList?.ToArray();

            private static CallResult<CreateBeaconCallback_t> _createBeaconCallbackT;
            private static CallResult<ChangeNumOpenSlotsCallback_t> _changeNumOpenSlotsCallbackT;
            private static CallResult<JoinPartyCallback_t> _joinPartyCallbackT;

            internal static List<ReservationNotificationCallback_t> ReservationList;
            private static List<PartyBeaconID_t> _createdBeacons;

            /// <summary>
            /// Retrieves a list of available beacon locations that can be used for Steam Party beacons.
            /// </summary>
            /// <returns>
            /// An array of <see cref="SteamPartyBeaconLocation_t"/> representing the available beacon locations.
            /// </returns>
            public static SteamPartyBeaconLocation_t[] GetAvailableBeaconLocations()
            {
                SteamParties.GetNumAvailableBeaconLocations(out uint locations);
                var output = new SteamPartyBeaconLocation_t[locations];
                SteamParties.GetAvailableBeaconLocations(output, locations);
                return output;
            }

            /// <summary>
            /// Creates a party beacon that advertises an available space for players to join.
            /// </summary>
            /// <param name="openSlots">The number of open slots available for the party.</param>
            /// <param name="location">The target location information for the party beacon.</param>
            /// <param name="connectionString">A connection string that other players can use to join the party.</param>
            /// <param name="metadata">Additional metadata associated with the party beacon.</param>
            /// <param name="callback">A callback function invoked with the result of the beacon creation and its status.</param>
            public static void CreateBeacon(uint openSlots, ref SteamPartyBeaconLocation_t location,
                string connectionString, string metadata, Action<CreateBeaconCallback_t, bool> callback)
            {
                if (callback == null)
                    return;

                _createBeaconCallbackT ??= CallResult<CreateBeaconCallback_t>.Create();

                var handle = SteamParties.CreateBeacon(openSlots, ref location, connectionString, metadata);
                _createBeaconCallbackT.Set(handle, (r,e) =>
                {
                    if (!e && r.m_eResult == EResult.k_EResultOK)
                    {
                        _createdBeacons ??= new List<PartyBeaconID_t>();

                        _createdBeacons.Add(r.m_ulBeaconID);
                    }

                    callback.Invoke(r, e);
                });
            }

            /// <summary>
            /// Handles the completion of a reservation for a party beacon.
            /// </summary>
            /// <param name="beacon">The unique identifier of the party beacon.</param>
            /// <param name="user">The Steam ID of the user who completed the reservation.</param>
            public static void OnReservationCompleted(PartyBeaconID_t beacon, CSteamID user)
            {
                SteamParties.OnReservationCompleted(beacon, user);

                if (ReservationList != null)
                    ReservationList.RemoveAll((p) => p.m_ulBeaconID == beacon && p.m_steamIDJoiner == user);
            }
            public static bool OnReservationCompleted(UserData user)
            {
                if (ReservationList.Any(p => p.m_steamIDJoiner == user))
                {
                    var beacon = ReservationList.FirstOrDefault(p => p.m_steamIDJoiner == user);
                    OnReservationCompleted(beacon.m_ulBeaconID, user);

                    return true;
                }
                else
                    return false;
            }

            /// <summary>
            /// Updates the number of open slots in an existing party beacon.
            /// </summary>
            /// <param name="beacon">
            /// The unique identifier of the party beacon to update.
            /// </param>
            /// <param name="openSlots">
            /// The number of open slots to set for the specified party beacon.
            /// </param>
            /// <param name="callback">
            /// The callback to be invoked when the operation is complete. The callback provides the result of the operation and a success flag.
            /// </param>
            public static void ChangeNumOpenSlots(PartyBeaconID_t beacon, uint openSlots,
                Action<ChangeNumOpenSlotsCallback_t, bool> callback)
            {
                if (callback == null)
                    return;

                _changeNumOpenSlotsCallbackT ??= CallResult<ChangeNumOpenSlotsCallback_t>.Create();

                var handle = SteamParties.ChangeNumOpenSlots(beacon, openSlots);
                _changeNumOpenSlotsCallbackT.Set(handle, callback.Invoke);
            }

            /// <summary>
            /// Destroys the specified Steam Party beacon, removing it from the list of active beacons.
            /// </summary>
            /// <param name="beacon">
            /// The <see cref="PartyBeaconID_t"/> identifying the beacon to be destroyed.
            /// </param>
            /// <returns>
            /// A boolean value indicating whether the beacon was successfully destroyed.
            /// </returns>
            public static bool DestroyBeacon(PartyBeaconID_t beacon)
            {
                if (_createdBeacons != null)
                    _createdBeacons.RemoveAll((p) => p == beacon);

                return SteamParties.DestroyBeacon(beacon);
            }

            /// <summary>
            /// Retrieves an array of active party beacons created by the current user.
            /// </summary>
            /// <returns>
            /// An array of <see cref="PartyBeaconID_t"/> representing active beacons created by the current user.
            /// </returns>
            public static PartyBeaconID_t[] GetBeacons()
            {
                var count = SteamParties.GetNumActiveBeacons();
                var results = new PartyBeaconID_t[count];
                for (uint i = 0; i < count; i++)
                {
                    results[i] = SteamParties.GetBeaconByIndex(i);
                }

                return results;
            }

            /// <summary>
            /// Retrieves the details of a specific party beacon, including its owner, location, and metadata.
            /// </summary>
            /// <param name="beacon">
            /// The ID of the party beacon to retrieve details for.
            /// </param>
            /// <returns>
            /// A nullable <see cref="PartyBeaconDetails"/> object containing the details of the specified party beacon,
            /// or null if the details could not be retrieved.
            /// </returns>
            public static PartyBeaconDetails? GetBeaconDetails(PartyBeaconID_t beacon)
            {
                if (SteamParties.GetBeaconDetails(beacon, out CSteamID owner, out SteamPartyBeaconLocation_t location, out string metadata, 8193))
                {
                    return new PartyBeaconDetails
                    {
                        id = beacon,
                        owner = owner,
                        Location = location,
                        metadata = metadata
                    };
                }
                else
                    return null;
            }

            /// <summary>
            /// Sends a request to join a party specified by the given beacon identifier.
            /// </summary>
            /// <param name="beacon">
            /// The <see cref="PartyBeaconID_t"/> representing the identifier of the party to join.
            /// </param>
            /// <param name="callback">
            /// A callback method invoked upon completion of the join party request. Provides the result of the operation as a <see cref="JoinPartyCallback_t"/> and a boolean indicating success.
            /// </param>
            public static void JoinParty(PartyBeaconID_t beacon, Action<JoinPartyCallback_t, bool> callback)
            {
                if (callback == null)
                    return;

                _joinPartyCallbackT ??= CallResult<JoinPartyCallback_t>.Create();

                var handle = SteamParties.JoinParty(beacon);
                _joinPartyCallbackT.Set(handle, callback.Invoke);
            }

            /// <summary>
            /// Retrieves specific data related to a Steam Party Beacon location.
            /// </summary>
            /// <param name="location">
            /// The beacon location for which the data is being retrieved.
            /// </param>
            /// <param name="data">
            /// The type of data to retrieve, specified as an <see cref="ESteamPartyBeaconLocationData"/> value.
            /// </param>
            /// <param name="result">
            /// Outputs the requested data as a string.
            /// </param>
            /// <returns>
            /// A boolean value indicating whether the operation succeeded. Returns true if the data was successfully retrieved, otherwise false.
            /// </returns>
            public static bool GetBeaconLocationData(SteamPartyBeaconLocation_t location,
                ESteamPartyBeaconLocationData data, out string result) =>
                SteamParties.GetBeaconLocationData(location, data, out result, 8193);
        }
    }
}
#endif