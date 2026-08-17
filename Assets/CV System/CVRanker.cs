using System.Collections.Generic;
using System.Linq;
 

public enum PickRating
{
    BestChoice,      // player picked the #1 ranked CV
    GoodChoice,      // top half, but not #1
    MediocreChoice,  // bottom half, but not last
    WorstChoice      // player picked the lowest ranked CV
}

public struct RoundResult
{
    public GeneratedCV PickedCV;
    public int PickedRank;          // 1 = best in the batch
    public int TotalCandidates;
    public float PickedScore;
    public float BestScoreInBatch;
    public float WorstScoreInBatch;
    public PickRating Rating;

    //How many points behind the best possible pick the player's choice was (0 = picked the best)
    public float ScoreGapFromBest => BestScoreInBatch - PickedScore;
}

public static class CVRanker
{
    public static List<GeneratedCV> Rank(IEnumerable<GeneratedCV> Batch)
    {
        return Batch.OrderByDescending(cv => cv.FinalScore).ToList();
    }

    public static RoundResult Evaluate(IEnumerable<GeneratedCV> Batch, GeneratedCV Picked)
    {
        var Ranked = Rank(Batch);

        int rank = Ranked.IndexOf(Picked) + 1;
        int Total = Ranked.Count;

        var Result = new RoundResult
        {
            PickedCV = Picked,
            PickedRank = rank,
            TotalCandidates = Total,
            PickedScore = Picked.FinalScore,
            BestScoreInBatch = Ranked.First().FinalScore,
            WorstScoreInBatch = Ranked.Last().FinalScore,
        };

        if (rank == 1)
            Result.Rating = PickRating.BestChoice;
        else if (rank == Total)
            Result.Rating = PickRating.WorstChoice;
        else if (rank <= Total / 2f)
            Result.Rating = PickRating.GoodChoice;
        else
            Result.Rating = PickRating.MediocreChoice;

        return Result;
    }
}
