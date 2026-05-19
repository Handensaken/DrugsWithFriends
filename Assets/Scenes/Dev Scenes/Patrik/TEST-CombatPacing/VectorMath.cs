using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    public static class VectorMath
    {
        /// <summary>
        /// Uses radians
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="deltaAngle"></param>
        /// <returns></returns>
        public static Vector2 Rotate(Vector2 vector, float deltaAngle)
        {
            Vector2 result = new Vector2
            (vector.x*Mathf.Cos(deltaAngle) + vector.y*Mathf.Sin(deltaAngle),
                -vector.x*Mathf.Sin(deltaAngle) + vector.y*Mathf.Cos(deltaAngle));
            
            return result;
        }
    }
}