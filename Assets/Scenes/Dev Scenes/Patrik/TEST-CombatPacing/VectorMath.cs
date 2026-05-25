using System;
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
        
        /// <summary>
        /// RotationRate in angles
        /// </summary>
        /// <param name="wantedDir"></param>
        /// <param name="currentDir"></param>
        /// <param name="rotationRate"></param>
        /// <param name="timeDelta"></param>
        /// <returns></returns>
        public static Vector2 SmoothRotation(Vector2 wantedDir, Vector2 currentDir, uint rotationRate ,float timeDelta)
        {
            float maxRadiansToRotate = Mathf.Deg2Rad*rotationRate * timeDelta;
            float totalAngles = Vector2.SignedAngle(currentDir, wantedDir);
            
            //Set to perfect rotation =)
            if (MathF.Abs(totalAngles)*Mathf.Deg2Rad <= maxRadiansToRotate)
            {
                return wantedDir;
            }
            
            if (Vector2.SignedAngle(currentDir, wantedDir) > 0) //CCW rotation --> will result in negative radian rotation, while CW rotation --> will result in positive radian rotation
            {
                maxRadiansToRotate = -maxRadiansToRotate;
            }
            return VectorMath.Rotate(currentDir, maxRadiansToRotate);
        }
    }
}