using System.Collections;
using System.Collections;
using UnityEngine;

public class AlienAppearStepped : MonoBehaviour
{
    public Sprite alienSprite;
    public int steps = 10;
    public int framesPerStep = 2;
    public Vector3 spawnPosition = new Vector3(3f, 1.5f, 0f);
    public Vector3 moveOffset = new Vector3(-1f, 0f, 0f);
    public Vector3 desiredScale = new Vector3(2f, 2f, 1f);

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Vector3 startPos;
    private Vector3 targetPos;

    public SpeechBubble speech;
    public OrderManager orderManager; // 새로 연결할 OrderManager 참조

    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = alienSprite;
        spriteRenderer.enabled = false;

        transform.localScale = desiredScale;
        transform.position = spawnPosition;

        spriteRenderer.color = Color.black;
        originalColor = Color.white;
    }

    // 버튼에서 실행
    public void SummonAlien()
    {
        spriteRenderer.enabled = true;

        startPos = spawnPosition;
        targetPos = spawnPosition + moveOffset;
        transform.position = startPos;
        transform.localScale = desiredScale;
        spriteRenderer.color = Color.black;

        StartCoroutine(SteppedFadeInMove());
    }

    IEnumerator SteppedFadeInMove()
    {
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            spriteRenderer.color = Color.Lerp(Color.black, originalColor, t);

            for (int f = 0; f < framesPerStep; f++)
                yield return new WaitForEndOfFrame();
        }

        transform.position = targetPos;
        spriteRenderer.color = originalColor;

        
        string orderText = orderManager.GetRandomOrderText();
        speech.ShowMessage(orderText, 3f);
    }
}
