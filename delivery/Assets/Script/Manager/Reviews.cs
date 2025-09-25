using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Reviews : MonoBehaviour
{
    [Header("UI Slots (최대 4개 권장)")]
    [SerializeField] private List<TextMeshProUGUI> reviewTextSlots = new(); // 인스펙터에서 연결

    [Header("Random / Debug")]
    [SerializeField] private int maxReviews = 4;
    [SerializeField] private int seed = -1; // -1이면 비결정 랜덤, 0 이상이면 고정

    void Start()
    {
        GenerateAndDisplayReview();
    }

    void GenerateAndDisplayReview()
    {
        // 1) 최종 재료/요청: 컨텍스트값 우선, 없으면 폴백(hard-coded)
        var finalIngredients = (GameRevieContext.SelectedIngredients.Count > 0)
            ? new List<string>(GameRevieContext.SelectedIngredients)
            : new List<string> { "상추", "토마토", "피클" };

        var excludeRequest = (GameRevieContext.ExcludeRequest.Count > 0)
            ? new List<string>(GameRevieContext.ExcludeRequest)
            : new List<string> { /* 예: "오이" */ };

        var includeRequest = (GameRevieContext.IncludeRequest.Count > 0)
            ? new List<string>(GameRevieContext.IncludeRequest)
            : new List<string> { /* 예: "피클", "치즈" */ };

        // 1-1) (선택) 동의어 정규화: 시트/풀과 키가 다를 때 맞춰줌
        Normalize(finalIngredients);
        Normalize(excludeRequest);
        Normalize(includeRequest);

        // 2) 리뷰 문구 풀(Pool) 정의
        //    - 실제 프로젝트에선 Google Sheet/JSON/ScriptableObject로 관리하는 걸 권장
        var positivePool = new Dictionary<string, List<string>> {
            { "토마토", new List<string> { "토마토가 신선해서 좋았어요!", "토마토의 상큼함이 최고예요." } },
            { "피클", new List<string> { "피클이 아삭해서 식감이 살아요." } },
            { "상추", new List<string> { "상추가 아삭아삭해서 식감이 좋아요." } },
            // { "양상추", new List<string> { ... } }, // 시트 키가 '양상추'라면 여기/정규화에서 맞춰줘
        };

        var negativeFlavorPool = new Dictionary<string, List<string>> {
            { "치즈", new List<string> { "치즈가 빠져서 너무 아쉬워요." } },
            // 부족/미이행/맛 관련 부정 문구들을 재료별로 추가
        };

        // 3) 리뷰 생성
        var lines = ReviewGenerator.Generate(
            finalIngredients,
            excludeRequest,
            includeRequest,
            positivePool,
            negativeFlavorPool,
            maxReviews: maxReviews,
            seed: seed
        );

        // 4) UI 표시
        ApplyToUI(lines);
    }

    // ====== 유틸 ======
    // 동의어 → 기준키 매핑(예: "상추" => "양상추")이 필요하면 여기서 처리
    void Normalize(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var s = list[i]?.Trim();
            if (string.IsNullOrEmpty(s)) continue;

            // 예시 매핑
            if (s == "상추") s = "상추";       // 시트/풀 키가 "상추"면 그대로
            if (s == "양상추") s = "상추";    // 시트/풀 키를 "상추"로 통일하고 싶다면 이렇게

            // 필요하면 더 추가: "머스타드" <-> "머스탓드" 같은 오타 케이스
            list[i] = s;
        }
    }

    void ApplyToUI(List<string> lines)
    {
        // 슬롯 부족/과다에 안전하게 동작
        int n = Mathf.Min(reviewTextSlots.Count, lines.Count);
        for (int i = 0; i < n; i++)
            if (reviewTextSlots[i] != null) reviewTextSlots[i].text = lines[i];

        // 남는 슬롯은 비우기
        for (int i = n; i < reviewTextSlots.Count; i++)
            if (reviewTextSlots[i] != null) reviewTextSlots[i].text = "";
    }
}
