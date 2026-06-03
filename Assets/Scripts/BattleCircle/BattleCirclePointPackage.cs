using System;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    public struct BattleCirclePointPackage : IEquatable<BattleCirclePointPackage>
    {
        public Vector3 PointInCircle;
        public float AngleInCircle;

        public bool Equals(BattleCirclePointPackage other)
        {
            return PointInCircle.Equals(other.PointInCircle) && AngleInCircle.Equals(other.AngleInCircle);
        }

        public override bool Equals(object obj)
        {
            return obj is BattleCirclePointPackage other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PointInCircle, AngleInCircle);
        }
    }
}