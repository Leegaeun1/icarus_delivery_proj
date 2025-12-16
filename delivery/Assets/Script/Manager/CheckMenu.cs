using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheckMenu : MonoBehaviour
{
    public static List<string> selectedNames = new List<string>();
    private Image image;
    private bool isSelect = false;
    public GameObject selectMenu;
    public GameObject completebtn;
    [SerializeField] private ReadSpreadSheet sheet;

    private void Awake()
    {
        if (image == null) 
            image = GetComponent<Image>();

        if (sheet == null) 
            sheet = FindObjectOfType<ReadSpreadSheet>(); // 인스펙터 미연결 시 보조
    }

    private void Start()
    {

        if (image == null)
        {
            Debug.LogError("[CheckMenu] image가 할당되지 않았습니다.");
            enabled = false;
            return;
        }
        if (selectMenu == null)
        {
            Debug.LogError("[CheckMenu] selectMenu가 할당되지 않았습니다.");
            enabled = false;
            return;
        }
        if (completebtn == null)
        {
            Debug.LogError("[CheckMenu] completebtn가 할당되지 않았습니다.");
            enabled = false;
            return;
        }
        
    }

    public void player_select()
    {
        if (image == null)
        {
            Debug.LogWarning("[CheckMenu] Image 컴포넌트가 없습니다.");
            return;
        }

        if (transform.childCount == 0)
        {
            Debug.LogWarning("[CheckMenu] 자식 오브젝트가 없어 이름을 가져올 수 없습니다.");
            return;
        }
        
        if (sheet == null)
        {
            Debug.LogError("[CheckMenu] ReadSpreadSheet 참조가 없습니다. 인스펙터에 할당하거나 씬에 존재하는지 확인하세요.");
            return;
        }

        string ingredientName = transform.GetChild(0).name;

        // 토글
        isSelect = !isSelect;
        
        if(transform.parent.name == "CardStackParent")
        {
            // 색상 변경 (UI 표시)
            transform.GetChild(0).GetComponent<Image>().color = isSelect ? new Color32(220, 200, 200, 255)
                                   : new Color32(255, 255, 255, 255);
        }
        else
        {
            // 색상 변경 (UI 표시)
            image.color = isSelect ? new Color32(220, 200, 200, 255)
                                   : new Color32(255, 255, 255, 255);
        }
            

        //미네랄 차감/환급 + 선택목록 관리는 ReadSpreadSheet가 처리
        sheet.OnIngredientToggled(ingredientName, isSelect);

        Debug.Log("선택된 목록: " + string.Join(", ", selectedNames));
    }

    public void completeBtn()
    {
        if (completebtn == null || selectMenu == null)
        {
            Debug.LogWarning("[CheckMenu] completeBtn 또는 selectMenu가 할당되지 않아 메뉴 전환 불가.");
            return;
        }

        gameObject.SetActive(false); // 버튼 클릭하면 사라질 현재 UI
        completebtn.SetActive(false); // 버튼 클릭하면 사라질 현재 UI
        selectMenu.SetActive(true); // 버튼 클릭하면 다음으로 보이게 할 UI
    }
}
