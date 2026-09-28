using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TutorialDay
{
    public string dayTitle = "Day 1";
    public string daySubtitle;

    [Header("Setup")]
    public JobRole role;
    public List<PresetCV> presetBatch = new List<PresetCV>();
    public bool computerWorking;

    [Header("Dialogue")]
    public DialogueSequence intro;
    public DialogueSequence nudge;                 
    public float nudgeAfterSeconds = 45f;
    public DialogueSequence feedbackSuccess;       
    public DialogueSequence feedbackFailure;

    [Header("Rules")]
    public bool repeatOnFailure = true;            // wrong pick replays this day instead of advancing
}

public class TutorialManager : MonoBehaviour, IRoundManager
{
    public static TutorialManager Instance { get; private set; }

    public enum GameState { InProgress, RoundComplete }
    [SerializeField] private GameState _currentState = GameState.InProgress;
    public GameState CurrentState => _currentState;

    [Header("Days")]
    [SerializeField] private List<TutorialDay> days = new List<TutorialDay>();

    [Header("Spawning")]
    [SerializeField] private GameObject CVPrefab;
    [SerializeField] private Transform[] CVSpawnPoints;

    [Header("References")]
    [SerializeField] private PlayerController player;

    [Header("Live CVs (filled at runtime)")]
    [SerializeField] private List<CVObject> allCVs = new();

    [Header("Events")]
    public UnityEvent<int, JobRole> onDayStarted;     // hook the "Current Open Role" label here
    public UnityEvent<bool> onComputerStateChanged;   // hook computer here
    public UnityEvent<CVObject> onCVStamped;
    public UnityEvent<RoundSummary> onRoundComplete;

    private int _dayIndex;
    private bool _dayActive;
    private bool _submitted;
    private bool _nudgeShown;
    private float _lastProgressTime;

    TutorialDay Day => days[_dayIndex];
    public JobRole currentRole => days.Count > 0 ? Day.role : null;

