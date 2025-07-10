using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI 관련 네임스페이스 추가

public class click : MonoBehaviour
{
    public Button checkButton;  // 버튼 참조
    public GameObject imageToShow;  // 보여줄 이미지 (GameObject로 참조)

    // Start is called before the first frame update
    void Start()
    {
        // 버튼에 클릭 이벤트 추가
        checkButton.onClick.AddListener(OnCheckButtonClick);

        // 처음에는 이미지가 보이지 않도록 설정
        imageToShow.SetActive(false);
    }

    // 버튼 클릭 시 실행되는 함수
   public void OnCheckButtonClick()
    {
        // 이미지를 토글하여 보이거나 숨깁니다.
        imageToShow.SetActive(!imageToShow.activeSelf);
    }

    // Update is called once per frame
    void Update()
    {
        // 매 프레임마다 할 일이 없다면 비워두어도 됩니다.
    }
}
