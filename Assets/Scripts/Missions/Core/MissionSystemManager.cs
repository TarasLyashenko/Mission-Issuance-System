using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class MissionSystemManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private MissionSystemConfig systemConfig;

    private List<MissionChain> _activeChains;
    private Dictionary<string, MissionChain> _chainsByName;

    public event System.Action<string> OnChainCompleted;
    public event System.Action<string, string> OnMissionStarted;
    public event System.Action<string, string> OnMissionCompleted;

    private void Awake()
    {
        _activeChains = new List<MissionChain>();
        _chainsByName = new Dictionary<string, MissionChain>();

        InitializeChains();

        if (systemConfig.startAllChainsOnAwake)
        {
            StartAllChains();
        }
    }

    private void InitializeChains()
    {
        foreach (var chainConfig in systemConfig.missionChains)
        {
            var chain = new MissionChain(chainConfig);

            // Подписка на события цепочки
            chain.OnChainCompleted += OnChainCompletedHandler;
            chain.OnMissionStarted += OnMissionStartedHandler;
            chain.OnMissionCompleted += OnMissionCompletedHandler;

            _activeChains.Add(chain);
            _chainsByName[chain.ChainName] = chain;
        }
    }

    public async void StartAllChains()
    {
        foreach (var chain in _activeChains)
        {
            if (!chain.IsRunning)
            {
                StartChainAsync(chain).Forget();
            }
        }
    }

    public async UniTask StartChainAsync(string chainName)
    {
        if (_chainsByName.TryGetValue(chainName, out var chain))
        {
            await StartChainAsync(chain);
        }
        else
        {
            Debug.LogWarning($"Chain '{chainName}' not found!");
        }
    }

    private async UniTask StartChainAsync(MissionChain chain)
    {
        await chain.StartChainAsync();
    }

    public void StopChain(string chainName)
    {
        if (_chainsByName.TryGetValue(chainName, out var chain))
        {
            chain.StopChain();
        }
    }

    public void StopAllChains()
    {
        foreach (var chain in _activeChains)
        {
            chain.StopChain();
        }
    }

    private void OnChainCompletedHandler(MissionChain chain)
    {
        OnChainCompleted?.Invoke(chain.ChainName);
        Debug.Log($"Chain '{chain.ChainName}' completed!");
    }

    private void OnMissionStartedHandler(MissionChain chain, IMission mission)
    {
        OnMissionStarted?.Invoke(chain.ChainName, mission.MissionName);
        Debug.Log($"Mission '{mission.MissionName}' started in chain '{chain.ChainName}'");
    }

    private void OnMissionCompletedHandler(MissionChain chain, IMission mission)
    {
        OnMissionCompleted?.Invoke(chain.ChainName, mission.MissionName);
        Debug.Log($"Mission '{mission.MissionName}' completed in chain '{chain.ChainName}'");
    }

    private void OnDestroy()
    {
        StopAllChains();

        foreach (var chain in _activeChains)
        {
            chain.Dispose();
        }

        _activeChains.Clear();
        _chainsByName.Clear();
    }
}