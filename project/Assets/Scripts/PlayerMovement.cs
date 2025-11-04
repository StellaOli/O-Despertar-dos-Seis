using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Velocidade de movimento do player

    private Rigidbody2D rb; // Referência ao componente Rigidbody2D

    void Start()
    {
        // Pega a referência do Rigidbody2D no início do jogo
        rb = GetComponent<Rigidbody2D>(); 
    }

    void Update()
    {
        // Input do Teclado
        // Pega os valores do input horizontal (A/D ou Setas Esquerda/Direita)
        float moveX = Input.GetAxisRaw("Horizontal"); 
        // Pega os valores do input vertical (W/S ou Setas Cima/Baixo)
        float moveY = Input.GetAxisRaw("Vertical"); 

        // Cria um vetor de movimento normalizado
        Vector2 movement = new Vector2(moveX, moveY).normalized; 

        // Aplica a velocidade ao Rigidbody
        // Usamos FixedUpdate para operações de física
        // mas o input é lido em Update, então vamos guardar o movimento aqui
        rb.velocity = movement * moveSpeed;
    }

    // Se preferir uma movimentação baseada em física mais precisa (para colisões e tal), 
    // é bom usar FixedUpdate. Porém, para jogos simples, Update geralmente funciona.
    // void FixedUpdate()
    // {
    //    // rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    // }
}