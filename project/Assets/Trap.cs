using UnityEngine;

public class Trap : MonoBehaviour
{
    [Header("Configuração da Trap")]
    public int damage = 20;
    public float damageInterval = 1f;
    public bool isContinuousDamage = false;

    [Header("Efeitos Visuais")]
    public Color damageColor = Color.red;
    public ParticleSystem trapEffect;

    private float lastDamageTime;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Garante que tem collider
        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogError("❌ Trap precisa de um Collider2D!");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("⚠️ Player entrou na trap!");

            // Dano instantâneo ao entrar
            ApplyDamage(other.gameObject);

            // Efeito visual
            StartCoroutine(TrapFlash());

            // Toca efeito de partícula
            if (trapEffect != null)
                trapEffect.Play();
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isContinuousDamage)
        {
            // Dano contínuo enquanto o player está na trap
            if (Time.time - lastDamageTime >= damageInterval)
            {
                ApplyDamage(other.gameObject);
                lastDamageTime = Time.time;
            }
        }
    }

    void ApplyDamage(GameObject player)
    {
        Player playerScript = player.GetComponent<Player>();
        if (playerScript != null)
        {
            playerScript.TakeDamage(damage);
            Debug.Log($"💥 Trap causou {damage} de dano no Player!");
        }
    }

    System.Collections.IEnumerator TrapFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(0.3f);
            spriteRenderer.color = originalColor;
        }
    }

    void OnDrawGizmos()
    {
        // Visualização da área da trap no Editor
        Gizmos.color = Color.red;
        if (GetComponent<BoxCollider2D>())
        {
            Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider2D>().bounds.size);
        }
        else if (GetComponent<CircleCollider2D>())
        {
            Gizmos.DrawWireSphere(transform.position, GetComponent<CircleCollider2D>().radius);
        }
    }
}