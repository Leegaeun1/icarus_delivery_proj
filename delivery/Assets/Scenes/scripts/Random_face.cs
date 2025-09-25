using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Random_face : MonoBehaviour
{
    // 눈
    public Sprite[] eyes;
    public SpriteRenderer eyesRenderer;

    // 입
    public Sprite[] mouths;
    public SpriteRenderer mouthRenderer;

    // 귀
    public Sprite[] ears;
    public SpriteRenderer earsRenderer;

    // 코
    public Sprite[] noses;
    public SpriteRenderer noseRenderer;

    // 기본 설정값
    public Vector3 desiredScale = new Vector3(1f, 1f, 1f);
    public Vector3 eyeOffset = new Vector3(0f, 0.5f, 0f);

    void Start()
    {
        // 시작 시 눈 숨기기
        if (eyesRenderer != null)
            eyesRenderer.enabled = false;

        // 아직 기능 없지만, 미리 꺼둬도 됨
        if (mouthRenderer != null)
            mouthRenderer.enabled = false;
        if (earsRenderer != null)
            earsRenderer.enabled = false;
        if (noseRenderer != null)
            noseRenderer.enabled = false;
    }

    // 버튼에 연결할 함수 (현재는 눈만)
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

    // 나중에 입/귀/코 랜덤으로 뽑는 함수도 여기 추가 가능
}
