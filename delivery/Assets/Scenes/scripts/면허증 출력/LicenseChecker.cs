using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LicenseChecker : MonoBehaviour
{
    // === 인스펙터 연결 필수 항목 ===
    public GameObject licenseCheckPanel;
    public RectTransform licenseBoxRect;
    public GameObject alienBodyPrefab;

    // 원본 캐릭터의 현재 Eye GameObject 참조
    public GameObject originalEyeObject; // <--- 새로 추가/연결해야 할 필드

    // 외계인 얼굴이 면허증에 표시될 때 적용할 크기
    public Vector3 scaleOnLicenseCheck = new Vector3(0.2f, 0.2f, 1f);

    private GameObject clonedAlien;
    private Camera mainCamera;

    void Start()
    {
        if (licenseCheckPanel != null)
        {
            licenseCheckPanel.SetActive(false);
        }
        mainCamera = Camera.main;
    }

    // 버튼 클릭 시 호출
    public void ShowLicenseCheck()
    {
        // 1. 초기 Null 체크
        if (licenseCheckPanel == null || alienBodyPrefab == null || licenseBoxRect == null || originalEyeObject == null)
        {
            Debug.LogError("LicenseChecker: 모든 필수 컴포넌트를 인스펙터에 연결해야 합니다.");
            return;
        }

        // 2. 암전 패널 활성화
        licenseCheckPanel.SetActive(true);

        // 3. 외계인 외형 (몸통) 복제
        clonedAlien = Instantiate(alienBodyPrefab);

        if (clonedAlien != null)
        {
            // 4. 크기 조정
            clonedAlien.transform.localScale = scaleOnLicenseCheck;

            // --- [핵심 복제 로직: Eye 오브젝트 복사 및 부착] ---

            // 4.1. 눈 GameObject 복제
            GameObject clonedEye = Instantiate(originalEyeObject);

            // 4.2. 복제된 눈을 복제된 외계인(clonedAlien)의 자식으로 설정
            clonedEye.transform.SetParent(clonedAlien.transform);

            // 4.3. 로컬 위치/회전/크기 복사 (눈이 몸통의 정확한 위치에 오도록 보장)
            // 원본 Eye 오브젝트의 Transform을 가져옴
            Transform originalEyeTransform = originalEyeObject.transform;

            clonedEye.transform.localPosition = originalEyeTransform.localPosition;
            clonedEye.transform.localRotation = originalEyeTransform.localRotation;
            clonedEye.transform.localScale = originalEyeTransform.localScale;

            // 4.4. 복사된 눈 강제 활성화 (숨겨져 있을 가능성 방지)
            clonedEye.SetActive(true);

            // 4.5. 눈의 렌더링 순서 조정 (몸통(10)보다 높게 11로 설정)
            SpriteRenderer eyeSr = clonedEye.GetComponent<SpriteRenderer>();
            if (eyeSr != null)
            {
                // clonedAlien의 sortingOrder보다 높게 설정하여 몸통 위에 그려지도록 함
                eyeSr.sortingOrder = 11;
            }
            // ----------------------------------------------------

            // 5. 몸통의 렌더링 순서 강제 조정
            SpriteRenderer bodySr = clonedAlien.GetComponent<SpriteRenderer>();
            if (bodySr != null)
            {
                bodySr.sortingOrder = 10;
            }

            // 6. 위치 이동: LicenseBox의 중앙으로 이동
            if (licenseBoxRect != null && mainCamera != null)
            {
                Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(mainCamera, licenseBoxRect.transform.position);

                Vector3 targetWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(
                    screenPoint.x,
                    screenPoint.y,
                    mainCamera.nearClipPlane + 0.5f
                ));

                clonedAlien.transform.position = targetWorldPos;
            }
        }
    }

    // '닫기' 버튼 등에 연결
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
    }
}