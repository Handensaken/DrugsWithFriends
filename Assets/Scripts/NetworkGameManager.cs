using UnityEngine;
using FishNet.Object;

public class NetworkGameManager : NetworkBehaviour
{
     [SerializeField] private NetworkObject[] globalObjects;

     public override void OnStartServer()
     {
          Debug.Log("GameManager");
          base.OnStartServer();
          foreach (NetworkObject networkObject in globalObjects)
          {
               NetworkObject obj = Instantiate(networkObject);
               Spawn(obj);
          }
          
     }
}
