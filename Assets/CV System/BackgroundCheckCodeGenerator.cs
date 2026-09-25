using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public static class BackgroundCheckCodeGenerator
{
    private const string SafeChars = "ABCDEFGHJKMNPQRSTUVWXYZ23456789"; // no O, I, L, 0, 1 to prevent font based confusion
    private const int DefaultLength = 8;
    private const int GroupSize = 4; // formats as "XXXX-XXXX"

    public static string GenerateUniqueCode(int Length = DefaultLength)
    {
        string code;
        int attempts = 0;

        do
        {
            code = GenerateRawCode(Length);
            attempts++;
        }
        while (BackgroundCheckRegistry.IsCodeInUse(code) && attempts < 100);

        return code;
    }

    private static string GenerateRawCode(int length)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            if (i > 0 && GroupSize > 0 && i % GroupSize == 0)
                sb.Append('-');
            sb.Append(SafeChars[Random.Range(0, SafeChars.Length)]);
        }
        return sb.ToString();
    }
}
