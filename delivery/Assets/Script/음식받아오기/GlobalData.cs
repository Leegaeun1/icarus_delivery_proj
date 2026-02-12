using UnityEngine;

public class GlobalDataManager : MonoBehaviour
{
    public static GlobalDataManager Instance;

    // 공유하고 싶은 변수 (예: 점수, 아이템 이름 등)
    public string sharedValue = "공유 데이터 없음";

    void Awake()
    {
        // 싱글톤 패턴: 단 하나만 존재하도록 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴 방지 [cite: 2026-02-12]
        }
        else
        {
            Destroy(gameObject);
        }
    }
}