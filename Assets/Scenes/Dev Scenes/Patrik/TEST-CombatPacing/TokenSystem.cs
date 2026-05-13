using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Random = System.Random;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    public class TokenSystem
    {
        private readonly BattleCircleData _data;
        private readonly List<BlackboardReference> _aisInCircle;
        private readonly List<BlackboardReference> _fightingAis;
        
        private float _currentTime = 0;

        public TokenSystem(BattleCircleData data,List<BlackboardReference> aisInCircle, List<BlackboardReference> fightingAis)
        {
            _data = data;
            _aisInCircle = aisInCircle;
            _fightingAis = fightingAis;
            
            SetRndNextTimeForNewFighter();
        }
        
        private void SetRndNextTimeForNewFighter()
        {
            _currentTime = new Random().Next(_data.tokenCreationData.minTime,_data.tokenCreationData.maxTime);
            _currentTime += (float)new Random().NextDouble();
        }
        
        public void UpdateTime(float timeDelta)
        {
            //Debug.Log("Amount of fightingAis: "+_fightingAis.Count);
            _currentTime -= timeDelta;
            if (_currentTime <= 0)
            {
                GiveToken();
                SetRndNextTimeForNewFighter();
            }
        }
        
        private void GiveToken()
        {
            BlackboardReference[] availableAIs = AvailableAIs();
            if (availableAIs.Length <= 0)
            {
                return;
            }
            _data.AssignAsFighting(availableAIs[0]);
            Debug.Log("Newly assigned fightingEnemy");
        }

        private BlackboardReference[] AvailableAIs()
        {
            if (_fightingAis.Count <= 0)
            {
                return _aisInCircle.ToArray();
            }
            
            List<BlackboardReference> result = new List<BlackboardReference>();
            
            foreach (BlackboardReference ai in _aisInCircle)
            {
                if (_fightingAis.Contains(ai))
                {
                    Debug.Log("Skipped");
                    continue;
                }
                
                result.Add(ai);
            }
            
            return _aisInCircle.ToArray();
        }
    }
}