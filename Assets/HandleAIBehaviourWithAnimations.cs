using Unity.Behavior;
using UnityEngine;

public class HandleAIBehaviourWithAnimations : StateMachineBehaviour
{
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponentInParent<BehaviorGraphAgent>().SetVariableValue("Attack", false);
    }
}
