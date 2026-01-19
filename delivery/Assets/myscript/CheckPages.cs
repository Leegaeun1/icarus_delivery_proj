using UnityEngine;
using UnityEngine.UI;

public class PageController : MonoBehaviour
{
    [Header("UI 연결")]
    public Button prevPageButton; // 좌측 화살표 버튼
    public Button nextPageButton; // 우측 화살표 버튼

    [Header("페이지 설정")]
    public int currentPage = 0;
    public int totalPages = 3; 
    void Start()
    {
        // 시작할 때 버튼 상태를 한 번 업데이트합니다.
        UpdatePageButtons();
    }

    void UpdatePageButtons()
    {
        // 이전 페이지 버튼: 현재 페이지가 0보다 클 때만 활성화
        if (prevPageButton != null)
            prevPageButton.gameObject.SetActive(currentPage > 0);

        // 다음 페이지 버튼: 현재 페이지가 (전체 페이지 - 1)보다 작을 때만 활성화
        if (nextPageButton != null)
            nextPageButton.gameObject.SetActive(currentPage < totalPages - 1);
    }

    public void NextPage()
    {
        if (currentPage < totalPages - 1)
        {
            currentPage++;
            UpdateOrderUI(); // 화면의 내용을 갱신하는 함수
            UpdatePageButtons(); // 버튼 활성화 상태 갱신
            Debug.Log($"다음 페이지: {currentPage + 1}");
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdateOrderUI(); // 화면의 내용을 갱신하는 함수
            UpdatePageButtons(); // 버튼 활성화 상태 갱신
            Debug.Log($"이전 페이지: {currentPage + 1}");
        }
    }

    void UpdateOrderUI()
    {
        // 예: 리스트에서 특정 범위의 아이템만 보여주기 등
    }

    void Update()
    {
        // 스페이스바를 누르면 새로운 주문 생성 (기존 코드 유지)
        if (Input.GetKeyDown(KeyCode.Space))
            // CreateNewOrder(); // 이 함수가 정의되어 있어야 합니다.

            // 추가 팁: 키보드 방향키로도 페이지를 넘기고 싶다면?
            if (Input.GetKeyDown(KeyCode.LeftArrow)) PrevPage();
        if (Input.GetKeyDown(KeyCode.RightArrow)) NextPage();
    }
}