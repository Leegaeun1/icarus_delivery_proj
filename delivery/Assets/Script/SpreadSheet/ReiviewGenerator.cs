using System;
using System.Collections.Generic;
using System.Linq;

public static class ReviewGenerator
{
    /// <summary>
    /// 최종 리뷰 생성 (부정 우선 → 긍정, 최대 maxReviews개)
    /// </summary>
    /// <param name="finalIngredients">실제 넣은 재료</param>
    /// <param name="excludeRequest">빼달라 한 재료</param>
    /// <param name="includeRequest">꼭 넣어달라 한 재료</param>
    /// <param name="positivePool">재료별 긍정 문구 풀 (시트 기반)</param>
    /// <param name="negativeFlavorPool">재료별 맛 관련 부정 문구 풀 (시트 기반)</param>
    /// <param name="maxReviews">최대 리뷰 개수</param>
    /// <param name="seed">결정론적 랜덤을 위한 seed (음수면 미사용)</param>
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

        // 1) 요청 위반: 빼달라 한 재료가 들어감 → 부정(최상위)
        foreach (var ing in excSet)
        {
            if (finalSet.Contains(ing))
                negatives.Add($"{ing} 빼달라 했는데 들어갔네요. 아쉬워요.");
        }

        // 2) 요청 미이행: 꼭 넣어달라 한 재료가 빠짐 → 부정
        foreach (var ing in incSet)
        {
            if (!finalSet.Contains(ing))
                negatives.Add(FromPoolOrMissingDefault(ing, negativeFlavorPool, rng));
        }

        // 3) 칭찬: 실제 들어간 재료 중 '빼달라 한 재료'는 제외
        foreach (var ing in finalSet)
        {
            if (excSet.Contains(ing)) continue;
            positives.Add(FromPoolOrPositiveDefault(ing, positivePool, rng));
        }

        // 3-1) 요청 100% 반영 시 메타 칭찬 추가
        if (excSet.All(x => !finalSet.Contains(x)) && incSet.All(x => finalSet.Contains(x)))
            positives.Add("요청을 빠짐없이 반영해줘서 정말 만족스러웠어요!");

        // 4) 부정 먼저, 그다음 긍정(섞어서 다양성)
        var result = new List<string>();
        result.AddRange(negatives.Distinct());

        foreach (var p in positives.Distinct().OrderBy(_ => rng.Next()))
        {
            if (result.Count >= maxReviews) break;
            result.Add(p);
        }

        if (result.Count > maxReviews)
            result = result.Take(maxReviews).ToList();

        return result;
    }

    // ===== 풀 조회 + 기본 문구 =====
    static string FromPoolOrPositiveDefault(string ing, IReadOnlyDictionary<string, List<string>> pos, Random rng)
    {
        if (TryPick(pos, ing, rng, out var line)) return line;
        return $"{ing} 덕분에 풍미가 살아났어요.";
    }

    static string FromPoolOrMissingDefault(string ing, IReadOnlyDictionary<string, List<string>> neg, Random rng)
    {
        if (TryPick(neg, ing, rng, out var line)) return line;
        return $"{ing}{JosaIga(ing)} 빠져서 아쉬웠어요.";
    }

    static bool TryPick(IReadOnlyDictionary<string, List<string>> pool, string key, Random rng, out string line)
    {
        line = null;
        if (pool != null && pool.TryGetValue(key, out var list) && list != null && list.Count > 0)
        {
            line = list[rng.Next(list.Count)];
            return true;
        }
        return false;
    }

    // ===== 유틸 =====
    static bool NotBlank(string s) => !string.IsNullOrWhiteSpace(s);
    static string Norm(string s) => s.Trim();

    // 간단한 조사 처리: 받침 있으면 "이", 없으면 "가"
    static string JosaIga(string word)
    {
        if (string.IsNullOrEmpty(word)) return "가";
        char c = word[^1];
        if (c < 0xAC00 || c > 0xD7A3) return "가";
        int code = c - 0xAC00;
        int jong = code % 28;
        return jong == 0 ? "가" : "이";
    }
}
