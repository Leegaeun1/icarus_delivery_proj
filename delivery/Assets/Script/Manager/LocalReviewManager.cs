using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LocalReviewManager : MonoBehaviour
{
    [Serializable]
    public class ReviewUI
    {
        public TextMeshProUGUI name;   // 리뷰어 이름 텍스트
        public TextMeshProUGUI dialog; // 리뷰 내용 텍스트
    }
    [Header("UI (이름/대사 슬롯)")]
    public List<ReviewUI> reviews = new(); // 인스펙터에서 4개 연결 권장

    [Header("입력 데이터 (컨텍스트 비면 사용)")]
    public List<string> FinalIngredients = new(); // 예: ["양상추","칠리","피클"]
    public List<string> ExcludeRequest = new(); // 예: ["머스타드"]
    public List<string> IncludeRequest = new(); // 예: ["피클"]

    [Header("리뷰 정책")]
    [Range(1, 8)] public int MaxReviews = 4;
    public int seed = -1; // -1: 비결정, 0 이상: 고정 랜덤

    [Header("로컬 이름 풀(중복 없이 샘플링)")]
    public List<string> ReviewerNamePool = new()
    {
        "알렉스", "보라", "치코", "디노", "에마", "피오", "지니", "해리"
    };
    // === 로컬 문구 풀(딕셔너리). 필요하면 수정/확장하세요 ===
    Dictionary<string, List<string>> positivePool;
    Dictionary<string, List<string>> negativeFlavorPool;

    void Awake()
    {
        BuildLocalPools();
    }

    void Start()
    {
        GenerateAndDisplay();
    }

    void BuildLocalPools()
    {
        // 키는 '재료명'과 정확히 일치해야 매칭됨
        positivePool = new Dictionary<string, List<string>>
        {
            { "양상추", new(){ "양상추가 아삭아삭해서 식감이 좋아요.", "신선한 양상추 덕분에 씹는 맛이 살아나요." } },
            { "칠리",   new(){ "칠리의 매콤함이 전체 맛을 끌어올려요.", "칠리가 적당히 매콤해서 중독적이에요." } },
            { "피클",   new(){ "피클이 상큼해서 느끼함을 잡아줘요.", "피클의 톡 쏘는 맛이 균형을 잡아요." } },
            { "머스타드", new(){ "머스타드 향이 은은해 잘 어울려요." } },
            // 필요하면 더 추가
        };
        negativeFlavorPool = new Dictionary<string, List<string>>
        {
            { "치즈", new(){ "치즈가 빠져서 너무 아쉬웠어요.", "치즈 없으니 풍미가 약해졌어요." } },
            { "피클", new(){ "피클이 빠져서 밸런스가 무너졌어요." } },
            { "머스타드", new(){ "머스타드가 과하게 들어가 다른 맛을 눌렀어요." } },
            // 필요하면 더 추가
        };
    }

    void GenerateAndDisplay()
    {
        // 1) 다른 씬에서 넘어온 값 우선 (GameReviewContext 사용)
        var finalIngs = (GameRevieContext.SelectedIngredients.Count > 0)
            ? new List<string>(GameRevieContext.SelectedIngredients)
            : new List<string>(FinalIngredients);

        var excl = (GameRevieContext.ExcludeRequest.Count > 0)
            ? new List<string>(GameRevieContext.ExcludeRequest)
            : new List<string>(ExcludeRequest);

        var incl = (GameRevieContext.IncludeRequest.Count > 0)
            ? new List<string>(GameRevieContext.IncludeRequest)
            : new List<string>(IncludeRequest);

        // 2) 리뷰 생성
        var lines = ReviewGenerator.Generate(
            finalIngs, excl, incl,
            positivePool, negativeFlavorPool,
            maxReviews: MaxReviews,
            seed: seed
        );

        // 3) 이름 샘플링 (중복 없이)
        var names = SampleNames(ReviewerNamePool, Mathf.Min(MaxReviews, reviews.Count), seed);

        // 4) UI 바인딩
        ApplyToUI(names, lines);
    }

    List<string> SampleNames(List<string> pool, int k, int seed)
    {
        var result = new List<string>();
        if (pool == null || pool.Count == 0 || k <= 0) return result;
        var rng = seed >= 0 ? new System.Random(seed) : new System.Random();
        var arr = pool.ToList();

        // Fisher–Yates 부분 셔플
        int n = Mathf.Min(k, arr.Count);
        for (int i = 0; i < n; i++)
        {
            int j = rng.Next(i, arr.Count);
            (arr[i], arr[j]) = (arr[j], arr[i]);
            result.Add(arr[i]);
        }
        return result;
    }

    void ApplyToUI(List<string> names, List<string> lines)
    {
        int n = Mathf.Min(reviews.Count, Mathf.Min(names.Count, lines.Count));
        for (int i = 0; i < n; i++)
        {
            var ui = reviews[i];
            if (ui?.name) ui.name.text = names[i];
            if (ui?.dialog) ui.dialog.text = lines[i];
        }
        // 남는 슬롯 비우기
        for (int i = n; i < reviews.Count; i++)
        {
            var ui = reviews[i];
            if (ui?.name) ui.name.text = "";
            if (ui?.dialog) ui.dialog.text = "";
        }
    }


}
