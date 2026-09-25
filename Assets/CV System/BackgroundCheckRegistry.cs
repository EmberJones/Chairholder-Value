using System.Collections.Generic;
using UnityEngine;

public struct BackgroundCheckResult
{
    public string CandidateName;
    public bool HasRecord;
    public List<CriminalRecordEntryDefinition> Offenses;
}

public static class BackgroundCheckRegistry
{
    private static readonly Dictionary<string, BackgroundCheckResult> _entries = new Dictionary<string, BackgroundCheckResult>();
    
    public static void Register(string Code, BackgroundCheckResult result)
    {
        _entries[Code] = result;
    }

    public static bool IsCodeInUse(string Code) => _entries.ContainsKey(Code);

    public static bool TryLookup(string Code, out BackgroundCheckResult result)
    {
        return _entries.TryGetValue(NormalizeCode(Code), out result);
    }

    public static void ClearAll()
    {
        _entries.Clear();
    }

    private static string NormalizeCode(string code)
    {
        return string.IsNullOrEmpty(code) ? code : code.Trim().ToUpperInvariant();
    }
}
