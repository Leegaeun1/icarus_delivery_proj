using UnityEngine;
using UnityEngine.EventSystems;

public class FoodDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 startPosition;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = rectTransform.anchoredPosition;
        canvasGroup.alpha = 0.6f; // 드래그 중 투명도 조절
        canvasGroup.blocksRaycasts = false; // 드래그 중 뒤의 오브젝트 감지 허용
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 마우스/터치 위치로 이동
        rectTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // 마우스 아래에 있는 오브젝트 확인
        GameObject target = eventData.pointerCurrentRaycast.gameObject;

        // 대상이 ManContainer이거나 그 자식(배달원 캐릭터)인 경우
        if (target != null && (target.name == "Man Container" || target.transform.IsChildOf(GameObject.Find("Man Container").transform)))
        {
            // 1. 배달 검증 실행 (새로운 매니저 호출)
            if (LicenseJudge.Instance != null)
            {
                LicenseJudge.Instance.AcceptDelivery();
            }

            // 2. 음식 오브젝트 파괴
            Destroy(gameObject);
        }
    }
}