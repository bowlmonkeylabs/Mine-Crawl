using System;
using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

public class ScoreController : MonoBehaviour
{
    [SerializeField] private bool _enableLogs = false;
    [SerializeField] private IntVariable _gameScore;
    [TitleGroup("Enemies Killed")]
    [SerializeField] private DynamicGameEvent _onEnemyKilled;
    [SerializeField] private int _enemyKilledScore = 500;
    //how fast the player was
    [TitleGroup("Level Time")]
    [SerializeField] private GameEvent _levelChange;
    [SerializeField] private IntVariable _currentLevel;
    [SerializeField] private List<TimerVariable> _wormSpawnTimers;
    [SerializeField] private int _timeRemainingScorePerSec = 50;

    //ore both types extracted
    [TitleGroup("Ore Extracted")]
    [SerializeField] private IntVariable _rareOreCount;
    [SerializeField] private int _rareOreScore = 1000;
    [SerializeField] private IntVariable _commonOreCount;
    [SerializeField] private int _commonOreScore = 100;

    void OnEnable()
    {
        _onEnemyKilled.Subscribe(OnEnemyKilled);
        _levelChange.Subscribe(OnLevelChange);
        _commonOreCount.Subscribe(OnCommonOreCountUpdated);
        _rareOreCount.Subscribe(OnRareOreCountUpdated);
    }

    void OnDisable()
    {
        _onEnemyKilled.Unsubscribe(OnEnemyKilled);
        _levelChange.Unsubscribe(OnLevelChange);
        _commonOreCount.Unsubscribe(OnCommonOreCountUpdated);
        _rareOreCount.Unsubscribe(OnRareOreCountUpdated);
    }

    private void OnEnemyKilled(object prevValue, object currValue)
    {
        if(_enableLogs) Debug.Log("Score Updated: Enemy Killed, +" + _enemyKilledScore + " points");
        _gameScore.Increment(_enemyKilledScore);
    }

    private void OnLevelChange()
    {
        int justExitedLevelIndex = _currentLevel.Value - 1;
        TimerVariable justExitedWormSpawnTimer = _wormSpawnTimers[justExitedLevelIndex];
        //we want to give them a higher score the less time they took
        float timeRemaining = justExitedWormSpawnTimer.Duration - Math.Min(justExitedWormSpawnTimer.ElapsedTime, justExitedWormSpawnTimer.Duration);
        int timeScore = (int)timeRemaining * _timeRemainingScorePerSec;
        if(_enableLogs) Debug.Log("Score Updated: Level Cleared, +" + timeScore + " points");
        _gameScore.Increment(timeScore);
    }

    private void OnCommonOreCountUpdated(int prevVal, int newVal)
    {
        int valDiff = newVal - prevVal;
        //ore count was increased
        if(valDiff > 0)
        {
            //add to game score for gaining ore
            int oreScore = valDiff * _commonOreScore;
            if(_enableLogs) Debug.Log("Score Updated: Common Ore Mined, +" + oreScore + " points");
            _gameScore.Increment(oreScore);
        }
    }

    private void OnRareOreCountUpdated(int prevVal, int newVal)
    {
        int valDiff = newVal - prevVal;
        //ore count was increased
        if(valDiff > 0)
        {
            //add to game score for gaining ore
            int oreScore = valDiff * _rareOreScore;
            if(_enableLogs) Debug.Log("Score Updated: Rare Ore Mined, +" + oreScore + " points");
            _gameScore.Increment(oreScore);
        }
    }
}
