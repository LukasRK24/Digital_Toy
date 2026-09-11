#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class CreadorNivelJugueteDigital
{
    [MenuItem("JugueteDigital/Generar Nivel Evaluacion U1")]
    public static void GenerarNivel()
    {
        // 1. Abrir o crear la escena principal
        SceneSetup();

        // 2. Cargar los sprites recortados de alta calidad
        Sprite spJugador = CargarSprite("Assets/_Project/Sprites/Elementos/NinjaFrog.png");
        Sprite spMaskDude = CargarSprite("Assets/_Project/Sprites/Elementos/MaskDude.png");
        Sprite spPinkMan = CargarSprite("Assets/_Project/Sprites/Elementos/PinkMan.png");
        Sprite spPollo = CargarSprite("Assets/_Project/Sprites/Elementos/Chicken.png");
        Sprite spConejo = CargarSprite("Assets/_Project/Sprites/Elementos/Bunny.png");

        Sprite spPasto = CargarSprite("Assets/_Project/Sprites/Elementos/Plataforma_Pasto.png");
        if (spPasto == null) spPasto = CargarSprite("Assets/_Project/Sprites/Elementos/Tile_Pasto.png");

        Sprite spHielo = CargarSprite("Assets/_Project/Sprites/Elementos/Tile_Hielo.png");
        Sprite spTrampolin = CargarSprite("Assets/_Project/Sprites/Elementos/Tile_Trampolin.png");
        Sprite spCaja = CargarSprite("Assets/_Project/Sprites/Elementos/Caja_Madera.png");

        Sprite spFondoMontanas = CargarSprite("Assets/_Project/Sprites/Background/Fondo_Montanas_Hermoso.png");

        Sprite spManzana = CargarSprite("Assets/_Project/Sprites/Elementos/Fruta_Manzana.png");
        Sprite spPlatano = CargarSprite("Assets/_Project/Sprites/Elementos/Fruta_Platano.png");
        Sprite spCerezas = CargarSprite("Assets/_Project/Sprites/Elementos/Fruta_Cerezas.png");
        Sprite spPina = CargarSprite("Assets/_Project/Sprites/Elementos/Fruta_Pina.png");
        Sprite spMelon = CargarSprite("Assets/_Project/Sprites/Elementos/Fruta_Melon.png");

        // Materiales fisicos 2D
        PhysicsMaterial2D matHielo = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/_Project/Materials/Mat_Hielo.physicsMaterial2D");
        PhysicsMaterial2D matRebote = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/_Project/Materials/Mat_Rebote.physicsMaterial2D");
        PhysicsMaterial2D matNormal = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/_Project/Materials/Mat_Normal.physicsMaterial2D");

        // Efectos de sonido y musica
        AudioClip bgmMusica = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Sounds/bgm_musica.wav");
        AudioClip sfxSalto = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Sounds/sfx_salto.wav");
        AudioClip sfxMoneda = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Sounds/sfx_moneda.wav");
        AudioClip sfxDash = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Sounds/sfx_dash.wav");
        AudioClip sfxAterrizaje = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Sounds/sfx_aterrizaje.wav");

        // Capa de Suelo
        int capaSuelo = LayerMask.NameToLayer("Suelo");
        if (capaSuelo < 0) capaSuelo = LayerMask.NameToLayer("Default");

        // 3. --- CONTROLADORES ---
        GameObject grpControladores = new GameObject("---CONTROLADORES---");
        GameObject objGM = new GameObject("ControladorJuego");
        objGM.transform.SetParent(grpControladores.transform);
        ControladorJuego compGM = objGM.AddComponent<ControladorJuego>();
        if (bgmMusica != null)
        {
            SerializedObject soGM = new SerializedObject(compGM);
            SerializedProperty propBGM = soGM.FindProperty("musicaFondo");
            if (propBGM != null) propBGM.objectReferenceValue = bgmMusica;
            soGM.ApplyModifiedProperties();
        }

        // 4. --- CAMARA ---
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject objCam = new GameObject("Main Camera");
            cam = objCam.AddComponent<Camera>();
            objCam.tag = "MainCamera";
            objCam.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.backgroundColor = new Color(0.24f, 0.45f, 0.75f);
        cam.transform.position = new Vector3(-4f, 0.5f, -10f);

        ControladorCamara camCtrl = cam.GetComponent<ControladorCamara>();
        if (camCtrl == null) camCtrl = cam.gameObject.AddComponent<ControladorCamara>();

        // 5. --- FONDO ESCENICO (Cielo azul, Cordillera Nevada y Bosque Verde) ---
        GameObject grpFondo = new GameObject("---FONDO---");

        GameObject objMontanas = new GameObject("Fondo_Montanas_Hermoso");
        objMontanas.transform.SetParent(grpFondo.transform);
        objMontanas.transform.position = new Vector3(5f, 3.2f, 8f);
        SpriteRenderer srMontanas = objMontanas.AddComponent<SpriteRenderer>();
        srMontanas.sprite = spFondoMontanas;
        srMontanas.sortingOrder = -20;
        objMontanas.transform.localScale = new Vector3(2.8f, 2.2f, 1f);

        // 6. --- ENTORNO Y PLATAFORMAS (Con las 3 fisicas requeridas) ---
        GameObject grpEntorno = new GameObject("---ENTORNO---");

        // Suelo principal largo
        CrearPlataforma("Suelo_Principal", new Vector2(5f, -3.5f), new Vector2(36f, 1.5f), spPasto, matNormal, capaSuelo, Color.white, grpEntorno.transform);

        // Paredes delimitadoras en los extremos del nivel y techo invisible
        CrearPlataforma("Pared_Izquierda", new Vector2(-13f, 4f), new Vector2(1.5f, 20f), spPasto, matNormal, capaSuelo, new Color(0.8f, 0.8f, 0.8f), grpEntorno.transform);
        CrearPlataforma("Pared_Derecha", new Vector2(23f, 4f), new Vector2(1.5f, 20f), spPasto, matNormal, capaSuelo, new Color(0.8f, 0.8f, 0.8f), grpEntorno.transform);
        CrearPlataforma("Techo_Limite", new Vector2(5f, 13f), new Vector2(38f, 1.5f), null, matNormal, capaSuelo, Color.clear, grpEntorno.transform);

        // a) Plataforma Normal
        CrearPlataforma("Plataforma_Normal", new Vector2(-1.5f, -1f), new Vector2(4.5f, 0.8f), spPasto, matNormal, capaSuelo, Color.white, grpEntorno.transform);

        // b) Plataforma de Hielo (Friccion muy baja: 0.02)
        CrearPlataforma("Plataforma_Hielo", new Vector2(4.5f, 0.5f), new Vector2(5f, 0.8f), spHielo, matHielo, capaSuelo, Color.white, grpEntorno.transform);

        // c) Plataforma de Rebote (Elasticidad alta: 0.85)
        CrearPlataforma("Plataforma_Rebote", new Vector2(13.5f, -1f), new Vector2(4f, 0.8f), spTrampolin, matRebote, capaSuelo, new Color(1f, 0.85f, 0.4f), grpEntorno.transform);

        // d) Plataforma de la Meta (En lo alto)
        CrearPlataforma("Plataforma_Meta", new Vector2(17f, 4f), new Vector2(5.5f, 0.8f), spPasto, matNormal, capaSuelo, new Color(0.9f, 0.8f, 1f), grpEntorno.transform);

        // 7. --- INTERACTIVOS (Plataforma Movil y Caja Empujable) ---
        GameObject grpInteractivos = new GameObject("---INTERACTIVOS---");

        // Plataforma Movil
        GameObject objPlatMovil = new GameObject("Plataforma_Movil");
        objPlatMovil.transform.SetParent(grpInteractivos.transform);
        objPlatMovil.transform.position = new Vector3(8.5f, 2.2f, 0f);
        objPlatMovil.layer = capaSuelo;

        SpriteRenderer srMovil = objPlatMovil.AddComponent<SpriteRenderer>();
        srMovil.sprite = (spPasto != null) ? spPasto : spHielo;
        srMovil.drawMode = SpriteDrawMode.Tiled;
        srMovil.size = new Vector2(3.5f, 0.65f);
        srMovil.sortingOrder = 2;
        srMovil.color = new Color(0.8f, 1f, 0.8f);

        BoxCollider2D colMovil = objPlatMovil.AddComponent<BoxCollider2D>();
        colMovil.size = new Vector2(3.5f, 0.65f);
        colMovil.sharedMaterial = matNormal;

        PlataformaMovil pmComp = objPlatMovil.AddComponent<PlataformaMovil>();
        SerializedObject soPM = new SerializedObject(pmComp);
        soPM.FindProperty("puntoA").vector2Value = new Vector2(7.5f, 2.2f);
        soPM.FindProperty("puntoB").vector2Value = new Vector2(11.5f, 2.2f);
        soPM.FindProperty("velocidad").floatValue = 2.4f;
        soPM.ApplyModifiedProperties();

        // Caja Empujable
        GameObject objCaja = new GameObject("Caja_Empujable");
        objCaja.transform.SetParent(grpInteractivos.transform);
        objCaja.transform.position = new Vector3(0.5f, -2.2f, 0f);
        objCaja.layer = capaSuelo;

        SpriteRenderer srCaja = objCaja.AddComponent<SpriteRenderer>();
        srCaja.sprite = spCaja;
        srCaja.sortingOrder = 3;

        BoxCollider2D colCaja = objCaja.AddComponent<BoxCollider2D>();
        colCaja.size = new Vector2(0.9f, 0.9f);
        colCaja.sharedMaterial = matNormal;

        Rigidbody2D rbCaja = objCaja.AddComponent<Rigidbody2D>();
        rbCaja.mass = 12f;
        rbCaja.linearDamping = 2f;
        rbCaja.freezeRotation = true;
        objCaja.AddComponent<CajaEmpujable>();

        // 8. --- COLECCIONABLES (5 Frutas) ---
        GameObject grpColeccionables = new GameObject("---COLECCIONABLES---");
        CrearFruta("Fruta_Manzana", new Vector3(-1.5f, 0.2f, 0f), spManzana, sfxMoneda, grpColeccionables.transform);
        CrearFruta("Fruta_Platano", new Vector3(4.5f, 1.8f, 0f), spPlatano, sfxMoneda, grpColeccionables.transform);
        CrearFruta("Fruta_Cerezas", new Vector3(9.5f, 3.2f, 0f), spCerezas, sfxMoneda, grpColeccionables.transform);
        CrearFruta("Fruta_Pina", new Vector3(13.5f, 2.8f, 0f), spPina, sfxMoneda, grpColeccionables.transform);
        CrearFruta("Fruta_Melon", new Vector3(18f, 5.1f, 0f), spMelon, sfxMoneda, grpColeccionables.transform);

        // 9. --- PERSONAJES SECUNDARIOS (NPCs con vida) ---
        GameObject grpPersonajes = new GameObject("---PERSONAJES---");

        // Guia: Mask Dude (inicio)
        CrearPersonajeNPC("Amigo_MaskDude", new Vector3(-3.0f, -2.15f, 0f), spMaskDude,
            "Mask Dude", "Recoge las 5 frutas para completar el nivel.", grpPersonajes.transform);

        // Compañero en la Meta: Pink Man (arriba)
        CrearPersonajeNPC("Amigo_PinkMan", new Vector3(16.5f, 4.85f, 0f), spPinkMan,
            "Pink Man", "Llegaste a la meta.", grpPersonajes.transform);

        // Conejito advertidor en el hielo
        CrearPersonajeNPC("Criatura_Conejo", new Vector3(4.5f, 1.4f, 0f), spConejo,
            "Conejito", "Cuidado con la plataforma de hielo, resbala.", grpPersonajes.transform);

        // Pollito consejo de dash
        CrearPersonajeNPC("Criatura_Pollito", new Vector3(-1.5f, -0.15f, 0f), spPollo,
            "Pollito", "Con Shift puedes hacer un dash en el aire.", grpPersonajes.transform);

        // 10. --- JUGADOR (Ninja Frog) ---
        GameObject grpJugador = new GameObject("---JUGADOR---");
        GameObject objJugador = new GameObject("Jugador");
        objJugador.transform.SetParent(grpJugador.transform);
        objJugador.transform.position = new Vector3(-5.5f, -2.1f, 0f);
        objJugador.tag = "Player";

        Rigidbody2D rbJ = objJugador.AddComponent<Rigidbody2D>();
        rbJ.bodyType = RigidbodyType2D.Dynamic;
        rbJ.freezeRotation = true;
        rbJ.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rbJ.gravityScale = 3.2f;

        CapsuleCollider2D colJ = objJugador.AddComponent<CapsuleCollider2D>();
        colJ.size = new Vector2(0.75f, 0.95f);

        // Visual del jugador
        GameObject objVisual = new GameObject("Visual");
        objVisual.transform.SetParent(objJugador.transform);
        objVisual.transform.localPosition = Vector3.zero;
        SpriteRenderer srJ = objVisual.AddComponent<SpriteRenderer>();
        srJ.sprite = spJugador;
        srJ.sortingOrder = 10;

        // Detector de suelo justo debajo del colisionador
        GameObject objDetector = new GameObject("DetectorSuelo");
        objDetector.transform.SetParent(objJugador.transform);
        objDetector.transform.localPosition = new Vector3(0f, -0.48f, 0f);

        // Script ControladorJugador
        ControladorJugador ctrlJ = objJugador.AddComponent<ControladorJugador>();
        SerializedObject soCtrl = new SerializedObject(ctrlJ);
        soCtrl.FindProperty("detectorSuelo").objectReferenceValue = objDetector.transform;
        soCtrl.FindProperty("capaSuelo").intValue = (1 << capaSuelo);
        soCtrl.FindProperty("radioSuelo").floatValue = 0.22f;
        soCtrl.FindProperty("modeloVisual").objectReferenceValue = objVisual.transform;
        soCtrl.FindProperty("spriteRenderer").objectReferenceValue = srJ;
        soCtrl.FindProperty("sonidoSalto").objectReferenceValue = sfxSalto;
        soCtrl.FindProperty("sonidoAterrizaje").objectReferenceValue = sfxAterrizaje;
        soCtrl.FindProperty("sonidoDash").objectReferenceValue = sfxDash;
        soCtrl.ApplyModifiedProperties();

        // Enlazar camara
        SerializedObject soCam = new SerializedObject(camCtrl);
        SerializedProperty propObj = soCam.FindProperty("objetivo");
        if (propObj != null) propObj.objectReferenceValue = objJugador.transform;
        SerializedProperty propDesfase = soCam.FindProperty("desfase");
        if (propDesfase != null) propDesfase.vector3Value = new Vector3(0f, 1.2f, -10f);
        SerializedProperty propSuav = soCam.FindProperty("suavizado");
        if (propSuav != null) propSuav.floatValue = 0.15f;
        soCam.ApplyModifiedProperties();

        // 11. Guardar Prefabs modulares en Assets/_Project/Prefabs para cumplir la rubrica
        string dirPrefabs = "Assets/_Project/Prefabs";
        if (!Directory.Exists(dirPrefabs)) Directory.CreateDirectory(dirPrefabs);

        PrefabUtility.SaveAsPrefabAsset(objJugador, dirPrefabs + "/Jugador.prefab");
        PrefabUtility.SaveAsPrefabAsset(objPlatMovil, dirPrefabs + "/PlataformaMovil.prefab");
        PrefabUtility.SaveAsPrefabAsset(objCaja, dirPrefabs + "/CajaEmpujable.prefab");

        GameObject primeraFruta = GameObject.Find("Fruta_Manzana");
        if (primeraFruta != null) PrefabUtility.SaveAsPrefabAsset(primeraFruta, dirPrefabs + "/FrutaColeccionable.prefab");

        GameObject primerNPC = GameObject.Find("Amigo_MaskDude");
        if (primerNPC != null) PrefabUtility.SaveAsPrefabAsset(primerNPC, dirPrefabs + "/PersonajeNPC.prefab");

        // Guardar escena
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("¡Nivel y Prefabs generados con exito!");
    }

    private static void SceneSetup()
    {
        string rutaEscena = "Assets/Scenes/SampleScene.unity";
        if (File.Exists(rutaEscena))
        {
            EditorSceneManager.OpenScene(rutaEscena);
        }

        string[] grupos = new string[] {
            "---CONTROLADORES---", "---FONDO---", "---ENTORNO---",
            "---INTERACTIVOS---", "---COLECCIONABLES---", "---PERSONAJES---", "---JUGADOR---"
        };
        foreach (string nombre in grupos)
        {
            GameObject obj = GameObject.Find(nombre);
            if (obj != null) Object.DestroyImmediate(obj);
        }
    }

    public static void ConstruirEscenaPorLineaDeComandos()
    {
        GenerarNivel();
        EditorApplication.Exit(0);
    }

    public static void CompilarJuegoLinux()
    {
        GenerarNivel();
        string[] escenas = new string[] { "Assets/Scenes/SampleScene.unity" };
        string carpetaBuild = "Builds/Linux";
        if (!Directory.Exists(carpetaBuild)) Directory.CreateDirectory(carpetaBuild);
        string rutaBuild = carpetaBuild + "/DigitalToy_Linux.x86_64";
        BuildPipeline.BuildPlayer(escenas, rutaBuild, BuildTarget.StandaloneLinux64, BuildOptions.None);
        EditorApplication.Exit(0);
    }

    public static void CompilarJuegoWindows()
    {
        GenerarNivel();
        string[] escenas = new string[] { "Assets/Scenes/SampleScene.unity" };
        string carpetaBuild = "Builds/Windows";
        if (!Directory.Exists(carpetaBuild)) Directory.CreateDirectory(carpetaBuild);
        string rutaBuild = carpetaBuild + "/DigitalToy_Windows.exe";
        BuildPipeline.BuildPlayer(escenas, rutaBuild, BuildTarget.StandaloneWindows64, BuildOptions.None);
        EditorApplication.Exit(0);
    }

    private static Sprite CargarSprite(string ruta)
    {
        Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
        if (sp != null) return sp;

        // Si es Multiple, intentar obtener el primer sub-sprite
        Object[] subObjetos = AssetDatabase.LoadAllAssetsAtPath(ruta);
        if (subObjetos != null)
        {
            foreach (Object o in subObjetos)
            {
                if (o is Sprite s) return s;
            }
        }
        return null;
    }

    private static GameObject CrearPlataforma(string nombre, Vector2 pos, Vector2 tamano, Sprite sprite, PhysicsMaterial2D mat, int capa, Color color, Transform padre)
    {
        GameObject obj = new GameObject(nombre);
        obj.transform.SetParent(padre);
        obj.transform.position = new Vector3(pos.x, pos.y, 0f);
        obj.layer = capa;

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        if (sprite != null)
        {
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = tamano;
        }
        sr.sortingOrder = 1;
        sr.color = color;

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.size = tamano;
        if (mat != null) col.sharedMaterial = mat;

        return obj;
    }

    private static GameObject CrearFruta(string nombre, Vector3 pos, Sprite sprite, AudioClip audio, Transform padre)
    {
        GameObject obj = new GameObject(nombre);
        obj.transform.SetParent(padre);
        obj.transform.position = pos;

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 5;

        CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
        col.radius = 0.35f;
        col.isTrigger = true;

        FrutaColeccionable comp = obj.AddComponent<FrutaColeccionable>();
        SerializedObject so = new SerializedObject(comp);
        so.FindProperty("sonidoColeccionable").objectReferenceValue = audio;
        so.ApplyModifiedProperties();

        return obj;
    }

    private static GameObject CrearPersonajeNPC(string nombreObj, Vector3 pos, Sprite sprite, string nombre, string saludo, Transform padre)
    {
        GameObject obj = new GameObject(nombreObj);
        obj.transform.SetParent(padre);
        obj.transform.position = pos;

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 8;

        PersonajeSecundario npc = obj.AddComponent<PersonajeSecundario>();
        SerializedObject so = new SerializedObject(npc);
        so.FindProperty("nombrePersonaje").stringValue = nombre;
        so.FindProperty("mensajeSaludo").stringValue = saludo;
        so.ApplyModifiedProperties();

        return obj;
    }
}
#endif
