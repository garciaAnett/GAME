using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2f; // Velocidad de movimiento
    public Transform leftPoint;  // Límite izquierdo
    public Transform rightPoint; // Límite derecho
    private bool movingRight = true; // Indica la dirección del movimiento

    void Update()
    {
        MoveEnemy();
    }

    void MoveEnemy()
    {
        if (movingRight)
        {
            // Mover hacia la derecha
            transform.position = new Vector2(transform.position.x + moveSpeed * Time.deltaTime, transform.position.y);

            // Cambiar dirección cuando alcanza el límite derecho
            if (transform.position.x >= rightPoint.position.x)
            {
                movingRight = false; // Cambia de dirección a la izquierda
                Flip(); // Voltea el sprite
            }
        }
        else
        {
            // Mover hacia la izquierda
            transform.position = new Vector2(transform.position.x - moveSpeed * Time.deltaTime, transform.position.y);

            // Cambiar dirección cuando alcanza el límite izquierdo
            if (transform.position.x <= leftPoint.position.x)
            {
                movingRight = true; // Cambia de dirección a la derecha
                Flip(); // Voltea el sprite
            }
        }
    }

    // Método para voltear el sprite del enemigo (opcional, para dar la impresión de cambio de dirección)
    void Flip()
    {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1; // Invierte el eje X para voltear el sprite
        transform.localScale = localScale;
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Aquí puedes implementar lo que sucede cuando Mario colisiona con el enemigo.
            // Ejemplo: Mario pierde una vida o muere.
        }
    }
}
