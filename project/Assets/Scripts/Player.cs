using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class Player : MonoBehaviour
{
    [Header("Movimentação - Setas")]
    public float speed = 5.0f; // Velocidade de ANDAR (não correr)

    [Header("Configurações de Vida")]
    public int maxHealth = 100;
    public int currentHealth;
    public bool isInvulnerable = false;
    public float invulnerabilityTime = 1f;

    [Header("Sistema de Poderes")]
    public KeyCode usePower1 = KeyCode.Z;
    public KeyCode usePower2 = KeyCode.X;
    public GameObject power1Effect;
    public GameObject power2Effect;
    public float power1Cooldown = 2f;
    public float power2Cooldown = 3f;
    public int power1Damage = 25;
    public int power2Damage = 40;

    private bool canUsePower1 = true;
    private bool canUsePower2 = true;

    [Header("Sistema de Iluminação")]
    public Light2D playerLight;
    public float lightRadius = 3f;

    [Header("Componentes")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    // Componentes privados
    private Rigidbody2D rb2d;
    private bool isGrounded;
    private bool isUsingPower = false;
    private string currentPowerAnimation = "";

    void Start()
    {
        // Inicializa componentes
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Configura iluminação
        SetupLighting();

        // Inicializa vida
        currentHealth = maxHealth;

        if (rb2d == null) Debug.LogError("Player: Rigidbody2D não encontrado!");
    }

    void SetupLighting()
    {
        if (playerLight != null)
        {
            playerLight.pointLightOuterRadius = lightRadius;
            playerLight.pointLightInnerRadius = lightRadius * 0.5f;
            Debug.Log("💡 Sistema de iluminação configurado");
        }
        else
        {
            Debug.LogWarning("💡 Light2D não atribuído ao Player");
        }
    }

    void Update()
    {
        // Input de poderes (verifica se estão desbloqueados no GameManager)
        if (CanUsePower1() && Input.GetKeyDown(usePower1) && canUsePower1 && !isUsingPower)
        {
            StartCoroutine(UsePower1());
        }

        if (CanUsePower2() && Input.GetKeyDown(usePower2) && canUsePower2 && !isUsingPower)
        {
            StartCoroutine(UsePower2());
        }
    }

    void FixedUpdate()
    {
        if (isUsingPower) return; // Não se move durante poderes

        CheckGrounded();
        ProcessMovement();
        UpdateAnimations();
    }

    void CheckGrounded()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
    }

    void ProcessMovement()
    {
        float moveHorizontal = 0f;
        float moveVertical = 0f;

        // Input horizontal (Setas Esquerda/Direita) - ANDAR
        if (Input.GetKey(KeyCode.RightArrow)) moveHorizontal = 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) moveHorizontal = -1f;

        // Input vertical (Setas Cima/Baixo) - ANDAR
        if (Input.GetKey(KeyCode.UpArrow)) moveVertical = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) moveVertical = -1f;

        // Movimento em grid (andar) - velocidade constante
        Vector2 movement = new Vector2(moveHorizontal * speed, moveVertical * speed);
        rb2d.velocity = movement;

        // Flip do sprite baseado na direção horizontal
        if (moveHorizontal != 0f && spriteRenderer != null)
        {
            spriteRenderer.flipX = moveHorizontal < 0f;
        }
    }

    void UpdateAnimations()
    {
        if (animator != null)
        {
            // Animação de andar (quando se movendo)
            bool isWalking = rb2d.velocity.magnitude > 0.1f;
            animator.SetBool("isWalking", isWalking);

            // Animação de poder
            animator.SetBool("isUsingPower", isUsingPower);

            if (!string.IsNullOrEmpty(currentPowerAnimation))
            {
                animator.SetTrigger(currentPowerAnimation);
                currentPowerAnimation = "";
            }
        }
    }

    // 🔧 VERIFICAÇÃO DE PODERES DESBLOQUEADOS
    bool CanUsePower1()
    {
        return GameManager.instance != null && GameManager.instance.Power1Unlocked;
    }

    bool CanUsePower2()
    {
        return GameManager.instance != null && GameManager.instance.Power2Unlocked;
    }

    // ⚡ PODER 1
    IEnumerator UsePower1()
    {
        isUsingPower = true;
        canUsePower1 = false;
        currentPowerAnimation = "UsePower1";

        Debug.Log("⚡ Usando Poder 1!");

        if (power1Effect != null)
        {
            GameObject effect = Instantiate(power1Effect, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }

        //ApplyPowerDamage(transform.position, 2f, power1Damage);

        yield return new WaitForSeconds(0.8f);
        isUsingPower = false;

        yield return new WaitForSeconds(power1Cooldown - 0.8f);
        canUsePower1 = true;
    }

    // 🔥 PODER 2
    IEnumerator UsePower2()
    {
        isUsingPower = true;
        canUsePower2 = false;
        currentPowerAnimation = "UsePower2";

        Debug.Log("🔥 Usando Poder 2!");

        if (power2Effect != null)
        {
            GameObject effect = Instantiate(power2Effect, transform.position, Quaternion.identity);
            Destroy(effect, 1.5f);
        }

        //ApplyPowerDamage(transform.position, 3f, power2Damage);

        yield return new WaitForSeconds(1.2f);
        isUsingPower = false;

        yield return new WaitForSeconds(power2Cooldown - 1.2f);
        canUsePower2 = true;
    }

  //void ApplyPowerDamage(Vector2 position, float radius, int damage)
  //  {
  //      Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(position, radius);
  //      foreach (Collider2D enemy in hitEnemies)
  //      {
  //          if (enemy.CompareTag("Enemy"))
  //          {
  //              Enemy enemyScript = enemy.GetComponent<Enemy>();
  //              if (enemyScript != null)
  //              {
  //                  enemyScript.TakeDamage(damage);
  //              }
  //          }
  //      }
  //  }

    // ❤️ SISTEMA DE VIDA
    public void TakeDamage(int damage)
    {
        if (isInvulnerable || currentHealth <= 0) return;

        currentHealth -= damage;

        // Atualiza GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.TakeDamage(damage);
        }

        StartCoroutine(InvulnerabilityFrames());

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DamageFlash());
        }
    }

    IEnumerator InvulnerabilityFrames()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false;
    }

    IEnumerator DamageFlash()
    {
        if (spriteRenderer == null) yield break;

        Color originalColor = spriteRenderer.color;
        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }

    // 💀 SISTEMA DE MORTE (RESPAWN)
    void Die()
    {
        Debug.Log("💀 Player morreu! Preparando respawn...");

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        this.enabled = false;
        rb2d.velocity = Vector2.zero;

        if (GameManager.instance != null)
        {
            GameManager.instance.PlayerDied();
        }
    }

    // 🔄 RESET DO PLAYER
    public void ResetPlayer()
    {
        this.enabled = true;
        currentHealth = maxHealth;
        isInvulnerable = false;
        isUsingPower = false;

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isUsingPower", false);
            animator.ResetTrigger("Die");
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    // 🎯 COLISÕES E TRIGGERS
    void OnTriggerEnter2D(Collider2D other)
    {
        // Porta para cena do Poder 1
        if (other.CompareTag("Power1Door"))
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.LoadPower1Scene();
            }
        }

        // Porta para cena do Poder 2
        if (other.CompareTag("Power2Door"))
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.LoadPower2Scene();
            }
        }

        // Saída para batalha final
        if (other.CompareTag("FinalExit"))
        {
            if (GameManager.instance != null && GameManager.instance.CanAccessFinalBattle())
            {
                GameManager.instance.LoadFinalBattle();
            }
        }

        // Desbloqueio de poder NAS CENAS DE PODER
        if (other.CompareTag("Power1Unlock"))
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.UnlockPower1();
            }
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Power2Unlock"))
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.UnlockPower2();
            }
            Destroy(other.gameObject);
        }

        // Voltar ao labirinto principal
        if (other.CompareTag("ReturnToLab"))
        {
            if (GameManager.instance != null)
            {
                // Verifica de qual cena de poder está voltando
                string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                if (currentScene == "Power1Scene")
                {
                    GameManager.instance.ReturnToMainLabFromPower1();
                }
                else if (currentScene == "Power2Scene")
                {
                    GameManager.instance.ReturnToMainLabFromPower2();
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !isInvulnerable)
        {
            TakeDamage(10);
        }
    }

    // 🔧 MÉTODOS PÚBLICOS
    public void Heal(int healAmount)
    {
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    // 🎨 DEBUG
    void OnDrawGizmosSelected()
    {
        // Área de iluminação
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lightRadius);

        // Área do Poder 1
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 2f);

        // Área do Poder 2
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 3f);
    }
}