using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace TestNetwork
{
    public class NewNetworkBehaviourTemplate : NetworkBehaviour
    {
        public NetworkObject cubePrefab;

        public override void OnStartClient()
        {
            if (IsOwner)
                GetComponent<PlayerInput>().enabled = true;
        }

        public void OnAttack(InputValue value)
        {
            if (value.isPressed)
                SpawnCube();
        }

        // We are using a ServerRpc here because the Server needs to do all network object spawning.
        [ServerRpc]
        private void SpawnCube()
        {
            var obj = Instantiate(cubePrefab, transform.position+new Vector3(0,1,0), Quaternion.identity);
            Spawn(obj); // NetworkBehaviour shortcut for ServerManager.Spawn(obj);
        }
    }
}
