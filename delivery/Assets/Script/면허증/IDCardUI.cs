using UnityEngine;
using UnityEngine.UI; // Image 컴포넌트 사용
using TMPro; // TextMeshPro 사용

public class IDCardUI : MonoBehaviour
{
    [Header("UI 연결")]
    public Image eyeImage;
    public Image mouthImage;
    public Image noseImage;
    public Image earImage;
    public TextMeshProUGUI idText;

    // [수정 포인트] 두 개의 매개변수(FaceData, string)를 받도록 정의되어 있어야 합니다.
    public void ShowCard(FaceData data, string idNumber)
    {
        gameObject.SetActive(true); // 꺼져있던 면허증 UI를 켭니다.

        // 전달받은 스프라이트들을 UI 이미지에 적용
        if (eyeImage != null) eyeImage.sprite = data.eye;
        if (mouthImage != null) mouthImage.sprite = data.mouth;
        if (noseImage != null) noseImage.sprite = data.nose;
        if (earImage != null) earImage.sprite = data.ear;

        // 전달받은 ID 번호를 텍스트에 적용
        if (idText != null) idText.text = idNumber;
    }

    // 닫기 버튼에 연결할 함수
    public void CloseCard()
    {
        gameObject.SetActive(false);
    }
}