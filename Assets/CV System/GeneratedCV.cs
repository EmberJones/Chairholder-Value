using System.Collections.Generic;

public class GeneratedCV        // a single CV that will be generated at runtime for the round
{
    public string CVName;               // name of the person submitting the CV

    public List<CVEntry> Entries = new List<CVEntry>();         // the content of the CV, skills /experience

    public CVFormat FormatProfile;
    
            // the scores given by the CV scorer
    public float ContentScore;      // 0 - 100      How relevant the content is to the role
    public float FormatScore;       // 0 - 100      How professionlly presented it is
    public float FinalScore;        // 0 - 100      weighted combination of the two scores

    public IEnumerable<CVEntry> EntriesInCategory(EntryCategory category)
    {
        foreach (var e in Entries)
        {
            if (e.Category == category) yield return e;
        }
    }
}
