using UnityEngine;

public class FrutaColeccionable : MonoBehaviour
{
    [Header("Valor")]
    [SerializeField] private int puntos = 1;

    [Header("Efecto de Flotacion")]
    [SerializeField] private float velocidadFlotacion = 3.5f;
    [SerializeField] private float alturaFlotacion = 0.12f;

    [Header("Sonido")]
    [SerializeField] private AudioClip sonidoColeccionable;

    private Vector3 posicionInicial;
    private bool recolectada = false;

    private void Start()
    {
        posicionInicial = transform.position;
    }

    private void Update()
    {
        if (recolectada) return;

        // Animacion sencilla de flotacion senoidal
        float nuevoY = posicionInicial.y + (Mathf.Sin(Time.time * velocidadFlotacion) * alturaFlotacion);
        transform.position = new Vector3(transform.position.x, nuevoY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (recolectada) return;

        if (otro.CompareTag("Player"))
        {
            recolectada = true;

            // Notificamos al GameManager
            if (ControladorJuego.instancia != null)
            {
                ControladorJuego.instancia.SumarFruta(puntos);
            }

            // Reproducimos sonido en la posicion
            if (sonidoColeccionable != null)
            {
                AudioSource.PlayClipAtPoint(sonidoColeccionable, Camera.main != null ? Camera.main.transform.position : transform.position, 1.0f);
            }

            Destroy(gameObject);
        }
    }
}
