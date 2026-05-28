using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Events;
using Random = System.Random;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    //TODO separate logic into different "Taunt-system" and "Attack-system" with an overHead "Token-system"
    public class TokenSystem
    {
        private readonly BattleCircleData _data;
        private readonly List<BlackboardReference> _aisInCircle;
        private readonly List<BlackboardReference> _fightingAis;
        private readonly List<BlackboardReference> _tauntingAIs;

        private readonly UnityAction<BlackboardReference> _attackingEvent;
        private readonly UnityAction<BlackboardReference> _tauntingEvent;
        
        private float _currentTimeAttack = 0;
        private float _currentTimeTaunt = 0;

        public TokenSystem(
            BattleCircleData data,
            List<BlackboardReference> aisInCircle,
            List<BlackboardReference> fightingAis,
            List<BlackboardReference> tauntingAIs,
            ref UnityAction<BlackboardReference> attackingEvent,
            ref UnityAction<BlackboardReference> tauntingEvent)
        {
            _data = data;
            
            _aisInCircle = aisInCircle;
            _fightingAis = fightingAis;
            _tauntingAIs = tauntingAIs;
            
            _attackingEvent = attackingEvent;
            _tauntingEvent = tauntingEvent;
            
            SetRndNextTime(ref _currentTimeAttack);
            SetRndNextTime(ref _currentTimeTaunt);
        }
        
        private void SetRndNextTime(ref float timer)
        {
            timer = new Random().Next(_data.tokenCreationData.minTime,_data.tokenCreationData.maxTime);
            timer += (float)new Random().NextDouble();
        }
        
        public void UpdateTime(float timeDelta)
        {
            _currentTimeAttack -= timeDelta;
            if (_currentTimeAttack <= 0 && GiveAttackToken())
            {
                SetRndNextTime(ref _currentTimeAttack);
            }
            
            _currentTimeTaunt -= timeDelta;
            if (_currentTimeTaunt <= 0 && GiveTauntToken())
            {
                SetRndNextTime(ref _currentTimeTaunt);
            }
        }
        
        private bool GiveAttackToken()
        {
            BlackboardReference[] availableAIs = AvailableAIs();
            if (availableAIs.Length <= 0)
            {
                return false;
            }
            //_data.AssignAsFighting(availableAIs[0]);
            _attackingEvent(availableAIs[0]); //TOD rnd
            
            Debug.Log("Newly assigned fightingEnemy");
            return true;
        }

        private bool GiveTauntToken()
        {
            BlackboardReference[] availableAIs = AvailableAIs();
            if (availableAIs.Length <= 0)
            {
                return false;
            }
            //_data.AssignAsFighting(availableAIs[0]);
            _tauntingEvent(availableAIs[0]); //TOD rnd
            
            Debug.Log("Newly assigned tauntingEnemy");
            return true;
        }

        private BlackboardReference[] AvailableAIs()
        {
            if (_aisInCircle.Count <= 0)
            {
                return Array.Empty<BlackboardReference>();
            }
            
            List<BlackboardReference> result = new List<BlackboardReference>();
            
            foreach (BlackboardReference ai in _aisInCircle)
            {
                if (_fightingAis.Contains(ai) || _tauntingAIs.Contains(ai))
                {
                    Debug.Log("Skipped");
                    continue;
                }
                
                result.Add(ai);
            }
            
            return result.ToArray();
        }
    }
}