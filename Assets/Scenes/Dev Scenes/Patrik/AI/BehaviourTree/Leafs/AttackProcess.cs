using System.Diagnostics;
using BehaviourTree;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Scenes.Dev_Scenes.Patrik.AI.BehaviourTree.Leafs
{
    public class AttackProcess : IProcess
    {
        private readonly Transform _attackPoint;
        private readonly GameObject _attackArea;

        private readonly float _attackCooldown = 5;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public AttackProcess(GameObject attackArea, Transform attackPoint)
        {
            _attackArea = attackArea;
            _attackPoint = attackPoint;
            _stopwatch.Start(); //TODO coroutine
        }
        
        public INode.NodeState Process()
        {
            if (_stopwatch.Elapsed.TotalSeconds >= _attackCooldown)
            {
                Object.Instantiate(_attackArea,_attackPoint.position, Quaternion.identity);
                _stopwatch.Restart();
                return INode.NodeState.Success;
            }

            return INode.NodeState.Failure;
        }

        public void Reset()
        {
            _stopwatch.Restart();
        }
    }
}
