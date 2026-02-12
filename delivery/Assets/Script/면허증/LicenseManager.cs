using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LicenseInfoManager : MonoBehaviour
{
    [Header("모니터링 대상")]
    public Transform manContainer;

    [Header("면허증 텍스트 필드")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI birthText;
    public TextMeshProUGUI companyText;
    public TextMeshProUGUI issueDateText;

    [Header("이름 구성 리스트")]
    public List<string> lastNames = new List<string>();  // 성
    public List<string> firstNames = new List<string>(); // 이름

    [Header("소속 업체 리스트")]
    public List<string> companies = new List<string>();  // 여러 업체명을 입력하세요.

    private GameObject lastDetectedMan;

    void Update()
    {
        if (manContainer.childCount > 0)
        {
            GameObject currentMan = manContainer.GetChild(0).gameObject;

            if (currentMan != lastDetectedMan)
            {
                UpdateLicenseInfo();
                lastDetectedMan = currentMan;
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
        // 1. 성 + 이름 무작위 조합
        if (nameText != null && lastNames.Count > 0 && firstNames.Count > 0)
        {
            string lastName = lastNames[Random.Range(0, lastNames.Count)];
            string firstName = firstNames[Random.Range(0, firstNames.Count)];
            nameText.text = $"{lastName} {firstName}";
        }

        // 2. 소속 업체 무작위 선택
        if (companyText != null && companies.Count > 0)
        {
            int randomIndex = Random.Range(0, companies.Count);
            companyText.text = companies[randomIndex];
        }

        // 3. 생년월일 랜덤 생성 (1980~2005년)
        if (birthText != null)
        {
            int year = Random.Range(1980, 2006);
            int month = Random.Range(1, 13);
            int day = Random.Range(1, 29);
            birthText.text = $"{year}.{month:D2}.{day:D2}";
        }

        // 4. 발급일자 랜덤 생성 (2020~2025년)
        if (issueDateText != null)
        {
            int year = Random.Range(2020, 2026);
            int month = Random.Range(1, 13);
            int day = Random.Range(1, 29);
            issueDateText.text = $"{year}.{month:D2}.{day:D2}";
        }
    }

    void ClearInfo()
    {
        if (nameText != null) nameText.text = "NONE";
        if (birthText != null) birthText.text = "-";
        if (companyText != null) companyText.text = "-";
        if (issueDateText != null) issueDateText.text = "-";
    }
}