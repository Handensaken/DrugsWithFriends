using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI
{
    public class TempDespawn : MonoBehaviour
    {
        private float timer = 0;
        private float time = 2;
    

        // Update is called once per frame
        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= time)
            {
                Destroy(this);
            }
        }

    
    }
}
