using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class MissionChain
{
    private MissionChainConfig _config;
    private List<IMission> _missionInstances;
    private int _currentMissionIndex;
    private bool _isRunning;
    private Timer _delayTimer;

    public event Action<MissionChain> OnChainCompleted;
    public event Action<MissionChain, IMission> OnMissionStarted;
    public event Action<MissionChain, IMission> OnMissionCompleted;

    public string ChainName => _config.chainName;
    public bool IsRunning => _isRunning;
    public IMission CurrentMission => _currentMissionIndex < _missionInstances.Count ?
        _missionInstances[_currentMissionIndex] : null;

    public MissionChain(MissionChainConfig config)
    {
        _config = config;
        _missionInstances = new List<IMission>();
        _delayTimer = new Timer();
        _currentMissionIndex = 0;

        InitializeMissions();
    }

    private void InitializeMissions()
    {
        foreach (var missionConfig in _config.missions)
        {
            var mission = missionConfig.CreateMissionInstance();
            if (mission != null)
            {
                _missionInstances.Add(mission);
            }
        }
    }

    public async UniTask StartChainAsync()
    {
        if (_isRunning || _missionInstances.Count == 0) return;

        _isRunning = true;
        _currentMissionIndex = 0;
        await ExecuteChainAsync();
    }

    private async UniTask ExecuteChainAsync()
    {
        while (_isRunning && _currentMissionIndex < _missionInstances.Count)
        {
            var currentMission = _missionInstances[_currentMissionIndex];
            var missionConfig = _config.missions[_currentMissionIndex];

            // Задержка перед запуском миссии
            if (missionConfig.delayBeforeStart > 0)
            {
                await _delayTimer.StartAsync(
                    Mathf.RoundToInt(missionConfig.delayBeforeStart * 1000));
            }

            // Подписка на события миссии
            currentMission.OnStarted += () => OnMissionStarted?.Invoke(this, currentMission);
            currentMission.OnFinished += OnCurrentMissionFinished;

            // Запуск миссии
            currentMission.Start();

            // Ждем завершения миссии
            await UniTask.WaitUntil(() => currentMission.IsCompleted);

            _currentMissionIndex++;
        }

        // Обработка зацикливания
        if (_config.loopChain && _isRunning)
        {
            await _delayTimer.StartAsync(
                Mathf.RoundToInt(_config.delayBetweenLoops * 1000));
            _currentMissionIndex = 0;
            await ExecuteChainAsync();
        }
        else
        {
            CompleteChain();
        }
    }

    private void OnCurrentMissionFinished()
    {
        var mission = CurrentMission;
        if (mission != null)
        {
            mission.OnFinished -= OnCurrentMissionFinished;
            OnMissionCompleted?.Invoke(this, mission);
        }
    }

    public void StopChain()
    {
        _isRunning = false;
        _delayTimer.Cancel();

        var currentMission = CurrentMission;
        if (currentMission != null && currentMission.IsActive)
        {
            currentMission.Stop();
        }
    }

    private void CompleteChain()
    {
        _isRunning = false;
        OnChainCompleted?.Invoke(this);
    }

    public void Dispose()
    {
        StopChain();

        foreach (var mission in _missionInstances)
        {
            if (mission is MonoBehaviour mb)
            {
                UnityEngine.Object.Destroy(mb.gameObject);
            }
        }

        _missionInstances.Clear();
    }
}