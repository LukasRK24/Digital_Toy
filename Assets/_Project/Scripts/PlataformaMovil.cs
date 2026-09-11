using UnityEngine;

public class PlataformaMovil : MonoBehaviour
{
    [Header("Puntos de Trayectoria")]
    [SerializeField] private Vector2 puntoA;
    [SerializeField] private Vector2 puntoB;

    [Header("Movimiento")]
    [SerializeField] private float velocidad = 2.5f;

    private Vector2 objetivoActual;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        objetivoActual = puntoB;
    }

    private void FixedUpdate()
    {
        Vector2 posicionActual = rb != null ? rb.position : (Vector2)transform.position;
        Vector2 nuevaPosicion = Vector2.MoveTowards(posicionActual, objetivoActual, velocidad * Time.fixedDeltaTime);

        if (rb != null)
        {
            rb.MovePosition(nuevaPosicion);
        }
        else
        {
            transform.position = nuevaPosicion;
        }

        // Si llego al objetivo, invertimos el destino
        if (Vector2.Distance(posicionActual, objetivoActual) < 0.05f)
        {
            if (objetivoActual == puntoA)
            {
                objetivoActual = puntoB;
            }
            else
            {
                objetivoActual = puntoA;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D colision)
    {
        // Emparentamos al jugador si se apoya sobre la plataforma
        if (colision.gameObject.CompareTag("Player"))
        {
            // Solo si esta parado encima (normal hacia arriba)
            ContactPoint2D contacto = colision.GetContact(0);
            if (contacto.normal.y < -0.5f)
            {
                colision.transform.SetParent(transform);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D colision)
    {
        if (colision.gameObject.CompareTag("Player"))
        {
            colision.transform.SetParent(null);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(puntoA, puntoB);
        Gizmos.DrawWireSphere(puntoA, 0.2f);
        Gizmos.DrawWireSphere(puntoB, 0.2f);
    }
}
