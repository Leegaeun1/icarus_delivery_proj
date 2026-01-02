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

    // 주문 정보 클래스
    public class Order
    {
        public List<string> SelectedIngredients { get; set; } = new();
        public List<string> ExcludeRequest { get; set; } = new();
        public List<string> IncludeRequest { get; set; } = new();

        // 주문의 성공 여부와 실패 사유를 저장할 변수
        public bool isSuccess;
        public string failReason;
    }

    // --- 인스펙터 변수 ---
    [Header("별 개수")]
    public float star_cnt = 2;
    public int max_star = 4;
    public GameObject star_prefab;
    public GameObject star_parent;
    List<GameObject> createdStars = new List<GameObject>();

    [Header("UI (이름/대사 슬롯)")]

    [Tooltip("일반 리뷰 프리팹")]
    public GameObject review_prefab;
    [Tooltip("반전된 리뷰 프리팹")]
    public GameObject review_flip_prefab;
    [Tooltip("리뷰들이 생성될 부모 오브젝트 (보통 Layout Group이 있는 Content)")]
    public GameObject review_parent;

    // 생성된 리뷰 오브젝트들을 추적 관리하기 위한 리스트
    private List<GameObject> spawnedReviewObjects = new List<GameObject>();


    [Header("입력 데이터 (주문 단위로 매칭됨)")]
    public List<StringList> FinalIngredients = new();
    public List<StringList> ExcludeRequest = new();
    public List<StringList> IncludeRequest = new();

    [Header("리뷰 정책")]
    public int MaxDisplayCount = 4; // 최대 표시 개수
    public int seed = -1;

    [Header("이름")]
    public List<string> ReviewerNamePool = new()
    {
        "김솰라", "모구리", "치코", "디노", "쿠모", "피오", "비콜로", "지르론"
    };

    private Dictionary<string, List<string>> positivePool;
    private System.Random rng;

    void Awake()
    {
        BuildLocalPools();
        rng = seed >= 0 ? new System.Random(seed) : new System.Random();
    }

    void Start()
    {
        // 리뷰 생성 및 별점 연출 시작
        StartCoroutine(GenerateReviewsSequence());
    }

    IEnumerator GenerateReviewsSequence()
    {
        // 0. 기존에 생성된 리뷰 오브젝트가 있다면 모두 삭제
        foreach (var obj in spawnedReviewObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedReviewObjects.Clear();

        // 1. 입력 데이터를 '주문' 객체로 변환하고 성공/실패 여부 미리 판별
        List<Order> allOrders = new List<Order>();
        List<Order> successOrders = new List<Order>();
        List<Order> failOrders = new List<Order>();

        for (int i = 0; i < FinalIngredients.Count; i++)
        {
            var order = new Order
            {
                SelectedIngredients = FinalIngredients[i].ingredients
            };
            if (i < ExcludeRequest.Count) order.ExcludeRequest = ExcludeRequest[i].ingredients;
            if (i < IncludeRequest.Count) order.IncludeRequest = IncludeRequest[i].ingredients;

            // 성공/실패 여부 판별
            CheckOrderResult(order);

            allOrders.Add(order);
            if (order.isSuccess) successOrders.Add(order);
            else failOrders.Add(order);
        }

        // 2. 개수 산정 로직
        int totalReviewCount = Mathf.Min(MaxDisplayCount, allOrders.Count);
        int negativeCount = Mathf.Min(totalReviewCount, failOrders.Count);
        int positiveCount = totalReviewCount - negativeCount;

        // 3. 실제 표시할 주문 목록 구성
        List<Order> finalDisplayOrders = new List<Order>();
        var selectedFails = failOrders.OrderBy(x => rng.Next()).Take(negativeCount).ToList();
        var selectedSuccesses = successOrders.OrderBy(x => rng.Next()).Take(positiveCount).ToList();

        finalDisplayOrders.AddRange(selectedFails);
        finalDisplayOrders.AddRange(selectedSuccesses);

        // 순서를 섞음
        finalDisplayOrders = finalDisplayOrders.OrderBy(x => rng.Next()).ToList();

        // 4. 이름 배정
        var names = SampleNames(ReviewerNamePool, finalDisplayOrders.Count, rng);

        // 5. 하나씩 리뷰 출력 
        for (int i = 0; i < finalDisplayOrders.Count; i++)
        {
            // i가 짝수면 normal prefab, 홀수면 flip prefab
            GameObject prefabToUse = (i % 2 == 0) ? review_prefab : review_flip_prefab;

            if (prefabToUse == null || review_parent == null)
            {
                Debug.LogError("Review Prefab 또는 Review Parent가 인스펙터에 할당되지 않았습니다.");
                yield break;
            }

            // 프리팹 생성 및 부모 설정
            GameObject newReviewGO = Instantiate(prefabToUse, review_parent.transform);
            spawnedReviewObjects.Add(newReviewGO); // 관리 리스트에 추가

            TextMeshProUGUI[] texts = newReviewGO.GetComponentsInChildren<TextMeshProUGUI>();
            TextMeshProUGUI nameUI = null;
            TextMeshProUGUI dialogUI = null;

            if (texts.Length >= 2)
            {
                nameUI = texts[0];   // 첫 번째로 발견된 TMP
                dialogUI = texts[1]; // 두 번째로 발견된 TMP
            }
            else
            {
                Debug.LogWarning($"리뷰 프리팹({newReviewGO.name}) 하위에 TextMeshProUGUI 컴포넌트가 2개 이상 필요합니다.");
            }

            // --- 텍스트 내용 결정 (기존 로직) ---
            Order currentOrder = finalDisplayOrders[i];
            string reviewText = "";

            if (!currentOrder.isSuccess)
            {
                reviewText = currentOrder.failReason;
            }
            else
            {
                reviewText = GeneratePositiveReview(currentOrder);
            }

            // UI 적용
            if (nameUI != null) nameUI.text = names[i];
            if (dialogUI != null) dialogUI.text = reviewText;

            // 리뷰가 하나 뜰 때마다 잠시 대기
            yield return new WaitForSeconds(0.5f);
        }

        // 6. 리뷰 작성이 끝난 후 별점 애니메이션 시작
        StartCoroutine(review_star());
    }

    void CheckOrderResult(Order order)
    {
        // 1. "빼달라는 재료"가 들어갔는지 검사 (실패)
        foreach (var excluded in order.ExcludeRequest)
        {
            if (order.SelectedIngredients.Contains(excluded))
            {
                order.isSuccess = false;
                order.failReason = $"{excluded} 빼달라고 했는데 들어있네요..";
                return;
            }
        }

        // 2. "넣어달라는 재료"가 빠졌는지 검사 (실패)
        foreach (var included in order.IncludeRequest)
        {
            if (!order.SelectedIngredients.Contains(included))
            {
                order.isSuccess = false;
                order.failReason = $"{included} 꼭 넣어달라고 했는데 빠졌어요.";
                return;
            }
        }

        // 통과 (성공)
        order.isSuccess = true;
        order.failReason = "";
    }

    string GeneratePositiveReview(Order order)
    {
        if (order.SelectedIngredients.Count > 0)
        {
            string targetIngredient = order.SelectedIngredients[rng.Next(order.SelectedIngredients.Count)];

            if (positivePool.TryGetValue(targetIngredient, out List<string> lines) && lines.Count > 0)
            {
                return lines[rng.Next(lines.Count)];
            }
        }
        return "음... 그냥 평범한 맛이네요.";
    }


    void BuildLocalPools()
    {
        positivePool = new Dictionary<string, List<string>>
        {
            { "양상추", new(){ "양상추가 아삭아삭해서 식감이 좋아요.", "신선한 양상추 덕분에 씹는 맛이 살아나요." } },
            { "칠리",   new(){ "칠리의 매콤함이 전체 맛을 끌어올려요.", "칠리가 적당히 매콤해서 중독적이에요." } },
            { "피클",   new(){ "피클이 상큼해서 느끼함을 잡아줘요.", "피클의 톡 쏘는 맛이 균형을 잡아요." } },
            { "머스타드", new(){ "머스타드 향이 은은해 잘 어울려요." } },
        };
    }

    List<string> SampleNames(List<string> pool, int k, System.Random random)
    {
        if (pool == null || pool.Count == 0 || k <= 0)
            return new List<string>();
        return pool.OrderBy(x => random.Next()).Take(k).ToList();
    }

    IEnumerator review_star()
    {
        // 기존 별 삭제 및 리스트 초기화
        foreach (var star in createdStars)
        {
            if (star != null) Destroy(star);
        }
        createdStars.Clear();

        // star_parent가 할당되지 않았으면 에러 방지
        if (star_parent == null) yield break;

        for (int i = 0; i < max_star; i++)
        {
            GameObject star = Instantiate(star_prefab, star_parent.transform);
            RectTransform rect = star.GetComponent<RectTransform>();

            createdStars.Add(star);
        }

        for (int i = 0; i < max_star; i++)
        {
            bool isfull = i < star_cnt;

            if (isfull)
            {
                var ishalf = 0f;
                var speed = 0.8f;
                if (star_cnt % i != 0 && i == (int)star_cnt)
                {
                    ishalf = 0.5f;
                    speed = 0.5f;
                }
                GameObject targetStar = createdStars[i];

                Slider slider = targetStar.transform.GetChild(0).GetComponent<Slider>();
                StartCoroutine(FillSlider(slider, 0f, 1f - ishalf, speed));
            }

            yield return new WaitForSeconds(0.8f);
        }
    }

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