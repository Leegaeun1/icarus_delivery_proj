using System;
using System.Collections.Generic;
using System.Linq;

public static class ReviewGenerator
{
    public static List<string> Generate(
        IEnumerable<string> finalIngredients,
        IEnumerable<string> excludeRequest,
        IEnumerable<string> includeRequest,
        IReadOnlyDictionary<string, List<string>> positivePool,
        IReadOnlyDictionary<string, List<string>> negativeFlavorPool,
        int maxReviews = 4,
        int seed = -1)
    {
        var rng = seed >= 0 ? new Random(seed) : new Random();

        var finalSet = new HashSet<string>(finalIngredients.Where(NotBlank).Select(Norm));
        var excSet = new HashSet<string>(excludeRequest.Where(NotBlank).Select(Norm));
        var incSet = new HashSet<string>(includeRequest.Where(NotBlank).Select(Norm));

        var negatives = new List<string>();
        var positives = new List<string>();

        foreach (var ing in excSet)
            if (finalSet.Contains(ing))
                negatives.Add($"{ing} 빼달라 했는데 들어갔네요. 아쉬워요.");

        foreach (var ing in incSet)
            if (!finalSet.Contains(ing))
                negatives.Add(FromPoolOrMissingDefault(ing, negativeFlavorPool, rng));

        foreach (var ing in finalSet)
        {
            if (excSet.Contains(ing)) continue;
            positives.Add(FromPoolOrPositiveDefault(ing, positivePool, rng));
        }

        bool hasAnyRequests = excSet.Count > 0 || incSet.Count > 0;
        bool allExcludedRespected = excSet.All(x => !finalSet.Contains(x));
        bool allIncludesMet = incSet.All(x => finalSet.Contains(x));
        if (hasAnyRequests && allExcludedRespected && allIncludesMet)
            positives.Add("요청을 빠짐없이 반영해줘서 정말 만족스러웠어요!");

        var result = new List<string>();
        result.AddRange(negatives.Distinct());
        foreach (var p in positives.Distinct().OrderBy(_ => rng.Next()))
        {
            if (result.Count >= maxReviews) break;
            result.Add(p);
        }
        if (result.Count > maxReviews) result = result.Take(maxReviews).ToList();
        return result;
    }

    static string FromPoolOrPositiveDefault(string ing, IReadOnlyDictionary<string, List<string>> pos, Random rng)
        => TryPick(pos, ing, rng, out var line) ? line : $"{ing} 덕분에 풍미가 살아났어요.";

    static string FromPoolOrMissingDefault(string ing, IReadOnlyDictionary<string, List<string>> neg, Random rng)
        => TryPick(neg, ing, rng, out var line) ? line : $"{ing}{JosaIga(ing)} 빠져서 아쉬웠어요.";

    static bool TryPick(IReadOnlyDictionary<string, List<string>> pool, string key, Random rng, out string line)
    {
        line = null;
        if (pool != null && pool.TryGetValue(key, out var list) && list != null && list.Count > 0)
        { line = list[rng.Next(list.Count)]; return true; }
        return false;
    }

    static bool NotBlank(string s) => !string.IsNullOrWhiteSpace(s);
    static string Norm(string s) => s.Trim();

    static string JosaIga(string word)
    {
        if (string.IsNullOrEmpty(word)) return "가";
        char c = word[^1];
        if (c < 0xAC00 || c > 0xD7A3) return "가";
        return ((c - 0xAC00) % 28) == 0 ? "가" : "이";
    }
}
