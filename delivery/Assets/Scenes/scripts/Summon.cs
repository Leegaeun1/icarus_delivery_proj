using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 외계인 종류별 데이터 구조 (인스펙터에서 설정 가능)
[System.Serializable]
public class AlienData
{
    public string alienName;       // 외계인 이름 (예: Green, Purple)
    public GameObject bodyPrefab;  // 외계인 몸체 프리팹 (Random_face 스크립트가 붙어있어야 함)
    public Sprite[] eyeSprites;    // 이 외계인 전용 눈 스프라이트 배열
    public Sprite[] noseSprites;   // 이 외계인 전용 코 스프라이트 배열
    public Sprite[] mouthSprites;  // 이 외계인 전용 입 스프라이트 배열
    public Sprite[] earSprites;    // 이 외계인 전용 귀 스프라이트 배열
}

public class Summon : MonoBehaviour
{
    [Header("Alien Types Configuration")]
    public List<AlienData> alienTypes; // 인스펙터에서 외계인 종류를 추가하세요.

    [Header("Movement Settings")]
    public int steps = 10;
    public int framesPerStep = 2;
    public Vector3 spawnPosition = new Vector3(3f, 1.5f, 0f);
    public Vector3 moveOffset = new Vector3(-1f, 0f, 0f);
    public Vector3 desiredScale = new Vector3(2f, 2f, 1f);

    [Header("References")]
    public DialogueManager dialogueManager;

    private GameObject currentAlienInstance; // 현재 씬에 소환된 인스턴스 저장

    void Start()
    {
        // 초기화 시 캐릭터를 소환하지 않음
    }

    // 버튼 이벤트 등에 연결하여 사용
    public void SummonAlien()
    {
        if (alienTypes == null || alienTypes.Count == 0)
        {
            Debug.LogError("[AlienAppearStepped] alienTypes 리스트가 비어 있습니다.");
            return;
        }

        // 1. 이미 소환된 외계인이 있다면 제거
        if (currentAlienInstance != null)
        {
            Destroy(currentAlienInstance);
        }

        // 2. 랜덤하게 외계인 종류 선택
        int randomIndex = Random.Range(0, alienTypes.Count);
        AlienData selectedAlien = alienTypes[randomIndex];

        // 3. 선택된 외계인 프리팹 복제 및 크기 설정
        currentAlienInstance = Instantiate(selectedAlien.bodyPrefab, spawnPosition, Quaternion.identity);
        currentAlienInstance.transform.localScale = desiredScale;

        // 4. Random_face 컴포넌트에 외계인 전용 파츠 데이터 전달
        Random_face randomFace = currentAlienInstance.GetComponent<Random_face>();
        if (randomFace != null)
        {
            // 선택된 외계인의 전용 스프라이트들을 Random_face의 변수에 할당
            randomFace.eyes = selectedAlien.eyeSprites;
            randomFace.noses = selectedAlien.noseSprites;
            randomFace.mouths = selectedAlien.mouthSprites;
            randomFace.ears = selectedAlien.earSprites;

            // 할당된 파츠 중 랜덤하게 하나씩 골라 화면에 출력
            randomFace.ShowAllRandomParts();
        }
        else
        {
            Debug.LogWarning($"[AlienAppearStepped] {selectedAlien.alienName} 프리팹에 Random_face 스크립트가 없습니다.");
        }

        // 5. 애니메이션 및 페이드 인 시작
        SpriteRenderer mainSr = currentAlienInstance.GetComponent<SpriteRenderer>();
        if (mainSr != null)
        {
            mainSr.enabled = true;
            // 코루틴 실행 (자식 오브젝트인 얼굴 파츠들의 렌더러까지 함께 처리함)
            StartCoroutine(SteppedFadeInMove(currentAlienInstance, Color.black, Color.white));
        }
    }

    // 애니메이션 코루틴: 이동과 색상 페이드를 동시에 처리
    IEnumerator SteppedFadeInMove(GameObject alienInstance, Color startColor, Color targetColor)
    {
        // 몸체와 모든 자식(눈, 코, 입, 귀)의 SpriteRenderer를 가져옴
        SpriteRenderer[] allRenderers = alienInstance.GetComponentsInChildren<SpriteRenderer>();
        
        Vector3 startPos = spawnPosition;
        Vector3 targetPos = spawnPosition + moveOffset;

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            
            // 위치 이동 (Lerp)
            alienInstance.transform.position = Vector3.Lerp(startPos, targetPos, t);
            
            // 모든 렌더러의 색상 페이드 (Lerp)
            foreach (SpriteRenderer sr in allRenderers)
            {
                if (sr != null)
                    sr.color = Color.Lerp(startColor, targetColor, t);
            }

            // 설정된 프레임만큼 대기
            for (int f = 0; f < framesPerStep; f++)
                yield return new WaitForEndOfFrame();
        }

        // 최종 위치 및 색상 확정
        alienInstance.transform.position = targetPos;
        foreach (SpriteRenderer sr in allRenderers)
        {
            if (sr != null)
                sr.color = targetColor;
        }

        // 6. 애니메이션 완료 후 대화 시작
        if (dialogueManager != null)
        {
            dialogueManager.StartInitialDialogue();
        }
    }
}