    public bool CanSubmitRound =>
        _dayActive && !_submitted && allCVs.Any(cv => cv != null && cv.Decision == StampType.Approve);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        CVObject.Stamped += HandleStamped;
        CVDetailUI.CVOpened += HandleCVOpened;
    }

    void OnDisable()
    {
        CVObject.Stamped -= HandleStamped;
        CVDetailUI.CVOpened -= HandleCVOpened;
    }

    void Start()
    {
        if (days.Count == 0) { Debug.LogWarning("[TutorialManager] No days configured."); return; }
        SetupDay(0);
        RunIntro();
    }

    void Update()
    {
        if (!_dayActive || _submitted || _nudgeShown || days.Count == 0 || Day.nudge == null) return;
        if (DialogueUI.Instance != null && DialogueUI.Instance.IsPlaying) return;

        if (player != null && player.IsHoldingSomething) { NotifyProgress(); return; }   // holding = active

        if (Time.time - _lastProgressTime >= Day.nudgeAfterSeconds)
        {
            _nudgeShown = true;
            PlayDialogue(Day.nudge, null);
        }
    }

    // Anything that counts as the player making progress resets the idle timer.
    public void NotifyProgress() => _lastProgressTime = Time.time;

    void HandleStamped(CVObject cv)
    {
        NotifyProgress();
        onCVStamped?.Invoke(cv);
    }

    void HandleCVOpened(CVObject cv) => NotifyProgress();

    // Day flow

    void SetupDay(int index)
    {
        _dayIndex = index;
        _submitted = false;
        _dayActive = false;
        _nudgeShown = false;
        _currentState = GameState.InProgress;

        CVDetailUI.Instance?.Close();
        ClearDesk();
        SpawnPresetBatch(Day);
        ScoreBatch(Day.role);

        onComputerStateChanged?.Invoke(Day.computerWorking);
        onDayStarted?.Invoke(index, Day.role);
    }

    void RunIntro()
    {
        PlayDialogue(Day.intro, () =>
        {
            _dayActive = true;
            _lastProgressTime = Time.time;
        });
    }

    void GoToDay(int index, string subtitleOverride = null)
    {
        var next = days[index];
        string subtitle = subtitleOverride ?? next.daySubtitle;

        if (DayTransition.Instance == null)
        {
            SetupDay(index);
            RunIntro();
            return;
        }

        DayTransition.Instance.Play(next.dayTitle, subtitle,
            onCovered: () => SetupDay(index),
            onFinished: RunIntro);
    }

    void ClearDesk()
    {
        foreach (var cv in allCVs)
            if (cv != null) Destroy(cv.gameObject);
        allCVs.Clear();
    }

    void SpawnPresetBatch(TutorialDay day)
    {
        if (day.presetBatch == null || day.presetBatch.Count == 0)
        {
            Debug.LogWarning($"[TutorialManager] {day.dayTitle} has no preset CVs.");
            return;
        }

        if (CVSpawnPoints == null || CVSpawnPoints.Length == 0)
        {
            Debug.LogWarning("[TutorialManager] No spawn points assigned.");
            return;
        }

        if (day.presetBatch.Count > CVSpawnPoints.Length)
            Debug.LogWarning("[TutorialManager] More preset CVs than spawn points - the extras won't be placed.");

        for (int i = 0; i < day.presetBatch.Count && i < CVSpawnPoints.Length; i++)
        {
            GeneratedCV data = day.presetBatch[i].ToGeneratedCV();
            GameObject obj = Instantiate(CVPrefab, CVSpawnPoints[i].position, CVSpawnPoints[i].rotation);
            CVObject cvObj = obj.GetComponent<CVObject>();
            cvObj.SetData(data);
            allCVs.Add(cvObj);
        }
    }

    void ScoreBatch(JobRole role)
    {
        if (role == null) { Debug.LogWarning("[TutorialManager] Day has no JobRole."); return; }

        foreach (var cv in allCVs)
        {
            if (cv.Data == null) continue;
            CVScorer.Score(cv.Data, role);
            Debug.Log($"[Tutorial] {cv.Data.CVName}: Content={cv.Data.ContentScore:F1} Format={cv.Data.FormatScore:F1} Final={cv.Data.FinalScore:F1}");
        }
    }

    // Round resolution (called by FaxMachine through IRoundManager)
    public void SubmitRound()
    {
        if (_submitted) return;
        _submitted = true;
        _currentState = GameState.RoundComplete;

        var allData = allCVs.Where(cv => cv.Data != null).Select(cv => cv.Data).ToList();
        var approvedCV = allCVs.FirstOrDefault(cv => cv.Decision == StampType.Approve);

        var summary = new RoundSummary();
        if (allData.Count > 0 && approvedCV?.Data != null)
        {
            summary.Result = CVRanker.Evaluate(allData, approvedCV.Data);
            summary.HasApproval = true;
        }

        var bestCV = CVRanker.Rank(allData).FirstOrDefault();

        foreach (var cv in allCVs)
        {
            if (cv.Data == null) continue;

            bool shouldApprove = cv.Data == bestCV;
            bool playerApproved = cv.Decision == StampType.Approve;
            bool correct = shouldApprove == playerApproved;

            summary.Entries.Add(new RoundSummary.Entry { CV = cv, WasCorrect = correct });
            if (correct) summary.CorrectCount++; else summary.IncorrectCount++;

            cv.gameObject.SetActive(false);
        }

        bool success = summary.HasApproval && summary.Result.Rating == PickRating.BestChoice;
        MetaProgressManager.Instance?.ReportHireResult(Day.role, success);

        onRoundComplete?.Invoke(summary);

        var tokens = new Dictionary<string, string>
        {
            { "picked", summary.HasApproval ? summary.Result.PickedCV.CVName : "nobody" },
            { "best", bestCV != null ? bestCV.CVName : "nobody" }
        };

        PlayDialogue(success ? Day.feedbackSuccess : Day.feedbackFailure,
                     () => AfterFeedback(success), tokens);
    }

    void AfterFeedback(bool success)
    {
        if (!success && Day.repeatOnFailure)
        {
            GoToDay(_dayIndex, "Try again");
            return;
        }

        if (_dayIndex + 1 < days.Count) GoToDay(_dayIndex + 1);
        else FinishTutorial();
    }

    public void FinishTutorial()
    {
        if (MetaProgressManager.Instance != null) MetaProgressManager.Instance.CompleteTutorial();
        else Debug.LogWarning("[TutorialManager] No MetaProgressManager (are you testing this scene directly?).");
    }

    // Helper: skips straight to the callback if there's no dialogue UI or sequence
    void PlayDialogue(DialogueSequence seq, System.Action onDone, Dictionary<string, string> tokens = null)
    {
        if (DialogueUI.Instance == null || seq == null) { onDone?.Invoke(); return; }
        DialogueUI.Instance.Play(seq, onDone, tokens);
    }
}
