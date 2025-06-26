using UnityEngine;

[CreateAssetMenu(fileName = "MissionChain", menuName = "Missions/Mission Chain")]
public class MissionChainConfig : ScriptableObject
{
    [Header("Chain Settings")]
    public string chainName;
    public bool startAutomatically = true;

    [Header("Missions")]
    public MissionConfig[] missions;

    [Header("Chain Behavior")]
    public bool loopChain = false;
    public float delayBetweenLoops = 5f;
}