using UnityEngine;

namespace AI_Experimental.Extra
{
    public class Activation : MonoBehaviour
    {
        [SerializeField] private GameObject gameObject;
        
        public void Activate()
        {
            //Debug.Log("activation");
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            //Debug.Log("deactivation");
            gameObject.SetActive(false);
        }
    
    }
}
