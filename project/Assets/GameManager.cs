using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Configuração do Jogo")]
    public int maxHealth = 200;
    

    [Header("Nomes das Cenas")]
    public string dialogueSceneName = "DialogueScene";
    public string gameSceneName = "LabirinthScene";
    public int geniusScene = "GeniusScene";
    public string victorySceneName = "VictoryScene";
    public string gameOverSceneName = "GameOverScene";

    
    private int currentHealth;
    private bool gameEnded = false;

    // ... (resto do código permanece igual)
    public int CurrentHealth { get { return currentHealth; } }
    public int CollectedItems { get { return collectedItems; } }
    public int RequiredItems { get { return requiredItemsToWin; } }
    public int CurrentScore { get { return currentScore; } }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (SceneManager.GetActiveScene().name == gameSceneName)
        {
            InitializeGame();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"?? Cena carregada: {scene.name}");

        if (scene.name == gameSceneName)
        {
            InitializeGame();
        }
        else if (scene.name == dialogueSceneName)
        {
            ResetGameState();
        }
    }

    void InitializeGame()
    {
        currentHealth = maxHealth;
        collectedItems = 0;
        currentScore = 0;
        gameEnded = false;
        Time.timeScale = 1f;
        Debug.Log("?? Jogo inicializado na SampleScene!");
    }

    void ResetGameState()
    {
        currentHealth = maxHealth;
        collectedItems = 0;
        currentScore = 0;
        gameEnded = false;
        Time.timeScale = 1f;
    }

    // === SISTEMA DE SCORE ===
    public void AddScore(int points)
    {
        if (gameEnded) return;

        currentScore += points;
        Debug.Log($"? Score: +{points} | Total: {currentScore}");
    }

    // === SISTEMA DE ITENS ===
    public void CollectItem()
    {
        if (gameEnded) return;

        collectedItems++;
        Debug.Log($"?? Item coletado! {collectedItems}/{requiredItemsToWin}");

        if (collectedItems >= requiredItemsToWin)
        {
            Debug.Log("? Todos os itens necessários coletados!");
        }
    }

    // === SISTEMA DE VIDA ===
    public void TakeDamage(int damage)
    {
        if (gameEnded) return;

        currentHealth -= damage;
        Debug.Log($"?? Dano: {damage} | Vida: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            PlayerDied();
        }
    }

    // === SISTEMA DE DERROTA ===
    public void PlayerDied()
    {
        if (gameEnded) return;

        gameEnded = true;
        Debug.Log("?? Game Over! Carregando cena de derrota...");

        StartCoroutine(GameOverSequence());
    }

    IEnumerator GameOverSequence()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(gameOverSceneName);
    }

    // === SISTEMA DE VITÓRIA ===
    public void CheckVictory()
    {
        if (gameEnded) return;

        if (collectedItems >= requiredItemsToWin)
        {
            Victory();
        }
        else
        {
            Debug.Log($"? Precisa de {requiredItemsToWin} amostras! Atual: {collectedItems}");
        }
    }

    public void Victory()
    {
        if (gameEnded) return;

        gameEnded = true;
        Debug.Log("?? Vitória! Carregando cena de vitória...");

        StartCoroutine(VictorySequence());
    }

    IEnumerator VictorySequence()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(victorySceneName);
    }

    // === CONTROLE DE CENAS ===
    public void StartGameFromDialogue()
    {
        Time.timeScale = 1f;
        Debug.Log($"?? Carregando: {gameSceneName}");
        SceneManager.LoadScene(gameSceneName);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void ReturnToDialogue()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(dialogueSceneName);
    }

    public void StartNewGame()
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
    public bool CanWin()
    {
        return collectedItems >= requiredItemsToWin;
    }

    public bool IsGameEnded()
    {
        return gameEnded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}