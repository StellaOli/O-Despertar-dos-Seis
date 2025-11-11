using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum GameState { STOPPED, PREPARING, SHOWING, WAITING_INPUT }

public class GameController : MonoBehaviour
{
    [Header("Interface")]
    public GameObject restartButton;

    [Header("Configurações Gerais")]
    public ColorButton[] colorButtons;
    public float flashDelay = 0.7f;
    public float timeLimitBase = 2.0f;

    [Header("Fases e Dificuldade")]
    public int[] phaseGoals = { 2, 5, 10 };
    public int maxErrors = 3;

    private GameState state = GameState.STOPPED;
    private List<ColorType> sequence = new List<ColorType>();
    private int phaseIndex = 0;
    private int errors = 0;
    private int playerIndex = 0;

    private float timer = 0f;
    private int showIndex = 0;

    void Start()
    {
        if (restartButton != null) restartButton.SetActive(false);

        foreach (var btn in colorButtons)
        {
            btn.OnButtonPressed.RemoveAllListeners();
            btn.OnButtonPressed.AddListener(OnPlayerClick);
        }
    }

    void Update()
    {
        switch (state)
        {
            case GameState.PREPARING:
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    state = GameState.SHOWING;
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
                        state = GameState.WAITING_INPUT;
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
                    HandleError();
                }
                break;
        }
    }

    public void StartGameSequence()
    {
        if (restartButton != null) restartButton.SetActive(false);
        sequence.Clear();
        phaseIndex = 0;
        errors = 0;
        ResetButtonColors();
        PrepareNextRound();
    }

    public void RestartGame()
    {
        if (restartButton != null) restartButton.SetActive(false);
        StartGameSequence();
    }

    void PrepareNextRound()
    {
        if (sequence.Count >= phaseGoals[phaseIndex])
        {
            phaseIndex++;
            if (phaseIndex >= phaseGoals.Length)
            {
                DoVictory();
                return;
            }
        }

        if (sequence.Count < phaseGoals[phaseIndex])
        {
            var types = System.Enum.GetValues(typeof(ColorType)).Cast<ColorType>().ToArray();
            sequence.Add(types[Random.Range(0, types.Length)]);
        }

        state = GameState.PREPARING;
        timer = 1.5f;
    }

    public void OnPlayerClick(ColorType color)
    {
        if (state != GameState.WAITING_INPUT) return;

        if (color == sequence[playerIndex])
        {
            playerIndex++;
            if (playerIndex >= sequence.Count)
            {
                state = GameState.PREPARING;
                timer = 1.5f;
                PrepareNextRound();
            }
        }
        else
        {
            HandleError();
        }
    }

    void HandleError()
    {
        errors++;
        if (errors >= maxErrors)
        {
            DoGameOver();
        }
        else
        {
            state = GameState.PREPARING;
            timer = 2.0f;
        }
    }

    void DoGameOver()
    {
        state = GameState.STOPPED;
        if (restartButton != null) restartButton.SetActive(true);
        foreach (var btn in colorButtons)
        {
            var sr = btn.GetComponent<SpriteRenderer>();
            if (sr) sr.color = Color.black;
        }
    }

    void DoVictory()
    {
        state = GameState.STOPPED;
        if (restartButton != null) restartButton.SetActive(false);
        foreach (var btn in colorButtons)
        {
            var sr = btn.GetComponent<SpriteRenderer>();
            if (sr) sr.color = Color.white;
        }
    }

    void ResetButtonColors()
    {
        foreach (var btn in colorButtons)
        {
            var sr = btn.GetComponent<SpriteRenderer>();
            if (sr) sr.color = Color.white;
        }
    }

    // --- O X9 ESTÁ DE VOLTA (HUD DO JOGADOR) ---
    void OnGUI()
    {
        // Só mostra se o jogo estiver rodando (ou preparando)
        if (state == GameState.STOPPED && errors < maxErrors && sequence.Count == 0) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 24; // Letra maior pro jogador ver bem
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;

        // Caixa de fundo semitransparente pra dar leitura
        GUI.Box(new Rect(10, 10, 300, 160), "");

        GUILayout.BeginArea(new Rect(25, 20, 280, 150));

        // Mostra o status atual de forma amigável
        string statusText = "";
        switch (state)
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
        
        // Vidas restantes (mais intuitivo que contar erros)
        int vidas = maxErrors - errors;
        string corVida = vidas == 1 ? "<color=red>" : "<color=white>"; // Fica vermelho se tiver 1 vida
        style.richText = true; // Habilita o uso de cores no texto
        GUILayout.Label($"Vidas: {corVida}{vidas}</color>", style);

        // Timer só aparece quando é a vez do jogador
        if (state == GameState.WAITING_INPUT)
        {
            string corTimer = timer <= 3.0f ? "<color=red>" : "<color=yellow>";
            GUILayout.Label($"Tempo: {corTimer}{timer:F1}s</color>", style);
        }

        GUILayout.EndArea();
    }
}