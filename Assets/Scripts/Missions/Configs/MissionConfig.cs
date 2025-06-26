using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MissionConfig", menuName = "Missions/Mission Config")]
public class MissionConfig : MonoBehaviour
{
    [Header("Mission Settings")]
    public string missionName;
    public float delayBeforeStart = 0f;

    [Header("Mission Implementation")]
    [SerializeField] private GameObject missionPrefab;

    [Header("Chain Settings")]
    public MissionConfig nextMission;

    public IMission CreateMissionInstance()
    {
        if (missionPrefab != null)
        {
            GameObject instance = Instantiate(missionPrefab);
            return instance.GetComponent<IMission>();
        }
        return null;
    }
}