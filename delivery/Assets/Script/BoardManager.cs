using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    [Header("원본(작은) 포스트잇 프리팹")]
    public GameObject postitPrefab;   // btn 대신 명확히
    private GameObject board_panel; // 끄고 킬 보드 패널
    private bool isBoardClikced = false;
    private GameObject spawned;       // 만든 복제본을 보관
    public bool isOpen = false;

    private void Start()
    {
        board_panel = postitPrefab.transform.parent.transform.parent.gameObject;
    }

    public void OnClick()
    {
        if (!isOpen)
        {
            // 부모를 바로 지정해서 생성 (RectTransform 유지)
            spawned = Instantiate(postitPrefab, postitPrefab.transform.parent);
            // 2) 복제본의 BoardManager(있다면) 제거해서 토글 중복 방지
            var mgrOnClone = spawned.GetComponent<BoardManager>();
            if (mgrOnClone) Destroy(mgrOnClone);
            // 3) 복제본을 누르면 Close 되도록 버튼 이벤트 연결 (원본의 Close 호출)
            var btnOnClone = spawned.GetComponent<Button>();
            if (btnOnClone)
            {
                btnOnClone.onClick.RemoveListener(Close); // 중복 방지
                btnOnClone.onClick.AddListener(Close);
            }

            var rect = spawned.GetComponent<RectTransform>();
            var pin = spawned.transform.Find("Image").GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x*10, rect.sizeDelta.y * 10);
            pin.sizeDelta = new Vector2(pin.sizeDelta.x * 10, pin.sizeDelta.y * 10);
            rect.anchoredPosition = Vector2.zero;
            // 5) "Explain" 텍스트가 있으면 폰트 크기 조정
            var explainTf = spawned.transform.Find("Explain");
            if (explainTf)
            {
                var tmp = explainTf.GetComponent<TextMeshProUGUI>();
                if (tmp) tmp.fontSize = 90;
            }
            //else
            //{
            //    var tmp = spawned.transform.GetChild(0).GetComponent<RectTransform>();
            //    tmp.sizeDelta = new Vector2(tmp.sizeDelta.x * 10, tmp.sizeDelta.y * 10);
            //}
            isOpen = true;
        }
        else
        {
            Close();
        }
    }
    private void Close()
    {
        if (spawned) Destroy(spawned);
        spawned = null;
        isOpen = false;

    }

    public void cookBoardClick()
    {
        isBoardClikced = !isBoardClikced;
        board_panel.gameObject.SetActive(isBoardClikced);
    }
}
