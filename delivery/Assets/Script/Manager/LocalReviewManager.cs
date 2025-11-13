using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocalReviewManager : MonoBehaviour
{
    // --- 데이터 구조 정의 ---

    [System.Serializable]
    public class StringList
    {
        public List<string> ingredients = new();
    }

    [System.Serializable]
    public class ReviewUI
    {
        public TextMeshProUGUI name;
        public TextMeshProUGUI dialog;
    }

    // ADDED: 단일 주문 정보를 묶어서 관리하기 위한 클래스
    public class Order
    {
        public List<string> SelectedIngredients { get; set; } = new();
        public List<string> ExcludeRequest { get; set; } = new();
        public List<string> IncludeRequest { get; set; } = new();
    }

    // --- 인스펙터 변수 ---
    [Header("별 개수")]
    public float star_cnt = 2;
    public int max_star = 4;
    public GameObject star_prefab;
    public GameObject parent;

    [Header("UI (이름/대사 슬롯)")]
    public List<ReviewUI> reviews = new();

    [Header("입력 데이터 (주문 단위로 매칭됨)")]
    public List<StringList> FinalIngredients = new();
    public List<StringList> ExcludeRequest = new();
    public List<StringList> IncludeRequest = new();

    [Header("리뷰 정책")]
    [Range(1, 8)] public int MaxReviews = 4;
    public int seed = -1;

    [Header("이름")]
    public List<string> ReviewerNamePool = new()
    {
        "김솰라", "모구리", "치코", "디노", "쿠모", "피오", "비콜로", "지르론"
    };

    // --- 내부 데이터 풀 ---
    private Dictionary<string, List<string>> positivePool;
    private Dictionary<string, List<string>> negativeFlavorPool;
    private System.Random rng;

    // --- MonoBehaviour 라이프사이클 ---

    void Awake()
    {
        BuildLocalPools();
        rng = seed >= 0 ? new System.Random(seed) : new System.Random();
    }

    void Start()
    {
        GenerateAndDisplay();
        StartCoroutine(review_star());
    }

    // --- 핵심 로직 ---

    // REFACTORED: 주문 단위 처리를 위해 로직 전체 변경
    void GenerateAndDisplay()
    {
        // 1. 입력 데이터를 '주문' 단위로 묶기
        List<Order> orders = new List<Order>();
        // GameReviewContext 처리 (선택적) - 지금은 인스펙터 값만 사용하도록 단순화
        // 실제로는 이 부분도 GameReviewContext에서 List<Order>를 구성하도록 수정.
        for (int i = 0; i < FinalIngredients.Count; i++)
        {
            var order = new Order
            {
                SelectedIngredients = FinalIngredients[i].ingredients
            };
            if (i < ExcludeRequest.Count)
                order.ExcludeRequest = ExcludeRequest[i].ingredients;
            if (i < IncludeRequest.Count)
                order.IncludeRequest = IncludeRequest[i].ingredients;

            orders.Add(order);
        }

        // 2. 각 주문을 개별적으로 평가하여 모든 리뷰 문장 수집
        List<string> allGeneratedLines = new List<string>();
        foreach (var order in orders)
        {
            // 각 주문에 대해 리뷰를 1개 생성하여 추가
            string reviewLine = GenerateReviewForOrder(order);
            allGeneratedLines.Add(reviewLine);
        }

        // 3. 최종 리뷰 목록을 정책에 맞게 조정 (랜덤 셔플, 개수 제한 등)
        // Take는 처음 n개의 원소만 꺼내서 만든다.
        var finalLines = allGeneratedLines.OrderBy(x => rng.Next()).Take(MaxReviews).ToList();

        // 4. 이름 샘플링 및 UI 바인딩
        var names = SampleNames(ReviewerNamePool, finalLines.Count, rng);
        ApplyToUI(names, finalLines);
    }

    // 단일 주문을 평가하고 리뷰 문장 1개를 생성하는 메서드
    private string GenerateReviewForOrder(Order order)
    {
        // --- 부정 리뷰 생성 ---

        // 1. "빼달라는 재료"가 들어갔는지 검사
        foreach (var excluded in order.ExcludeRequest)
        {
            if (order.SelectedIngredients.Contains(excluded))
            { 
                return $"{excluded} 빼달라고 했는데 들어있네요..";
            }
        }

        // 2. "넣어달라는 재료"가 빠졌는지 검사
        foreach (var included in order.IncludeRequest)
        {
            if (!order.SelectedIngredients.Contains(included))
            {
                return $"{included} 꼭 넣어달라고 했는데 빠졌어요.";
            }
        }

        // --- 긍정 리뷰 생성 ---

        // 성공했다면, 만들어진 재료 중 하나에 대해 긍정 리뷰 생성
        if (order.SelectedIngredients.Count > 0)
        {
            // 포함된 재료 중 랜덤으로 하나 선택
            string targetIngredient = order.SelectedIngredients[rng.Next(order.SelectedIngredients.Count)];

            // 해당 재료에 대한 긍정 리뷰 문구가 있다면 사용, 없다면 기본 문구 사용. 키 있으면 out으로 내보냄 
            if (positivePool.TryGetValue(targetIngredient, out List<string> lines) && lines.Count > 0)
            {
                return lines[rng.Next(lines.Count)];
            }
        }

        // 모든 조건을 통과했지만 마땅한 리뷰가 없을 경우 기본 문구
        return "음... 그냥 평범한 맛이네요.";
    }

    // --- 기존 유틸리티 메서드 ---
    void BuildLocalPools()
    {
        positivePool = new Dictionary<string, List<string>>
        {
            { "양상추", new(){ "양상추가 아삭아삭해서 식감이 좋아요.", "신선한 양상추 덕분에 씹는 맛이 살아나요." } },
            { "칠리",   new(){ "칠리의 매콤함이 전체 맛을 끌어올려요.", "칠리가 적당히 매콤해서 중독적이에요." } },
            { "피클",   new(){ "피클이 상큼해서 느끼함을 잡아줘요.", "피클의 톡 쏘는 맛이 균형을 잡아요." } },
            { "머스타드", new(){ "머스타드 향이 은은해 잘 어울려요." } },
        };
        negativeFlavorPool = new Dictionary<string, List<string>>
        {
            { "피클", new(){ "피클이 들어가서 맛이 이상해요." } }, 
            { "머스타드", new(){ "머스타드가 빠져서 아쉬워요." } },
            { "칠리", new(){ "칠리가 빠져서 아쉬워요." } },
            { "양상추", new(){ "양상추가 빠져서 아쉬워요." } },
        };
    }

    List<string> SampleNames(List<string> pool, int k, System.Random random)
    {
        if (pool == null || pool.Count == 0 || k <= 0) 
            return new List<string>();
        // Fisher–Yates 셔플 사용
        return pool.OrderBy(x => random.Next()).Take(k).ToList();
    }

    void ApplyToUI(List<string> names, List<string> lines)
    {
        int n = Mathf.Min(reviews.Count, Mathf.Min(names.Count, lines.Count)); // 작은 것 기준으로
        for (int i = 0; i < n; i++)
        {
            var ui = reviews[i];
            if (ui?.name) ui.name.text = names[i]; // null이 아니면 저장.
            if (ui?.dialog) ui.dialog.text = lines[i]; // null이 아니면 저장.
        }
        for (int i = n; i < reviews.Count; i++) // 나머지 칸이 필요없을 때 비워줌
        {
            var ui = reviews[i];
            if (ui?.name) ui.name.text = "";
            if (ui?.dialog) ui.dialog.text = "";
        }
    }
    IEnumerator review_star()
    {
        for (int i = 0; i < max_star; i++)
        {
            bool isfull = i < star_cnt; // 채워져야하는 별이면 true
            // 별 생성
            GameObject star = Instantiate(star_prefab, parent.transform);
            
            if (isfull){ // 채워져야한다면 노란색으로 변경
                var ishalf = 0f;
                var speed = 0.5f;
                if (i == star_cnt - 0.5) // 반개일 때
                {
                    ishalf = 0.5f;
                    speed = 0.25f;
                }
                // 회전할때 필요함
                //star.GetComponent<Image>().color = Color.yellow; 
                Slider slider = star.transform.GetChild(0).GetComponent<Slider>();
                StartCoroutine(FillSlider(slider, 0f, 1f- ishalf, speed)); // 0.5초 동안 부드럽게 채우기
            }

                RectTransform rect = star.GetComponent<RectTransform>();
            if (rect != null)
                rect.anchoredPosition = new Vector2(0f, 0f); // 가운데로 anchor 바꾸기 

            // 별 회전 애니메이션 시작
            //StartCoroutine(RotateStar(star.transform, Quaternion.Euler(0f, 180f, 0f), 0.5f));

            yield return new WaitForSeconds(0.5f); // 별 간 생성 간격
        }
    }

    // 회전하는 코드
    IEnumerator RotateStar(Transform target, Quaternion targetRotation, float duration)
    {
        // 시작점 기록
        Quaternion startRotation = target.rotation;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // starRotation부터 targetRotation까지 t시간 동안 부드럽게 회전하도록! 
            target.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

    }
    // 슬라이더로 채우는 코드
    IEnumerator FillSlider(Slider slider, float startValue, float endValue, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            slider.value = Mathf.Lerp(startValue, endValue, t);
            yield return null;
        }
        slider.value = endValue;
    }

}