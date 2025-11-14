using UnityEngine;
using UnityEngine.SceneManagement; 
using System.Collections; 

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimento")]
    public float moveSpeed = 5f;
    private Rigidbody2D rb;

    [Header("Vidas e Respawn")]
    public int lives = 3;
    public float invulnerabilityDuration = 2f; 
    public int blinkCount = 3; 
    public float blinkSpeed = 0.2f; 

    private Vector3 spawnPoint; 
    private SpriteRenderer sr;
    private bool canMove = true;
    private bool isInvulnerable = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        
        spawnPoint = transform.position; 
    }

    void Update()
    {
        if (!canMove)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        float moveX = Input.GetAxisRaw("Horizontal"); 
        float moveY = Input.GetAxisRaw("Vertical"); 

        Vector2 movement = new Vector2(moveX, moveY).normalized; 
        rb.velocity = movement * moveSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Se encostou no Laser E NÃO ESTÁ INVULNERÁVEL
        if (other.CompareTag("Laser") && !isInvulnerable)
        {
            HandleDeath();
        }

        // Se encostou no Campo de Força E NÃO ESTÁ INVULNERÁVEL
        if (other.CompareTag("ForceField") && !isInvulnerable)
        {
            HandleDeath(); 
        }
    }

    // --- AQUI ESTÁ A CORREÇÃO ---
    // Adiciona "public" para o Trap.cs E O ENEMY.CS poderem chamar
    public void HandleDeath()
    {
        // Se já estiver invulnerável (renascendo), não morre de novo
        if (isInvulnerable) return; 

        lives--; // Perde uma vida
        Debug.Log($"TOMOU! Vidas restantes: {lives}");

        if (lives <= 0)
        {
            // --- GAME OVER ---
            Debug.LogError("GAME OVER, MAN! ACABOU AS VIDAS!");
            gameObject.SetActive(false);
        }
        else
        {
            // Se ainda tem vidas, começa a rotina de renascer
            StartCoroutine(RespawnRoutine());
        }
    }

    IEnumerator RespawnRoutine()
    {
        // 1. FICA ESTÁTICO E INVULNERÁVEL
        canMove = false;
        isInvulnerable = true;
        
        // 2. RENASCE AONDE NASCEU
        transform.position = spawnPoint;

        Debug.Log("Renasceu! Piscando 3 vezes (estático)...");

        // 3. PISCA 3 VEZES
        for (int i = 0; i < blinkCount; i++)
        {
            sr.enabled = false; // Apaga
            yield return new WaitForSeconds(blinkSpeed);
            sr.enabled = true; // Acende
            yield return new WaitForSeconds(blinkSpeed);
        }

        // 4. LIBERA PARA ANDAR
        canMove = true;
        Debug.Log("Liberado pra andar! (Ainda invulnerável por um tempo)");

        // 5. ESPERA A INVULNERABILIDADE ACABAR
        float timeToWait = invulnerabilityDuration - (blinkCount * blinkSpeed * 2);
        if (timeToWait < 0) timeToWait = 0; // Evita tempo negativo
        yield return new WaitForSeconds(timeToWait);

        isInvulnerable = false;
        Debug.Log("Invulnerabilidade ACABOU! Cuidado!");
    }
}