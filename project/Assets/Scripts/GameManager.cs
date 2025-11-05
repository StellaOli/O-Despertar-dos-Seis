using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Configuração do Jogo")]
    public int maxHealth = 100;
    public int requiredItemsToWin = 2;

    [Header("Nomes das Cenas")]
    public string dialogueSceneName = "DialogueScene";
    public string mainLabSceneName = "SampleScene";     // Labirinto principal
    public string power1SceneName = "Power1Scene";      // Cena do poder 1
    public string power2SceneName = "Power2Scene";      // Cena do poder 2
    public string finalBattleSceneName = "FinalBattleScene"; // Batalha final
    public string heroEndingSceneName = "HeroEndingScene";   // Final herói
    public string villainEndingSceneName = "VillainEndingScene"; // Final vilão

    [Header("Sistema de Progresso")]
    public Vector3 labSpawnPosition = new Vector3(0, 0, 0);
    public Vector3 power1ReturnPosition = new Vector3(5, 0, 0);
    public Vector3 power2ReturnPosition = new Vector3(-5, 0, 0);

    // Variáveis do jogador
    private int currentScore = 0;
    private int currentHealth;
    private int collectedItems = 0;
    private bool gameEnded = false;

    // Progresso de poderes e decisões
    private bool power1Unlocked = false;
    private bool power2Unlocked = false;
    private bool choseVillainPath = false;

    // Propriedades públicas
    public int CurrentHealth { get { return currentHealth; } }
    public int CollectedItems { get { return collectedItems; } }
    public int RequiredItems { get { return requiredItemsToWin; } }
    public int CurrentScore { get { return currentScore; } }
    public bool Power1Unlocked { get { return power1Unlocked; } }
    public bool Power2Unlocked { get { return power2Unlocked; } }
    public bool ChoseVillainPath { get { return choseVillainPath; } set { choseVillainPath = value; } }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("🎮 Game Manager inicializado!");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"🔄 Cena carregada: {scene.name}");

        // Inicializa o jogo se for uma cena de gameplay
        if (scene.name == mainLabSceneName || scene.name == power1SceneName ||
            scene.name == power2SceneName || scene.name == finalBattleSceneName)
        {
            InitializeGame();
        }
        // Reseta estado completo se voltar ao diálogo inicial
        else if (scene.name == dialogueSceneName)
        {
            ResetGameState();
        }
    }

    void InitializeGame()
    {
        // Garante que a vida não seja resetada entre cenas
        if (currentHealth <= 0) currentHealth = maxHealth;
        Time.timeScale = 1f;
        gameEnded = false;

        Debug.Log($"🎮 Jogo inicializado na cena: {SceneManager.GetActiveScene().name}");
        Debug.Log($"❤️ Vida: {currentHealth} | ⚡ Poder 1: {power1Unlocked} | 🔥 Poder 2: {power2Unlocked}");
    }

    void ResetGameState()
    {
        currentHealth = maxHealth;
        collectedItems = 0;
        currentScore = 0;
        power1Unlocked = false;
        power2Unlocked = false;
        choseVillainPath = false;
        gameEnded = false;
        Time.timeScale = 1f;

        Debug.Log("🔄 Estado do jogo resetado completamente");
    }

    // === SISTEMA DE SCORE E ITENS ===
    public void AddScore(int points)
    {
        if (gameEnded) return;

        currentScore += points;
        Debug.Log($"⭐ Score: +{points} | Total: {currentScore}");
    }

    public void CollectItem()
    {
        if (gameEnded) return;

        collectedItems++;
        Debug.Log($"🔬 Item coletado! {collectedItems}/{requiredItemsToWin}");

        if (collectedItems >= requiredItemsToWin)
        {
            Debug.Log("✅ Todos os itens necessários coletados!");
        }
    }

    // === SISTEMA DE VIDA ===
    public void TakeDamage(int damage)
    {
        if (gameEnded) return;

        currentHealth -= damage;
        Debug.Log($"💔 Dano: {damage} | Vida: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            PlayerDied();
        }
    }

    public void Heal(int healAmount)
    {
        if (gameEnded) return;

        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        Debug.Log($"❤️ Cura: +{healAmount} | Vida: {currentHealth}/{maxHealth}");
    }

    // === SISTEMA DE PODERES ===
    public void UnlockPower1()
    {
        if (!power1Unlocked)
        {
            power1Unlocked = true;
            Debug.Log("⚡ PODER 1 DESBLOQUEADO permanentemente!");
        }
    }

    public void UnlockPower2()
    {
        if (!power2Unlocked)
        {
            power2Unlocked = true;
            Debug.Log("🔥 PODER 2 DESBLOQUEADO permanentemente!");
        }
    }

    // === SISTEMA DE MORTE (REINICIA NA MESMA CENA) ===
    public void PlayerDied()
    {
        if (gameEnded) return;

        Debug.Log("💀 Player morreu! Reiniciando na mesma cena...");
        StartCoroutine(RespawnPlayer());
    }

    IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(1.5f);
        RespawnAtCurrentScene();
    }

    public void RespawnAtCurrentScene()
    {
        currentHealth = maxHealth;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Reposiciona baseado na cena atual
            Vector3 respawnPosition = GetRespawnPositionForCurrentScene();
            player.transform.position = respawnPosition;

            Player playerScript = player.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.ResetPlayer();
            }

            Debug.Log($"🔄 Player respawned em: {respawnPosition}");
        }
    }

    Vector3 GetRespawnPositionForCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        switch (currentScene)
        {
            case "Power1Scene": return Vector3.zero;
            case "Power2Scene": return Vector3.zero;
            case "FinalBattleScene": return new Vector3(0, 0, 0);
            default: return labSpawnPosition;
        }
    }

    // === SISTEMA DE DECISÃO FINAL ===
    public void MakeFinalDecision(bool chooseVillainPath)
    {
        choseVillainPath = chooseVillainPath;

        if (chooseVillainPath)
        {
            Debug.Log("😈 Player escolheu o caminho do VILÃO!");
            SceneManager.LoadScene(villainEndingSceneName);
        }
        else
        {
            Debug.Log("🦸 Player escolheu o caminho do HERÓI!");
            SceneManager.LoadScene(heroEndingSceneName);
        }
    }

    // === CONTROLE DE CENAS ===
    public void StartGameFromDialogue()
    {
        SceneManager.LoadScene(mainLabSceneName);
    }

    public void LoadPower1Scene()
    {
        SceneManager.LoadScene(power1SceneName);
    }

    public void LoadPower2Scene()
    {
        SceneManager.LoadScene(power2SceneName);
    }

    public void ReturnToMainLabFromPower1()
    {
        SceneManager.LoadScene(mainLabSceneName);
    }

    public void ReturnToMainLabFromPower2()
    {
        SceneManager.LoadScene(mainLabSceneName);
    }

    public void LoadFinalBattle()
    {
        if (collectedItems >= requiredItemsToWin)
        {
            SceneManager.LoadScene(finalBattleSceneName);
        }
        else
        {
            Debug.Log($"❌ Precisa de {requiredItemsToWin} itens para a batalha final!");
        }
    }

    public void RestartGame()
    {
        ResetGameState();
        SceneManager.LoadScene(dialogueSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // === VERIFICAÇÕES ===
    public bool CanAccessFinalBattle()
    {
        return collectedItems >= requiredItemsToWin;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}