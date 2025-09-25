using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq; // GroupBy, Select, ToDictionary 등
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Networking;
using static ReviewManager;


public class ReviewManager : MonoBehaviour
{
    private const string ADDRESS = "https://docs.google.com/spreadsheets/d/1SUvkrIiBEfRl-J2_887gtT8MJNJgfKvjd-QEr4NglY0";
    private const long SHEET_ID = 1349230387;
    private const string RANGE = "A2:Z";
    System.Random rand = new System.Random();

    // 디버그/확인용 샘플링 결과
    public List<string> SampledNames { get; private set; }
    public List<string> SampledNegatives { get; private set; }
    public List<string> SampledPositives { get; private set; }

    [System.Serializable]
    public class ReviewUI
    {
        public GameObject name;   // TextMeshProUGUI 포함 오브젝트
        public GameObject dialog; // 말풍선(첫 번째 자식에 TextMeshProUGUI 있는 구조)
    }
    public List<ReviewUI> reviews;

    [Serializable]
    public class ReviewRow
    {
        public string name;             // 재료명
        public string negative_review;  // 재료별 부정 문구
        public string positive_review;  // 재료별 긍정 문구
        public int[] stage;             // optional
    }

    [Serializable]
    public class SampledColumns
    {
        public int k;
        public List<string> names;
        public List<string> negatives;
        public List<string> positives;
    }

    public List<ReviewRow> Rows { get; private set; } = new List<ReviewRow>();

    // ===== 리뷰 입력(인스펙터/컨텍스트에서 채움) =====
    [Header("Review Inputs")]
    public List<string> FinalIngredients = new(); // 실제 넣은 재료
    public List<string> ExcludeRequest = new();   // 빼달라 한 재료
    public List<string> IncludeRequest = new();   // 꼭 넣어달라 한 재료
    public int MaxReviews = 4;
    public int seed = -1; // -1: 비결정 랜덤, 0 이상: 고정 시드

    // 시트에서 구성한 재료별 문구 풀
    Dictionary<string, List<string>> posPool, negPool;

    void Start()
    {
        StartCoroutine(LoadData());
    }

    private static string GetTSV(string address, string range, long gid)
        => $"{address}/export?format=tsv&range={UnityWebRequest.EscapeURL(range)}&gid={gid}";

    private IEnumerator LoadData()
    {
        using var www = UnityWebRequest.Get(GetTSV(ADDRESS, RANGE, SHEET_ID));
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Sheets load failed: {www.error}");
            yield break;
        }

        // 1) 파싱
        Rows = ParseTSV(www.downloadHandler.text);
        Debug.Log($"Loaded {Rows.Count} rows");

        // 2) 이름/문구 열 샘플링(리뷰어 이름 용도로만 사용)
        AfterLoad_SampleAndPrint(4);

        // 3) 시트 → 재료별 긍/부정 문구 풀 구성
        BuildPhrasePools();

        // 4) 다른 씬의 선택 재료 가져오기 (컨텍스트 비어 있으면 인스펙터 값 그대로 사용)
        if (FinalIngredients == null || FinalIngredients.Count == 0)
            FinalIngredients = new List<string>(GameRevieContext.SelectedIngredients);
        if (ExcludeRequest == null || ExcludeRequest.Count == 0)
            ExcludeRequest = new List<string>(GameRevieContext.ExcludeRequest);
        if (IncludeRequest == null || IncludeRequest.Count == 0)
            IncludeRequest = new List<string>(GameRevieContext.IncludeRequest);

        // 5) 리뷰 생성
        var lines = ReviewGenerator.Generate(
            FinalIngredients,
            ExcludeRequest,
            IncludeRequest,
            posPool,
            negPool,
            maxReviews: MaxReviews,
            seed: seed
        );

        // 6) 리뷰어 이름과 매칭하여 UI 바인딩
        var names = (SampledNames != null && SampledNames.Count > 0)
            ? SampledNames
            : new List<string> { "리뷰어 1", "리뷰어 2", "리뷰어 3", "리뷰어 4" };

        AssignData(names, lines);

