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

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void player_select()
    {
        string thisname = transform.GetChild(0).name;
        print(thisname);

        if (isSelect)
        {
            isSelect = false;
            selectedNames.Remove(thisname);
            image.color = new Color32(255, 255, 255, 255); // 원래 색
        }
        else
        {
            isSelect = true;
            selectedNames.Add(thisname);
            image.color = new Color32(220, 200, 200, 255); // 선택된 색
        }

        Debug.Log("선택된 목록: " + string.Join(", ", selectedNames));
    }

    public void completeBtn()
    {
        this.gameObject.SetActive(false);
        selectMenu.gameObject.SetActive(true);
    }
}
