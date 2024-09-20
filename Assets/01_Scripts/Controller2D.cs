using UnityEngine;

public class Controller2D : RaycastController
{
    // �ngulos m�ximos para escalar y descender pendientes
    float maxClimbAngle = 80;  // �ngulo m�ximo que el jugador puede escalar
    float maxDescendAngle = 80;  // �ngulo m�ximo que el jugador puede descender

    public CollisionInfo collisions;  // Estructura para almacenar la informaci�n de las colisiones
    [HideInInspector]
    public Vector2 playerInput;  // Almacena la entrada del jugador (horizontal y vertical)

    public override void Start()
    {
        base.Start();  // Llamada al m�todo Start del padre (RaycastController)
        collisions.faceDir = 1;  // Direcci�n predeterminada del jugador (1 es hacia la derecha)
    }

    // M�todo para mover al jugador, recibe la velocidad y un booleano para saber si est� sobre una plataforma
    public void Move(Vector3 velocity, bool standingOnPlatform)
    {
        // Llamar a la sobrecarga de Move pasando input vac�o
        Move(velocity, Vector2.zero, standingOnPlatform);
    }

    // M�todo principal de movimiento, calcula colisiones y actualiza la posici�n del jugador
    public void Move(Vector3 velocity, Vector2 input, bool standingOnPlatform = false)
    {
        UpdateRaycastOrigins();  // Actualiza los or�genes de los rayos
        collisions.Reset();  // Reinicia el estado de las colisiones
        collisions.velocityOld = velocity;  // Guarda la velocidad antigua
        playerInput = input;  // Almacena la entrada del jugador

        // Actualizar la direcci�n del jugador dependiendo de la velocidad en X
        if (velocity.x != 0)
        {
            collisions.faceDir = (int)Mathf.Sign(velocity.x);
        }

        // Si el jugador se mueve hacia abajo, comprobar si est� descendiendo una pendiente
        if (velocity.y < 0)
        {
            DescendSlope(ref velocity);
        }

        // Manejar colisiones horizontales
        HorizontalCollisions(ref velocity);

        // Manejar colisiones verticales
        if (velocity.y != 0)
        {
            VerticalCollisions(ref velocity);
        }

        // Aplicar el movimiento al jugador
        transform.Translate(velocity);

        // Si est� sobre una plataforma, marcar la colisi�n como "abajo"
        if (standingOnPlatform)
        {
            collisions.below = true;
        }
    }

