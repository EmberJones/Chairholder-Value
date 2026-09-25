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

    [Header("Criminal History Relevance")]
    [Tooltip("How disqualifying each CrimeTag is for this role, 0 (irrelevant) to 1 (instant red flag). " +
             "Tags not listed here default to 0 - i.e. that offense type doesn't affect hireability for this role. " +
             "E.g. Theft might be 0.9 for a Cashier role but 0.1 for a Warehouse Labourer role.")]
    public List<CrimeTagWeight> CriminalTagWeights = new List<CrimeTagWeight>();

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

    public float GetCrimeTagWeight(CrimeTag tag)        // looks up how disqualifying a crime is for this role
    {
        if (tag == null) return 0f;
        foreach (var cw in CriminalTagWeights)
        {
            if (cw.Tag == tag) return cw.Weight;
        }
        return 0f;
    }

    // Convenience: the single highest-weighted tag for this role, useful for possible UI hints
    public SkillTag GetTopTag()
    {
        return TagWeights.OrderByDescending(t => t.Weight).Select(t => t.Tag).FirstOrDefault();
    }
}
