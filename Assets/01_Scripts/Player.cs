using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{
    public float speed = 5f; // Velocidad de movimiento
    public float jumpForce = 7f; // Fuerza del salto
    public bool isGrounded; // Verifica si el personaje está en el suelo
    private Rigidbody2D rb; // Referencia al Rigidbody2D
    public Transform firePoint;
   
    // Raycast settings
    public float longDistance = 10f;  // Long distance range
    public float shortDistance = 3f;  // Short distance range
    public LayerMask enemyLayer;      // Layer for enemies
    public float raycastDistance = 10f; // para los rayos de 360 grados
 
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Inicializamos el Rigidbody2D
      
        enemyLayer = LayerMask.GetMask("Enemy");
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer(); // Método para movimiento
        Jump(); // Método para salto
     // DetectEnemiesBackForward();  // Utiliza el método DetectEnemies para manejar los raycasts
        DetectEnemies360();
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
    void DetectEnemiesBackForward() // Este es un metodo que se genera una 1 linea horizontal donde indicando si el enemigo esta cerca o no
    {                               // se uso un firepoint para simplificar el trabajo
                                    // indicandole la distancia y usando layer mask señalamos al enemigo.
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, firePoint.right, longDistance, enemyLayer);
        if (hit.collider != null)
        {
            Debug.Log("Enemigo detectado: " + hit.collider.name);
            float distanceToEnemy = Vector2.Distance(firePoint.position, hit.collider.transform.position);
            if (distanceToEnemy <= shortDistance)
            {                   //inicio           fin          color    duracion
                Debug.DrawLine(firePoint.position, hit.point, Color.red, 1f);  // Visible por 1 segundo la linea generada
            }
            else
            {
                Debug.DrawLine(firePoint.position, hit.point, Color.green, 1f);  // Visible por 1 segundo
            }
        }
        else
        {   // de la misma manera funciona solo que mutiplicamos con longdistance para determinar su distancia y generar la linea blanca
            Vector3 endRaycast = firePoint.position + firePoint.right * longDistance;
            Debug.DrawLine(firePoint.position, endRaycast, Color.white, 1f);  // Visible por 1 segundo
        }
    }

    void DetectEnemies360()
    {
        int numberOfRays = 8; // Cantidad de rayos, más rayos significa una cobertura más precisa pero más coste computacional
        float angleStep = 360f / numberOfRays; // Calcula el paso de cada ángulo

        for (int i = 0; i < numberOfRays; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad; // Convierte el ángulo a radianes
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, raycastDistance, enemyLayer);

            if (hit.collider != null)
            {
                // Calcular la distancia entre firePoint y el enemigo
                float distanceToEnemy = Vector2.Distance(firePoint.position, hit.point);

                // Si la distancia es menor o igual a 2f, la línea será roja
                if (distanceToEnemy <= 2f)
                {
                    Debug.DrawLine(firePoint.position, hit.point, Color.red, 0f); // Dibuja línea roja si el enemigo está cerca
                }
                else
                {
                    Debug.DrawLine(firePoint.position, hit.point, Color.yellow, 0f); // Dibuja línea amarilla si el enemigo está lejos
                }

                // Puedes también usar Debug.Log para ver la distancia
                // Debug.Log("Distancia al enemigo: " + distanceToEnemy);
            }
            else
            {
                Debug.DrawLine(firePoint.position, firePoint.position + (Vector3)(direction * raycastDistance), Color.white, 0f); // Dibuja un raycast blanco si no golpea nada
            }
        }
    }


}
