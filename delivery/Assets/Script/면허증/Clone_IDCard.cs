using UnityEngine;
using UnityEngine.UI;

public class CharacterWatcher : MonoBehaviour
{
    [Header("모니터링 대상")]
    public Transform manContainer;

    [Header("복사본 설정 (면허증 위치)")]
    public Transform targetContainer;

    [Header("캐릭터 정밀 조정")]
    public Vector2 customPosition = Vector2.zero;
    public float customScale = 1.0f;

    private GameObject lastDetectedMan;
    private GameObject clonedMan;

    void Update()
    {
        if (manContainer.childCount > 0)
        {
            GameObject currentMan = manContainer.GetChild(0).gameObject;
            if (currentMan != lastDetectedMan)
            {
                CloneToPanel(currentMan);
                lastDetectedMan = currentMan;
            }
        }
        else
        {
            if (clonedMan != null) Destroy(clonedMan);
            lastDetectedMan = null;
        }
    }

    void CloneToPanel(GameObject original)
    {
        if (clonedMan != null) Destroy(clonedMan);

        // 1. 프리팹 전체 복사
        clonedMan = Instantiate(original, targetContainer);
        clonedMan.name = "Cloned_Photo";

        // 2. 기능성 스크립트만 제거 (이미지는 유지)
        // GetComponentsInChildren을 사용하여 자식 오브젝트의 스크립트까지 모두 제거합니다.
        MonoBehaviour[] allScripts = clonedMan.GetComponentsInChildren<MonoBehaviour>();
        foreach (var script in allScripts)
        {
            // Image, RectTransform, CanvasRenderer, TextMeshPro는 UI 렌더링에 필요하므로 제외
            if (script is Image || script is CanvasRenderer || script is TMPro.TextMeshProUGUI || script is RectTransform)
            {
                continue;
            }
            Destroy(script); // 그 외 모든 커스텀 기능 삭제
        }

        // 3. UI 위치 및 크기 강제 초기화
        RectTransform rt = clonedMan.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = customPosition;
            rt.localScale = new Vector3(customScale, customScale, 1f);
        }

        // 4. 모든 이미지 투명도 강제 복구 (원본이 페이드 중이었을 경우 대비)
        Image[] images = clonedMan.GetComponentsInChildren<Image>();
        foreach (var img in images)
        {
            Color c = img.color;
            c.a = 1f;
            img.color = c;
        }

        // 5. CanvasGroup이 남아있다면 투명도 조절
        CanvasGroup cg = clonedMan.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 1f;

        clonedMan.transform.SetAsLastSibling();
    }
}