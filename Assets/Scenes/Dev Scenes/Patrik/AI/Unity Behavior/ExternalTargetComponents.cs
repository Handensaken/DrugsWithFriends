using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/ExternalTargetComponents")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "ExternalTargetComponents", message: "Agent is [aggressive]", category: "Events", id: "cd39eb5e10502b8da72cd8b0fff4ffe5")]
public sealed partial class ExternalTargetComponents : EventChannel<bool> { }

