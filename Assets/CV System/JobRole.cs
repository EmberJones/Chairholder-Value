using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "NewJobRole", menuName = "CV System/Job Role")]
public class JobRole : ScriptableObject                     // each of the roles the player will try to fill
{
    [Header("Display")]
    public string RoleTitle;

    [TextArea]
    public string RoleDescription;

    [Header("Relevance")]
    [Tooltip("How much each Skill matters for this role. Skills not listed here default to 0 relevance.")]
    public List<TagWeight> TagWeights = new List<TagWeight>();

    [Tooltip("Optional per-category weighting (e.g. Certifications matter more for Nursing). " +
             "Categories not listed default to a weight of 1.")]
    public List<CategoryWeight> CategoryWeights = new List<CategoryWeight>();

    [Header("Baseline Requirements (optional, used by the generator)")]
    [Tooltip("Minimum years of relevant experience a 'good' candidate should have. Purely a generation hint.")]
    public int PreferredMinYearsExperience = 2;

    public float GetTagWeight(SkillTag tag)             // gets the relevance from a tag
    {
        if (tag == null) return 0f;
        foreach (var tw in TagWeights)
        {
            if (tw.Tag == tag) return tw.Weight;
        }
        return 0f;
    }

    public float GetCategoryWeight(EntryCategory category)      // gets the weight from a category
    {
        foreach (var cw in CategoryWeights)
        {
            if (cw.Category == category) return cw.Weight;
        }
        return 1f;
    }

    // Convenience: the single highest-weighted tag for this role, useful for possible UI hints
    public SkillTag GetTopTag()
    {
        return TagWeights.OrderByDescending(t => t.Weight).Select(t => t.Tag).FirstOrDefault();
    }
}
