using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CheckMenu : MonoBehaviour
{
    public static List<string> selectedNames = new List<string>();
    private Image image;
    private bool isSelect = false;

    // UI 및 매니저
    public GameObject selectMenu;
    public GameObject completebtn;
    [SerializeField] private ReadSpreadSheets sheet;
    public MenuManager menuManager;
    public TimeManager timeManager;
    public TextMeshProUGUI money_effect;
    public Sprite errorbtn;
    public Sprite combtn;


    public float fadeSpeed = 1f;

    private void Awake()
    {
        if (image == null) image = GetComponent<Image>();
        if (sheet == null) sheet = GameObject.Find("sheet").GetComponent<ReadSpreadSheets>();
    }

    private void Start()
    {

        // 매니저 찾기
        if (menuManager == null) menuManager = FindObjectOfType<MenuManager>();
        if (timeManager == null) timeManager = FindObjectOfType<TimeManager>();
        money_effect = GameObject.Find("money_effect").GetComponent<TextMeshProUGUI>();

        if (image == null && transform.childCount > 0)
        {
            // 이미지가 없을 경우 자식에서 찾기 시도 (구조에 따라 다름)
            var childImg = transform.GetChild(0).GetComponent<Image>();
            if (childImg != null) image = childImg;
        }

        // 게임 시작 시, 내가 이미 선택된 목록에 있는지 확인하여 UI 갱신
        if (transform.childCount > 0)
        {
            string myName = transform.GetChild(0).name;
            if (selectedNames.Contains(myName))
            {
                isSelect = true;
                UpdateColor(); // 색상 켜기
            }
        }
    }

    // 색상 변경 로직을 함수로 분리
    void UpdateColor()
    {
        Color selectedColor = new Color32(220, 200, 200, 255);
        Color defaultColor = new Color32(255, 255, 255, 255);
        Color targetColor = isSelect ? selectedColor : defaultColor;

        if (transform.parent.name == "CardStackParent")
        {
            if (transform.childCount > 0)
                transform.GetChild(0).GetComponent<Image>().color = targetColor;
        }
        else if (image != null)
        {
            image.color = targetColor;
        }
    }

    public void player_select()
    {
        if (transform.childCount == 0 || sheet == null) return;

        string ingredientName = transform.GetChild(0).name;

        isSelect = !isSelect;
        UpdateColor(); // 색상 변경

        sheet.OnIngredientToggled(ingredientName, isSelect);
        Debug.Log("선택된 목록: " + string.Join(", ", selectedNames));
    }

    public void completeBtn()
    {
        if (completebtn == null || selectMenu == null || timeManager == null) return;

        bool isSuccess = sheet.ApplyPurchase();

        if (isSuccess)
        {
            completebtn.GetComponent<Image>().sprite = combtn;
            gameObject.SetActive(false);
            completebtn.SetActive(false);
            selectMenu.SetActive(true);


            if (menuManager != null) menuManager.CreateCardStack();

            // 타임어택 시작
            if (!timeManager.isRunning)
                timeManager.StartTimeAttack();
            else // 타임어택 중이면
                timeManager.StopTimeAttack();

            Debug.Log("결제 성공! 타임어택 시작 (1번째 클릭)");
        }
        else
        {
            completebtn.GetComponent<Image>().sprite = errorbtn;
            Debug.Log("돈이 부족합니다.");
        }
    }
}