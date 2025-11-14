using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BarrierButton : MonoBehaviour
{
    [Header("Conexão")]
    [Tooltip("ARRASTA O 'Barrier' (PAI) DA HIERARQUIA AQUI!")]
    public BarrierController targetBarrier; // A referência direta!

    public string playerTag = "Player";
    private bool hasBeenTriggered = false;

    void Start()
    {
        // Garante que o botão é um gatilho
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Só roda uma vez
        if (hasBeenTriggered) return;

        if (other.CompareTag(playerTag))
        {
            if (targetBarrier != null)
            {
                Debug.Log("Botão ativado! Mandando barreira desligar...");
                
                // CHAMA O MÉTODO LÁ NO SCRIPT DA BARREIRA
                targetBarrier.DisableBarrier();

                // Desliga o próprio botão
                gameObject.SetActive(false);
                hasBeenTriggered = true;
            }
            else
            {
                Debug.LogError($"O Botão {name} foi ativado, mas não sabe qual barreira desligar!");
            }
        }
    }
}