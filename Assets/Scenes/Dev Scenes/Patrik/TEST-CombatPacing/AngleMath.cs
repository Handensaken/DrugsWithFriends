using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    public static class AngleMath
    {
        public static bool IsWithinAngles(float pointAngle, uint angleStart, uint angleEnd)
        {
            if (Mathf.Approximately(angleStart, angleEnd))
            {
                Debug.Log("Same angleStart and angleEnd value --> No point can be used");
                return false;
            }
            
            if (angleStart < angleEnd)
            {
                return pointAngle >= angleStart && pointAngle <= angleEnd;
            }
            else
            {
                return pointAngle >= angleStart || pointAngle <= angleEnd;
            }
        }
    }
}