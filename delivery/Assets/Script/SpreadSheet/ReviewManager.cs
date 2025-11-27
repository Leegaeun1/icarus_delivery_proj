using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class ReviewManager : MonoBehaviour
{
    private const string ADDRESS = "https://docs.google.com/spreadsheets/d/1SUvkrIiBEfRl-J2_887gtT8MJNJgfKvjd-QEr4NglY0";
    private const long SHEET_ID = 1349230387;
    private const string RANGE = "A2:Z";
    private System.Random rand; // Awake에서 시드 기반으로 초기화

    // --- Public Properties for Debugging ---
    public List<string> SampledNames { get; private set; }
    public List<ReviewRow> Rows { get; private set; } = new();

    // --- Serializable Inner Classes ---
    // REFACTORED: UI 컴포넌트 직접 참조로 변경
    [Serializable]
    public class ReviewUI
    {
        public TextMeshProUGUI nameTMP;
        public TextMeshProUGUI dialogTMP;
    }

    [Serializable]
    public class ReviewRow
    {
        public string name;
        public string negative_review;
        public string positive_review;
        public int[] stage;
    }

    // ADDED: 다중 주문 입력을 위한 헬퍼 클래스
    [Serializable]
    public class StringList
    {
        public List<string> items = new();
    }

    // --- Inspector Fields ---
    [Header("UI Slots")]
    public List<ReviewUI> reviews;

    [Header("Review Inputs (Fallback Data)")]
    // REFACTORED: 다중 주문을 지원하도록 타입 변경
    public List<StringList> FinalIngredients = new();
    public List<StringList> ExcludeRequest = new();
    public List<StringList> IncludeRequest = new();

    [Header("Review Policy")]
    public int MaxReviews = 4;
    public int seed = -1;

    // --- Private Fields ---
    private Dictionary<string, List<string>> posPool, negPool;

    void Awake()
    {
        // 시드 값에 따라 Random 객체 초기화
        rand = (seed >= 0) ? new System.Random(seed) : new System.Random();
    }

    void Start()
    {
        StartCoroutine(LoadDataAndGenerateReviews());
    }

    private static string GetTSV(string address, string range, long gid)
        => $"{address}/export?format=tsv&range={UnityWebRequest.EscapeURL(range)}&gid={gid}";

    private IEnumerator LoadDataAndGenerateReviews()
    {
        using var www = UnityWebRequest.Get(GetTSV(ADDRESS, RANGE, SHEET_ID));
        yield return www.SendWebRequest();

        // ADDED: 코루틴 안정성 강화
        if (!this.isActiveAndEnabled) yield break;

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Sheets load failed: {www.error}");
            yield break;
        }

        // 1) TSV 파싱
        Rows = ParseTSV(www.downloadHandler.text);
        Debug.Log($"Loaded {Rows.Count} rows");

        // 2) 리뷰어 이름 후보 샘플링
        SampledNames = SampleReviewerNames(MaxReviews);

        // 3) 재료별 긍/부정 문구 풀 구성
        BuildPhrasePools();

        // 4) 다른 씬의 데이터 또는 인스펙터 폴백 데이터 가져오기
        // REFACTORED: 다중 주문 로직 및 컨텍스트 우선 로직 적용
        var contextIngredients = GameReviewContext.SelectedIngredients;
        var finalIngredientsLists = (contextIngredients != null && contextIngredients.Count > 0)
            ? contextIngredients
            : FinalIngredients.Select(sl => sl.items).ToList();

        var contextExclude = GameReviewContext.ExcludeRequest;
        var excludeRequestLists = (contextExclude != null && contextExclude.Count > 0)
            ? contextExclude
            : ExcludeRequest.Select(sl => sl.items).ToList();

        var contextInclude = GameReviewContext.IncludeRequest;
        var includeRequestLists = (contextInclude != null && contextInclude.Count > 0)
            ? contextInclude
            : IncludeRequest.Select(sl => sl.items).ToList();

        // 4-1) 데이터를 단일 리스트로 평탄화
        var finalIngsFlat = finalIngredientsLists.SelectMany(list => list).ToList();
        var excludeReqFlat = excludeRequestLists.SelectMany(list => list).ToList();
        var includeReqFlat = includeRequestLists.SelectMany(list => list).ToList();

        // 5) 리뷰 생성
        var lines = ReviewGenerator.Generate(
            finalIngsFlat,
            excludeReqFlat,
            includeReqFlat,
            posPool,
            negPool,
            maxReviews: MaxReviews,
            seed: seed
        );

        // 6) 리뷰어 이름과 매칭하여 UI 바인딩
        var names = (SampledNames != null && SampledNames.Count > 0)
            ? SampledNames
            : Enumerable.Range(1, MaxReviews).Select(i => $"리뷰어 {i}").ToList();

        AssignDataToUI(names, lines);
    }

    // ... ParseTSV, BuildPhrasePools 메서드는 변경 없음 ...
    private static List<ReviewRow> ParseTSV(string tsv)
    {
        var list = new List<ReviewRow>();
        if (string.IsNullOrEmpty(tsv)) return list;
        var lines = tsv.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var raw in lines)
        {
            var line = raw.Trim('\r', ' ', '\t');
            if (string.IsNullOrEmpty(line)) continue;
            var cols = line.Split('\t');
            if (cols.Length == 0) continue;
            string name = cols.Length >= 1 ? cols[0] : string.Empty;
            string neg = cols.Length >= 2 ? cols[1] : string.Empty;
            string pos = cols.Length >= 3 ? cols[2] : string.Empty;
            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(neg) && string.IsNullOrEmpty(pos)) continue;
            var row = new ReviewRow { name = name, negative_review = neg, positive_review = pos };
            list.Add(row);
        }
        return list;
    }

    void BuildPhrasePools()
    {
        posPool = Rows
            .Where(r => !string.IsNullOrWhiteSpace(r.name) && !string.IsNullOrWhiteSpace(r.positive_review))
            .GroupBy(r => r.name.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Select(r => r.positive_review.Trim()).Distinct().ToList());
        negPool = Rows
            .Where(r => !string.IsNullOrWhiteSpace(r.name) && !string.IsNullOrWhiteSpace(r.negative_review))
            .GroupBy(r => r.name.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Select(r => r.negative_review.Trim()).Distinct().ToList());
    }

    // --- Sampling & UI Binding ---
    List<string> SampleReviewerNames(int k)
    {
        var names = Rows.Select(r => r.name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        return SampleWithoutReplacement(names, k, rand);
    }

    List<string> SampleWithoutReplacement(List<string> src, int k, System.Random rng)
    {
        var n = src.Count;
        k = Math.Min(k, n);
        var arr = src.ToArray();
        for (int i = 0; i < k; i++)
        {
            int j = rng.Next(i, n);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
        return arr.Take(k).ToList();
    }

    // REFACTORED: 이름 변경 및 로직 단순화
    void AssignDataToUI(List<string> nameData, List<string> dialogData)
    {
        int n = Math.Min(reviews.Count, Math.Min(nameData?.Count ?? 0, dialogData?.Count ?? 0));
        for (int i = 0; i < n; i++)
        {
            var ui = reviews[i];
            if (ui?.nameTMP != null) ui.nameTMP.text = nameData[i];
            if (ui?.dialogTMP != null) ui.dialogTMP.text = dialogData[i];
        }
        for (int i = n; i < reviews.Count; i++)
        {
            var ui = reviews[i];
            if (ui?.nameTMP != null) ui.nameTMP.text = "";
            if (ui?.dialogTMP != null) ui.dialogTMP.text = "";
        }
    }
}
