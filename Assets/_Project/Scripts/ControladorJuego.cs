using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorJuego : MonoBehaviour
{
    public static ControladorJuego instancia;

    [Header("Conteo de Frutas")]
    [SerializeField] private int totalFrutas = 0;
    [SerializeField] private int frutasRecogidas = 0;

    [Header("Condiciones de Caida y Reinicio")]
    [SerializeField] private float limiteAbismo = -10f;

    [Header("Musica de Fondo")]
    [SerializeField] private AudioClip musicaFondo;
    private AudioSource fuenteMusica;

    private Transform jugador;
    private Vector3 puntoAparicionInicial;
    private bool nivelCompletado = false;

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
        {
            jugador = p.transform;
            puntoAparicionInicial = jugador.position;
        }

        // Iniciar musica de fondo alegre en bucle
        if (musicaFondo != null)
        {
            fuenteMusica = GetComponent<AudioSource>();
            if (fuenteMusica == null)
            {
                fuenteMusica = gameObject.AddComponent<AudioSource>();
            }
            fuenteMusica.clip = musicaFondo;
            fuenteMusica.loop = true;
            fuenteMusica.volume = 0.35f;
            fuenteMusica.Play();
        }

        // Si no se contaron las frutas, las buscamos en la escena para que nunca salga 0 / 0
        FrutaColeccionable[] frutas = Object.FindObjectsByType<FrutaColeccionable>(FindObjectsSortMode.None);
        if (frutas != null && frutas.Length > 0)
        {
            totalFrutas = frutas.Length;
        }
        else if (totalFrutas == 0)
        {
            totalFrutas = 5;
        }
    }

    private void Update()
    {
        // 1. Reinicio con la tecla R de forma segura
        bool presionoR = false;
        try
        {
            presionoR = Input.GetKeyDown(KeyCode.R);
        }
        catch {}

        #if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            if (UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
            {
                presionoR = true;
            }
        }
        #endif

        if (presionoR)
        {
            ReiniciarNivel();
        }

        // 2. Si el personaje cae al vacio, lo regresamos al inicio
        if (jugador != null && jugador.position.y < limiteAbismo)
        {
            ReaparecerJugador();
        }
    }

    public void RegistrarFruta()
    {
        // El conteo se gestiona de forma centralizada en Start para evitar duplicados
    }

    public void RegistrarFrutaTotal()
    {
        // El conteo se gestiona de forma centralizada en Start para evitar duplicados
    }

    public void SumarFruta()
    {
        SumarFruta(1);
    }

    public void SumarFruta(int cantidad)
    {
        frutasRecogidas = frutasRecogidas + cantidad;

        if (frutasRecogidas >= totalFrutas && totalFrutas > 0)
        {
            nivelCompletado = true;
        }
    }

    public void ReaparecerJugador()
    {
        if (jugador != null)
        {
            jugador.position = puntoAparicionInicial;
            Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    public void ReiniciarNivel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Interfaz visual OnGUI con estilo retro
    private void OnGUI()
    {
        // 1. Recuadro HUD superior izquierdo
        int altoCaja = 105;
        GUI.Box(new Rect(15, 15, 335, altoCaja), "");

        GUILayout.BeginArea(new Rect(25, 20, 315, altoCaja - 10));
        
        GUIStyle estiloTitulo = new GUIStyle(GUI.skin.label);
        estiloTitulo.fontStyle = FontStyle.Bold;
        estiloTitulo.fontSize = 13;
        estiloTitulo.normal.textColor = Color.yellow;
        GUILayout.Label("Digital Toy", estiloTitulo);

        GUIStyle estiloTexto = new GUIStyle(GUI.skin.label);
        estiloTexto.fontSize = 12;
        estiloTexto.normal.textColor = Color.white;

        string textoFrutas = "Frutas: " + frutasRecogidas + " / " + totalFrutas;
        if (nivelCompletado)
        {
            GUIStyle estiloFrutasCompletas = new GUIStyle(estiloTexto);
            estiloFrutasCompletas.normal.textColor = new Color(0.3f, 1f, 0.4f);
            estiloFrutasCompletas.fontStyle = FontStyle.Bold;
            GUILayout.Label(textoFrutas + " (Completado)", estiloFrutasCompletas);
        }
        else
        {
            GUILayout.Label(textoFrutas, estiloTexto);
        }

        GUILayout.Label("Moverse: A / D o Flechas | Salto: Espacio", estiloTexto);
        GUILayout.Label("Dash: Shift | Reiniciar: R", estiloTexto);

        GUILayout.EndArea();

        // 2. Banner de victoria destacado y centrado al completar el nivel
        if (nivelCompletado)
        {
            float anchoBanner = 320f;
            float altoBanner = 55f;
            float posX = (Screen.width - anchoBanner) * 0.5f;
            float posY = 20f;

            GUI.Box(new Rect(posX, posY, anchoBanner, altoBanner), "");

            GUIStyle estiloBanner = new GUIStyle(GUI.skin.label);
            estiloBanner.alignment = TextAnchor.MiddleCenter;
            estiloBanner.fontStyle = FontStyle.Bold;
            estiloBanner.fontSize = 15;
            estiloBanner.normal.textColor = new Color(0.2f, 1f, 0.35f);
            GUI.Label(new Rect(posX, posY + 6, anchoBanner, 24), "¡NIVEL COMPLETADO!", estiloBanner);

            GUIStyle estiloSub = new GUIStyle(GUI.skin.label);
            estiloSub.alignment = TextAnchor.MiddleCenter;
            estiloSub.fontSize = 12;
            estiloSub.normal.textColor = Color.white;
            GUI.Label(new Rect(posX, posY + 28, anchoBanner, 20), "¡Recolectaste las 5 frutas con exito!", estiloSub);
        }
    }
}
