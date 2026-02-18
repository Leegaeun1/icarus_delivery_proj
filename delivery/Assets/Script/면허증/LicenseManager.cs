using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LicenseInfoManager : MonoBehaviour
{
    public static LicenseInfoManager Instance;

    [Header("모니터링 및 텍스트 필드")]
    public Transform manContainer;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI birthText;
    public TextMeshProUGUI companyText;
    public TextMeshProUGUI issueDateText;

    [Header("랜덤 데이터 리스트")]
    public List<string> lastNames;
    public List<string> firstNames;
    public List<string> companies;

    [Header("페이지 및 사진 설정")]
    public GameObject certificationGroup;
    public GameObject infoGroup;
    public GameObject photoContainer;
    public GameObject deliveryPage;
    public GameObject checkListPhoto;

    [Header("배달원 정보 필드 (뒷면/진실)")]
    public TextMeshProUGUI clNameText;
    public TextMeshProUGUI clBirthText;
    public TextMeshProUGUI clCompanyText;
    public TextMeshProUGUI clDateText;

    // 데이터 저장용
    private string realName, realBirth, realCompany, realDate;
    public bool isPhotoForgery { get; private set; } // 사진 위조 여부
    private GameObject lastDetectedMan;

    void Awake() { Instance = this; }

    void Update()
    {
        if (manContainer.childCount > 0)
        {
            GameObject currentMan = manContainer.GetChild(0).gameObject;
            if (currentMan != lastDetectedMan)
            {
                UpdateLicenseInfo();
                lastDetectedMan = currentMan;
                ResetToFrontPage();
            }
        }
        else
        {
            if (lastDetectedMan != null) ClearInfo();
            lastDetectedMan = null;
        }
    }

    void UpdateLicenseInfo()
    {
        isPhotoForgery = false; // 초기화

        // 1. 진짜 데이터 생성
        realName = $"{lastNames[Random.Range(0, lastNames.Count)]} {firstNames[Random.Range(0, firstNames.Count)]}";
        realBirth = $"{Random.Range(1980, 2006)}.{Random.Range(1, 13):D2}.{Random.Range(1, 29):D2}";
        realCompany = companies[Random.Range(0, companies.Count)];
        realDate = $"{Random.Range(2020, 2026)}.{Random.Range(1, 13):D2}.{Random.Range(1, 29):D2}";

        // UI 기본 세팅
        nameText.text = realName;
        birthText.text = realBirth;
        companyText.text = realCompany;
        issueDateText.text = realDate;

        // 2. 20% 확률로 위조 로직 실행
        if (Random.value < 0.9f)
        {
            ApplyAdvancedForgery();
        }
    }

    void ApplyAdvancedForgery()
    {
        int errorCount = Random.Range(1, 3); // 1~2개 항목 위조
        List<int> targetFields = new List<int> { 0, 1, 2, 3, 4 }; // 이름, 생일, 업체, 날짜, 사진

        for (int i = 0; i < errorCount; i++)
        {
            int randomIndex = Random.Range(0, targetFields.Count);
            int fieldIndex = targetFields[randomIndex];
            targetFields.RemoveAt(randomIndex);

            switch (fieldIndex)
            {
                case 0: // 이름 위조: 리스트에서 다른 이름 조합
                    string fakeName;
                    do
                    {
                        fakeName = $"{lastNames[Random.Range(0, lastNames.Count)]} {firstNames[Random.Range(0, firstNames.Count)]}";
                    } while (fakeName == realName);
                    nameText.text = fakeName;
                    break;

                case 1: // 생년월일 위조: 다른 날짜 생성
                    string fakeBirth;
                    do
                    {
                        fakeBirth = $"{Random.Range(1980, 2006)}.{Random.Range(1, 13):D2}.{Random.Range(1, 29):D2}";
                    } while (fakeBirth == realBirth);
                    birthText.text = fakeBirth;
                    break;

                case 2: // 업체 위조: 리스트에서 다른 업체 선택
                    string fakeCo;
                    do
                    {
                        fakeCo = companies[Random.Range(0, companies.Count)];
                    } while (fakeCo == realCompany);
                    companyText.text = fakeCo;
                    break;

                case 3: // 날짜 위조
                    issueDateText.text = $"{Random.Range(2020, 2026)}.{Random.Range(1, 13):D2}.{Random.Range(1, 29):D2}";
                    break;

                case 4: // 사진 위조 플래그 설정
                    isPhotoForgery = true;
                    break;
            }
        }
    }

    public void ShowDeliveryWorkerInfo()
    {
        // 뒷면(CheckList) 페이지 활성화 시 무조건 '진짜' 정보만 표시
        if (certificationGroup) certificationGroup.SetActive(false);
        if (infoGroup) infoGroup.SetActive(false);
        if (photoContainer) photoContainer.SetActive(false);

        if (deliveryPage)
        {
            deliveryPage.SetActive(true);
            if (checkListPhoto) checkListPhoto.SetActive(true);

            clNameText.text = realName;
            clBirthText.text = realBirth;
            clCompanyText.text = realCompany;
            clDateText.text = realDate;
        }
    }

    // [Prev 버튼 또는 초기화 시 실행] 다시 면허증으로 전환
    public void ResetToFrontPage()
    {
        // 1. 면허증 그룹 및 사진 활성화
        if (certificationGroup != null) certificationGroup.SetActive(true);
        if (infoGroup != null) infoGroup.SetActive(true);
        if (photoContainer != null) photoContainer.SetActive(true);

        // 2. 배달원 정보창 및 사진 비활성화
        if (deliveryPage != null) deliveryPage.SetActive(false);
        if (checkListPhoto != null) checkListPhoto.SetActive(false);
    }

    void ClearInfo()
    {
        if (nameText != null) nameText.text = "";
        if (birthText != null) birthText.text = "";
        if (companyText != null) companyText.text = "";
        if (issueDateText != null) issueDateText.text = "";
    }
}