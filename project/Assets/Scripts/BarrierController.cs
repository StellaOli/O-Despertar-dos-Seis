using UnityEngine;

public class BarrierController : MonoBehaviour
{
    [Header("Componentes da Barreira")]
    [Tooltip("Arrasta o filho que tem o Sprite/Collider (Ex: 'Laser_Barreira')")]
    public GameObject forceFieldObject; // O filho (Laser_Barreira)

    [Tooltip("Marca isso se a barreira já deve começar LIGADA")]
    public bool startEnabled = true;

    void Start()
    {
        // Garante que o campo de força começa no estado certo
        if (forceFieldObject != null)
        {
            forceFieldObject.SetActive(startEnabled);
        }
    }

    // Método público que o BOTÃO vai chamar
    public void DisableBarrier()
    {
        Debug.Log("Barreira recebendo ordem para DESLIGAR!");
        
        if (forceFieldObject != null)
        {
            forceFieldObject.SetActive(false); // Desliga o filho
        }
        
        // Opcional: Desliga o objeto pai também
        // gameObject.SetActive(false);
    }
}