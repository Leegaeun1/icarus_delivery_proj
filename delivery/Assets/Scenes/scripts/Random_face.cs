using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Random_face : MonoBehaviour
{
    // [기존 변수 유지]
    public Sprite[] eyes;
    public SpriteRenderer eyesRenderer;

    public Sprite[] mouths;
    public SpriteRenderer mouthRenderer;

    public Sprite[] ears;
    public SpriteRenderer earsRenderer;

    public Sprite[] noses;
    public SpriteRenderer noseRenderer;

    public Vector3 desiredScale = new Vector3(1f, 1f, 1f);
    public Vector3 eyeOffset = new Vector3(0f, 0.5f, 0f);

    // [추가된 위치 변수] - 인스펙터에서 각 부위의 위치를 맞추기 위해 사용합니다.
    public Vector3 mouthOffset = new Vector3(0f, -0.3f, 0f);
    public Vector3 noseOffset = new Vector3(0f, 0.1f, 0f);
    public Vector3 earOffset = new Vector3(0f, 0f, 0f);

    public FaceData GetCurrentFaceData()
    {
        FaceData data = new FaceData();
        data.eye = eyesRenderer.sprite;
        data.mouth = mouthRenderer.sprite;
        data.nose = noseRenderer.sprite;
        data.ear = earsRenderer.sprite;
        return data;
    }

    void Start()
    {
        // 시작 시 모든 부위 숨기기
        if (eyesRenderer != null) eyesRenderer.enabled = false;
        if (mouthRenderer != null) mouthRenderer.enabled = false;
        if (earsRenderer != null) earsRenderer.enabled = false;
        if (noseRenderer != null) noseRenderer.enabled = false;
    }

    // 눈 랜덤 출력 (기존 유지)
    public void ShowRandomEyes()
    {
        if (eyes.Length > 0 && eyesRenderer != null)
        {
            int idx = Random.Range(0, eyes.Length);
            eyesRenderer.sprite = eyes[idx];
            eyesRenderer.transform.localScale = desiredScale;
            eyesRenderer.transform.localPosition = eyeOffset;
            eyesRenderer.enabled = true;
        }
    }

    // [추가] 입 랜덤 출력
    public void ShowRandomMouth()
    {
        if (mouths.Length > 0 && mouthRenderer != null)
        {
            int idx = Random.Range(0, mouths.Length);
            mouthRenderer.sprite = mouths[idx];
            mouthRenderer.transform.localScale = desiredScale;
            mouthRenderer.transform.localPosition = mouthOffset; // 입 오프셋 적용
            mouthRenderer.enabled = true;
        }
    }

    // [추가] 코 랜덤 출력
    public void ShowRandomNose()
    {
        if (noses.Length > 0 && noseRenderer != null)
        {
            int idx = Random.Range(0, noses.Length);
            noseRenderer.sprite = noses[idx];
            noseRenderer.transform.localScale = desiredScale;
            noseRenderer.transform.localPosition = noseOffset; // 코 오프셋 적용
            noseRenderer.enabled = true;
        }
    }

    // [추가] 귀 랜덤 출력
    public void ShowRandomEars()
    {
        if (ears.Length > 0 && earsRenderer != null)
        {
            int idx = Random.Range(0, ears.Length);
            earsRenderer.sprite = ears[idx];
            earsRenderer.transform.localScale = desiredScale;
            earsRenderer.transform.localPosition = earOffset; // 귀 오프셋 적용
            earsRenderer.enabled = true;
        }
    }

    // [추가] 모든 얼굴 부위를 한 번에 랜덤으로 생성하는 함수
    public void ShowAllRandomParts()
    {
        ShowRandomEyes();
        ShowRandomMouth();
        ShowRandomNose();
        ShowRandomEars();
    }
}