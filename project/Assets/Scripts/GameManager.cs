using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

    [Header("Sistema Genius - Mini Game")]
    public ColorButton[] colorButtons;
    public float flashDelay = 0.7f;
    public float timeLimitBase = 2.0f;
    public int[] phaseGoals = { 2, 5, 10 };
    public int maxErrors = 3;
    public GameObject restartButton;

    // Variáveis do jogador
    private int currentScore = 0;
    private int currentHealth;
    private int collectedItems = 0;
    private bool gameEnded = false;

    // Progresso de poderes e decisões
    private bool power1Unlocked = false;
    private bool power2Unlocked = false;
    private bool choseVillainPath = false;

    // Sistema Genius
    private GameState geniusState = GameState.STOPPED;
    private List<ColorType> sequence = new List<ColorType>();
    private int phaseIndex = 0;
    private int errors = 0;
    private int playerIndex = 0;
    private float timer = 0f;
    private int showIndex = 0;
    private bool geniusGameActive = false;

    // Propriedades públicas
    public int CurrentHealth { get { return currentHealth; } }
    public int CollectedItems { get { return collectedItems; } }
    public int RequiredItems { get { return requiredItemsToWin; } }
    public int CurrentScore { get { return currentScore; } }
    public bool Power1Unlocked { get { return power1Unlocked; } }
    public bool Power2Unlocked { get { return power2Unlocked; } }
    public bool ChoseVillainPath { get { return choseVillainPath; } set { choseVillainPath = value; } }
    public bool GeniusGameActive { get { return geniusGameActive; } }

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
        InitializeGeniusSystem();
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

        // Reinicializa o sistema Genius
        InitializeGeniusSystem();
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

    // === SISTEMA GENIUS (MINI GAME) ===
    void InitializeGeniusSystem()
    {
        // Encontra os botões coloridos na cena atual
        if (colorButtons == null || colorButtons.Length == 0)
        {
            colorButtons = FindObjectsOfType<ColorButton>();
        }

        // Configura os listeners dos botões
        foreach (var btn in colorButtons)
        {
            if (btn != null)
            {
                btn.OnButtonPressed.RemoveAllListeners();
                btn.OnButtonPressed.AddListener(OnPlayerGeniusClick);
            }
        }

        if (restartButton != null) restartButton.SetActive(false);
    }

    void Update()
    {
        if (!geniusGameActive) return;

        // Atualiza o estado do jogo Genius
        switch (geniusState)
        {
            case GameState.PREPARING:
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    geniusState = GameState.SHOWING;
                    showIndex = 0;
                    timer = 0.5f;
                }
                break;

            case GameState.SHOWING:
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    if (showIndex < sequence.Count)
                    {
                        ColorType colorToShow = sequence[showIndex];
                        var btn = System.Array.Find(colorButtons, b => b.colorId == colorToShow);
                        if (btn) btn.Press();
                        showIndex++;
                        timer = flashDelay;
                    }
                    else
                    {
                        geniusState = GameState.WAITING_INPUT;
                        playerIndex = 0;
                        timer = (sequence.Count * 1.0f) + timeLimitBase;
                    }
                }
                break;

            case GameState.WAITING_INPUT:
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    Debug.LogError("⏰ TEMPO ESGOTADO!");
                    HandleGeniusError();
                }
                break;
        }
    }

    public void StartGeniusGame()
    {
        if (geniusGameActive) return;

        geniusGameActive = true;
        sequence.Clear();
        phaseIndex = 0;
        errors = 0;
        ResetButtonColors();
        PrepareNextGeniusRound();

        Debug.Log("🎮 Jogo Genius iniciado!");
    }

    public void StopGeniusGame()
    {
        geniusGameActive = false;
        geniusState = GameState.STOPPED;

        Debug.Log("🎮 Jogo Genius parado!");
    }

    void PrepareNextGeniusRound()
    {
        if (sequence.Count >= phaseGoals[phaseIndex])
        {
            phaseIndex++;
            if (phaseIndex >= phaseGoals.Length)
            {
                DoGeniusVictory();
                return;
            }
        }

        if (sequence.Count < phaseGoals[phaseIndex])
        {
            var types = System.Enum.GetValues(typeof(ColorType)).Cast<ColorType>().ToArray();
            sequence.Add(types[Random.Range(0, types.Length)]);
        }

        geniusState = GameState.PREPARING;
        timer = 1.5f;
    }

    public void OnPlayerGeniusClick(ColorType color)
    {
        if (geniusState != GameState.WAITING_INPUT || !geniusGameActive) return;

        if (color == sequence[playerIndex])
        {
            playerIndex++;
            if (playerIndex >= sequence.Count)
            {
                geniusState = GameState.PREPARING;
                timer = 1.5f;
                PrepareNextGeniusRound();

                // Recompensa por completar uma rodada
                AddScore(50);
                Heal(10);
            }
        }
        else
        {
            HandleGeniusError();
        }
    }

    void HandleGeniusError()
    {
        errors++;
        if (errors >= maxErrors)
        {
            DoGeniusGameOver();
        }
        else
        {
            geniusState = GameState.PREPARING;
            timer = 2.0f;
        }
    }

    void DoGeniusGameOver()
    {
        geniusState = GameState.STOPPED;
        geniusGameActive = false;
        if (restartButton != null) restartButton.SetActive(true);
        foreach (var btn in colorButtons)
        {
            var sr = btn.GetComponent<SpriteRenderer>();
            if (sr) sr.color = Color.black;
        }

        Debug.Log("💀 Game Over no Genius!");
    }

    void DoGeniusVictory()
    {
        geniusState = GameState.STOPPED;
        geniusGameActive = false;
        if (restartButton != null) restartButton.SetActive(false);
        foreach (var btn in colorButtons)
        {
            var sr = btn.GetComponent<SpriteRenderer>();
            if (sr) sr.color = Color.white;
        }

        // Grande recompensa por vencer o Genius
        AddScore(200);
        Heal(30);
        CollectItem(); // Item especial do Genius

        Debug.Log("🏆 Vitória no Genius!");
    }

    void ResetButtonColors()
    {
        foreach (var btn in colorButtons)
        {
            var sr = btn.GetComponent<SpriteRenderer>();
            if (sr) sr.color = Color.white;
        }
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

    // === INTERFACE GENIUS (OnGUI) ===
    void OnGUI()
    {
        if (!geniusGameActive) return;

        // Só mostra se o jogo Genius estiver rodando
        if (geniusState == GameState.STOPPED && errors < maxErrors && sequence.Count == 0) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;

        // Caixa de fundo semitransparente
        GUI.Box(new Rect(10, 10, 300, 160), "");

        GUILayout.BeginArea(new Rect(25, 20, 280, 150));

        string statusText = "";
        switch (geniusState)
        {
            case GameState.PREPARING: statusText = "⏳ Preparando..."; break;
            case GameState.SHOWING: statusText = "👁️ Observe..."; break;
            case GameState.WAITING_INPUT: statusText = "👉 SUA VEZ!"; break;
            case GameState.STOPPED:
                if (errors >= maxErrors) statusText = "💀 GAME OVER";
                else if (sequence.Count > 0) statusText = "🏆 VITÓRIA!";
                break;
        }

        GUILayout.Label($"Status: {statusText}", style);
        GUILayout.Space(10);
        GUILayout.Label($"Fase: {phaseIndex + 1} / {phaseGoals.Length}", style);
        GUILayout.Label($"Sequência: {playerIndex}/{sequence.Count} cores", style);

        int vidas = maxErrors - errors;
        string corVida = vidas == 1 ? "<color=red>" : "<color=white>";
        style.richText = true;
        GUILayout.Label($"Vidas: {corVida}{vidas}</color>", style);

        if (geniusState == GameState.WAITING_INPUT)
        {
            string corTimer = timer <= 3.0f ? "<color=red>" : "<color=yellow>";
            GUILayout.Label($"Tempo: {corTimer}{timer:F1}s</color>", style);
        }

        GUILayout.EndArea();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
public enum GameState { STOPPED, PREPARING, SHOWING, WAITING_INPUT }
public enum ColorType { RED, GREEN, BLUE, YELLOW }