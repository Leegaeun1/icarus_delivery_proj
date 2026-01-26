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
    public List<GameObject> resultPrefabs; // 0:수익, 1:지출, 2:순이익
    public Transform total_parent;

    [Header("별점 설정")]
    public float star_cnt = 2;
    public int max_star = 4;
    public GameObject star_prefab;
    public GameObject star_parent;
    public Button nextdayBtn;
    public TimeManager timerManager;

    List<GameObject> createdStars = new List<GameObject>();
    void Awake()
    {
        nextdayBtn = GameObject.Find("NextDayBtn").GetComponent<Button>();
        timerManager = GameObject.Find("TimeManager").GetComponent<TimeManager>();
    }

    void Start()
    {
        StartCoroutine(GenerateReviewsSequence());
        nextdayBtn.gameObject.SetActive(false);
        // [수정 1] Start에서 버튼 이벤트를 미리 연결합니다.
        if (nextdayBtn != null && timerManager != null)
        {
            nextdayBtn.onClick.RemoveAllListeners();
            nextdayBtn.onClick.AddListener(() => timerManager.StartNextDay());
        }
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
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
        yield return new WaitForSeconds(1f);

        // 4. 1초 뒤에 다음으로 가는 버튼 활성화
        nextdayBtn.gameObject.SetActive(true);

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