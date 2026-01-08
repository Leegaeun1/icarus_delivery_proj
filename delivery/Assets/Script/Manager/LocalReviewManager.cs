using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocalReviewManager : MonoBehaviour
{
    [System.Serializable]
    public class StringList { public List<string> ingredients = new(); }

    // --- 인스펙터 변수 ---
    [Header("종합 평가 & UI")]
    public List<GameObject> resultPrefabs;
    public Transform total_parent;
    public GameObject review_prefab;
    public GameObject review_flip_prefab;
    public GameObject review_parent;

    [Header("별점 설정")]
    public float star_cnt = 2;
    public int max_star = 4;
    public GameObject star_prefab;
    public GameObject star_parent;
    List<GameObject> createdStars = new List<GameObject>();

    [Header("입력 데이터 (인스펙터 설정)")]
    public List<StringList> FinalIngredients = new();
    public List<StringList> ExcludeRequest = new();
    public List<StringList> IncludeRequest = new();

    [Space(10)]
    public List<string> RequestedSpecials = new();
    public List<string> ProvidedSpecials = new();

    [Space(10)]
    public List<StringList> RequestedSides = new();
    public List<StringList> ProvidedSides = new();

    [Space(10)]
    public List<bool> IsDeliverySuccessList = new();

    [Header("설정")]
    public int MaxDisplayCount = 4;
    public int seed = -1;
    public List<string> ReviewerNamePool = new() { "김솰라", "모구리", "치코", "디노", "쿠모", "피오", "비콜로", "지르론" };

    private System.Random rng;
    private List<GameObject> spawnedReviewObjects = new List<GameObject>();

    void Awake()
    {
        rng = seed >= 0 ? new System.Random(seed) : new System.Random();
    }

    void Start()
    {
        StartCoroutine(GenerateReviewsSequence());
    }

    IEnumerator GenerateReviewsSequence()
    {
        // 0. 별점 애니메이션
        yield return StartCoroutine(review_star());

        // 0.5 종합 평가 프리팹 생성
        foreach (GameObject prefab in resultPrefabs)
        {
            if (prefab != null) Instantiate(prefab, total_parent);
            yield return new WaitForSeconds(0.5f);
        }

        // 1. 기존 리뷰 청소
        foreach (var obj in spawnedReviewObjects) if (obj != null) Destroy(obj);
        spawnedReviewObjects.Clear();

        // 2. 데이터 가공 및 판정 (OrderChecker 사용)
        List<Order_check> allOrders = CreateAndCheckOrders();

        // 3. 화면 표시
        yield return StartCoroutine(DisplayReviews(allOrders));
    }

    // 데이터를 Order_check 객체로 변환하고 OrderChecker에게 판정을 맡김
    List<Order_check> CreateAndCheckOrders()
    {
        List<Order_check> orders = new List<Order_check>();
        int count = FinalIngredients.Count;

        for (int i = 0; i < count; i++)
        {
            var order = new Order_check { SelectedIngredients = FinalIngredients[i].ingredients };

            // 인덱스 안전하게 데이터 주입
            if (i < ExcludeRequest.Count) order.ExcludeRequest = ExcludeRequest[i].ingredients;
            if (i < IncludeRequest.Count) order.IncludeRequest = IncludeRequest[i].ingredients;

            if (i < RequestedSpecials.Count) order.RequestedSpecial = RequestedSpecials[i];
            if (i < ProvidedSpecials.Count) order.ProvidedSpecial = ProvidedSpecials[i];

            if (i < RequestedSides.Count) order.RequestedSides = RequestedSides[i].ingredients;
            if (i < ProvidedSides.Count) order.ProvidedSides = ProvidedSides[i].ingredients;

            if (i < IsDeliverySuccessList.Count) order.IsDeliverySuccess = IsDeliverySuccessList[i];

            // OrderChecker가 성공/실패 판별
            OrderChecker.Check(order);

            orders.Add(order);
        }
        return orders;
    }

    IEnumerator DisplayReviews(List<Order_check> orders)
    {
        // 출력할 리뷰 선정 (성공/실패 비율 맞추기)
        var successOrders = orders.Where(o => o.isSuccess).ToList();
        var failOrders = orders.Where(o => !o.isSuccess).ToList();

        int total = Mathf.Min(MaxDisplayCount, orders.Count);
        int negCount = Mathf.Min(total, failOrders.Count);
        int posCount = total - negCount;

        var displayList = failOrders.OrderBy(x => rng.Next()).Take(negCount)
                          .Concat(successOrders.OrderBy(x => rng.Next()).Take(posCount))
                          .OrderBy(x => rng.Next()).ToList(); // 섞기

        var names = ReviewerNamePool.OrderBy(x => rng.Next()).Take(displayList.Count).ToList();

        for (int i = 0; i < displayList.Count; i++)
        {
            GameObject prefab = (i % 2 == 0) ? review_prefab : review_flip_prefab;
            if (prefab == null) continue;

            GameObject obj = Instantiate(prefab, review_parent.transform);
            spawnedReviewObjects.Add(obj);

            var texts = obj.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                texts[0].text = names[i];
                // 성공이면 긍정 리뷰 생성, 실패면 실패 사유 출력
                texts[1].text = displayList[i].isSuccess ? GeneratePositiveReview(displayList[i]) : displayList[i].failReason;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    // 긍정 리뷰 멘트 생성 (ReviewDataProvider 사용)
    string GeneratePositiveReview(Order_check order)
    {
        string review = null;

        // 1. 스페셜 메뉴 (50%)
        if (!string.IsNullOrEmpty(order.ProvidedSpecial) && rng.Next(2) == 0)
        {
            review = ReviewDataProvider.GetRandomReview(order.ProvidedSpecial, rng);
            if (review != null) return review;
        }

        // 2. 사이드 메뉴 (30%)
        if (order.ProvidedSides.Count > 0 && rng.Next(3) == 0)
        {
            string randomSide = order.ProvidedSides[rng.Next(order.ProvidedSides.Count)];
            review = ReviewDataProvider.GetRandomReview(randomSide, rng);
            if (review != null) return review;
        }

        // 3. 기본 재료
        if (order.SelectedIngredients.Count > 0)
        {
            string target = order.SelectedIngredients[rng.Next(order.SelectedIngredients.Count)];
            review = ReviewDataProvider.GetRandomReview(target, rng);
            if (review != null) return review;
        }

        return "배달도 빠르고 맛도 무난하네요. 잘 먹었습니다.";
    }

    // --- 별점 UI ---
    IEnumerator review_star()
    {
        foreach (var star in createdStars) if (star != null) Destroy(star);
        createdStars.Clear();
        if (star_parent == null) yield break;

        for (int i = 0; i < max_star; i++) createdStars.Add(Instantiate(star_prefab, star_parent.transform));

        for (int i = 0; i < max_star; i++)
        {
            if (i < star_cnt)
            {
                float fill = 1f; float speed = 0.8f;
                if (Mathf.Abs(star_cnt % 1) > 0.01f && i == (int)star_cnt) { fill = 0.5f; speed = 0.5f; }
                if (createdStars[i].transform.childCount > 0)
                {
                    var slider = createdStars[i].transform.GetChild(0).GetComponent<Slider>();
                    if (slider != null) StartCoroutine(FillSlider(slider, 0f, fill, speed));
                }
            }
            yield return new WaitForSeconds(0.8f);
        }
    }

    IEnumerator FillSlider(Slider slider, float start, float end, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            slider.value = Mathf.Lerp(start, end, time / duration);
            yield return null;
        }
        slider.value = end;
    }
}