using UnityEngine;
using UnityEngine.SceneManagement; // Precisa disso pra trocar de cena!

public class SceneLoaderTrigger : MonoBehaviour
{
    // Coloque o nome da cena do Genius aqui no Inspector
    public string sceneToLoad = "GeniusScene"; 
    
    // Tag do seu player (geralmente "Player")
    public string playerTag = "Player"; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se quem entrou foi o Player
        if (other.CompareTag(playerTag))
        {
            Debug.Log($"Player encostou no fliperama! Carregando cena: {sceneToLoad}");
            
            // Carrega a outra cena!
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}