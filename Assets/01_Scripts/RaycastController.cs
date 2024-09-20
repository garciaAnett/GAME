using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
    // Capa con la que se van a detectar colisiones
    public LayerMask collisionMask;

    // Grosor del "skin" del colisionador para detectar colisiones con precisi�n
    public const float skinWidth = .015f;
    public int horizontalRayCount = 4;  // N�mero de rayos horizontales que se emitir�n
    public int verticalRayCount = 4;  // N�mero de rayos verticales que se emitir�n

    // Espaciado entre los rayos
    [HideInInspector]
    public float horizontalRaySpacing;
    [HideInInspector]
    public float verticalRaySpacing;

    // Referencia al BoxCollider2D del objeto
    [HideInInspector]
    public BoxCollider2D collider;

    // Estructura que guarda los or�genes de los rayos para detectar colisiones
    public RaycastOrigins raycastOrigins;

    // M�todo que se llama cuando el script es activado. Se obtiene el componente BoxCollider2D
    public virtual void Awake()
    {
        collider = GetComponent<BoxCollider2D>();
    }

    // M�todo que se llama al iniciar el juego. Calcula el espaciado entre los rayos
    public virtual void Start()
    {
        CalculateRaySpacing();  // Llamada para calcular la distancia entre rayos
    }

    // Actualiza los or�genes de los rayos bas�ndose en los l�mites actuales del colisionador
    public void UpdateRaycastOrigins()
    {
        // Obtener los l�mites del colisionador y reducirlos en funci�n del skinWidth
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);  // Expande los l�mites negativos para ajustar la precisi�n de los rayos

        // Asignar los puntos de origen de los rayos a las esquinas del colisionador
        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    // Calcula el espaciado entre los rayos para asegurarse de que cubren toda el �rea del colisionador
    public void CalculateRaySpacing()
    {
        // Obtener los l�mites del colisionador y ajustarlos por el skinWidth
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);  // Ajustar los l�mites para la precisi�n del raycast

        // Asegurar que el n�mero de rayos horizontales y verticales sea al menos 2
        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        // Calcular el espacio entre rayos horizontales y verticales
        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    // Estructura que contiene las posiciones de los or�genes de los rayos en las esquinas del colisionador
    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;  // Esquinas superiores izquierda y derecha
        public Vector2 bottomLeft, bottomRight;  // Esquinas inferiores izquierda y derecha
    }
}
