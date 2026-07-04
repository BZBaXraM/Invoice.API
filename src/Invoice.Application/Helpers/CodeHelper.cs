namespace Invoice.Application.Helpers;

public static class CodeHelper
{
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string GenerateRandom(int length = 6)
    {
        var random = Random.Shared;
        return new string(Enumerable.Repeat(Chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
