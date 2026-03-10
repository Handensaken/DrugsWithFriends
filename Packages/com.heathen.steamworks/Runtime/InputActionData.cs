#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using System;
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct InputActionData
    {
        public readonly InputActionType Type => type;
        public readonly string Name => name;

        public InputActionData(string actionName, InputActionType actionType)
        {
            type = actionType;
            name = actionName;
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [SerializeField]
        private InputActionType type;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [SerializeField]
        private string name;

        public readonly Steamworks.InputAnalogActionHandle_t AnalogHandle =>
            API.Input.Client.GetAnalogActionHandle(name);

        public readonly Steamworks.InputDigitalActionHandle_t DigitalHandle =>
            API.Input.Client.GetDigitalActionHandle(name);

        public readonly InputActionStateData GetActionData(Steamworks.InputHandle_t controller) =>
            API.Input.Client.GetActionData(controller, name);
        public readonly InputActionStateData GetActionData() => API.Input.Client.GetActionData(name);

        public readonly Texture2D[] GetInputGlyphs(Steamworks.InputHandle_t controller, InputActionSetData set) =>
            GetInputGlyphs(controller, new Steamworks.InputActionSetHandle_t(set.Handle));

        public readonly Texture2D[] GetInputGlyphs(Steamworks.InputHandle_t controller, InputActionSetLayerData set) =>
            GetInputGlyphs(controller, new Steamworks.InputActionSetHandle_t(set.Data.Handle));
        public readonly Texture2D[] GetInputGlyphs(Steamworks.InputHandle_t controller, Steamworks.InputActionSetHandle_t set)
        {
            if (type == InputActionType.Analog)
            {
                var origins = API.Input.Client.GetAnalogActionOrigins(controller, set, AnalogHandle);

                var textArray = new Texture2D[origins.Length];
                for (int i = 0; i < origins.Length; i++)
                {
                    textArray[i] = API.Input.Client.GetGlyphActionOrigin(origins[i]);
                }

                return textArray;
            }
            else
            {
                var origins = API.Input.Client.GetDigitalActionOrigins(controller, set, DigitalHandle);

                var textArray = new Texture2D[origins.Length];
                for (int i = 0; i < origins.Length; i++)
                {
                    textArray[i] = API.Input.Client.GetGlyphActionOrigin(origins[i]);
                }

                return textArray;
            }
        }

        public readonly string[] GetInputNames(Steamworks.InputHandle_t controller, InputActionSetData set) =>
            GetInputNames(controller, new Steamworks.InputActionSetHandle_t(set.Handle));

        public readonly string[] GetInputNames(Steamworks.InputHandle_t controller, InputActionSetLayerData set) =>
            GetInputNames(controller, new InputActionSetHandle_t(set.Data.Handle));
        public readonly string[] GetInputNames(Steamworks.InputHandle_t controller, Steamworks.InputActionSetHandle_t set)
        {
            if (type == InputActionType.Analog)
            {
                var origins = API.Input.Client.GetAnalogActionOrigins(controller, set, AnalogHandle);

                var stringArray = new string[origins.Length];
                for (int i = 0; i < origins.Length; i++)
                {
                    stringArray[i] = API.Input.Client.GetStringForActionOrigin(origins[i]);
                }

                return stringArray;
            }
            else
            {
                var origins = API.Input.Client.GetDigitalActionOrigins(controller, set, DigitalHandle);

                var stringArray = new string[origins.Length];
                for (int i = 0; i < origins.Length; i++)
                {
                    stringArray[i] = API.Input.Client.GetStringForActionOrigin(origins[i]);
                }

                return stringArray;
            }
        }
    }
}
#endif