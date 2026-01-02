using System.Collections;
using UnityEngine;

public class AlienAppearStepped : MonoBehaviour
{
    // 원본 캐릭터 스프라이트 대신, 소환할 외계인 캐릭터 프리팹을 참조합니다.
    public GameObject alienPrefab; // <--- 프리팹을 연결할 새 필드

    public int steps = 10;
    public int framesPerStep = 2;
    public Vector3 spawnPosition = new Vector3(3f, 1.5f, 0f);
    public Vector3 moveOffset = new Vector3(-1f, 0f, 0f);
    public Vector3 desiredScale = new Vector3(2f, 2f, 1f);

    public DialogueManager dialogueManager;


    private GameObject currentAlienInstance; // <--- 현재 씬에 소환된 인스턴스를 저장
    
    // Start() 함수는 이제 이 컴포넌트(summon)의 초기화만 담당하며, 캐릭터는 소환하지 않습니다.
    void Start()
    {
        // 기존의 spriteRenderer 초기화 로직은 제거
        // 원본 오브젝트를 이동시키지 않으므로 transform.position 설정도 제거
    }

    // 버튼에서 실행
    public void SummonAlien()
    {
        if (alienPrefab == null)
        {
            Debug.LogError("Alien Prefab이 연결되지 않았습니다.");
            return;
        }

        // 이미 외계인이 소환되어 있다면 제거하고 새로 시작 (선택 사항)
        if (currentAlienInstance != null)
        {
            Destroy(currentAlienInstance);
        }

        // 1. 새로운 외계인 인스턴스 복제
        currentAlienInstance = Instantiate(alienPrefab, spawnPosition, Quaternion.identity);
        currentAlienInstance.transform.localScale = desiredScale;
        
        SpriteRenderer sr = currentAlienInstance.GetComponent<SpriteRenderer>();
        if (sr == null) return; // SpriteRenderer가 없으면 중단

        // 2. 초기 상태 설정
        sr.enabled = true;
        sr.color = Color.black; // 페이드 인을 위해 검은색으로 시작

        // 3. 애니메이션 코루틴 시작
        StartCoroutine(SteppedFadeInMove(currentAlienInstance, sr, sr.color, Color.white));
    }

    // 애니메이션 코루틴은 이제 소환된 인스턴스와 해당 SpriteRenderer를 매개변수로 받습니다.
    IEnumerator SteppedFadeInMove(GameObject alienInstance, SpriteRenderer sr, Color startColor, Color targetColor)
    {
        Vector3 startPos = spawnPosition;
        Vector3 targetPos = spawnPosition + moveOffset;

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            alienInstance.transform.position = Vector3.Lerp(startPos, targetPos, t);
            sr.color = Color.Lerp(startColor, targetColor, t);

            for (int f = 0; f < framesPerStep; f++)
                yield return new WaitForEndOfFrame();
        }

        alienInstance.transform.position = targetPos;
        sr.color = targetColor;

        // 4. 애니메이션 완료 후 대화 시작
        dialogueManager.StartInitialDialogue();
    }
}