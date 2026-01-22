using System.Collections;
using UnityEngine;
using TMPro;

public class ManManager : MonoBehaviour
{
    [Header("설정")]
    // 1. 여러 프리팹을 넣을 수 있도록 배열로 변경
    public GameObject[] manPrefabs;
    public Transform manContainer;
    public float minWait = 5f, maxWait = 15f;
    public string[] speechQuotes = { "A동 샌드위치", "주문번호 2번", "눈알주스 세트" };

    private GameObject currentMan;

    void Start() => StartCoroutine(WaitAndSpawn());

    IEnumerator WaitAndSpawn()
    {
        yield return new WaitForSeconds(Random.Range(minWait, maxWait));

        // 프리팹 배열이 비어있지 않은지 확인
        if (currentMan == null && manPrefabs != null && manPrefabs.Length > 0)
        {
            // 2. 배열 중 하나를 랜덤으로 골라서 생성
            int randomIndex = Random.Range(0, manPrefabs.Length);
            currentMan = Instantiate(manPrefabs[randomIndex], manContainer);

            Debug.Log("<color=green>캐릭터 생성 성공!</color> 이름: " + currentMan.name);

            // 3. 생성된 사람의 Random_face 컴포넌트 실행
            Random_face faceScript = currentMan.GetComponent<Random_face>();
            if (faceScript != null)
            {
                faceScript.ShowAllRandomParts(); // 모든 부위 랜덤 설정 함수 호출
            }

            // 말풍선 텍스트 설정
            var speechText = currentMan.GetComponentInChildren<TextMeshProUGUI>();
            if (speechText != null && speechQuotes.Length > 0)
                speechText.text = speechQuotes[Random.Range(0, speechQuotes.Length)];
        }
    }

    public void CompleteMan()
    {
        if (currentMan != null)
        {
            Destroy(currentMan);
            currentMan = null;
            StartCoroutine(WaitAndSpawn());
        }
    }
}