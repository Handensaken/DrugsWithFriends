#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// Steam Input API is a flexible action-based API that supports all major controller types - Xbox, PlayStation, Nintendo Switch Pro, and Steam Controllers.
    /// </summary>
    /// <remarks>
    /// https://partner.steamgames.com/doc/api/isteaminput
    /// </remarks>
    public static class Input
    {
        /// <summary>
        /// Provides methods and events for interacting with Steam Input devices and managing controller states.
        /// </summary>
        /// <remarks>
        /// This class simplifies handling controller connections, input states, action sets, and analogue actions
        /// within the Steam Input framework, allowing for seamless integration of Steamworks controller support.
        /// </remarks>
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void RuntimeInit()
            {
                _mInputActionSetHandles = new Dictionary<string, InputActionSetHandle_t>();
                _mInputAnalogActionHandles = new Dictionary<string, InputAnalogActionHandle_t>();
                _mInputDigitalActionHandles = new Dictionary<string, InputDigitalActionHandle_t>();

                foreach (var pair in _glyphs)
                {
                    if (pair.Value != null)
                        GameObject.Destroy(pair.Value);
                }

                _glyphs = new Dictionary<EInputActionOrigin, Texture2D>();
                _actions = new List<(string name, InputActionType type)>();
                _controllers = new Dictionary<InputHandle_t, InputControllerStateData>();
                _controllerUpdates = new Dictionary<InputHandle_t, int>();
                
                _controllerHandleBuffer = new InputHandle_t[Constants.STEAM_INPUT_MAX_COUNT];
                _currentControllers = new HashSet<InputHandle_t>();
                _currentArrayBuffer = new InputHandle_t[Constants.STEAM_INPUT_MAX_COUNT];
                _addedBuffer = new InputHandle_t[Constants.STEAM_INPUT_MAX_COUNT];
                _removedBuffer = new InputHandle_t[Constants.STEAM_INPUT_MAX_COUNT];
                ConnectedControllers = new(Constants.STEAM_INPUT_MAX_COUNT);
                _mInitialized = false;
            }

            private static InputHandle_t[] _currentArrayBuffer = new InputHandle_t[Constants.STEAM_INPUT_MAX_COUNT];
            private static InputHandle_t[] _addedBuffer = new InputHandle_t[Constants.STEAM_INPUT_MAX_COUNT];
            private static InputHandle_t[] _removedBuffer = new InputHandle_t[Constants.STEAM_INPUT_MAX_COUNT];
            private static InputHandle_t[] _controllerHandleBuffer = new InputHandle_t[Constants.STEAM_INPUT_MAX_COUNT];
            private static HashSet<InputHandle_t> _currentControllers = new HashSet<InputHandle_t>();

            /// <summary>
            /// Indicates whether the Steam Input system is successfully initialized.
            /// This property reflects the current state of the initialization process for
            /// managing controller inputs through the Steamworks SDK.
            /// </summary>
            public static bool Initialized => _mInitialized;
            /// <summary>
            /// This will allocate new memory for each controller on each frame but provides an easy to use map of input changes
            /// If you're sensitive about allocations use actions directly for each control as needed
            /// If you're sensitive to code maintenance, then use this
            /// </summary>
            public static bool IsAutoRefreshControllerState = false;

            private static bool _mInitialized = false;
            private static Dictionary<string, InputActionSetHandle_t> _mInputActionSetHandles = new();
            private static Dictionary<string, InputAnalogActionHandle_t> _mInputAnalogActionHandles = new();
            private static Dictionary<string, InputDigitalActionHandle_t> _mInputDigitalActionHandles = new();
            private static Dictionary<EInputActionOrigin, Texture2D> _glyphs = new();
            private static List<(string name, InputActionType type)> _actions = new();
            private static Dictionary<InputHandle_t, InputControllerStateData> _controllers = new();
            private static Dictionary<InputHandle_t, int> _controllerUpdates = new();

            /// <summary>
            /// Poles for and returns the handles for all connected controllers
            /// </summary>
            public static List<InputHandle_t> ConnectedControllers = new(Constants.STEAM_INPUT_MAX_COUNT);

            /// <summary>
            /// Record an input to be tracked
            /// </summary>
            /// <param name="name"></param>
            /// <param name="type"></param>
            public static void AddInput(string name, InputActionType type) => _actions.Add((name, type));
            /// <summary>
            /// Remove an input from tracking
            /// </summary>
            /// <param name="name"></param>
            public static void RemoveInput(string name) => _actions.RemoveAll(p => p.name == name);
            /// <summary>
            /// Gets the data for the action from the first controller in the collection
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public static InputActionStateData GetActionData(string name)
            {
                if(_controllers.Count > 0)
                    return _controllers.First().Value.GetActionData(name);
                else
                {
                    if (ConnectedControllers.Count > 0)
                    {
                        var controllerData = Update(ConnectedControllers[0]);
                        return controllerData.GetActionData(name);
                    }
                    else
                        return default;
                }
            }

            /// <summary>
            /// Retrieves the action data for a specific controller and action name.
            /// </summary>
            /// <param name="controller">The input handle representing the specific controller.</param>
            /// <param name="name">The name of the action for which data is being requested.</param>
            /// <returns>An <see cref="InputActionStateData"/> structure containing the action data for the specified controller and action name. Returns the default value if the controller or action name is not found.</returns>
            public static InputActionStateData GetActionData(InputHandle_t controller, string name)
            {
                if (_controllers.ContainsKey(controller))
                    return _controllers[controller].GetActionData(name);
                else
                    return default;
            }

            /// <summary>
            /// This will allocate new memory for each call but provides an easy-to-use map of input changes
            /// If you're sensitive about allocations, use actions directly for each control as needed
            /// If you're sensitive to code maintenance, then use this
            /// </summary>
            /// <param name="controller"></param>
            /// <returns></returns>
            public static InputControllerStateData Update(InputHandle_t controller)
            {
                if (!_controllerUpdates.ContainsKey(controller))
                    _controllerUpdates.Add(controller, -1);

                if (_controllerUpdates[controller] != Time.frameCount)
                {
                    _controllerUpdates[controller] = Time.frameCount;

                    InputControllerStateData conData = new InputControllerStateData
                    {
                        handle = controller,
                        inputs = new InputActionStateData[_actions.Count],
                    };

                    if (!_controllers.ContainsKey(controller))
                        _controllers.Add(controller, new()
                        {
                            handle = controller,
                            inputs = new InputActionStateData[0]
                        });

                    var currentController = _controllers[controller];
                    var updates = new List<InputActionUpdate>();

                    for (int i = 0; i < _actions.Count; i++)
                    {
                        var (name, type) = _actions[i];
                        if (type == InputActionType.Analog)
                        {
                            var handle = GetAnalogActionHandle(name);

                            if (handle.m_InputAnalogActionHandle != 0)
                            {
                                var currentInput = currentController.inputs.FirstOrDefault(p => p.name == name && p.type == type);
                                var rawData = GetAnalogActionData(controller, handle);

                                var update = new InputActionUpdate
                                {
                                    name = name,
                                    controller = controller,
                                    mode = rawData.eMode,
                                    type = type,
                                    wasActive = currentInput.active,
                                    wasState = currentInput.state,
                                    wasX = currentInput.x,
                                    wasY = currentInput.y,
                                    isActive = rawData.bActive != 0,
                                    isState = rawData.x != 0 || rawData.y != 0,
                                    isX = rawData.x,
                                    isY = rawData.y,
                                };

                                var change = currentInput.x != rawData.x
                                    || currentInput.y != rawData.y
                                    || currentInput.active != (rawData.bActive == 0 ? true : false)
                                    || currentInput.state != (rawData.x != 0 || rawData.y != 0);

                                conData.inputs[i] = update.Data;
                                if (change)
                                    updates.Add(update);
                            }
                        }
                        else
                        {
                            var handle = GetDigitalActionHandle(name);

                            if (handle.m_InputDigitalActionHandle != 0)
                            {
                                var rawData = GetDigitalActionData(controller, handle);
                                var currentInput = currentController.inputs.FirstOrDefault(p => p.name == name && p.type == type);

                                var update = new InputActionUpdate
                                {
                                    name = name,
                                    controller = controller,
                                    mode = EInputSourceMode.k_EInputSourceMode_None,
                                    type = currentInput.type,
                                    wasActive = currentInput.active,
                                    wasState = currentInput.state,
                                    wasX = currentInput.x,
                                    wasY = currentInput.y,
                                    isActive = rawData.bActive != 0,
                                    isState = rawData.bState != 0,
                                    isX = rawData.bState,
                                    isY = rawData.bState,
                                };

                                var change = rawData.bState != 0 != currentInput.state;

                                conData.inputs[i] = update.Data;
                                if (change)
                                    updates.Add(update);
                            }
                        }
                    }

                    conData.changes = updates.ToArray();

                    _controllers[controller] = conData;

                    if (conData.changes.Length > 0)
                        SteamTools.Events.InvokeOnInputDataChanged(conData);

                    return conData;
                }
                else
                    return _controllers[controller];
            }
                        
            /// <summary>
            /// Reconfigure the controller to use the specified action set (ie "Menu", "Walk", or "Drive").
            /// </summary>
            /// <param name="controllerHandle">The handle of the controller you want to activate an action set for.</param>
            /// <param name="actionSetHandle">The handle of the action set you want to activate.</param>
            public static void ActivateActionSet(InputHandle_t controllerHandle, InputActionSetHandle_t actionSetHandle) => SteamInput.ActivateActionSet(controllerHandle, actionSetHandle);

            /// <summary>
            /// Activates the specified action set for a controller. If no specific controller is provided, it applies to the first available connected controller.
            /// </summary>
            /// <param name="actionSetHandle">The handle of the action set to be activated.</param>
            public static void ActivateActionSet(InputActionSetHandle_t actionSetHandle)
            {
                if (_controllers.Count > 0)
                    ActivateActionSet(_controllers.First().Key, actionSetHandle);
                else
                {
                    if (ConnectedControllers.Count > 0)
                    {
                        var controllerData = Update(ConnectedControllers[0]);
                        ActivateActionSet(controllerData.handle, actionSetHandle);
                    }
                }
            }
            /// <summary>
            /// Reconfigure the controller to use the specified action set (ie "Menu", "Walk", or "Drive").
            /// </summary>
            /// <param name="controllerHandle">The handle of the controller you want to activate an action set for.</param>
            /// <param name="actionSet">The name of the set to use ... we will read this from cache if available or fetch it if required</param>
            public static void ActivateActionSet(InputHandle_t controllerHandle, string actionSet)
            {
                if (_mInputActionSetHandles.ContainsKey(actionSet))
                    SteamInput.ActivateActionSet(controllerHandle, _mInputActionSetHandles[actionSet]);
                else
                {
                    var handle = GetActionSetHandle(actionSet);
                    SteamInput.ActivateActionSet(controllerHandle, _mInputActionSetHandles[actionSet]);
                }
            }
            /// <summary>
            /// Reconfigure the controller to use the specified action set layer.
            /// </summary>
            /// <param name="controllerHandle">The handle of the controller you want to activate an action set layer for.</param>
            /// <param name="actionSetHandle">The handle of the action set layer you want to activate.</param>
            public static void ActivateActionSetLayer(InputHandle_t controllerHandle, InputActionSetHandle_t actionSetHandle) => SteamInput.ActivateActionSetLayer(controllerHandle, actionSetHandle);

            /// <summary>
            /// Activates a specific action set layer for the provided controller.
            /// </summary>
            /// <param name="controllerHandle">The handle of the controller for which the action set layer should be activated.</param>
            /// <param name="actionSetHandle">The handle of the action set layer to activate.</param>
            public static void ActivateActionSetLayer(InputActionSetHandle_t actionSetHandle)
            {
                if (_controllers.Count > 0)
                    ActivateActionSetLayer(_controllers.First().Key, actionSetHandle);
                else
                {
                    if (ConnectedControllers.Count > 0)
                    {
                        var controllerData = Update(ConnectedControllers[0]);
                        ActivateActionSetLayer(controllerData.handle, actionSetHandle);
                    }
                }
            }
            /// <summary>
            /// Reconfigure the controller to use the specified action set (ie "Menu", "Walk", or "Drive").
            /// </summary>
            /// <param name="controllerHandle">The handle of the controller you want to activate an action set for.</param>
            /// <param name="actionSet">The name of the set to use ... we will read this from cache if available or fetch it if required</param>
            public static void ActivateActionSetLayer(InputHandle_t controllerHandle, string actionSet)
            {
                if (_mInputActionSetHandles.ContainsKey(actionSet))
                    SteamInput.ActivateActionSetLayer(controllerHandle, _mInputActionSetHandles[actionSet]);
                else
                {
                    var handle = GetActionSetHandle(actionSet);
                    SteamInput.ActivateActionSetLayer(controllerHandle, _mInputActionSetHandles[actionSet]);
                }
            }
            /// <summary>
            /// Reconfigure the controller to stop using the specified action set layer.
            /// </summary>
            /// <param name="controllerHandle">The handle of the controller you want to deactivate an action set layer for.</param>
            /// <param name="actionSetHandle">The handle of the action set layer you want to deactivate.</param>
            public static void DeactivateActionSetLayer(InputHandle_t controllerHandle, InputActionSetHandle_t actionSetHandle) => SteamInput.DeactivateActionSetLayer(controllerHandle, actionSetHandle);
            /// <summary>
            /// Reconfigure the controller to stop using the specified action set layer.
            /// </summary>
            /// <param name="controllerHandle">The handle of the controller you want to deactivate an action set layer for.</param>
            /// <param name="actionSet">The action set layer you want to deactivate.</param>
            public static void DeactivateActionSetLayer(InputHandle_t controllerHandle, string actionSet)
            {
                if (_mInputActionSetHandles.ContainsKey(actionSet))
                    SteamInput.DeactivateActionSetLayer(controllerHandle, _mInputActionSetHandles[actionSet]);
                else
                {
                    var handle = GetActionSetHandle(actionSet);
                    SteamInput.DeactivateActionSetLayer(controllerHandle, _mInputActionSetHandles[actionSet]);
                }
            }
            /// <summary>
            /// Reconfigure the controller to stop using all action set layers.
            /// </summary>
            /// <param name="controllerHandle">The handle of the controller you want to deactivate all action set layers for.</param>
            public static void DeactivateAllActionSetLayers(InputHandle_t controllerHandle) => SteamInput.DeactivateAllActionSetLayers(controllerHandle);
            /// <summary>
            /// Get the currently active action set layers for a specified controller handle.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <returns></returns>
            public static InputActionSetHandle_t[] GetActiveActionSetLayers(InputHandle_t controllerHandle)
            {
                var actionSetHandles = new InputActionSetHandle_t[Constants.STEAM_INPUT_MAX_COUNT];
                var size = SteamInput.GetActiveActionSetLayers(controllerHandle, actionSetHandles);
                Array.Resize(ref actionSetHandles, size);
                return actionSetHandles;
            }
            /// <summary>
            /// Lookup the handle for an Action Set.
            /// </summary>
            /// <param name="setName">The name of the set to fetch</param>
            /// <returns></returns>
            public static InputActionSetHandle_t GetActionSetHandle(string setName)
            {
                var result = SteamInput.GetActionSetHandle(setName);
                if (_mInputActionSetHandles.ContainsKey(setName))
                    _mInputActionSetHandles[setName] = result;
                else
                    _mInputActionSetHandles.Add(setName, result);

                return result;
            }
            /// <summary>
            /// Returns the current state of the supplied analogue game action.
            /// </summary>
            /// <param name="controllerHandle">The handle of the controller you want to query.</param>
            /// <param name="analogActionHandle">The handle of the analogue action you want to query.</param>
            /// <returns></returns>
            public static InputAnalogActionData_t GetAnalogActionData(InputHandle_t controllerHandle, InputAnalogActionHandle_t analogActionHandle) => SteamInput.GetAnalogActionData(controllerHandle, analogActionHandle);
            /// <summary>
            /// Returns the current state of the supplied analogue game action.
            /// </summary>
            /// <param name="controllerHandle">The handle of the controller you want to query.</param>
            /// <param name="actionName">The analog action you want to query.</param>
            /// <returns></returns>
            public static InputAnalogActionData_t GetAnalogActionData(InputHandle_t controllerHandle, string actionName)
            {
                if (_mInputAnalogActionHandles.ContainsKey(actionName))
                    return SteamInput.GetAnalogActionData(controllerHandle, _mInputAnalogActionHandles[actionName]);
                else
                {
                    var handle = GetAnalogActionHandle(actionName);
                    return SteamInput.GetAnalogActionData(controllerHandle, handle);
                }
            }
            /// <summary>
            /// Get the handle of the specified Analogue action.
            /// </summary>
            /// <remarks>
            /// This function does not take an action set handle parameter. That means that each action in your VDF file must have a unique string identifier. In other words, if you use an action called "up" in two different action sets, this function will only ever return one of them and the other will be ignored.
            /// </remarks>
            /// <param name="actionName">The string identifier of the analogue action defined in the game's VDF file.</param>
            /// <returns></returns>
            public static InputAnalogActionHandle_t GetAnalogActionHandle(string actionName)
            {
                var result = SteamInput.GetAnalogActionHandle(actionName);
                if (_mInputAnalogActionHandles.ContainsKey(actionName))
                    _mInputAnalogActionHandles[actionName] = result;
                else
                    _mInputAnalogActionHandles.Add(actionName, result);

                return result;
            }
            /// <summary>
            /// Get the origin(s) for an analogue action within an action set by filling originsOut with EInputActionOrigin handles. Use this to display the appropriate on-screen prompt for the action.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="actionSetHandle"></param>
            /// <param name="analogActionHandle"></param>
            /// <returns></returns>
            public static EInputActionOrigin[] GetAnalogActionOrigins(InputHandle_t controllerHandle, InputActionSetHandle_t actionSetHandle, InputAnalogActionHandle_t analogActionHandle)
            {
                var origins = new EInputActionOrigin[Constants.STEAM_INPUT_MAX_ORIGINS];

                SteamInput.GetAnalogActionOrigins(controllerHandle, actionSetHandle, analogActionHandle, origins);

                return origins;
            }
            /// <summary>
            /// Get the origin(s) for an analog action within an action set by filling originsOut with EInputActionOrigin handles. Use this to display the appropriate on-screen prompt for the action.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="actionSet"></param>
            /// <param name="analogName"></param>
            /// <returns></returns>
            public static EInputActionOrigin[] GetAnalogActionOrigins(InputHandle_t controllerHandle, string actionSet, string analogName)
            {
                var origins = new EInputActionOrigin[Constants.STEAM_INPUT_MAX_ORIGINS];

                if (!_mInputAnalogActionHandles.ContainsKey(analogName))
                    GetAnalogActionHandle(analogName);

                if (!_mInputActionSetHandles.ContainsKey(actionSet))
                    GetActionSetHandle(actionSet);

                SteamInput.GetAnalogActionOrigins(controllerHandle, _mInputActionSetHandles[actionSet], _mInputAnalogActionHandles[analogName], origins);

                return origins;
            }
            /// <summary>
            /// Returns the associated controller handle for the specified emulated gamepad. Can be used with GetInputTypeForHandle to determine the controller type of a controller using Steam Input Gamepad Emulation.
            /// </summary>
            /// <param name="index">The index of the emulated gamepad you want to get a controller handle for.</param>
            /// <returns></returns>
            public static InputHandle_t GetControllerForGamepadIndex(int index) => SteamInput.GetControllerForGamepadIndex(index);
            /// <summary>
            /// Get the currently active action set for the specified controller.
            /// </summary>
            /// <param name="controllerHandle">	The handle of the controller you want to query.</param>
            /// <returns></returns>
            public static InputActionSetHandle_t GetCurrentActionSet(InputHandle_t controllerHandle) => SteamInput.GetCurrentActionSet(controllerHandle);
            /// <summary>
            /// Returns the current state of the supplied digital game action.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="actionHandle"></param>
            /// <returns></returns>
            public static InputDigitalActionData_t GetDigitalActionData(InputHandle_t controllerHandle, InputDigitalActionHandle_t actionHandle) => SteamInput.GetDigitalActionData(controllerHandle, actionHandle);
            /// <summary>
            /// Returns the current state of the supplied digital game action.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="actionName"></param>
            /// <returns></returns>
            public static InputDigitalActionData_t GetDigitalActionData(InputHandle_t controllerHandle, string actionName)
            {
                if (!_mInputDigitalActionHandles.ContainsKey(actionName))
                {
                    var actionHandle = GetDigitalActionHandle(actionName);
                    return SteamInput.GetDigitalActionData(controllerHandle, actionHandle);
                }
                else
                {
                    return SteamInput.GetDigitalActionData(controllerHandle, _mInputDigitalActionHandles[actionName]);
                }
            }
            /// <summary>
            /// Get the handle of the specified digital action.
            /// </summary>
            /// <remarks>
            /// NOTE: This function does not take an action set handle parameter. That means that each action in your VDF file must have a unique string identifier. In other words, if you use an action called "up" in two different action sets, this function will only ever return one of them and the other will be ignored.
            /// </remarks>
            /// <param name="actionName"></param>
            /// <returns></returns>
            public static InputDigitalActionHandle_t GetDigitalActionHandle(string actionName)
            {
                var result = SteamInput.GetDigitalActionHandle(actionName);
                if (_mInputDigitalActionHandles.ContainsKey(actionName))
                    _mInputDigitalActionHandles[actionName] = result;
                else
                    _mInputDigitalActionHandles.Add(actionName, result);

                return result;
            }
            /// <summary>
            /// Get the origin(s) for an digital action within an action set by filling originsOut with EInputActionOrigin handles. Use this to display the appropriate on-screen prompt for the action.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="actionSetHandle"></param>
            /// <param name="analogActionHandle"></param>
            /// <returns></returns>
            public static EInputActionOrigin[] GetDigitalActionOrigins(InputHandle_t controllerHandle, InputActionSetHandle_t actionSetHandle, InputDigitalActionHandle_t digitalActionHandle)
            {
                var origins = new EInputActionOrigin[Constants.STEAM_INPUT_MAX_ORIGINS];

                SteamInput.GetDigitalActionOrigins(controllerHandle, actionSetHandle, digitalActionHandle, origins);

                return origins;
            }
            /// <summary>
            /// Get the origin(s) for an analogue action within an action set by filling originsOut with EInputActionOrigin handles. Use this to display the appropriate on-screen prompt for the action.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="actionSet"></param>
            /// <param name="actionName"></param>
            /// <returns></returns>
            public static EInputActionOrigin[] GetDigitalActionOrigins(InputHandle_t controllerHandle, string actionSet, string actionName)
            {
                var origins = new EInputActionOrigin[Constants.STEAM_INPUT_MAX_ORIGINS];

                if (!_mInputDigitalActionHandles.ContainsKey(actionName))
                    GetDigitalActionHandle(actionName);

                if (!_mInputDigitalActionHandles.ContainsKey(actionSet))
                    GetActionSetHandle(actionSet);

                SteamInput.GetDigitalActionOrigins(controllerHandle, _mInputActionSetHandles[actionSet], _mInputDigitalActionHandles[actionName], origins);

                return origins;
            }
            /// <summary>
            /// Returns the associated gamepad index for the specified controller, if emulating a gamepad.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <returns></returns>
            public static int GetGamepadIndexForController(InputHandle_t controllerHandle) => SteamInput.GetGamepadIndexForController(controllerHandle);
            /// <summary>
            /// Get and cache glyph images
            /// </summary>
            /// <param name="origin"></param>
            /// <returns></returns>
            public static Texture2D GetGlyphActionOrigin(EInputActionOrigin origin)
            {
                if (_glyphs.ContainsKey(origin))
                    return _glyphs[origin];
                else
                {
                    var path = GetGlyphPNGForActionOrigin(origin, ESteamInputGlyphSize.k_ESteamInputGlyphSize_Large, 0);
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (System.IO.File.Exists(path))
                        {
                            var fileData = System.IO.File.ReadAllBytes(path);
                            var tex = new Texture2D(2, 2);
                            tex.LoadImage(fileData);

                            _glyphs.Add(origin, tex);
                            return tex;
                        }
                        else
                            return null;
                    }
                    else
                        return null;
                }
            }

            /// <summary>
            /// Releases and clears all glyph images being tracked, freeing up resources.
            /// </summary>
            public static void UnloadGlyphImages()
            {
                foreach (var pair in _glyphs)
                {
                    if (pair.Value != null)
                        GameObject.Destroy(pair.Value);
                }

                _glyphs = new Dictionary<EInputActionOrigin, Texture2D>();
            }

            /// <summary>
            /// Retrieves the PNG image path for the glyph associated with a specific input action origin.
            /// </summary>
            /// <param name="origin">The input action origin for which the glyph is requested.</param>
            /// <param name="size">The desired size of the glyph image.</param>
            /// <param name="flags">Optional flags to customise the retrieval of the glyph image.</param>
            /// <returns>A string representing the path to the glyph PNG image for the specified input action origin.</returns>
            public static string GetGlyphPNGForActionOrigin(EInputActionOrigin origin, ESteamInputGlyphSize size,
                uint flags) => SteamInput.GetGlyphPNGForActionOrigin(origin, size, flags);

            /// <summary>
            /// Retrieves the SVG glyph representation for a specified input action origin.
            /// </summary>
            /// <param name="origin">The input action origin for which the SVG glyph is to be retrieved.</param>
            /// <param name="flags">Flags that modify how the glyph is retrieved.</param>
            /// <returns>A string containing the SVG glyph for the specified input action origin.</returns>
            public static string GetGlyphSvgForActionOrigin(EInputActionOrigin origin, uint flags) =>
                SteamInput.GetGlyphSVGForActionOrigin(origin, flags);

            /// <summary>
            /// Returns the input type (device model) for the specified controller. This tells you if a given controller is a Steam controller, XBox 360 controller, PS4 controller, etc.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <returns></returns>
            public static ESteamInputType GetInputTypeForHandle(InputHandle_t controllerHandle) => SteamInput.GetInputTypeForHandle(controllerHandle);
            /// <summary>
            /// Returns raw motion data for the specified controller.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <returns></returns>
            public static InputMotionData_t GetMotionData(InputHandle_t controllerHandle) => SteamInput.GetMotionData(controllerHandle);
            /// <summary>
            /// Returns a localised string (from Steam's language setting) for the specified origin.
            /// </summary>
            /// <param name="origin"></param>
            /// <returns></returns>
            public static string GetStringForActionOrigin(EInputActionOrigin origin) => SteamInput.GetStringForActionOrigin(origin);
            /// <summary>
            /// Must be called when starting use of the Input interface.
            /// </summary>
            public static bool Init(IEnumerable<(string name, InputActionType type)> actions = null)
            {
                _mInitialized = SteamInput.Init(false);
                foreach(var action in actions)
                {
                    _actions.Add(action);
                }
                return _mInitialized;
            }

            /// <summary>
            /// Synchronise the API state with the latest Steam Controller inputs available. This is performed automatically by SteamAPI_RunCallbacks, but for the absolute lowest possible latency, you can call this directly before reading controller state.
            /// </summary>
            public static void RunFrame()
            {
                SteamInput.RunFrame();

                int count = SteamInput.GetConnectedControllers(_controllerHandleBuffer);

                int addedCount = 0;
                int removedCount = 0;

                // Detect newly connected controllers
                for (int i = 0; i < count; i++)
                {
                    var handle = _controllerHandleBuffer[i];
                    if (!_currentControllers.Contains(handle))
                    {
                        _currentControllers.Add(handle);
                        _addedBuffer[addedCount++] = handle;
                    }
                }

                // Detect disconnected controllers
                int currentCount = _currentControllers.Count;
                _currentControllers.CopyTo(_currentArrayBuffer, 0);
                for (int i = 0; i < currentCount; i++)
                {
                    var handle = _currentArrayBuffer[i];
                    bool stillConnected = false;
                    for (int j = 0; j < count; j++)
                    {
                        if (_controllerHandleBuffer[j] == handle)
                        {
                            stillConnected = true;
                            break;
                        }
                    }

                    if (!stillConnected)
                    {
                        _currentControllers.Remove(handle);
                        _removedBuffer[removedCount++] = handle;
                    }
                }

                // Update the public Controllers list
                ConnectedControllers.Clear();
                ConnectedControllers.AddRange(_currentControllers);

                if(IsAutoRefreshControllerState)
                {
                    for(int i = 0; i < ConnectedControllers.Count; i++)
                    {
                        Update(ConnectedControllers[i]);
                    }
                }

                // Raise events after list is up-to-date
                for (int i = 0; i < addedCount; i++)
                    SteamTools.Events.InvokeOnControllerConnected(_addedBuffer[i]);

                for (int i = 0; i < removedCount; i++)
                    SteamTools.Events.InvokeOnControllerDisconnected(_removedBuffer[i]);
            }

            /// <summary>
            /// Sets the color of the controllers LED
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="color"></param>
            public static void SetLedColor(InputHandle_t controllerHandle, Color32 color) => SteamInput.SetLEDColor(controllerHandle, color.r, color.g, color.b, 0);
            /// <summary>
            /// Resets the color fo the controllers LED to the users default
            /// </summary>
            /// <param name="controllerHandle"></param>
            public static void ResetLedColor(InputHandle_t controllerHandle) => SteamInput.SetLEDColor(controllerHandle, 0, 0, 0, 1);
            /// <summary>
            /// Must be called when ending use of the Input interface.
            /// </summary>
            public static bool Shutdown()
            {
                _mInitialized = false;
                return SteamInput.Shutdown();
            }
            /// <summary>
            /// Invokes the Steam overlay and brings up the binding screen.
            /// </summary>
            public static void ShowBindingPanel(InputHandle_t controllerHandle) => SteamInput.ShowBindingPanel(controllerHandle);
            /// <summary>
            /// Stops the momentum of an analog action (where applicable, ie a touchpad w/ virtual trackball settings).
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="analogAction"></param>
            public static void StopAnalogActionMomentum(InputHandle_t controllerHandle, InputAnalogActionHandle_t analogAction) => SteamInput.StopAnalogActionMomentum(controllerHandle, analogAction);
            /// <summary>
            /// Stops the momentum of an analog action (where applicable, ie a touchpad w/ virtual trackball settings).
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="actionName"></param>
            public static void StopAnalogActionMomentum(InputHandle_t controllerHandle, string actionName)
            {
                if (_mInputAnalogActionHandles.TryGetValue(actionName, out var handle))
                    SteamInput.StopAnalogActionMomentum(controllerHandle, handle);
                else
                {
                    var action = GetAnalogActionHandle(actionName);
                    SteamInput.StopAnalogActionMomentum(controllerHandle, action);
                }
            }
            /// <summary>
            /// Trigger a vibration event on supported controllers.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="leftSpeed"></param>
            /// <param name="rightSpeed"></param>
            public static void TriggerVibration(InputHandle_t controllerHandle, ushort leftSpeed, ushort rightSpeed) => SteamInput.TriggerVibration(controllerHandle, leftSpeed, rightSpeed);
            /// <summary>
            /// Get an action origin that you can use in your glyph look up table or passed into GetGlyphForActionOrigin or GetStringForActionOrigin
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="origin"></param>
            public static void GetActionOriginFromXboxOrigin(InputHandle_t controllerHandle, EXboxOrigin origin) => SteamInput.GetActionOriginFromXboxOrigin(controllerHandle, origin);
            /// <summary>
            /// Get the equivalent origin for a given controller type or the closest controller type that existed in the SDK you built into your game if eDestinationInputType is k_ESteamInputType_Unknown. This action origin can be used in your glyph look up table or passed into GetGlyphForActionOrigin or GetStringForActionOrigin
            /// </summary>
            /// <param name="destination"></param>
            /// <param name="source"></param>
            public static void TranslateActionOrigin(ESteamInputType destination, EInputActionOrigin source) => SteamInput.TranslateActionOrigin(destination, source);
            /// <summary>
            /// Gets the major and minor device binding revisions for Steam Input API configurations. Major revisions are to be used when changing the number of action sets or otherwise reworking configurations to the degree that older configurations are no longer usable. When a user's binding disagrees with the major revision of the current official configuration Steam will forcibly update the user to the new configuration. New configurations will need to be made for every controller when updating the major revision. Minor revisions are for small changes such as adding a new optional action or updating localization in the configuration. When updating the minor revision you generally can update a single configuration and check the "Use Action Block" to apply the action block changes to the other configurations.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="major"></param>
            /// <param name="minor"></param>
            /// <returns></returns>
            public static bool GetDeviceBindingRevision(InputHandle_t controllerHandle, out int major, out int minor) => SteamInput.GetDeviceBindingRevision(controllerHandle, out major, out minor);
            /// <summary>
            /// Get the Steam Remote Play session ID associated with a device, or 0 if there is no session associated with it. See iSteamRemotePlay.h for more information on Steam Remote Play sessions
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <returns></returns>
            public static uint GetRemotePlaySessionID(InputHandle_t controllerHandle) => SteamInput.GetRemotePlaySessionID(controllerHandle);
        }
    }
}
#endif