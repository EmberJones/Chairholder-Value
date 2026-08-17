using UnityEngine;
using System;

[Serializable]
public struct TagWeight         // how relevnant a skill is to a job role: 0 irrelevant, 1 Core Requirement, -0.X means it is a detriment
{
    public SkillTag Tag;
    [Range(-1f, 1f)]
    public float Weight;
}


[Serializable]
public struct CategoryWeight        // how relevant the entry category is to a job role, i.e certifications are important for nurses, but maybe less for janitors...
{
    public EntryCategory Category;
    [Range(0f, 2f)]
    public float Weight;
}