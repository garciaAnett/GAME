using UnityEngine;

[RequireComponent(typeof(Controller2D))]
[RequireComponent(typeof(RaycastController))] //asegurar que un componente necesario esté siempre presente 
public class Player : MonoBehaviour
{
    private AudioSource m_AudioSource; // Referencia al componente de AudioSource
    // Parámetros de movimiento del jugador
    public float maxJumpHeight = 4;  // Altura máxima que puede alcanzar el jugador al saltar
    public float minJumpHeight = 1;  // Altura mínima al hacer un salto corto
    public float timeToJumpApex = .4f;  // Tiempo que tarda el jugador en llegar al punto más alto del salto
    float accelerationTimeAirborne = .2f;  // Tiempo de aceleración mientras el jugador está en el aire
    float accelerationTimeGrounded = .1f;  // Tiempo de aceleración mientras el jugador está en el suelo
    float moveSpeed = 6;  // Velocidad de movimiento del jugador en el eje X

    // Parámetros para el salto en paredes
    public Vector2 wallJumpClimb;  // Fuerza aplicada cuando el jugador escala una pared durante un salto
    public Vector2 wallJumpOff;  // Fuerza aplicada cuando el jugador se separa de la pared en un salto
    public Vector2 wallLeap;  // Fuerza aplicada al hacer un salto largo desde una pared

    public float wallSlideSpeedMax = 3;  // Velocidad máxima al deslizarse por una pared
    public float wallStickTime = .25f;  // Tiempo durante el cual el jugador se queda pegado a la pared antes de poder moverse
    float timeToWallUnstick;  // Tiempo restante antes de que el jugador pueda despegarse de la pared

    // Parámetros relacionados con la gravedad y el salto
    float gravity;  // Valor de la gravedad aplicado al jugador
    float maxJumpVelocity;  // Velocidad máxima de salto
    float minJumpVelocity;  // Velocidad mínima para un salto pequeño
    Vector3 velocity;  // Vector que guarda la velocidad actual del jugador
    float velocityXSmoothing;  // Suavizado de la velocidad en el eje X para hacer el movimiento más fluido

    Controller2D controller;  // Referencia al componente que controla las colisiones del jugador
                              //declarando la clase Collider2D

    RaycastController controller2d;

    // Parámetros para la detección de enemigos con raycasts
    public Transform firePoint;  // Punto desde donde se originan los rayos para la detección de enemigos
    public float longDistance = 10f;  // Distancia larga para la detección de enemigos
    public float shortDistance = 3f;  // Distancia corta para la detección de enemigos
    public LayerMask enemyLayer;  // Capa a la que pertenecen los enemigos
    public float raycastDistance = 10f;  // Distancia máxima a la que se disparan los rayos

    void Start()
    {
        m_AudioSource = GetComponent<AudioSource>();
        
        // Inicialización del componente y cálculos iniciales
        controller = GetComponent<Controller2D>();
        //otro
        controller2d = GetComponent<RaycastController>(); // asignando su clase correspondiente
        // Calcular la gravedad y las velocidades de salto según la altura máxima y mínima del salto
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        print("Gravity: " + gravity + "  Jump Velocity: " + maxJumpVelocity);
    }

