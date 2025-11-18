using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LicenseChecker : MonoBehaviour
{
    // === 인스펙터 연결 필수 항목 ===
    public GameObject licenseCheckPanel;
    public RectTransform licenseBoxRect;    // LicenseBox의 RectTransform
    public GameObject alienFacePrefab;     // <--- 새로 연결할 외계인 얼굴 프리팹

    // 외계인 얼굴이 면허증에 표시될 때 적용할 크기
    public Vector3 scaleOnLicenseCheck = new Vector3(0.2f, 0.2f, 1f);

    // 복제된 외계인 인스턴스를 저장할 변수
    private GameObject clonedAlien;

    void Start()
    {
        if (licenseCheckPanel != null)
        {
            licenseCheckPanel.SetActive(false);
        }
    }

    // 버튼 클릭 시 호출
    public void ShowLicenseCheck()
    {
        if (licenseCheckPanel == null || alienFacePrefab == null)
        {
            Debug.LogError("LicenseChecker: 필요한 패널 또는 외계인 프리팹이 연결되지 않았습니다!");
            return;
        }

        // 1. 암전 패널 활성화
        licenseCheckPanel.SetActive(true);

        // 2. 외계인 복제 및 설정
        clonedAlien = Instantiate(alienFacePrefab);

        if (clonedAlien != null)
        {
            // 크기 조정
            clonedAlien.transform.localScale = scaleOnLicenseCheck;

            // 렌더링 순서 강제 조정 (암전 패널 위로)
            SpriteRenderer sr = clonedAlien.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // UI Depth (Layer)보다 높은 순위 부여 (예: 10)
                sr.sortingOrder = 10;
            }

            // 3. 위치 이동: LicenseBox의 중앙으로 이동
            if (licenseBoxRect != null)
            {
                // UI 위치를 WorldSpace로 변환하여 3D 캐릭터 위치에 할당
                Vector3 worldPos = licenseBoxRect.transform.position;
                clonedAlien.transform.position = worldPos;
            }

            // 원본 외계인 숨기기 (선택 사항)
            // 원본.gameObject.SetActive(false); 
        }
    }

    // 면허증 검사 후 '닫기' 버튼 등에 연결
    public void HideLicenseCheck()
    {
        if (licenseCheckPanel != null)
        {
            licenseCheckPanel.SetActive(false);
        }

        // 복제된 외계인 제거
        if (clonedAlien != null)
        {
            Destroy(clonedAlien);
            clonedAlien = null;
        }

        // 원본 외계인 다시 보이기 (선택 사항)
        // 원본.gameObject.SetActive(true); 
    }
}