using UnityEngine;

public class SpriteCarrier : MonoBehaviour
{
    public static SpriteCarrier Instance;
    public Sprite carriedSprite;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ¾À ³Ñ¾î°¡µµ ¾È »ç¶óÁü
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
