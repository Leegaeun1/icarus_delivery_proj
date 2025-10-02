using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

// 인스펙터에서 List<List<string>>을 편집하기 위한 헬퍼 클래스
[Serializable]
public class StringList
{
    public List<string> items = new();
}

public class Reviews : MonoBehaviour
{
    [Header("UI Slots")]
    [SerializeField] private List<TextMeshProUGUI> reviewTextSlots = new(); // 인스펙터에서 연결

    [Header("Fallback Data (테스트용)")]
    [SerializeField] private List<StringList> fallbackIngredients = new();
    [SerializeField] private List<StringList> fallbackExclude = new();
    [SerializeField] private List<StringList> fallbackInclude = new();

    [Header("Random / Debug")]
    [SerializeField] private int maxReviews = 4;
    [SerializeField] private int seed = -1; // -1이면 비결정 랜덤, 0 이상이면 고정

    // 리뷰 데이터 풀과 동의어 사전을 멤버 변수로 선언
    private Dictionary<string, List<string>> positivePool;
    private Dictionary<string, List<string>> negativeFlavorPool;
    private Dictionary<string, string> synonymMap;

    void Awake()
    {
        // 게임 시작 시 데이터 풀을 한 번만 빌드
        BuildReviewPools();
        BuildSynonymMap();
    }

    void Start()
    {
        GenerateAndDisplayReview();
    }

    void GenerateAndDisplayReview()
    {
        // 1) 최종 재료/요청: 컨텍스트값 우선, 없으면 폴백(인스펙터 값)
        var finalIngredientsLists = (GameReviewContext.SelectedIngredients.Count > 0)
            ? GameReviewContext.SelectedIngredients
            : fallbackIngredients.Select(sl => sl.items).ToList();

        var excludeRequestLists = (GameReviewContext.ExcludeRequest.Count > 0)
            ? GameReviewContext.ExcludeRequest
            : fallbackExclude.Select(sl => sl.items).ToList();

        var includeRequestLists = (GameReviewContext.IncludeRequest.Count > 0)
            ? GameReviewContext.IncludeRequest
            : fallbackInclude.Select(sl => sl.items).ToList();

        // 1-1) 모든 목록을 하나의 목록으로 평탄화
        var finalIngredients = finalIngredientsLists.SelectMany(list => list).ToList();
        var excludeRequest = excludeRequestLists.SelectMany(list => list).ToList();
        var includeRequest = includeRequestLists.SelectMany(list => list).ToList();

        // 1-2) 동의어 정규화
        Normalize(finalIngredients);
        Normalize(excludeRequest);
        Normalize(includeRequest);

        // 2) 리뷰 생성
        var lines = ReviewGenerator.Generate(
            finalIngredients,
            excludeRequest,
            includeRequest,
            positivePool,
            negativeFlavorPool,
            maxReviews: maxReviews,
            seed: seed
        );

        // 3) UI 표시
        ApplyToUI(lines);
    }

    // ====== 초기화 ======
    void BuildReviewPools()
    {
        positivePool = new Dictionary<string, List<string>> {
            { "토마토", new List<string> { "토마토가 신선해서 좋았어요!", "토마토의 상큼함이 최고예요." } },
            { "피클", new List<string> { "피클이 아삭해서 식감이 살아요." } },
            { "상추", new List<string> { "상추가 아삭아삭해서 식감이 좋아요." } },
        };

        negativeFlavorPool = new Dictionary<string, List<string>> {
            { "치즈", new List<string> { "치즈가 빠져서 너무 아쉬워요." } },
        };
    }

    void BuildSynonymMap()
    {
        synonymMap = new Dictionary<string, string>
        {
            { "양상추", "상추" },
            { "머스탓드", "머스타드" },
        };
    }

    // ====== 유틸 ======
    void Normalize(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var original = list[i]?.Trim();
            if (string.IsNullOrEmpty(original)) continue;

            if (synonymMap.TryGetValue(original, out string standardized))
            {
                list[i] = standardized;
            }
        }
    }

    void ApplyToUI(List<string> lines)
    {
        int n = Mathf.Min(reviewTextSlots.Count, lines.Count);
        for (int i = 0; i < n; i++)
            if (reviewTextSlots[i] != null) reviewTextSlots[i].text = lines[i];

        for (int i = n; i < reviewTextSlots.Count; i++)
            if (reviewTextSlots[i] != null) reviewTextSlots[i].text = "";
    }
}
