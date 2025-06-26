using System;
using UnityEngine;

public interface IMission
{
    event Action OnStarted;
    event Action OnMissionPointReached;
    event Action OnFinished;

    string MissionName { get; }
    bool IsActive { get; }
    bool IsCompleted { get; }

    void Start();
    void Stop();
}