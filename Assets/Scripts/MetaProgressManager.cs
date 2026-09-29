using UnityEngine;
using UnityEngine.SceneManagement;

public class MetaProgressManager : MonoBehaviour
{
    public static MetaProgressManager Instance { get; private set; }

    [Header("Win/Lose Thresholds")]
    [SerializeField] private int winScore = 5;
    [SerializeField] private int loseScore = -5;

    [Header("Scenes")]
    [SerializeField] private string tutorialSceneName = "TutorialScene";
    [SerializeField] private string mainGameplaySceneName = "DeskTestScene";
    [SerializeField] private string winSceneName = "WinScene";
    [SerializeField] private string loseSceneName = "LoseScene";
    [SerializeField] private string WorseSceneName = "WorsetScene";
    [SerializeField] private string BestSceneName = "BestScene";
    [SerializeField] private string MenuSceneName = "MainMenu";

    [Header("Unlocks")]
    [SerializeField] private JobRole itGuyRole;   
    public bool ITGuyHired { get; private set; }

    public int MetaScore { get; private set; }

    public int CurrentDay { get; private set; } = 1;

    const string TutorialDoneKey = "ChairholderValue_TutorialDone";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartGame()
    {
        bool tutorialDone = PlayerPrefs.GetInt(TutorialDoneKey, 0) == 0;
        SceneManager.LoadScene(tutorialDone ? mainGameplaySceneName : tutorialSceneName);
    }

    public void CompleteTutorial()
    {
        PlayerPrefs.SetInt(TutorialDoneKey, 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(mainGameplaySceneName);
    }

    // GameManager calls this as soon as a round resolves - just updates the score, no scene load yet
    public void ReportHireResult(JobRole roleThisRound, bool wasSuccessfulHire)
    {
        MetaScore += wasSuccessfulHire ? 1 : -1;

        if (wasSuccessfulHire && roleThisRound == itGuyRole)
            ITGuyHired = true;
    }

    // Whoever owns the round-end dialogue/popup calls this once the player has dismissed it
    public void ProceedAfterRound()
    {
        if (MetaScore >= winScore) { PlayTransitionThenLoad(winSceneName, "Game Complete"); return; }
        if (MetaScore <= loseScore) { PlayTransitionThenLoad(loseSceneName, "Game Over"); return; }

        CurrentDay++;
        PlayTransitionThenLoad(mainGameplaySceneName, $"Day {CurrentDay}");
    }

    public void LoadWorstScene() => PlayTransitionThenLoad(WorseSceneName, "Game Over");
    public void LoadMiddleScene() => PlayTransitionThenLoad(mainGameplaySceneName, $"Day {CurrentDay}");
    public void LoadBestScene() => PlayTransitionThenLoad(BestSceneName, "Game Complete");
    public void LoadMenu() => PlayTransitionThenLoad(MenuSceneName, null);

    void PlayTransitionThenLoad(string sceneName, string title, string subtitle = null)
    {
        if (DayTransition.Instance != null)
        {
            DayTransition.Instance.Play(title, subtitle,
                onCovered: () => SceneManager.LoadScene(sceneName));
        }
        else
        {
            Debug.LogWarning("[MetaProgressManager] No DayTransition in this scene - loading without a cover fade.");
            SceneManager.LoadScene(sceneName);
        }
    }
}