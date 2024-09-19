using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 5f; // Velocidad de movimiento
    public float jumpForce = 7f; // Fuerza del salto
    public bool isGrounded; // Verifica si el personaje está en el suelo
    private Rigidbody2D rb; // Referencia al Rigidbody2D

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Inicializamos el Rigidbody2D
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer(); // Método para movimiento
        Jump(); // Método para salto
    }

    // Método para movimiento con teclas A y D
    void MovePlayer()
    {
        float moveInput = Input.GetAxis("Horizontal"); // Obtiene input de teclas A y D o Flechas Izq/Der
        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y); // Mueve al personaje
    }

    // Método para el salto con la barra espaciadora
    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce); // Aplica fuerza de salto
        }
    }

    // Método para verificar si el personaje está en el suelo
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
