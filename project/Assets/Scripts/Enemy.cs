using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Configurações de Vida")]
    public int maxHealth = 50;
    public int currentHealth;
    public bool isDead = false;

    [Header("Sistema de Combate")]
    public int damage = 10;
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    public float detectionRange = 5f;

    [Header("Movimentação")]
    public float moveSpeed = 2f;
    public float stoppingDistance = 1f;

    [Header("Configurações 2.5D")]
    public LayerMask groundLayer; // Camada do chão (Z=0)
    public LayerMask obstacleLayer; // Camada de obstáculos no mesmo Z

    [Header("Limites de Tela")]
    public bool useScreenLimits = true;
    public float screenPadding = 0.5f;

    [Header("Componentes")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    // Componentes privados
    private Rigidbody2D rb2d;
    private Transform player;
    private bool canAttack = true;
    private bool isAttacking = false;
    private Vector2 movement;

    // Sistema de knockback
    private bool isKnockback = false;
    private float knockbackForce = 5f;
    private float knockbackDuration = 0.3f;

    // Limites de tela
    private Vector2 screenMin;
    private Vector2 screenMax;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        transform.position = new Vector3(transform.position.x, transform.position.y, -1f);

        if (rb2d != null)
        {
            rb2d.gravityScale = 0;
            rb2d.drag = 3f;
            rb2d.angularDrag = 0.05f;
            rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        CalculateScreenBounds();
        FindPlayer();
        currentHealth = maxHealth;

        if (rb2d == null) Debug.LogError("Enemy: Rigidbody2D não encontrado!");
    }

    void CalculateScreenBounds()
    {
        if (useScreenLimits)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                screenMin = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
                screenMax = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));
                screenMin.x += screenPadding;
                screenMin.y += screenPadding;
                screenMax.x -= screenPadding;
                screenMax.y -= screenPadding;
            }
        }
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log("🎯 Player encontrado: " + player.name);
        }
        else
        {
            Debug.LogWarning("🎯 Player não encontrado! Verifique a tag 'Player'");
        }
    }

    void Update()
    {
        if (isDead) return;

        if (player == null)
        {
            FindPlayer();
            return;
        }

        CheckPlayerDistance();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (isDead || isAttacking || isKnockback || player == null) return;

        MoveTowardsPlayer();
    }

    void CheckPlayerDistance()
    {
        float distanceToPlayer = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.y),
            new Vector2(player.position.x, player.position.y)
        );

        bool hasLineOfSight = HasLineOfSightToPlayer();

        if (distanceToPlayer <= detectionRange && hasLineOfSight)
        {
            if (distanceToPlayer <= attackRange && canAttack && !isAttacking)
            {
                StartCoroutine(Attack());
            }
        }
    }

    bool HasLineOfSightToPlayer()
    {
        if (player == null) return false;

        Vector2 enemyPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 playerPos = new Vector2(player.position.x, player.position.y);
        Vector2 direction = (playerPos - enemyPos).normalized;
        float distance = Vector2.Distance(enemyPos, playerPos);

        RaycastHit2D hit = Physics2D.Raycast(enemyPos, direction, distance, obstacleLayer);

        Debug.DrawRay(transform.position, player.position - transform.position,
                       hit.collider == null ? Color.green : Color.red);

        return hit.collider == null;
    }

    void MoveTowardsPlayer()
    {
        Vector2 direction = (new Vector2(player.position.x, player.position.y) -
                           new Vector2(transform.position.x, transform.position.y)).normalized;

        float distanceToPlayer = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.y),
            new Vector2(player.position.x, player.position.y)
        );

        if (distanceToPlayer > stoppingDistance)
        {
            Vector2 newPosition = rb2d.position + direction * moveSpeed * Time.fixedDeltaTime;

            if (useScreenLimits)
            {
                newPosition.x = Mathf.Clamp(newPosition.x, screenMin.x, screenMax.x);
                newPosition.y = Mathf.Clamp(newPosition.y, screenMin.y, screenMax.y);
            }

            rb2d.MovePosition(newPosition);

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = direction.x < 0f;
            }
        }
        else
        {
            rb2d.velocity = Vector2.zero;
        }
    }

    IEnumerator Attack()
    {
        isAttacking = true;
        canAttack = false;
        rb2d.velocity = Vector2.zero;
        Debug.Log("🗡️ Inimigo atacando!");

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(0.5f);

        if (player != null && HasLineOfSightToPlayer())
        {
            float distance = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.y),
                new Vector2(player.position.x, player.position.y)
            );

            if (distance <= attackRange)
            {
                // --- AQUI ESTÁ A CORREÇÃO (LINHAS 236-239) ---
                // 1. Procura o script 'PlayerMovement', e não 'Player'
                PlayerMovement playerComponent = player.GetComponent<PlayerMovement>();
                if (playerComponent != null)
                {
                    // 2. Chama o método 'HandleDeath()' que JÁ EXISTE no seu player
                    playerComponent.HandleDeath();
                }
                // --- FIM DA CORREÇÃO ---
            }
        }

        yield return new WaitForSeconds(0.3f);
        isAttacking = false;

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        bool isMoving = Vector2.Distance(transform.position, player.position) > stoppingDistance && !isAttacking;
        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsAttacking", isAttacking);
        animator.SetBool("IsDead", isDead);
    }

    public void TakeDamage(int damage, Vector2 hitDirection = default(Vector2))
    {
        if (isDead) return;
        currentHealth -= damage;
        Debug.Log($"💥 {gameObject.name} tomou {damage} de dano! Vida: {currentHealth}/{maxHealth}");

        StartCoroutine(DamageFlash());

        if (hitDirection != Vector2.zero)
        {
            StartCoroutine(ApplyKnockback(hitDirection));
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator DamageFlash()
    {
        if (spriteRenderer == null) yield break;
        Color originalColor = spriteRenderer.color;
        for (int i = 0; i < 2; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator ApplyKnockback(Vector2 direction)
    {
        isKnockback = true;
        Vector2 knockbackPos = rb2d.position + direction.normalized * knockbackForce * 0.1f;
        rb2d.MovePosition(knockbackPos);
        yield return new WaitForSeconds(knockbackDuration);
        isKnockback = false;
    }

    void Die()
    {
        isDead = true;
        Debug.Log("💀 Inimigo morreu!");
        rb2d.velocity = Vector2.zero;

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;

        StartCoroutine(DestroyAfterDeath());
    }

    IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.position.z != -1f) return;

        if (other.CompareTag("PlayerPower"))
        {
            Vector2 hitDirection = (new Vector2(transform.position.x, transform.position.y) -
                                  new Vector2(other.transform.position.x, other.transform.position.y)).normalized;
            TakeDamage(GetPowerDamage(other.gameObject), hitDirection);
        }
    }

    int GetPowerDamage(GameObject powerObject)
    {
        if (powerObject.name.Contains("Power1") || powerObject.CompareTag("Power1"))
            return 25;
        else if (powerObject.name.Contains("Power2") || powerObject.CompareTag("Power2"))
            return 40;
        else
            return 20;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (Application.isPlaying && useScreenLimits)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(new Vector3((screenMin.x + screenMax.x) * 0.5f, (screenMin.y + screenMax.y) * 0.5f, 0f),
                               new Vector3(screenMax.x - screenMin.x, screenMax.y - screenMin.y, 0f));
        }
    }
}