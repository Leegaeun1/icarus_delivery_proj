using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Random_face : MonoBehaviour
{
    public Sprite[] eyes;                 // 눈 후보 스프라이트
    public SpriteRenderer eyesRenderer;   // 눈 SpriteRenderer
    public Vector3 desiredScale = new Vector3(1f, 1f, 1f); // 원하는 크기
    public Vector3 eyeOffset = new Vector3(0f, 0.5f, 0f);

    void Start()
    {
        // 시작 시 눈 숨기기
        if (eyesRenderer != null)
            eyesRenderer.enabled = false;
    }

    // 버튼에 연결할 함수
    public void ShowRandomEyes()
    {
        if (eyes.Length > 0 && eyesRenderer != null)
        {
            int idx = Random.Range(0, eyes.Length);
            eyesRenderer.sprite = eyes[idx];

            // 크기 조절
            eyesRenderer.transform.localScale = desiredScale;
            eyesRenderer.transform.localPosition = eyeOffset;

            // 렌더러 켜기
            eyesRenderer.enabled = true;
        }
    }
}
