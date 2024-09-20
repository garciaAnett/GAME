using UnityEngine;

public class Controller2D : RaycastController
{
    // Ángulos máximos para escalar y descender pendientes
    float maxClimbAngle = 80;  // Ángulo máximo que el jugador puede escalar
    float maxDescendAngle = 80;  // Ángulo máximo que el jugador puede descender

    public CollisionInfo collisions;  // Estructura para almacenar la información de las colisiones
    [HideInInspector]
    public Vector2 playerInput;  // Almacena la entrada del jugador (horizontal y vertical)

    public override void Start()
    {
        base.Start();  // Llamada al método Start del padre (RaycastController)
        collisions.faceDir = 1;  // Dirección predeterminada del jugador (1 es hacia la derecha)
    }

    // Método para mover al jugador, recibe la velocidad y un booleano para saber si está sobre una plataforma
    public void Move(Vector3 velocity, bool standingOnPlatform)
    {
        // Llamar a la sobrecarga de Move pasando input vacío
        Move(velocity, Vector2.zero, standingOnPlatform);
    }

    // Método principal de movimiento, calcula colisiones y actualiza la posición del jugador
    public void Move(Vector3 velocity, Vector2 input, bool standingOnPlatform = false)
    {
        UpdateRaycastOrigins();  // Actualiza los orígenes de los rayos
        collisions.Reset();  // Reinicia el estado de las colisiones
        collisions.velocityOld = velocity;  // Guarda la velocidad antigua
        playerInput = input;  // Almacena la entrada del jugador

        // Actualizar la dirección del jugador dependiendo de la velocidad en X
        if (velocity.x != 0)
        {
            collisions.faceDir = (int)Mathf.Sign(velocity.x);
        }

        // Si el jugador se mueve hacia abajo, comprobar si está descendiendo una pendiente
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

        // Si está sobre una plataforma, marcar la colisión como "abajo"
        if (standingOnPlatform)
        {
            collisions.below = true;
        }
    }

    // Detecta y maneja colisiones horizontales
    void HorizontalCollisions(ref Vector3 velocity)
    {
        float directionX = collisions.faceDir;  // Dirección en la que se está moviendo el jugador
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;  // Longitud del rayo

        // Si la velocidad es muy pequeña, usar un rayo corto
        if (Mathf.Abs(velocity.x) < skinWidth)
        {
            rayLength = 2 * skinWidth;
        }

        // Emitir rayos en la dirección horizontal
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

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);  // Obtener el ángulo de la pendiente

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

                // Si no está escalando o el ángulo es demasiado alto, detener el movimiento horizontal
                if (!collisions.climbingSlope || slopeAngle > maxClimbAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;

                    if (collisions.climbingSlope)
                    {
                        velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }

                    collisions.left = directionX == -1;  // Colisión a la izquierda
                    collisions.right = directionX == 1;  // Colisión a la derecha
                }
            }
        }
    }

    // Detecta y maneja colisiones verticales
    void VerticalCollisions(ref Vector3 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);  // Dirección en la que se mueve el jugador en el eje Y
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;  // Longitud del rayo vertical

        // Emitir rayos en la dirección vertical
        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);  // Ajustar la posición del rayo
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

                collisions.below = directionY == -1;  // Colisión debajo
                collisions.above = directionY == 1;  // Colisión arriba
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
    // Método para manejar el descenso de pendientes
    void DescendSlope(ref Vector3 velocity)
    {
        // Determina la dirección horizontal en la que se está moviendo el jugador
        float directionX = Mathf.Sign(velocity.x);

        // Define el origen del rayo dependiendo de la dirección en la que se mueve el jugador
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;

        // Emitir un rayo hacia abajo para detectar el ángulo de la pendiente
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        if (hit)
        {
            // Calcula el ángulo de la pendiente basado en la normal del rayo
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

            // Si el ángulo de la pendiente no es 0 y está dentro del ángulo máximo permitido para descender
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
            {
                // Si el jugador se mueve en la misma dirección de la pendiente
                if (Mathf.Sign(hit.normal.x) == directionX)
                {
                    // Si el jugador está lo suficientemente cerca del inicio de la pendiente
                    if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {
                        // Calcula la distancia que se moverá el jugador y la velocidad en Y al descender
                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

                        // Ajusta la velocidad en X e Y basándose en el ángulo de la pendiente
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                        velocity.y -= descendVelocityY;

                        // Actualiza la información de colisión para reflejar que está descendiendo una pendiente
                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.below = true;  // Marca que el jugador está tocando el suelo
                    }
                }
            }
        }
    }

    // Método para reiniciar el estado de colisión al caer a través de una plataforma
    void ResetFallingThroughPlatform()
    {
        collisions.fallingThroughPlatform = false;  // Establece que el jugador ya no está cayendo a través de una plataforma
    }

    // Estructura para almacenar la información de las colisiones
    public struct CollisionInfo
    {
        public bool above, below;  // Indica si el jugador está colisionando por arriba o por abajo
        public bool left, right;  // Indica si el jugador está colisionando por la izquierda o derecha

        public bool climbingSlope;  // Indica si el jugador está subiendo una pendiente
        public bool descendingSlope;  // Indica si el jugador está descendiendo una pendiente
        public float slopeAngle, slopeAngleOld;  // Almacena el ángulo de la pendiente actual y anterior
        public Vector3 velocityOld;  // Almacena la velocidad del jugador antes de colisiones
        public int faceDir;  // Dirección en la que está mirando el jugador (1 para derecha, -1 para izquierda)
        public bool fallingThroughPlatform;  // Indica si el jugador está cayendo a través de una plataforma

        // Método para reiniciar todos los valores de colisión
        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;

            // Actualiza el ángulo de la pendiente
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }
}