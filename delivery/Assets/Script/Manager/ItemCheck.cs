using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemCheck : MonoBehaviour
{
    private static ItemCheck selectedCookie = null; // 현재 선택된 쿠키 버튼
    private static ItemCheck selectedDrink = null;  // 현재 선택된 음료 버튼
    private static ItemCheck selectedSpecial = null;  // 현재 선택된 음료 버튼
    private bool isSelect = false;
    private Image image;

    [SerializeField] 
    public ReadSpreadSheets sheet;

    private void Start()
    {
        image = GetComponent<Image>();

        if (sheet == null)
        {
            // 인스턴스가 아직 할당되지 않았다면 찾아옴
            sheet = ReadSpreadSheets.Instance;

            // 만약 인스턴스가 아직도 null이라면 (정말 드문 경우) 직접 오브젝트를 찾아봄
            if (sheet == null)
            {
                sheet = FindObjectOfType<ReadSpreadSheets>();
            }
        }

        if (sheet != null)
        {
            Debug.Log($"{this.name}: ReadSpreadSheets 연결 성공");
        }
        else
        {
            Debug.LogError($"{this.name}: ReadSpreadSheets를 찾을 수 없습니다!");
        }
    }



    public void select()
    {
        string name = gameObject.transform.GetChild(0).name; // 선택된 재료의 이름

        if (name.Contains("cookie")) // 쿠키일 때
        {
            HandleSelection(ref selectedCookie, name); // ref는 함수 내부에서 값을 변경해도 원래 변수의 값도 함께 변경되도록 함.
        }
        else if (name.Contains("drink")) // 쉐이크일 때
        {
            HandleSelection(ref selectedDrink, name);
        }

    }
    private void HandleSelection(ref ItemCheck selectedItem, string name)
    {
        // 다른 버튼이 선택되어 있으면 해제
        if (selectedItem != null && selectedItem != this)
        {
            selectedItem.Deselect();
        }

        // 현재 버튼 상태 토글
        isSelect = !isSelect;

        if (isSelect)
        {
            selectedItem = this;
        }
        else
        {
            selectedItem = null;
        }

        // 스프레드시트 반영
        sheet.OnIngredientToggled(name, isSelect);

        // 색상 반영
        image.color = isSelect ? new Color32(220, 200, 200, 255)
                               : new Color32(255, 255, 255, 255);
    }

    private void Deselect() // 선택 해제
    {
        if (!isSelect) return;
        isSelect = false;
        image.color = new Color32(255, 255, 255, 255); // 원래 색으로 돌림
        string name = gameObject.transform.GetChild(0).name; 
        sheet.OnIngredientToggled(name, false); // 스프레드시트 반영
    }
}
