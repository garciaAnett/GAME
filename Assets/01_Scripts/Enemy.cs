using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2f; // Velocidad de movimiento
    public Transform leftPoint;  // L�mite izquierdo
    public Transform rightPoint; // L�mite derecho
    private bool movingRight = true; // Indica la direcci�n del movimiento

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

            // Cambiar direcci�n cuando alcanza el l�mite derecho
            if (transform.position.x >= rightPoint.position.x)
            {
                movingRight = false; // Cambia de direcci�n a la izquierda
                Flip(); // Voltea el sprite
            }
        }
        else
        {
            // Mover hacia la izquierda
            transform.position = new Vector2(transform.position.x - moveSpeed * Time.deltaTime, transform.position.y);

            // Cambiar direcci�n cuando alcanza el l�mite izquierdo
            if (transform.position.x <= leftPoint.position.x)
            {
                movingRight = true; // Cambia de direcci�n a la derecha
                Flip(); // Voltea el sprite
            }
        }
    }

    // M�todo para voltear el sprite del enemigo (opcional, para dar la impresi�n de cambio de direcci�n)
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
            // Aqu� puedes implementar lo que sucede cuando Mario colisiona con el enemigo.
            // Ejemplo: Mario pierde una vida o muere.
        }
    }
}
