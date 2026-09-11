using System.Collections;
using UnityEngine;

public class ControladorCamara : MonoBehaviour
{
    public static ControladorCamara instancia;

    [Header("Seguimiento del Objetivo")]
    [SerializeField] private Transform objetivo;
    [SerializeField] private Vector3 desfase = new Vector3(0f, 1.5f, -10f);
    [SerializeField] private float suavizado = 0.2f;

    private Vector3 velocidadReferencia = Vector3.zero;
    private Vector3 desplazamientoSacudida = Vector3.zero;

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
        if (objetivo == null)
        {
            GameObject jugador = GameObject.FindWithTag("Player");
            if (jugador != null)
            {
                objetivo = jugador.transform;
            }
        }

        // Posicionamos inmediatamente la camara cerca del objetivo
        if (objetivo != null)
        {
            transform.position = objetivo.position + desfase;
        }
    }

    private void LateUpdate()
    {
        if (objetivo == null)
        {
            GameObject jugador = GameObject.FindWithTag("Player");
            if (jugador != null)
            {
                objetivo = jugador.transform;
            }
            return;
        }

        Vector3 posicionDeseada = objetivo.position + desfase;
        Vector3 posicionSuave = Vector3.SmoothDamp(transform.position, posicionDeseada, ref velocidadReferencia, suavizado);

        transform.position = posicionSuave + desplazamientoSacudida;
    }

    public void SacudirCamara(float duracion = 0.15f, float fuerza = 0.2f)
    {
        StartCoroutine(RutinaSacudida(duracion, fuerza));
    }

    private IEnumerator RutinaSacudida(float duracion, float fuerza)
    {
        float transcurrido = 0f;

        while (transcurrido < duracion)
        {
            float x = Random.Range(-1f, 1f) * fuerza;
            float y = Random.Range(-1f, 1f) * fuerza;
            desplazamientoSacudida = new Vector3(x, y, 0f);

            transcurrido = transcurrido + Time.deltaTime;
            yield return null;
        }

        desplazamientoSacudida = Vector3.zero;
    }
}
