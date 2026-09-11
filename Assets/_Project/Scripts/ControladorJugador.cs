using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ControladorJugador : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidadCaminar = 8.5f;
    [SerializeField] private float aceleracion = 45f;
    [SerializeField] private float desaceleracion = 35f;

    [Header("Salto y Suelo")]
    [SerializeField] private float fuerzaSalto = 13.5f;
    [SerializeField] private float gravedadCaida = 2.8f;
    [SerializeField] private float gravedadCorta = 2.2f;
    [SerializeField] private Transform detectorSuelo;
    [SerializeField] private float radioSuelo = 0.28f;
    [SerializeField] private LayerMask capaSuelo;

    [Header("Game Feel")]
    [SerializeField] private float tiempoCoyote = 0.15f;
    [SerializeField] private float bufferSalto = 0.15f;

    [Header("Habilidad Dash")]
    [SerializeField] private float velocidadDash = 18f;
    [SerializeField] private float duracionDash = 0.18f;
    [SerializeField] private float tiempoRecargaDash = 0.8f;

    [Header("Efectos Visuales")]
    [SerializeField] private Transform modeloVisual;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Sonidos")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoSalto;
    [SerializeField] private AudioClip sonidoAterrizaje;
    [SerializeField] private AudioClip sonidoDash;

    // Variables privadas para el calculo del movimiento
    private Rigidbody2D rb;
    private float moverX;
    private bool tocandoSuelo;
    private bool estabaEnSuelo;
    private float relojCoyote;
    private float relojBuffer;
    private bool sostieneSalto;

    private bool enDash;
    private bool puedeDash = true;
    private float tiempoDashActual;
    private float tiempoRecargaActual;
    private float direccionX = 1f;
    private Vector3 escalaInicial;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Si no se asigno en el inspector, buscamos el modelo visual
        if (modeloVisual == null)
        {
            Transform hijo = transform.Find("Visual");
            if (hijo != null)
            {
                modeloVisual = hijo;
            }
            else
            {
                modeloVisual = transform;
            }
        }
        escalaInicial = modeloVisual.localScale;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Evitar que el personaje gire al chocar con esquinas
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Update()
    {
        // 1. Leer los controles del jugador (compatible con cualquier configuracion de Unity)
        LeerEntradaControles();

        // 2. Gestion de la recarga del dash
        if (!puedeDash)
        {
            tiempoRecargaActual = tiempoRecargaActual - Time.deltaTime;
            if (tiempoRecargaActual <= 0f)
            {
                puedeDash = true;
            }
        }

        // 3. Temporizadores de coyote time y salto buffer
        RevisarSuelo();

        if (tocandoSuelo)
        {
            relojCoyote = tiempoCoyote;
        }
        else
        {
            relojCoyote = relojCoyote - Time.deltaTime;
        }

        if (relojBuffer > 0f)
        {
            relojBuffer = relojBuffer - Time.deltaTime;
        }

        // 4. Saltar si el jugador presiono salto y tiene tiempo de coyote
        if (relojBuffer > 0f && relojCoyote > 0f && !enDash)
        {
            HacerSalto();
        }

        // 5. Voltear el sprite segun la direccion donde camina
        if (moverX > 0.05f)
        {
            direccionX = 1f;
            if (spriteRenderer != null) spriteRenderer.flipX = false;
        }
        else if (moverX < -0.05f)
        {
            direccionX = -1f;
            if (spriteRenderer != null) spriteRenderer.flipX = true;
        }
    }

    private void FixedUpdate()
    {
        // Si estamos haciendo dash, procesamos ese impulso especial
        if (enDash)
        {
            ProcesarDash();
            return;
        }

        // Movimiento horizontal normal con aceleracion
        MoverHorizontal();

        // Gravedad dinamica para mejorar la sensacion al caer
        AjustarGravedadSalto();
    }

    // Metodo para leer teclado de forma segura sin excepciones de Unity
    private void LeerEntradaControles()
    {
        float h = 0f;
        bool saltoAbajo = false;
        bool saltoSostenido = false;
        bool dashBoton = false;

        // Intentamos con el sistema clasico
        try
        {
            h = Input.GetAxisRaw("Horizontal");
            saltoAbajo = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
            saltoSostenido = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
            dashBoton = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.J);
        }
        catch
        {
            // Si Unity tiene configurado solo el Input System nuevo
        }

        #if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            var teclado = UnityEngine.InputSystem.Keyboard.current;
            if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed) h = -1f;
            else if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed) h = 1f;

            if (teclado.spaceKey.wasPressedThisFrame || teclado.wKey.wasPressedThisFrame || teclado.upArrowKey.wasPressedThisFrame)
                saltoAbajo = true;

            if (teclado.spaceKey.isPressed || teclado.wKey.isPressed || teclado.upArrowKey.isPressed)
                saltoSostenido = true;

            if (teclado.leftShiftKey.wasPressedThisFrame || teclado.rightShiftKey.wasPressedThisFrame || teclado.jKey.wasPressedThisFrame)
                dashBoton = true;
        }
        #endif

        moverX = h;
        sostieneSalto = saltoSostenido;

        if (saltoAbajo)
        {
            relojBuffer = bufferSalto;
        }

        if (dashBoton && puedeDash && !enDash)
        {
            HacerDash();
        }
    }

    private void RevisarSuelo()
    {
        estabaEnSuelo = tocandoSuelo;
        tocandoSuelo = false;

        // Si el personaje esta subiendo en el aire, no puede estar tocando suelo
        if (rb != null && rb.linearVelocity.y > 0.15f)
        {
            return;
        }

        Vector2 punto = (detectorSuelo != null) ? (Vector2)detectorSuelo.position : new Vector2(transform.position.x, transform.position.y - 0.5f);
        Collider2D[] contactos = Physics2D.OverlapCircleAll(punto, radioSuelo, capaSuelo);

        for (int i = 0; i < contactos.Length; i++)
        {
            Collider2D c = contactos[i];
            if (c == null || c.isTrigger) continue;

            // Ignoramos el propio collider del jugador y sus objetos hijos
            if (c.gameObject == gameObject || c.transform.IsChildOf(transform)) continue;

            tocandoSuelo = true;
            break;
        }

        // Si recien toca el suelo tras caer
        if (!estabaEnSuelo && tocandoSuelo)
        {
            if (audioSource != null && sonidoAterrizaje != null)
            {
                audioSource.PlayOneShot(sonidoAterrizaje, 0.6f);
            }
            StopAllCoroutines();
            StartCoroutine(EfectoAplastar(new Vector3(1.3f, 0.7f, 1f), 14f));
        }
    }

    private void MoverHorizontal()
    {
        float velocidadObjetivo = moverX * velocidadCaminar;
        float velocidadActual = rb.linearVelocity.x;

        float tasa = (Mathf.Abs(moverX) > 0.05f) ? aceleracion : desaceleracion;
        float nuevoX = Mathf.MoveTowards(velocidadActual, velocidadObjetivo, tasa * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(nuevoX, rb.linearVelocity.y);
    }

    private void AjustarGravedadSalto()
    {
        // Al caer, baja con mas peso para que no parezca que flota
        if (rb.linearVelocity.y < 0f)
        {
            float extra = Physics2D.gravity.y * (gravedadCaida - 1f) * Time.fixedDeltaTime;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y + extra);
        }
        // Si solto el boton de saltar rapido, corta el salto
        else if (rb.linearVelocity.y > 0f && !sostieneSalto)
        {
            float corte = Physics2D.gravity.y * (gravedadCorta - 1f) * Time.fixedDeltaTime;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y + corte);
        }
    }

    private void HacerSalto()
    {
        // En Unity 6 usamos linearVelocity directamente
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
        relojBuffer = 0f;
        relojCoyote = 0f;
        tocandoSuelo = false;

        if (audioSource != null && sonidoSalto != null)
        {
            audioSource.PlayOneShot(sonidoSalto, 0.8f);
        }

        StopAllCoroutines();
        StartCoroutine(EfectoAplastar(new Vector3(0.75f, 1.35f, 1f), 12f));
    }

    private void HacerDash()
    {
        enDash = true;
        puedeDash = false;
        tiempoDashActual = duracionDash;
        tiempoRecargaActual = tiempoRecargaDash;

        rb.linearVelocity = new Vector2(direccionX * velocidadDash, 0f);

        if (audioSource != null && sonidoDash != null)
        {
            audioSource.PlayOneShot(sonidoDash, 0.85f);
        }

        if (ControladorCamara.instancia != null)
        {
            ControladorCamara.instancia.SacudirCamara(0.12f, 0.25f);
        }
    }

    private void ProcesarDash()
    {
        tiempoDashActual = tiempoDashActual - Time.fixedDeltaTime;
        rb.linearVelocity = new Vector2(direccionX * velocidadDash, 0f);

        if (tiempoDashActual <= 0f)
        {
            enDash = false;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.4f, 0f);
        }
    }

    // Efecto visual de estirar y aplastar para que el personaje se sienta vivo
    private IEnumerator EfectoAplastar(Vector3 deformacion, float velocidadRecuperar)
    {
        if (modeloVisual == null) yield break;

        modeloVisual.localScale = Vector3.Scale(escalaInicial, deformacion);
        while (Vector3.Distance(modeloVisual.localScale, escalaInicial) > 0.02f)
        {
            modeloVisual.localScale = Vector3.Lerp(modeloVisual.localScale, escalaInicial, Time.deltaTime * velocidadRecuperar);
            yield return null;
        }
        modeloVisual.localScale = escalaInicial;
    }

    // Dibujamos el radio de deteccion en la escena para verificar en el editor
    private void OnDrawGizmosSelected()
    {
        if (detectorSuelo != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(detectorSuelo.position, radioSuelo);
        }
    }
}
