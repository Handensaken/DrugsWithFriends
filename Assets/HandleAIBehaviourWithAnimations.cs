using Unity.Behavior;
using UnityEngine;

[Tooltip("Will change if requested - not a good solution")]
public class HandleAIBehaviourWithAnimations : StateMachineBehaviour
{
    [SerializeField] private string boolName;
    [SerializeField] private bool value;
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponentInParent<BehaviorGraphAgent>().SetVariableValue(boolName, value);
    }
}
