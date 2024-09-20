using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 5f; // Velocidad de movimiento
    public float jumpForce = 7f; // Fuerza del salto
    public bool isGrounded; // Verifica si el personaje está en el suelo
    private Rigidbody2D rb; // Referencia al Rigidbody2D
    public Transform firePoint;

    public LayerMask wallLayer; // Capa de las paredes
    public LayerMask groundLayer; // Capa del suelo
    public float wallCheckRadius = 0.2f; // Radio para detectar si el personaje está tocando la pared
    public Transform wallCheck; // Punto para verificar si toca la pared

    // Variables para el deslizamiento en la pared y salto en la pared
    public float wallSlideSpeedMax = 3f; // Velocidad máxima de deslizamiento en la pared
    public Vector2 wallJumpForce = new Vector2(8f, 15f); // Fuerza de salto en la pared
    private bool wallSliding = false; // Verifica si el personaje está deslizando en la pared
    private int wallDirX; // Dirección de la pared en la que el personaje está deslizando

    private bool touchingWall = false; // Verifica si está tocando la pared

    private bool canWallJumpUnlimited = false; // Habilita saltos ilimitados mientras se desliza en la pared

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Inicializa el Rigidbody2D
    }

    void Update()
    {
        MovePlayer(); // Movimiento del personaje
        Jump(); // Salto del personaje
        CheckWallSlide(); // Verifica si el personaje está deslizándose en la pared

        Debug.Log("WallSliding: " + wallSliding); // Mensaje para verificar si está deslizándose en la pared
    }

    // Método para el movimiento del personaje
    void MovePlayer()
    {
        float moveInput = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
    }

    // Método para el salto del personaje
    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce); // Salto normal cuando está en el suelo
            }
            else if (wallSliding || canWallJumpUnlimited)
            {
                Debug.Log("Salto en la pared"); // Mensaje para salto en la pared
                rb.velocity = new Vector2(-wallDirX * wallJumpForce.x, wallJumpForce.y); // Fuerza de salto en la pared
                wallSliding = false; // Deja de deslizarse al saltar

                // Si está deslizándose, habilita los saltos ilimitados
                if (wallSliding)
                {
                    canWallJumpUnlimited = true;
                }
            }
        }
    }

    // Método para verificar si el personaje está en el suelo
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
            canWallJumpUnlimited = false; // Desactiva los saltos ilimitados al tocar el suelo
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
        }
    }

    // Verificación del deslizamiento en la pared
    void CheckWallSlide()
    {
        touchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);

        if (touchingWall && !isGrounded && rb.velocity.y < 0)
        {
            wallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeedMax); // Velocidad máxima de deslizamiento
            Debug.Log("Deslizándose en la pared"); // Mensaje cuando está deslizándose en la pared
        }
        else
        {
            wallSliding = false;
            canWallJumpUnlimited = false; // Desactiva los saltos ilimitados al dejar de deslizarse
        }
    }
}
