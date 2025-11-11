using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Velocidade de movimento do player

    private Rigidbody2D rb; // Referência ao componente Rigidbody2D

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); 
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal"); 
        float moveY = Input.GetAxisRaw("Vertical"); 

        Vector2 movement = new Vector2(moveX, moveY).normalized; 
        rb.velocity = movement * moveSpeed;
    }
}