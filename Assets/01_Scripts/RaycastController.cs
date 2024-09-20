using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
    // Capa con la que se van a detectar colisiones
    public LayerMask collisionMask;

    // Grosor del "skin" del colisionador para detectar colisiones con precisión
    public const float skinWidth = .015f;
    public int horizontalRayCount = 4;  // Número de rayos horizontales que se emitirán
    public int verticalRayCount = 4;  // Número de rayos verticales que se emitirán

    // Espaciado entre los rayos
    [HideInInspector]
    public float horizontalRaySpacing;
    [HideInInspector]
    public float verticalRaySpacing;

    // Referencia al BoxCollider2D del objeto
    [HideInInspector]
    public BoxCollider2D collider;

    // Estructura que guarda los orígenes de los rayos para detectar colisiones
    public RaycastOrigins raycastOrigins;

    // Método que se llama cuando el script es activado. Se obtiene el componente BoxCollider2D
    public virtual void Awake()
    {
        collider = GetComponent<BoxCollider2D>();
    }

    // Método que se llama al iniciar el juego. Calcula el espaciado entre los rayos
    public virtual void Start()
    {
        CalculateRaySpacing();  // Llamada para calcular la distancia entre rayos
    }

    // Actualiza los orígenes de los rayos basándose en los límites actuales del colisionador
    public void UpdateRaycastOrigins()
    {
        // Obtener los límites del colisionador y reducirlos en función del skinWidth
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);  // Expande los límites negativos para ajustar la precisión de los rayos

        // Asignar los puntos de origen de los rayos a las esquinas del colisionador
        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    // Calcula el espaciado entre los rayos para asegurarse de que cubren toda el área del colisionador
    public void CalculateRaySpacing()
    {
        // Obtener los límites del colisionador y ajustarlos por el skinWidth
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);  // Ajustar los límites para la precisión del raycast

        // Asegurar que el número de rayos horizontales y verticales sea al menos 2
        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        // Calcular el espacio entre rayos horizontales y verticales
        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    // Estructura que contiene las posiciones de los orígenes de los rayos en las esquinas del colisionador
    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;  // Esquinas superiores izquierda y derecha
        public Vector2 bottomLeft, bottomRight;  // Esquinas inferiores izquierda y derecha
    }
}
