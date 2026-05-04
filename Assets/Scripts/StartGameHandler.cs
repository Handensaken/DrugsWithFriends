using System.Linq;
using UnityEngine;

public class StartGameHandler : MonoBehaviour
{
    [SerializeField] private Transform playerSpawnPoint;
    void Start()
    {
        Transform[] players = GameObject.FindGameObjectsWithTag("Player").Select(p => p.transform).ToArray();
        
        foreach (Transform player in players)
        {
            player.position = playerSpawnPoint.position;
        }
    }
}
