using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DayFinishManager : MonoBehaviour
{
    [System.Serializable]
    public class StringList { public List<string> ingredients = new(); }

    // --- 인스펙터 변수 ---
    [Header("종합 평가 & UI")]
    public List<GameObject> resultPrefabs; // 0:수익, 1:지출, 2:순이익
    public Transform total_parent;
    public GameObject review_prefab;
    public GameObject review_flip_prefab;
    public GameObject review_parent;
    public TextMeshProUGUI Days_txt;


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

    public Button nextdayBtn;
    public TimeManager timerManager;

    void Awake()
    {
        rng = seed >= 0 ? new System.Random(seed) : new System.Random();
        total_parent = GameObject.Find("day_total").GetComponent<Transform>();
        nextdayBtn = GameObject.Find("NextDayBtn").GetComponent<Button>();
        timerManager = GameObject.Find("TimeManager").GetComponent<TimeManager>();

        Days_txt = GameObject.Find("result_Panel").transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();

        LoadDataFromGameManager();
    }
    void LoadDataFromGameManager()
    {
        if (GameManager.Instance == null) return;

        // [안전 장치] 리스트가 혹시 null이면 새로 만듭니다.
        if (this.FinalIngredients == null) this.FinalIngredients = new List<StringList>();
        if (this.ExcludeRequest == null) this.ExcludeRequest = new List<StringList>();
        if (this.IncludeRequest == null) this.IncludeRequest = new List<StringList>();
        if (this.RequestedSpecials == null) this.RequestedSpecials = new List<string>();
        if (this.ProvidedSpecials == null) this.ProvidedSpecials = new List<string>();

        // 1. 데이터 이어붙이기 (AddRange)
        // 기존에 데이터가 있더라도 유지하면서, GameManager의 데이터를 뒤에 추가합니다.
        this.FinalIngredients.AddRange(GameManager.Instance.PendingFinalIngredients);
        this.ExcludeRequest.AddRange(GameManager.Instance.PendingExcludeRequest);
        this.IncludeRequest.AddRange(GameManager.Instance.PendingIncludeRequest);


        // 2. 가져온 후 GameManager의 임시 저장소 비우기 (중복 방지)
        GameManager.Instance.PendingFinalIngredients.Clear();
        GameManager.Instance.PendingExcludeRequest.Clear();
        GameManager.Instance.PendingIncludeRequest.Clear();

        Debug.Log($"[DayFinishManager] 데이터 로드 완료. 총 주문 수: {this.FinalIngredients.Count}");
    }
    void Start()
    {
        StartCoroutine(GenerateReviewsSequence());
        nextdayBtn.gameObject.SetActive(false);
        print(GameObject.Find("Days").name);
        // [수정 1] Start에서 버튼 이벤트를 미리 연결합니다.
        if (nextdayBtn != null && timerManager != null)
        {
            nextdayBtn.onClick.RemoveAllListeners();
            nextdayBtn.onClick.AddListener(() => timerManager.StartNextDay());
        }
    }

    IEnumerator GenerateReviewsSequence()
    {

        // 저장된 재정 데이터 불러오기
        int dailySpent = GameObject.Find("sheet").GetComponent<ReadSpreadSheets>().dailySpent; // 하루에 사용한 금약
        int dailyRevenue = GameObject.Find("sheet").GetComponent<ReadSpreadSheets>().dailyRevenue;
        int netProfit = dailyRevenue - dailySpent;
        Days_txt.text = timerManager.date.ToString() + "일차";
        // 0.5 종합 평가 프리팹 생성 및 텍스트 갱신
        for (int i = 0; i < resultPrefabs.Count; i++)
        {
            if (resultPrefabs[i] == null) continue;

            GameObject obj = Instantiate(resultPrefabs[i], total_parent);

            // 프리팹 순서에 따른 텍스트 설정
            // 0번: 수익, 1번: 지출, 2번: 순이익
            if (obj.transform.childCount > 1)
            {
                TextMeshProUGUI uiText = obj.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

                if (uiText != null)
                {
                    if (i == 0) // 1번째 프리팹: 총 수익 (+TotalRevenue)
                    {
                        uiText.text = "+" + dailyRevenue.ToString();
                    }
                    else if (i == 1) // 2번째 프리팹: 총 지출 (-TotalSpent)
                    {
                        uiText.text = "-" + dailySpent.ToString();
                    }
                    else if (i == 2) // 3번째 프리팹: 순이익 (Revenue - Spent)
                    {
                        string sign = netProfit >= 0 ? "+" : ""; // 양수면 + 붙이기
                        uiText.text = sign + netProfit.ToString();
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);
        }

        // 1. 기존 리뷰 청소
        foreach (var obj in spawnedReviewObjects) if (obj != null) Destroy(obj);
        spawnedReviewObjects.Clear();
        
        // 2. 데이터 가공 및 판정 (OrderChecker 사용)
        List<Order_check> allOrders = CreateAndCheckOrders();

        // 3. 화면 표시
        yield return StartCoroutine(DisplayReviews(allOrders));

        yield return new WaitForSeconds(1f);

        // 4. 1초 뒤에 다음으로 가는 버튼 활성화
        nextdayBtn.gameObject.SetActive(true);
    }

    List<Order_check> CreateAndCheckOrders()
    {
        List<Order_check> orders = new List<Order_check>();
        int count = FinalIngredients.Count;

        for (int i = 0; i < count; i++)
        {
            var order = new Order_check { SelectedIngredients = FinalIngredients[i].ingredients };

            if (i < ExcludeRequest.Count) order.ExcludeRequest = ExcludeRequest[i].ingredients;
            if (i < IncludeRequest.Count) order.IncludeRequest = IncludeRequest[i].ingredients;

            if (i < RequestedSpecials.Count) order.RequestedSpecial = RequestedSpecials[i];
            if (i < ProvidedSpecials.Count) order.ProvidedSpecial = ProvidedSpecials[i];

            if (i < RequestedSides.Count) order.RequestedSides = RequestedSides[i].ingredients;
            if (i < ProvidedSides.Count) order.ProvidedSides = ProvidedSides[i].ingredients;

            if (i < IsDeliverySuccessList.Count) order.IsDeliverySuccess = IsDeliverySuccessList[i];

            OrderChecker.Check(order);
            orders.Add(order);
        }
        return orders;
    }
    IEnumerator DisplayReviews(List<Order_check> orders)
    {
        // 주문이 하나도 없을 경우 (샌드위치를 만들지 않음)
        if (orders.Count == 0)
        {
            if (review_prefab != null)
            {
                GameObject obj = Instantiate(review_prefab, review_parent.transform);
                spawnedReviewObjects.Add(obj); // 나중에 지울 수 있게 리스트에 추가

                var texts = obj.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 2)
                {
                    texts[0].text = "-";          // 이름 칸 (비워두거나 '-' 표시)
                    texts[1].text = "리뷰 없음";   // 내용 칸
                }
            }
            yield break; // 더 이상 진행하지 않고 코루틴 종료
        }

        // --- 기존 로직 (주문이 있을 때만 실행됨) ---
        var successOrders = orders.Where(o => o.isSuccess).ToList();
        var failOrders = orders.Where(o => !o.isSuccess).ToList();

        int total = Mathf.Min(MaxDisplayCount, orders.Count);
        int negCount = Mathf.Min(total, failOrders.Count);
        int posCount = total - negCount;

        var displayList = failOrders.OrderBy(x => rng.Next()).Take(negCount)
                          .Concat(successOrders.OrderBy(x => rng.Next()).Take(posCount))
                          .OrderBy(x => rng.Next()).ToList();

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
                texts[1].text = displayList[i].isSuccess ? GeneratePositiveReview(displayList[i]) : displayList[i].failReason;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    string GeneratePositiveReview(Order_check order)
    {
        string review = null;
        if (!string.IsNullOrEmpty(order.ProvidedSpecial) && rng.Next(2) == 0)
        {
            review = ReviewDataProvider.GetRandomReview(order.ProvidedSpecial, rng);
            if (review != null) return review;
        }
        if (order.ProvidedSides.Count > 0 && rng.Next(3) == 0)
        {
            string randomSide = order.ProvidedSides[rng.Next(order.ProvidedSides.Count)];
            review = ReviewDataProvider.GetRandomReview(randomSide, rng);
            if (review != null) return review;
        }
        if (order.SelectedIngredients.Count > 0)
        {
            string target = order.SelectedIngredients[rng.Next(order.SelectedIngredients.Count)];
            review = ReviewDataProvider.GetRandomReview(target, rng);
            if (review != null) return review;
        }
        return "배달도 빠르고 맛도 무난하네요. 잘 먹었습니다.";
    }

}