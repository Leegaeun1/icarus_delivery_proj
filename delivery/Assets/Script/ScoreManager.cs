using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("게임 통계")]
    public int totalScore = 0;
    public int deliverySuccessCount = 0;
    public int cookSuccessCount = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }   
    }

    // 점수 추가 및 통계 기록 함수
    public void AddScore(int amount, bool isDelivery, bool isSuccess)
    {
        if (isSuccess)
        {
            totalScore += amount;
            if (isDelivery) deliverySuccessCount++;
            else cookSuccessCount++;
            Debug.Log($"<color=green>[점수 획득]</color> +{amount} | 총점: {totalScore}");
        }
        else
        {
            totalScore = Mathf.Max(0, totalScore - (amount / 2)); // 감점 처리 (0점 이하 방지)
            Debug.Log($"<color=red>[점수 감점]</color> -{amount / 2} | 총점: {totalScore}");
        }
    }
}