    // Detecta y maneja colisiones horizontales
    void HorizontalCollisions(ref Vector3 velocity)
    {
        float directionX = collisions.faceDir;  // Direcci�n en la que se est� moviendo el jugador
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;  // Longitud del rayo

        // Si la velocidad es muy peque�a, usar un rayo corto
        if (Mathf.Abs(velocity.x) < skinWidth)
        {
            rayLength = 2 * skinWidth;
        }

        // Emitir rayos en la direcci�n horizontal
        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);  // Ajusta la altura del rayo
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);  // Dibujar el rayo en la escena

            // Si el rayo no impacta nada, continuar con el siguiente
            if (hit)
            {
                if (hit.distance == 0)
                {
                    continue;
                }

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);  // Obtener el �ngulo de la pendiente

                // Manejar la subida de pendientes
                if (i == 0 && slopeAngle <= maxClimbAngle)
                {
                    if (collisions.descendingSlope)
                    {
                        collisions.descendingSlope = false;
                        velocity = collisions.velocityOld;
                    }
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collisions.slopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlopeStart * directionX;
                }

                // Si no est� escalando o el �ngulo es demasiado alto, detener el movimiento horizontal
                if (!collisions.climbingSlope || slopeAngle > maxClimbAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;

                    if (collisions.climbingSlope)
                    {
                        velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }

                    collisions.left = directionX == -1;  // Colisi�n a la izquierda
                    collisions.right = directionX == 1;  // Colisi�n a la derecha
                }
            }
        }
    }

    // Detecta y maneja colisiones verticales
    void VerticalCollisions(ref Vector3 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);  // Direcci�n en la que se mueve el jugador en el eje Y
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;  // Longitud del rayo vertical

        // Emitir rayos en la direcci�n vertical
        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);  // Ajustar la posici�n del rayo
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);  // Dibujar el rayo en la escena

            if (hit)
            {
                // Manejar plataformas atravesables
                if (hit.collider.tag == "Through")
                {
                    if (directionY == 1 || hit.distance == 0)
                    {
                        continue;
                    }
                    if (collisions.fallingThroughPlatform)
                    {
                        continue;
                    }
                    if (playerInput.y == -1)
                    {
                        collisions.fallingThroughPlatform = true;
                        Invoke("ResetFallingThroughPlatform", .5f);
                        continue;
                    }
                }

                // Ajustar la velocidad en el eje Y
                velocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                if (collisions.climbingSlope)
                {
                    velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }

                collisions.below = directionY == -1;  // Colisi�n debajo
                collisions.above = directionY == 1;  // Colisi�n arriba
            }
        }

        // Verificar si el jugador sigue escalando pendientes
        if (collisions.climbingSlope)
        {
            float directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collisions.slopeAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }
    }

    // Manejar el ascenso de pendientes
    void ClimbSlope(ref Vector3 velocity, float slopeAngle)
    {
        float moveDistance = Mathf.Abs(velocity.x);  // Distancia que el jugador se va a mover
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;  // Velocidad en Y al escalar

        if (velocity.y <= climbVelocityY)
        {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
        }
    }
    // M�todo para manejar el descenso de pendientes
    void DescendSlope(ref Vector3 velocity)
    {
        // Determina la direcci�n horizontal en la que se est� moviendo el jugador
        float directionX = Mathf.Sign(velocity.x);

        // Define el origen del rayo dependiendo de la direcci�n en la que se mueve el jugador
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;

        // Emitir un rayo hacia abajo para detectar el �ngulo de la pendiente
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        if (hit)
        {
            // Calcula el �ngulo de la pendiente basado en la normal del rayo
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

            // Si el �ngulo de la pendiente no es 0 y est� dentro del �ngulo m�ximo permitido para descender
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
            {
                // Si el jugador se mueve en la misma direcci�n de la pendiente
                if (Mathf.Sign(hit.normal.x) == directionX)
                {
                    // Si el jugador est� lo suficientemente cerca del inicio de la pendiente
                    if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {
                        // Calcula la distancia que se mover� el jugador y la velocidad en Y al descender
                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

                        // Ajusta la velocidad en X e Y bas�ndose en el �ngulo de la pendiente
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                        velocity.y -= descendVelocityY;

                        // Actualiza la informaci�n de colisi�n para reflejar que est� descendiendo una pendiente
                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.below = true;  // Marca que el jugador est� tocando el suelo
                    }
                }
            }
        }
    }

    // M�todo para reiniciar el estado de colisi�n al caer a trav�s de una plataforma
    void ResetFallingThroughPlatform()
    {
        collisions.fallingThroughPlatform = false;  // Establece que el jugador ya no est� cayendo a trav�s de una plataforma
    }

    // Estructura para almacenar la informaci�n de las colisiones
    public struct CollisionInfo
    {
        public bool above, below;  // Indica si el jugador est� colisionando por arriba o por abajo
        public bool left, right;  // Indica si el jugador est� colisionando por la izquierda o derecha

        public bool climbingSlope;  // Indica si el jugador est� subiendo una pendiente
        public bool descendingSlope;  // Indica si el jugador est� descendiendo una pendiente
        public float slopeAngle, slopeAngleOld;  // Almacena el �ngulo de la pendiente actual y anterior
        public Vector3 velocityOld;  // Almacena la velocidad del jugador antes de colisiones
        public int faceDir;  // Direcci�n en la que est� mirando el jugador (1 para derecha, -1 para izquierda)
        public bool fallingThroughPlatform;  // Indica si el jugador est� cayendo a trav�s de una plataforma

        // M�todo para reiniciar todos los valores de colisi�n
        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;

            // Actualiza el �ngulo de la pendiente
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }
}