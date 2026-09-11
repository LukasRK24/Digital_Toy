using UnityEngine;

public class PersonajeSecundario : MonoBehaviour
{
    [Header("Identidad")]
    [SerializeField] private string nombrePersonaje = "Amigo";
    [SerializeField] private string mensajeSaludo = "¡Hola!";

    [Header("Efecto de Respiracion")]
    [SerializeField] private float rapidezRespiracion = 2.5f;
    [SerializeField] private float intensidad = 0.05f;

    private Vector3 escalaOriginal;
    private Transform jugador;
    private SpriteRenderer sr;
    private bool mostrarMensaje = false;

    private void Start()
    {
        escalaOriginal = transform.localScale;
        sr = GetComponent<SpriteRenderer>();

        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
        {
            jugador = p.transform;
        }
    }

    private void Update()
    {
        // 1. Respiracion suave senoidal para dar sensacion de vida
        float factor = 1f + (Mathf.Sin(Time.time * rapidezRespiracion) * intensidad);
        transform.localScale = new Vector3(escalaOriginal.x, escalaOriginal.y * factor, escalaOriginal.z);

        // 2. Orientarse hacia donde esta el jugador si esta cerca
        if (jugador != null && sr != null)
        {
            float distancia = Vector2.Distance(transform.position, jugador.position);
            mostrarMensaje = (distancia < 3.2f);

            // Voltear sprite hacia el jugador
            sr.flipX = (jugador.position.x < transform.position.x);
        }
    }

    private void OnGUI()
    {
        if (mostrarMensaje && Camera.main != null)
        {
            Vector3 posPantalla = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0f, 1.25f, 0f));
            if (posPantalla.z > 0f)
            {
                float ancho = 230f;
                float alto = 54f;
                float x = posPantalla.x - (ancho / 2f);
                float y = Screen.height - posPantalla.y - alto;

                // Fondo del recuadro
                GUI.Box(new Rect(x, y, ancho, alto), "");

                // Nombre del personaje en la parte superior
                GUIStyle estiloNombre = new GUIStyle(GUI.skin.label);
                estiloNombre.fontStyle = FontStyle.Bold;
                estiloNombre.fontSize = 11;
                estiloNombre.normal.textColor = Color.yellow;
                GUI.Label(new Rect(x + 8, y + 4, ancho - 16, 16), nombrePersonaje, estiloNombre);

                // Mensaje en la parte inferior con ajuste de linea
                GUIStyle estiloMensaje = new GUIStyle(GUI.skin.label);
                estiloMensaje.wordWrap = true;
                estiloMensaje.fontSize = 11;
                estiloMensaje.normal.textColor = Color.white;
                GUI.Label(new Rect(x + 8, y + 20, ancho - 16, alto - 22), mensajeSaludo, estiloMensaje);
            }
        }
    }
}
