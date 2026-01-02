using System.Collections;
using UnityEngine;
using TMPro; // 텍스트 제어를 위해 추가

public class ManManager : MonoBehaviour
{
    [Header("Man 생성 설정")]
    public GameObject manPrefab;
    public Transform manContainer;
    public float minSpawnInterval = 5f;
    public float maxSpawnInterval = 15f;

    [Header("말풍선 멘트 설정")]
    // 3가지 멘트를 인스펙터 창에서 수정할 수 있게 배열로 만듭니다.
    public string[] speechQuotes = {
        "A동 마녀 샌드위치 가지러 왔습니다",
        "주문번호 2번입니다",
        "눈알주스, 지구 샌드위치 세트 주세요"
    };

    private GameObject currentMan;

    void Start()
    {
        StartCoroutine(GenerateRandomMan());
    }

    IEnumerator GenerateRandomMan()
    {
        float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
        yield return new WaitForSeconds(waitTime);

        SpawnMan();
    }

    public void SpawnMan()
    {
        if (currentMan != null) return;

        if (manPrefab != null && manContainer != null)
        {
            // 1. Man 생성
            currentMan = Instantiate(manPrefab, manContainer);

            // 2. 랜덤 멘트 설정
            SetRandomSpeech();

            Debug.Log("새 Man과 말풍선 생성 완료");
        }
    }

    private void SetRandomSpeech()
    {
        // 생성된 Man 오브젝트의 자식들 중에서 TextMeshProUGUI 컴포넌트를 찾습니다.
        TextMeshProUGUI speechText = currentMan.GetComponentInChildren<TextMeshProUGUI>();

        if (speechText != null && speechQuotes.Length > 0)
        {
            // 0~2 사이의 랜덤한 인덱스 선택
            int randomIndex = Random.Range(0, speechQuotes.Length);
            speechText.text = speechQuotes[randomIndex];
        }
        else
        {
            Debug.LogWarning("Man 프리팹 내부에서 TextMeshProUGUI를 찾을 수 없습니다.");
        }
    }

    public void CompleteMan()
    {
        if (currentMan != null)
        {
            Destroy(currentMan);
            currentMan = null;
            Debug.Log("Man 처리 완료");

            // 처리 후 다음 사람 생성을 위해 코루틴 다시 시작
            StartCoroutine(GenerateRandomMan());
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnMan();
        }
    }
}