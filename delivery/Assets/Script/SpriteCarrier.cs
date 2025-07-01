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
            DontDestroyOnLoad(gameObject); // �� �Ѿ�� �� �����
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
