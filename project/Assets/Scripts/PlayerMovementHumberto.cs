using UnityEngine;
using UnityEngine.SceneManagement; // Pra reiniciar a cena (se quiser)
using System.Collections; // Pra usar Coroutines (o pisca-pisca)

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimento")]
    public float moveSpeed = 5f;
    private Rigidbody2D rb;

    [Header("Vidas e Respawn")]
    public int lives = 3;
    public float invulnerabilityDuration = 2f; // Tempo total invulnerável após morrer
    public int blinkCount = 3; // Quantas vezes pisca
    public float blinkSpeed = 0.2f; // Velocidade do pisca (0.2s apagado, 0.2s aceso)

    // Controles internos
    private Vector3 spawnPoint; // Onde ele nasceu a primeira vez
    private SpriteRenderer sr;
    private bool canMove = true;
    private bool isInvulnerable = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        
        // Salva a posição inicial como o ponto de respawn
        spawnPoint = transform.position; 
    }

    void Update()
    {
        // Se não puder se mover (morto/renascendo), trava a velocidade
        if (!canMove)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // Código de movimento normal
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(moveX, moveY).normalized;
        rb.velocity = movement * moveSpeed;
    }

    // Função que detecta os Triggers (Laser, Botões, etc.)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Se encostou no Laser E NÃO ESTÁ INVULNERÁVEL
        if (other.CompareTag("Laser") && !isInvulnerable)
        {
            HandleDeath();
        }
    }

    // A "porra da lógica da morte" que você pediu!
    void HandleDeath()
    {
        lives--; // Perde uma vida
        Debug.Log($"TOMOU! Vidas restantes: {lives}");

        if (lives <= 0)
        {
            // --- GAME OVER ---
            Debug.LogError("GAME OVER, MAN! ACABOU AS VIDAS!");
            // Desativa o player de vez
            gameObject.SetActive(false);
            
            // Aqui você pode chamar um Menu de Game Over ou reiniciar o jogo todo
            // Ex: SceneManager.LoadScene("MainMenu");
        }
        else
        {
            // Se ainda tem vidas, começa a rotina de renascer
            StartCoroutine(RespawnRoutine());
        }
    }

    // A rotina de Renascer (piscar, ficar estático, etc.)
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
        // O tempo total de invulnerabilidade (2s) é maior que o pisca
        // (que durou 3 * (0.2 + 0.2) = 1.2s)
        float timeToWait = invulnerabilityDuration - (blinkCount * blinkSpeed * 2);
        yield return new WaitForSeconds(timeToWait);

        isInvulnerable = false;
        Debug.Log("Invulnerabilidade ACABOU! Cuidado!");
    }
}