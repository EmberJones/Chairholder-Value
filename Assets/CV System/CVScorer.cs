using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CVScorer           // what we will use to score a  CV against a specific job role, we then store that on the CV itself for OOP purposes
{
    public static float Score(GeneratedCV CV, JobRole Role, float ContentWeight = 0.6f, float FormatWeight = 0.4f)
    {
        CV.ContentScore = ComputeContentScore(CV, Role);
        CV.FormatScore = CV.FormatProfile != null ? CV.FormatProfile.FormatScore : 50f;   // default to 50 if not set
        CV.FinalScore = Mathf.Clamp(CV.ContentScore * ContentWeight + CV.FormatScore * FormatWeight, 0f, 100f);
        return CV.FinalScore;
    }

    public static void ScoreAll(IEnumerable<GeneratedCV> cvs, JobRole role, float contentWeight = 0.7f, float formatWeight = 0.3f)
    {
        foreach (var cv in cvs)
            Score(cv, role, contentWeight, formatWeight);
    }

    public static float ComputeContentScore(GeneratedCV CV, JobRole Role)
    {
        if (CV.Entries == null || CV.Entries.Count == 0) return 0;

        float Total = 0f;

        foreach (var Entry in CV.Entries)
        {
            Total += ComputeEntryContribution(Entry, Role);
        }

        float avg = Total / CV.Entries.Count;       // this will be between -1 and 1

        float Rescaled = (Mathf.Clamp(avg, -1f, 1f) + 1f) * 50f;        // so we rescale it to between 0 and 100
        return Rescaled;
    }

    public static float ComputeEntryContribution(CVEntry Entry, JobRole Role)
    {
        if (Entry == null) return 0;

        float TagRelevance = ComputeTagRelevance(Entry, Role);
        float CategoryWeight = Role.GetCategoryWeight(Entry.Category);
        return Entry.QualityScore * TagRelevance * CategoryWeight;
    }

    public static float ComputeTagRelevance(CVEntry Entry, JobRole Role)
    {
        if (Entry.Tags == null || Entry.Tags.Count == 0) return 0f;

        return Entry.Tags.Average(tag => Role.GetTagWeight(tag));
    }
}
