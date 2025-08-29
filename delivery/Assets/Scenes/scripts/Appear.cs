using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Appear : MonoBehaviour
{
    public int steps = 10;                // 총 단계 수
    public int framesPerStep = 3;         // 단계당 프레임 수
    public Vector3 moveOffset = new Vector3(-1f, 0, 0);

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Vector3 startPos;
    private Vector3 targetPos;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 원래 색 저장 및 검은색으로 설정
        originalColor = spriteRenderer.color;
        spriteRenderer.color = new Color(0f, 0f, 0f, 1f);

        startPos = transform.position;
        targetPos = startPos + moveOffset;

        StartCoroutine(SteppedAppearRoutine());
    }

    IEnumerator SteppedAppearRoutine()
    {
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;

            // 색상: 검은색 → 원래 색상
            spriteRenderer.color = Color.Lerp(Color.black, originalColor, t);

            // 위치: 시작 → 목표 위치
            transform.position = Vector3.Lerp(startPos, targetPos, t);

            // framesPerStep 만큼 프레임 대기
            for (int f = 0; f < framesPerStep; f++)
                yield return new WaitForEndOfFrame();
        }

        // 최종 보정
        spriteRenderer.color = originalColor;
        transform.position = targetPos;
    }
}