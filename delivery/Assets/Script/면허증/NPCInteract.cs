using UnityEngine;
using UnityEngine.EventSystems; // UI 클릭 감지를 위해 추가

// IPointerClickHandler를 추가하여 UI 방식의 클릭도 지원하게 만듭니다.
public class NPCInteract : MonoBehaviour, IPointerClickHandler
{
    // 1. 기존 물리 방식 (Canvas 밖으로 뺄 경우 대비)
    void OnMouseDown()
    {
        Debug.Log("<color=cyan>물리 방식(OnMouseDown) 클릭 감지!</color>");
        ExecuteClick();
    }

    // 2. UI 방식 (현재 캐릭터가 Canvas 안에 있을 때 작동)
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("<color=yellow>UI 방식(OnPointerClick) 클릭 감지!</color>");
        ExecuteClick();
    }

    void ExecuteClick()
    {
        Random_face myFace = GetComponent<Random_face>();
        IDCardUI ui = FindObjectOfType<IDCardUI>(true);

        if (myFace != null && ui != null)
        {
            ui.ShowCard(myFace.GetCurrentFaceData(), "REG-" + Random.Range(1000, 9999));
        }
        else
        {
            if (myFace == null) Debug.LogError("Random_face를 찾을 수 없습니다!");
            if (ui == null) Debug.LogError("IDCardUI를 찾을 수 없습니다!");
        }
    }
}