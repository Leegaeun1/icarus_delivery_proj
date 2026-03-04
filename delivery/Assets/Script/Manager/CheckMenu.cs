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


    //public ReadSpreadSheets sheet;


    private ReadSpreadSheets _sheet;
    public ReadSpreadSheets sheet
    {
        get
        {
            if (_sheet == null)
            {
                _sheet = ReadSpreadSheets.Instance;
                if (_sheet == null)
                {
                    _sheet = FindObjectOfType<ReadSpreadSheets>();
                }
            }
            return _sheet;
        }
    }




    public MenuManager menuManager;
    public TimeManager timeManager;
    public TextMeshProUGUI money_effect;
    public Sprite errorbtn;
    public Sprite combtn;


    public float fadeSpeed = 1f;

    private void Awake()
    {
       
    }

    // CheckMenu.cs

    public void SaveSelectedMenuToManager()
    {
        if (GameManager.Instance == null) return;
        // _sheet가 연결 안 되어있을 경우를 대비해 프로퍼티 호출
        if (_sheet == null) _sheet = this.sheet;

        if (_sheet != null)
        {
            // 선택된 메뉴 리스트를 복사하여 전달
            _sheet.currentRequestName = new List<string>(selectedNames);
        }

        // 포장지(StringList) 생성
        DayFinishManager.StringList allItems = new DayFinishManager.StringList();

        // selectedNames에 있는 걸 통째로 복사해서 넣음
        allItems.ingredients = new List<string>(selectedNames);

        // GameManager의 PendingFinalIngredients에 추가
        GameManager.Instance.PendingFinalIngredients.Add(allItems);

        // 2. 주문서(요청사항) 저장 

        // (A) 포함 요청 (Include)
        DayFinishManager.StringList incList = new DayFinishManager.StringList();
        if (_sheet != null)
        {
            incList.ingredients = new List<string>(_sheet.CurrentRequest_Include);
        }
        GameManager.Instance.PendingIncludeRequest.Add(incList);

        // (B) 제외 요청 (Exclude)
        DayFinishManager.StringList excList = new DayFinishManager.StringList();
        if (_sheet != null)
        {
            excList.ingredients = new List<string>(_sheet.CurrentRequest_Exclude);
        }
        GameManager.Instance.PendingExcludeRequest.Add(excList);

        Debug.Log($"[저장 완료] 분류 없이 총 {allItems.ingredients.Count}개의 항목을 FinalIngredients에 저장했습니다.");
    }
    private void OnEnable()
    {
        if (image == null) image = GetComponent<Image>();

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

        if (transform.childCount > 0)
        {
            string myName = transform.GetChild(0).name;

            // 만약 리스트에 내 이름이 있으면 켜고, 없으면 끈다.
            // 3일차에 들어오면 리스트가 비어있을 테니, 자연스럽게 꺼지게 됨.
            if (selectedNames.Contains(myName))
            {
                isSelect = true;
            }
            UpdateColor();
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

        string ingredientName = (transform.childCount > 0) ? transform.GetChild(0).name : gameObject.name;
        ingredientName = ingredientName.Replace("(Clone)", "").Trim();
        string finalName = ingredientName;

        if (sheet == null) _sheet = ReadSpreadSheets.Instance;
        if (transform.childCount == 0 || sheet == null) return;

        if (transform.parent != null && transform.parent.name == "CardStackParent")
        {
            finalName = ingredientName + "_sand";
            if (!selectedNames.Contains(finalName)) selectedNames.Add(finalName);
        }

        

        ingredientName = transform.GetChild(0).name;

        isSelect = !isSelect;
        UpdateColor(); // 색상 변경

        sheet.OnIngredientToggled(ingredientName, isSelect);
        Debug.Log($"[클릭 감지] {ingredientName} | 선택 목록: {string.Join(", ", selectedNames)}");
    }

    public IEnumerator StartCardStack()
    {
        // 1. 메뉴판을 켭니다.
        selectMenu.SetActive(true);
        // 2. 카드를 새로 생성합니다.
        if (menuManager != null) yield return menuManager.StartCoroutine(menuManager.CreateCardStack());

        // 3. 타이머를 시작합니다.
        timeManager.StartTimeAttack();
        
    }

    public void completeBtn()
    {
        if (sheet == null) _sheet = ReadSpreadSheets.Instance;

        if (completebtn == null || selectMenu == null || timeManager == null || sheet == null) return;

        bool isSuccess = sheet.ApplyPurchase();

        if (isSuccess)
        {
            //SaveSelectedMenuToManager();
            completebtn.GetComponent<Image>().sprite = combtn;
            gameObject.SetActive(false);
            completebtn.SetActive(false);

            if (!timeManager.isRunning)
            {
                if (menuManager != null)
                {
                    menuManager.StartCoroutine(StartCardStack());
                }
                else
                {
                    Debug.LogError("MenuManager가 없어서 게임을 시작할 수 없습니다.");
                }

                Debug.Log(">> 게임 시작! (카드 생성 O, 메뉴판 ON)");
            }
            else
            {
                // 1. 타이머를 멈춥니다.
                timeManager.StopTimeAttack();
                SaveSelectedMenuToManager();
                Debug.Log(">> 게임 정지! (카드 생성 X)");
            }

        }
        else
        {
            completebtn.GetComponent<Image>().sprite = errorbtn;
            Debug.Log("돈이 부족합니다.");
        }

    }
}