    void Update()
    {
        // Obtener la entrada del usuario para movimiento horizontal y vertical
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        int wallDirX = (controller.collisions.left) ? -1 : 1;  // Determinar la dirección en la que el jugador está tocando una pared

        // Suavizar el movimiento horizontal usando interpolación, dependiendo de si el jugador está en el suelo o en el aire
        float targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing,
                    (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

        // Detección de enemigos usando raycast en 360 grados
        DetectEnemies360();

        // Manejar el deslizamiento en paredes y los saltos
        HandleWallSliding(input, wallDirX);
        HandleJumping(input, wallDirX);

        // Actualizar la velocidad vertical aplicando gravedad y mover al jugador
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime, input);

        // Reiniciar la velocidad vertical si el jugador colisiona con algo arriba o abajo
        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }
    }

    // Función para manejar el deslizamiento en paredes
    void HandleWallSliding(Vector2 input, int wallDirX)
    {
        bool wallSliding = false;
        // Si el jugador está tocando una pared y no está en el suelo, activar el deslizamiento
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)
        {
            wallSliding = true;

            // Limitar la velocidad de deslizamiento
            if (velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }

            // Controlar el tiempo que el jugador permanece pegado a la pared antes de despegarse
            if (timeToWallUnstick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;

                if (input.x != wallDirX && input.x != 0)
                {
                    timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
            else
            {
                timeToWallUnstick = wallStickTime;
            }
        }
    }

    // Función para manejar los saltos del jugador, tanto en el suelo como en las paredes
    void HandleJumping(Vector2 input, int wallDirX)
    {
        // Detectar si el jugador presiona la tecla de salto
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Si el jugador está tocando una pared, manejar los diferentes tipos de saltos en pared
            if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below)
            {
                if (wallDirX == input.x)  // Si el jugador intenta saltar en la dirección de la pared
                {
                    velocity.x = -wallDirX * wallJumpClimb.x;
                    velocity.y = wallJumpClimb.y;
                }
                else if (input.x == 0)  // Si el jugador no está moviéndose horizontalmente
                {
                    velocity.x = -wallDirX * wallJumpOff.x;
                    velocity.y = wallJumpOff.y;
                }
                else  // Si el jugador salta en dirección opuesta a la pared
                {
                    velocity.x = -wallDirX * wallLeap.x;
                    velocity.y = wallLeap.y;
                }
            }

            // Si el jugador está en el suelo, realizar un salto normal
            if (controller.collisions.below)
            {
                velocity.y = maxJumpVelocity;
            }
        }

        // Si el jugador suelta la tecla de salto, reducir la altura del salto
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (velocity.y > minJumpVelocity)
            {
                velocity.y = minJumpVelocity;
            }
        }
    }

    // Función para detectar enemigos usando raycast en 360 grados
    void DetectEnemies360()
    {
        int numberOfRays = 8;  // Número de rayos emitidos en diferentes direcciones
        float angleStep = 360f / numberOfRays;  // Ángulo entre cada rayo

        for (int i = 0; i < numberOfRays; i++)
        {
            // Calcular la dirección de cada rayo basado en el ángulo
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            // Disparar el rayo y verificar si impacta con un enemigo
            RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, raycastDistance, enemyLayer);

            // Si el rayo impacta un enemigo, dibujar una línea en la escena dependiendo de la distancia al enemigo
            if (hit.collider != null)
            {
                float distanceToEnemy = Vector2.Distance(firePoint.position, hit.point);

                if (distanceToEnemy <= 2f)  // Si el enemigo está cerca, dibujar línea roja
                {
                    Debug.DrawLine(firePoint.position, hit.point, Color.red, 0f);
                }
                else  // Si el enemigo está lejos, dibujar línea amarilla
                {
                    Debug.DrawLine(firePoint.position, hit.point, Color.yellow, 0f);
                }
            }
            else
            {
                // Si el rayo no impacta, dibujar una línea blanca
                Debug.DrawLine(firePoint.position, firePoint.position + (Vector3)(direction * raycastDistance), Color.white, 0f);
            }
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Flag"))
        {
            m_AudioSource.Stop();
            // Aquí puedes implementar lo que sucede cuando Mario colisiona con el enemigo.
            // Ejemplo: Mario pierde una vida o muere.
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Vector2 knockbackDirection;

            // Si el enemigo está a la izquierda, empuja al jugador hacia la derecha
            if (collision.transform.position.x < transform.position.x)
            {
                knockbackDirection = new Vector2(1, 0); // Hacia la derecha
            }
            // Si el enemigo está a la derecha, empuja al jugador hacia la izquierda
            else
            {
                knockbackDirection = new Vector2(-1, 0); // Hacia la izquierda
            }

            float knockbackForce = 10f; // Ajusta esta fuerza según sea necesario
            velocity.x = knockbackDirection.x * knockbackForce;

            // Si quieres también un pequeño empuje hacia arriba al ser golpeado:
            velocity.y = 5f; // Ajusta según sea necesario

            // Aplicar la fuerza de retroceso al jugador
            controller.Move(velocity * Time.deltaTime, Vector2.zero);
            Debug.Log("Si esta chocando");
            m_AudioSource.Play();
        }
    }
}
