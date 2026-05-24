using UnityEngine;

public class AnimationMiddleman : MonoBehaviour
{
    [SerializeField] private PlayerNetwork playerNetwork;

    public void EnableHitbox()
    {
        playerNetwork.EnableHitBox();
    }
    
    public void DisableHitbox()
    {
        playerNetwork.DisableHitBox();
    }

    public void AttackStart()
    {
        playerNetwork.OnAttackStart();
    }
    
    public void AttackEnd()
    {
        playerNetwork.OnAttackEnd();
    }
}
