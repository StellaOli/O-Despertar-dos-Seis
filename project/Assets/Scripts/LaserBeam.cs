using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [Header("Configuração")]
    public float growSpeed = 20f; // Velocidade que o laser cresce
    public float maxLength = 15f; // Comprimento máximo

    private SpriteRenderer sr;
    private BoxCollider2D boxCollider;
    private float currentLength = 0f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        // Força o Is Trigger, foda-se o Inspector
        if (boxCollider != null && !boxCollider.isTrigger)
        {
            Debug.LogWarning($"Collider no prefab {name} não era Trigger! Forçando 'Is Trigger = true'.");
            boxCollider.isTrigger = true; 
        }

        // Começa minúsculo
        UpdateLaserSize(0.01f);
    }

    void Update()
    {
        // Só cresce até o máximo
        if (currentLength < maxLength)
        {
            currentLength += growSpeed * Time.deltaTime;
            currentLength = Mathf.Min(currentLength, maxLength); 
            UpdateLaserSize(currentLength);
        }
    }

    // Função que atualiza o visual e o collider
    void UpdateLaserSize(float length)
    {
        if (sr != null) 
        {
            sr.size = new Vector2(length, sr.size.y);
        }

        if (boxCollider != null)
        {
            boxCollider.size = new Vector2(length, boxCollider.size.y);
            boxCollider.offset = new Vector2(length / 2f, boxCollider.offset.y);
        }
    }
}