        // (선택) 디버그 출력
        print($"[names   ({names.Count})] => [{string.Join(", ", names)}]");
        print($"[reviews ({lines.Count})] => [{string.Join(" | ", lines)}]");
    }

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

            var row = new ReviewRow
            {
                name = name,
                negative_review = neg,
                positive_review = pos
            };

            if (cols.Length > 3)
            {
                var stages = new List<int>();
                for (int i = 3; i < cols.Length; i++)
                {
                    var s = cols[i];
                    if (string.IsNullOrWhiteSpace(s))
                    {
                        stages.Add(0);
                        continue;
                    }
                    if (int.TryParse(s, out int v)) stages.Add(v);
                    else stages.Add(0);
                }
                if (stages.Count > 0)
                    row.stage = stages.ToArray();
            }

            // 완전 빈 행 스킵하려면 아래 주석 해제
            // if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(neg) && string.IsNullOrEmpty(pos)) continue;

            list.Add(row);
        }
        return list;
    }

    // ---------------------------
    // 랜덤 샘플링 + 저장 유틸리티
    // ---------------------------

    // 열별 랜덤 샘플링 후 리스트 저장 (UI 할당은 하지 않음)
    void AfterLoad_SampleAndPrint(int k, int? seed = null, bool dedup = true)
    {
        // 1) 열별 원본 값 수집(빈 값 제거)
        var names = CollectNonEmpty(r => r.name);
        var negatives = CollectNonEmpty(r => r.negative_review);
        var positives = CollectNonEmpty(r => r.positive_review);

        // 2) 중복 제거 여부 선택
        if (dedup)
        {
            names = Dedup(names);
            negatives = Dedup(negatives);
            positives = Dedup(positives);
        }

        // 3) k를 컬렉션 크기에 맞춰 조정
        int kNames = Mathf.Min(k, names.Count);
        int kNegs = Mathf.Min(k, negatives.Count);
        int kPos = Mathf.Min(k, positives.Count);

        // 4) RNG (시드 고정 가능)
        var rng = seed.HasValue ? new System.Random(seed.Value) : rand;

        // 5) 중복 없이 샘플
        var sampledNames = SampleWithoutReplacement(names, kNames, rng);
        var sampledNegs = SampleWithoutReplacement(negatives, kNegs, rng);
        var sampledPos = SampleWithoutReplacement(positives, kPos, rng);

        // 6) 저장(디버깅/확인용)
        SampledNames = sampledNames;
        SampledNegatives = sampledNegs;
        SampledPositives = sampledPos;

        // UI에는 여기서 바로 꽂지 않고, 최종 리뷰 생성 후 AssignData에서 처리
    }

    List<string> CollectNonEmpty(Func<ReviewRow, string> selector)
    {
        var list = new List<string>();
        foreach (var r in Rows)
        {
            var v = selector(r);
            if (!string.IsNullOrWhiteSpace(v)) list.Add(v.Trim());
        }
        return list;
    }

    List<string> Dedup(List<string> src)
    {
        // 대소문자 무시하고 공백 트림 기준으로 중복 제거
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var outList = new List<string>();
        foreach (var s in src)
        {
            var norm = s.Trim();
            if (set.Add(norm)) outList.Add(norm);
        }
        return outList;
    }

    List<string> SampleWithoutReplacement(List<string> src, int k, System.Random rng)
    {
        var n = src.Count;
        var result = new List<string>(k);
        if (k <= 0 || n == 0) return result;

        // Fisher–Yates 부분 셔플
        var arr = src.ToArray();
        for (int i = 0; i < k; i++)
        {
            int j = rng.Next(i, n);
            (arr[i], arr[j]) = (arr[j], arr[i]);
            result.Add(arr[i]);
        }
        return result;
    }

    // 안전한 UI 바인딩 (세 리스트 중 최소 길이만큼만 표시)
    void AssignData(List<string> nameData, List<string> dialogData)
    {
        int n = Math.Min(reviews.Count, Math.Min(nameData?.Count ?? 0, dialogData?.Count ?? 0));
        for (int i = 0; i < n; i++)
        {
            var ui = reviews[i];
            if (ui?.name != null)
            {
                var nameTmp = ui.name.GetComponent<TextMeshProUGUI>();
                if (nameTmp != null) nameTmp.text = nameData[i];
            }

            if (ui?.dialog != null && ui.dialog.transform.childCount > 0)
            {
                var dialogTmp = ui.dialog.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                if (dialogTmp != null) dialogTmp.text = dialogData[i];
            }
        }

        // 남는 슬롯은 비우기(선택)
        for (int i = n; i < reviews.Count; i++)
        {
            var ui = reviews[i];
            if (ui?.name != null)
            {
                var nameTmp = ui.name.GetComponent<TextMeshProUGUI>();
                if (nameTmp != null) nameTmp.text = "";
            }
            if (ui?.dialog != null && ui.dialog.transform.childCount > 0)
            {
                var dialogTmp = ui.dialog.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                if (dialogTmp != null) dialogTmp.text = "";
            }
        }
    }

    void SaveSamplesToJson(SampledColumns data, string path)
    {
        try
        {
            var json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(path, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveSamplesToJson failed: {e.Message}");
        }
    }

    // ===== 시트 → 풀 구성 =====
    void BuildPhrasePools()
    {
        posPool = Rows
            .Where(r => !string.IsNullOrWhiteSpace(r.name) && !string.IsNullOrWhiteSpace(r.positive_review))
            .GroupBy(r => r.name.Trim())
            .ToDictionary(
                g => g.Key,
                g => g.Select(r => r.positive_review.Trim()).Where(s => s.Length > 0).Distinct().ToList()
            );

        negPool = Rows
            .Where(r => !string.IsNullOrWhiteSpace(r.name) && !string.IsNullOrWhiteSpace(r.negative_review))
            .GroupBy(r => r.name.Trim())
            .ToDictionary(
                g => g.Key,
                g => g.Select(r => r.negative_review.Trim()).Where(s => s.Length > 0).Distinct().ToList()
            );
    }
}
