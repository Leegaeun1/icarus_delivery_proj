using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoundFinishManager : MonoBehaviour
{
    // --- 인스펙터 변수 ---
    [Header("종합 평가 & UI")]
    public List<GameObject> resultPrefabs; // 0:수익, 1:지출, 2:순이익 3: 잘못된 요리 4: 잘못된 배달
    public Transform total_parent;

    [Header("별점 설정")]
    public float star_cnt = 1;
    public int max_star = 4;
    public GameObject star_prefab;
    public GameObject star_parent;
    public Button nextdayBtn;
    public TimeManager timerManager;
    public ReadSpreadSheets sheet;
    public GameObject gameOverPanel;

    List<GameObject> createdStars = new List<GameObject>();
    void Awake()
    {
        nextdayBtn = GameObject.Find("NextDayBtn").GetComponent<Button>();
        timerManager = GameObject.Find("TimeManager").GetComponent<TimeManager>();
        sheet = GameObject.Find("sheet").GetComponent<ReadSpreadSheets>();
        gameOverPanel = GameObject.Find("Canvas").transform.GetChild(2).gameObject;
    }

    void Start()
    {
        max_star = 2;
        
        nextdayBtn.gameObject.SetActive(false);
        gameOverPanel.gameObject.SetActive(false);
        if (nextdayBtn != null && timerManager != null)
        {
            nextdayBtn.onClick.RemoveAllListeners();
            nextdayBtn.onClick.AddListener(() => timerManager.StartNextDay());
        }

        // 1. 0으로 나누기 방지 (주문이 0개였으면 그냥 별 0개 혹은 기본값)
        if (sheet.menu_num == 0)
        {
            star_cnt = 0;
        }
        else
        {
            // 2. 정확도 비율 계산 (틀린 개수 / 전체 개수) -> float 캐스팅 필수!
            // 예: 10개 중 2개 틀림 -> 오답률 0.2 -> 정답률 0.8 (80%)
            float errorRate = (float)(sheet.menu_incorrect + sheet.delivery_incorrect) / (float)sheet.menu_num;
            float accuracyScore = Mathf.Clamp01(1.0f - errorRate);

            // 3. 매출 달성률 계산 (현재 수익 / 목표 수익)
            // 예: 목표 300, 수익 300 -> 1.0 (100%)
            float revenueRatio = 0f;
            if (sheet.stand_money > 0)
            {
                revenueRatio = (float)sheet.totalRevenue / (float)sheet.stand_money;
                revenueRatio = Mathf.Clamp01(revenueRatio); // 최대 100%까지만 인정 (원하면 제거 가능)
            }

            // 4. 가중치 적용 (예: 정확도 중요도 0.7, 매출 중요도 0.3)
            // 만약 둘 다 완벽하면 1.0 * max_star = 4개
            float finalScoreRatio = (accuracyScore * 0.7f) + (revenueRatio * 0.3f);

            star_cnt = finalScoreRatio * max_star;
            star_cnt = (float)System.Math.Round(star_cnt * 2) / 2;

            Debug.Log($"[별점 계산] 정답률: {accuracyScore}, 매출달성: {revenueRatio}, 에러비율: {errorRate} -> 최종 별점: {star_cnt}");

            
        }
            
        StartCoroutine(GenerateReviewsSequence());
        
    }

    IEnumerator GenerateReviewsSequence()
    {
        // 0. 별점 애니메이션
        yield return StartCoroutine(review_star());

        // 저장된 재정 데이터 불러오기
        int totalSpent = PlayerPrefs.GetInt("TotalSpent", 0);
        int totalRevenue = PlayerPrefs.GetInt("TotalRevenue", 0);
        int netProfit = totalRevenue - totalSpent;

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
                        uiText.text = "+" + totalRevenue.ToString();
                    }
                    else if (i == 1) // 2번째 프리팹: 총 지출 (-TotalSpent)
                    {
                        uiText.text = "-" + totalSpent.ToString();
                    }
                    else if (i == 2) // 3번째 프리팹: 순이익 (Revenue - Spent)
                    {
                        string sign = netProfit >= 0 ? "+" : ""; // 양수면 + 붙이기
                        uiText.text = sign + netProfit.ToString();
                    }
                    else if (i == 3) // 4번째 프리팹: 잘못된 요리
                    {
                        string sign = sheet.menu_incorrect.ToString(); 
                        uiText.text = sign;
                    }
                    else if (i == 4) // 5번째 프리팹: 잘못된 배달
                    {
                        string sign = sheet.delivery_incorrect.ToString();
                        uiText.text = sign;
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
        yield return new WaitForSeconds(1f);


        sheet.totalRevenue = 0;
        sheet.totalSpent = 0;
        sheet.delivery_incorrect = 0;
        sheet.menu_incorrect = 0;
        // 4. 1초 뒤에 다음으로 가는 버튼 활성화
        if (star_cnt < 1)
        {
            gameOverPanel.gameObject.SetActive(true);
            
        }
        else {
            nextdayBtn.gameObject.SetActive(true);

        }

    }
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