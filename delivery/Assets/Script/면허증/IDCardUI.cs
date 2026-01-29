using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IDCardUI : MonoBehaviour
{
    [Header("UI 연결")]
    public Image eyeImage;
    public Image mouthImage;
    public Image noseImage;
    public Image earImage;
    public TextMeshProUGUI idText;

    // 면허증 창을 켜고 끄는 함수
    public void ShowCard(FaceData data, string idNumber)
    {
        gameObject.SetActive(true); // 면허증 활성화

        // NPC의 스프라이트를 UI 이미지에 할당
        if (eyeImage != null) eyeImage.sprite = data.eye;
        if (mouthImage != null) mouthImage.sprite = data.mouth;
        if (noseImage != null) noseImage.sprite = data.nose;
        if (earImage != null) earImage.sprite = data.ear;

        if (idText != null) idText.text = "ID: " + idNumber;
    }

    public void CloseCard()
    {
        gameObject.SetActive(false); // 면허증 닫기
    }
}
