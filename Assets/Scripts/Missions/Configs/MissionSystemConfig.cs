using UnityEngine;

[CreateAssetMenu(fileName = "MissionSystemConfig", menuName = "Missions/Mission System Config")]
public class MissionSystemConfig : ScriptableObject
{
    [Header("Mission Chains")]
    public MissionChainConfig[] missionChains;

    [Header("System Settings")]
    public bool startAllChainsOnAwake = true;
}