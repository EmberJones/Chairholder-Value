using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// just an example class I've made to test the Whole CV system
public class TestRoundController : MonoBehaviour
{
    public CVGenerator Generator;
    
    [Header("Round Config")]
    public JobRole CurrentRole;
    [Range(0f, 1f)] public float ContentWeight = 0.6f;
    [Range(0f, 1f)] public float FormatWeight = 0.4f;

    [SerializeField] private List<GeneratedCV> CurrentBatch;

    public List<GeneratedCV> StartRound(JobRole Role)
    {
        CurrentRole = Role;
        CurrentBatch = Generator.GenerateBatch(Role);
        CVScorer.ScoreAll(CurrentBatch, Role, ContentWeight, FormatWeight);
        LogBatch(CurrentBatch);
        return CurrentBatch;
    }

    public RoundResult SubmitPlayerChoice(GeneratedCV PickedCV)
    {
        var Result = CVRanker.Evaluate(CurrentBatch, PickedCV);
        LogResult(Result);
        return Result;
    }

    private void LogResult(RoundResult result)
    {
        string verdict = "";
        if (result.Rating == PickRating.BestChoice) verdict = "That was the strongest candidate in the pool!";
        else if (result.Rating == PickRating.GoodChoice) verdict = "A solid pick - not the very best, but a good hire.";
        else if (result.Rating == PickRating.MediocreChoice) verdict = "A weaker pick - there were better candidates available.";
        else if (result.Rating == PickRating.WorstChoice) verdict = "That was actually the weakest candidate in the pool.";

        Debug.Log(
            $"[CV Round] Picked '{result.PickedCV.CVName}' " +
            $"(rank {result.PickedRank}/{result.TotalCandidates}, score {result.PickedScore:F1}). " +
            $"Best available: {result.BestScoreInBatch:F1} (gap: {result.ScoreGapFromBest:F1}). {verdict}"
        );
    }

    private void LogBatch(List<GeneratedCV> batch)
    {
        foreach (GeneratedCV p in batch)
        {
            Debug.Log(p.CVName + ":" + "\nContent Score: " + p.ContentScore +
                                       "\nFormatScore: " + p.FormatScore +
                                       "\nFINAL SCORE: " + p.FinalScore +
                                       "\nEntry 1: " + p.Entries.First().name);
        }
    }
}
