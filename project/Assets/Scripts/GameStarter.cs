using UnityEngine;

public class GameStarter : MonoBehaviour
{
    // Define a tag do seu player no Inspector (ex: "Player")
    public string playerTag = "Player"; 
    
    // Referência ao script que vai iniciar o Genius (ex: GameController)
    public GameController gameController; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se o objeto que entrou é o Player
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Player entrou! Bora começar o Genius!");
            
            // Chama o método no seu GameController para iniciar o jogo
            if (gameController != null)
            {
                gameController.StartGameSequence(); 
                // Você pode desabilitar o Collider depois para não reativar
                GetComponent<Collider2D>().enabled = false; 
            }
        }
    }
}