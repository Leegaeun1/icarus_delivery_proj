using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraBtn : MonoBehaviour
{
    public Transform targetButton; // World Space 버튼
    public CinemachineVirtualCamera vcam;
    public float moveDuration = 0.5f;
    public float orthoSize = 5f; // 화면 가득 보여줄 크기

    private Vector3 originalPos;
    private float originalSize;

    private void Start()
    {
        if (vcam == null)
            vcam = FindObjectOfType<CinemachineVirtualCamera>();

        originalPos = vcam.transform.position;
        originalSize = vcam.m_Lens.OrthographicSize;
    }

    public void OnClick()
    {
        StartCoroutine(MoveCameraToTarget());
    }

    private IEnumerator MoveCameraToTarget()
    {
        float t = 0f;
        Vector3 startPos = vcam.transform.position;
        float startSize = vcam.m_Lens.OrthographicSize;
        Vector3 targetPos = new Vector3(targetButton.position.x, targetButton.position.y, startPos.z);

        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            vcam.transform.position = Vector3.Lerp(startPos, targetPos, t);
            vcam.m_Lens.OrthographicSize = Mathf.Lerp(startSize, orthoSize, t);
            yield return null;
        }

        // 잠시 기다린 후 원래 위치로 돌아가기
        yield return new WaitForSeconds(1f);

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            vcam.transform.position = Vector3.Lerp(targetPos, originalPos, t);
            vcam.m_Lens.OrthographicSize = Mathf.Lerp(orthoSize, originalSize, t);
            yield return null;
        }
    }
}
