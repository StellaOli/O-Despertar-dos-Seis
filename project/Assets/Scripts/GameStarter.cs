using UnityEngine;

public class GameStarter : MonoBehaviour
{
    [Header("Configurações do Trigger")]
    public string playerTag = "Player";
    public bool disableAfterUse = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se o objeto que entrou é o Player
        if (other.CompareTag(playerTag))
        {
            Debug.Log("🎮 Player entrou na área do Genius!");

            // Chama o método no GameManager para iniciar o jogo Genius
            if (GameManager.instance != null)
            {
                GameManager.instance.StartGeniusGame();

                // Desativa o Collider se configurado
                if (disableAfterUse)
                {
                    GetComponent<Collider2D>().enabled = false;
                }
            }
            else
            {
                Debug.LogWarning("❌ GameManager.instance não encontrado!");
            }
        }
    }

    // Método público para reiniciar o trigger (útil se quiser reusar)
    public void ResetTrigger()
    {
        GetComponent<Collider2D>().enabled = true;
        Debug.Log("🔄 Trigger do Genius resetado!");
    }
}