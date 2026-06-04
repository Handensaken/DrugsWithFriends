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
        private readonly PlayerNetwork _player;
        
        private readonly BattleCircleData _data;
        private readonly List<BlackboardReference> _aisInCircle;
        private readonly List<BlackboardReference> _fightingAis;
        private readonly List<BlackboardReference> _tauntingAIs;

        private readonly UnityAction<BlackboardReference> _attackingEvent;
        private readonly UnityAction<BlackboardReference> _tauntingEvent;
        
        private float _currentTimeAttack = 0;
        private float _currentTimeTaunt = 0;

        public TokenSystem(
            //PlayerNetwork player,
            BattleCircleData data,
            List<BlackboardReference> aisInCircle,
            List<BlackboardReference> fightingAis,
            List<BlackboardReference> tauntingAIs,
            ref UnityAction<BlackboardReference> attackingEvent,
            ref UnityAction<BlackboardReference> tauntingEvent)
        {
            //_player = player;
            
            _data = data;
            
            _aisInCircle = aisInCircle;
            _fightingAis = fightingAis;
            _tauntingAIs = tauntingAIs;
            
            _attackingEvent = attackingEvent;
            _tauntingEvent = tauntingEvent;
            
            SetRndNextTime(ref _currentTimeAttack, _data.attackTokenCreationData);
            SetRndNextTime(ref _currentTimeTaunt,_data.tauntTokenCreationData);
        }
        
        private void SetRndNextTime(ref float timer, TokenManagingPackage data)
        {
            timer = new Random().Next(data.minTime,data.maxTime);
            timer += (float)new Random().NextDouble();
        }
        
        public void UpdateTime(float timeDelta)
        {
            _currentTimeAttack -= timeDelta;
            if (_currentTimeAttack <= 0 && GiveAttackToken())
            {
                SetRndNextTime(ref _currentTimeAttack, _data.attackTokenCreationData);
            }
            
            _currentTimeTaunt -= timeDelta;
            if (_currentTimeTaunt <= 0 && GiveTauntToken())
            {
                SetRndNextTime(ref _currentTimeTaunt, _data.tauntTokenCreationData);
            }
        }
        
        private bool GiveAttackToken()
        {
            BlackboardReference[] availableAIs = AvailableAIs();
            if (availableAIs.Length <= 0)
            {
                return false;
            }
            
            //TODO Utility AI
            // if lockOn --> attack
            // else slump
            //Debug.Log();
            _attackingEvent(availableAIs[0]); 
            
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
            
            Random rnd = new Random();
            int rndIndex = rnd.Next(0, availableAIs.Length);
            _tauntingEvent(availableAIs[rndIndex]);
            
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
                    continue;
                }
                
                result.Add(ai);
            }
            
            return result.ToArray();
        }
    }
}