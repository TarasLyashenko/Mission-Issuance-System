using UnityEngine;
using Cysharp.Threading.Tasks;

public class ExampleMission : MonoBehaviour, IMission
{
    [Header("Mission Settings")]
    [SerializeField] private string missionName = "Example Mission";
    [SerializeField] private float missionDuration = 3f;

    public event System.Action OnStarted;
    public event System.Action OnMissionPointReached;
    public event System.Action OnFinished;

    public string MissionName => missionName;
    public bool IsActive { get; private set; }
    public bool IsCompleted { get; private set; }

    private Timer _missionTimer;

    private void Awake()
    {
        _missionTimer = new Timer();
    }

    public async void Start()
    {
        if (IsActive || IsCompleted) return;

        IsActive = true;
        OnStarted?.Invoke();

        Debug.Log($"Mission '{MissionName}' started!");

        // —имул€ци€ выполнени€ миссии
        await ExecuteMissionAsync();

        CompleteMission();
    }

    private async UniTask ExecuteMissionAsync()
    {
        // —имул€ци€ точки миссии в середине выполнени€
        await _missionTimer.StartAsync(Mathf.RoundToInt(missionDuration * 500));
        OnMissionPointReached?.Invoke();

        // «авершение миссии
        await _missionTimer.StartAsync(Mathf.RoundToInt(missionDuration * 500));
    }

    private void CompleteMission()
    {
        IsActive = false;
        IsCompleted = true;

        Debug.Log($"Mission '{MissionName}' completed!");
        OnFinished?.Invoke();
    }

    public void Stop()
    {
        if (!IsActive) return;

        _missionTimer.Cancel();
        IsActive = false;

        Debug.Log($"Mission '{MissionName}' stopped!");
    }
}