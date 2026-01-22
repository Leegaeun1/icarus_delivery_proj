using System.Collections;
using UnityEngine;
using TMPro;

public class ManManager : MonoBehaviour
{
    [Header("설정")]
    public GameObject manPrefab;
    public Transform manContainer;
    public float minWait = 5f, maxWait = 15f;
    public string[] speechQuotes = { "A동 샌드위치", "주문번호 2번", "눈알주스 세트" };

    private GameObject currentMan;

    void Start() => StartCoroutine(WaitAndSpawn());

    // 대기 후 생성 루틴
    IEnumerator WaitAndSpawn()
    {
        yield return new WaitForSeconds(Random.Range(minWait, maxWait));

        if (currentMan == null && manPrefab != null)
        {
            currentMan = Instantiate(manPrefab, manContainer);

            // 말풍선 텍스트 설정
            var speechText = currentMan.GetComponentInChildren<TextMeshProUGUI>();
            if (speechText != null && speechQuotes.Length > 0)
                speechText.text = speechQuotes[Random.Range(0, speechQuotes.Length)];
        }
    }

    // 외부(주문 완료 시)에서 호출할 함수
    public void CompleteMan()
    {
        if (currentMan != null)
        {
            Destroy(currentMan);
            currentMan = null;
            StartCoroutine(WaitAndSpawn()); // 다음 사람 대기 시작
        }
    }
}