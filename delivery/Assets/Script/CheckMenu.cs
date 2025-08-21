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

    private void Awake()
    {
        image = GetComponent<Image>();
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

        string thisname = transform.GetChild(0).name;
        print(thisname);

        if (isSelect)
        {
            isSelect = false;
            selectedNames.Remove(thisname);
            image.color = new Color32(255, 255, 255, 255);
        }
        else
        {
            isSelect = true;
            if (!selectedNames.Contains(thisname))
                selectedNames.Add(thisname);
            image.color = new Color32(220, 200, 200, 255);
        }

        Debug.Log("선택된 목록: " + string.Join(", ", selectedNames));
    }

    public void completeBtn()
    {
        if (completebtn == null || selectMenu == null)
        {
            Debug.LogWarning("[CheckMenu] completeBtn 또는 selectMenu가 할당되지 않아 메뉴 전환 불가.");
            return;
        }

        gameObject.SetActive(false);
        completebtn.SetActive(false);
        selectMenu.SetActive(true);
    }
}
