// VidaJefe.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SnakeController))]
public class VidaJefe : MonoBehaviour
{
    public enum TipoEnemigo { Ametralladora, Pistola, Escopeta }

    [Header("Configuración de vida")]
    public float vida = 500f;
    public Slider sliderVida;

    [Header("Umbrales de vida (%)")]
    public float umbralPorcentaje80 = 80f;
    public float umbralPorcentaje60 = 60f;
    public float umbralPorcentaje40 = 40f;

    [Header("Daño por bala")]
    public float danioAlto = 20f;
    public float danioBajo = 5f;

    [Header("Parpadeo al recibir daño")]
    public int blinkCount = 4;
    public float blinkInterval = 0.1f;

    [Header("Cambio de tipo")]
    public float intervaloCambio = 10f;

    [Header("Columnas de wrap")]
    public Transform columna1;
    public Transform columna2;
    public Transform columna3;

    [Header("Spawn en columna")]
    public float tiempoEnColumna = 5f;
    public int totalEnemigos = 3;
    public GameObject[] enemyPrefabs;

    [Header("Fragmentos al morir")]
    [SerializeField] private int fragments = 200;

    // Estado interno
    private float vidaMaxima;
    private bool isDead = false;
    private float tiempoAcumulado = 0f;
    private bool fue80 = false, fue60 = false, fue40 = false;
    private bool isBlinking = false;
    private int currentPhase = 0;
    private Coroutine faseRutina;

    // Referencias
    private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
    private SnakeController snakeController;
    [SerializeField] private CobraPoseController cobraPose;
    [SerializeField] private ColumnWrapController columnWrap;

    private IEnumerator Start()
    {
        vidaMaxima = vida;
        snakeController = GetComponent<SnakeController>();
        yield return null; // espera inicialización de segmentos

        foreach (Transform seg in snakeController.Segmentos)
        {
            var rend = seg.GetComponent<MeshRenderer>();
            if (rend != null) meshRenderers.Add(rend);
        }

        cobraPose = GetComponent<CobraPoseController>();
        columnWrap = GetComponent<ColumnWrapController>();
        cobraPose.enabled = true;
        columnWrap.enabled = false;

        ActualizarTipoYColor();
        InicializarSlider();
    }

    private void Update()
    {
        if (isDead) return;

        float pct = vida / vidaMaxima * 100f;
        Log($"[VidaJefe] Vida%={pct:F1} | fue80={fue80} | fue60={fue60} | fue40={fue40}");

        // Cambio de tipo periódico
        tiempoAcumulado += Time.deltaTime;
        if (tiempoAcumulado >= intervaloCambio)
        {
            tiempoAcumulado = 0f;
            ActualizarTipoYColor();
        }

        // Umbrales de vida
        if (!fue80 && pct <= umbralPorcentaje80)
        {
            fue80 = true;
            IniciarFaseColumna(1, columna1);
        }
        else if (!fue60 && pct <= umbralPorcentaje60)
        {
            fue60 = true;
            IniciarFaseColumna(2, columna2);
        }
        else if (!fue40 && pct <= umbralPorcentaje40)
        {
            fue40 = true;
            IniciarFaseColumna(3, columna3);
        }
    }

    private void IniciarFaseColumna(int faseId, Transform targetCol)
    {
        if (columnWrap == null)
        {
            Log("ColumnWrapController no está asignado en VidaJefe.");
            return;
        }
        if (targetCol == null)
        {
            Log($"Columna para fase {faseId} no está asignada.");
            return;
        }

        if (faseRutina != null) StopCoroutine(faseRutina);

        currentPhase = faseId;
        if (cobraPose != null) cobraPose.enabled = false;

        columnWrap.columna = targetCol;
        columnWrap.enabled = true;
        Log($"[VidaJefe] Iniciando fase {faseId} en columna {targetCol.name}");

        faseRutina = StartCoroutine(EsperarWrapYSpawn(faseId));
    }

    private IEnumerator EsperarWrapYSpawn(int faseId)
    {
        while (columnWrap.enabled)
            yield return null;

        while (currentPhase == faseId && !isDead)
        {
            float t = 0f;
            while (t < tiempoEnColumna)
            {
                t += Time.deltaTime;
                yield return null;
            }

            for (int i = 0; i < totalEnemigos; i++)
            {
                if (enemyPrefabs == null || enemyPrefabs.Length == 0) break;

                int idx = Random.Range(0, enemyPrefabs.Length);
                var prefab = enemyPrefabs[idx];
                Vector3 dir = Random.onUnitSphere; dir.y = 0; dir.Normalize();
                Vector3 pos = columnWrap.columna.position + dir * 2f + Vector3.up * 0.5f;
                Instantiate(prefab, pos, Quaternion.identity);
            }
        }
    }

    private void ActualizarTipoYColor()
    {
        var tipo = (TipoEnemigo)Random.Range(0, System.Enum.GetValues(typeof(TipoEnemigo)).Length);
        Color c = tipo == TipoEnemigo.Ametralladora ? Color.blue
                 : tipo == TipoEnemigo.Pistola ? Color.red
                 : Color.green;
        meshRenderers.ForEach(r => r.material.color = c);
    }

    private void InicializarSlider()
    {
        if (sliderVida != null)
        {
            sliderVida.maxValue = vida;
            sliderVida.value = vida;
        }
    }

    public void RecibirDanio(float d, bool skipBlink = false)
    {
        if (isDead) return;
        if (!skipBlink && !isBlinking) StartCoroutine(ParpadeoMejorado());

        vida -= d;
        if (sliderVida != null) sliderVida.value = vida;
        if (vida <= 0f) { vida = 0f; Morir(); }
    }

    public void RecibirDanioPorBala(BalaPlayer.TipoBala tb)
    {
        RecibirDanio(tb.ToString() == meshRenderers[0].material.color.ToString()
                     ? danioBajo : danioAlto);
    }

    private IEnumerator ParpadeoMejorado()
    {
        isBlinking = true;
        Color baseC = meshRenderers.Count > 0 ? meshRenderers[0].material.color : Color.white;
        float half = blinkInterval * 0.5f;

        for (int i = 0; i < blinkCount; i++)
        {
            meshRenderers.ForEach(r => r.material.color = Color.white);
            yield return new WaitForSeconds(half);
            meshRenderers.ForEach(r => r.material.color = baseC);
            yield return new WaitForSeconds(half);
        }
        isBlinking = false;
    }

    private void Morir()
    {
        isDead = true;

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.AddInfoFragment(fragments); // Actualiza los fragments de informacion
        }
        else
        {
            Log("[VidaEnemigo] No se ha asignado el HUDManager en el Inspector.");
        }

        Destroy(gameObject);
    }
#if UNITY_EDITOR
    private void Log(string message)
    {
        Debug.Log(message);
    }
#endif
}



//// VidaJefe.cs
//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;
//using System.Collections.Generic;

//[RequireComponent(typeof(SnakeController))]
//public class VidaJefe : MonoBehaviour
//{
//    public enum TipoEnemigo { Ametralladora, Pistola, Escopeta }

//    [Header("Configuración de vida")]
//    public float vida = 500f;
//    public Slider sliderVida;

//    [Header("Umbrales de vida (%)")]
//    public float umbralPorcentaje80 = 80f;
//    public float umbralPorcentaje60 = 60f;
//    public float umbralPorcentaje40 = 40f;

//    [Header("Daño por bala")]
//    public float danioAlto = 20f;
//    public float danioBajo = 5f;

//    [Header("Parpadeo al recibir daño")]
//    public int blinkCount = 4;
//    public float blinkInterval = 0.1f;

//    [Header("Cambio de tipo")]
//    public float intervaloCambio = 10f;

//    [Header("Columnas de wrap")]
//    public Transform columna1;
//    public Transform columna2;
//    public Transform columna3;

//    [Header("Spawn en columna")]
//    public float tiempoEnColumna = 5f;
//    public int totalEnemigos = 3;
//    public GameObject[] enemyPrefabs;

//    [Header("Fragmentos al morir")]
//    [SerializeField] private int fragments = 200;

//    // Estado interno
//    private float vidaMaxima;
//    private bool isDead = false;
//    private float tiempoAcumulado = 0f;
//    private bool fue80 = false, fue60 = false, fue40 = false;
//    private bool isBlinking = false;
//    private int currentPhase = 0;
//    private Coroutine faseRutina;

//    // Referencias
//    private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
//    private SnakeController snakeController;
//    [SerializeField] private CobraPoseController cobraPose;
//    [SerializeField] private ColumnWrapController columnWrap;

//    private IEnumerator Start()
//    {
//        vidaMaxima = vida;
//        snakeController = GetComponent<SnakeController>();
//        yield return null; // espera inicialización de segmentos

//        foreach (Transform seg in snakeController.Segmentos)
//        {
//            var rend = seg.GetComponent<MeshRenderer>();
//            if (rend != null) meshRenderers.Add(rend);
//        }

//        cobraPose = GetComponent<CobraPoseController>();
//        columnWrap = GetComponent<ColumnWrapController>();
//        cobraPose.enabled = true;
//        columnWrap.enabled = false;

//        ActualizarTipoYColor();
//        InicializarSlider();
//    }

//    private void Update()
//    {
//        if (isDead) return;

//        float pct = vida / vidaMaxima * 100f;
//        Debug.Log($"[VidaJefe] Vida%={pct:F1} | fue80={fue80} | fue60={fue60} | fue40={fue40}");

//        // Cambio de tipo periódico
//        tiempoAcumulado += Time.deltaTime;
//        if (tiempoAcumulado >= intervaloCambio)
//        {
//            tiempoAcumulado = 0f;
//            ActualizarTipoYColor();
//        }

//        // Umbrales de vida
//        if (!fue80 && pct <= umbralPorcentaje80)
//        {
//            fue80 = true;
//            IniciarFaseColumna(1, columna1);
//        }
//        else if (!fue60 && pct <= umbralPorcentaje60)
//        {
//            fue60 = true;
//            IniciarFaseColumna(2, columna2);
//        }
//        else if (!fue40 && pct <= umbralPorcentaje40)
//        {
//            fue40 = true;
//            IniciarFaseColumna(3, columna3);
//        }
//    }

//    private void IniciarFaseColumna(int faseId, Transform targetCol)
//    {
//        if (columnWrap == null)
//        {
//            Debug.LogError("ColumnWrapController no está asignado en VidaJefe.");
//            return;
//        }
//        if (targetCol == null)
//        {
//            Debug.LogError($"Columna para fase {faseId} no está asignada.");
//            return;
//        }

//        // Detener fase previa
//        if (faseRutina != null) StopCoroutine(faseRutina);

//        currentPhase = faseId;
//        if (cobraPose != null) cobraPose.enabled = false;

//        columnWrap.columna = targetCol;
//        columnWrap.enabled = true;
//        Debug.Log($"[VidaJefe] Iniciando fase {faseId} en columna {targetCol.name}");

//        faseRutina = StartCoroutine(EsperarWrapYSpawn(faseId));
//    }

//    private IEnumerator EsperarWrapYSpawn(int faseId)
//    {
//        // Espera a que termine el wrap
//        while (columnWrap.enabled)
//            yield return null;

//        // Spawneo periódico
//        while (currentPhase == faseId && !isDead)
//        {
//            float t = 0f;
//            while (t < tiempoEnColumna)
//            {
//                t += Time.deltaTime;
//                yield return null;
//            }

//            for (int i = 0; i < totalEnemigos; i++)
//            {
//                if (enemyPrefabs == null || enemyPrefabs.Length == 0) break;

//                int idx = Random.Range(0, enemyPrefabs.Length);
//                var prefab = enemyPrefabs[idx];
//                Vector3 dir = Random.onUnitSphere; dir.y = 0; dir.Normalize();
//                Vector3 pos = columnWrap.columna.position + dir * 2f + Vector3.up * 0.5f;
//                Instantiate(prefab, pos, Quaternion.identity);
//            }
//        }
//    }

//    private void ActualizarTipoYColor()
//    {
//        var tipo = (TipoEnemigo)Random.Range(0, System.Enum.GetValues(typeof(TipoEnemigo)).Length);
//        Color c = tipo == TipoEnemigo.Ametralladora ? Color.blue
//                 : tipo == TipoEnemigo.Pistola ? Color.red
//                 : Color.green;
//        meshRenderers.ForEach(r => r.material.color = c);
//    }

//    private void InicializarSlider()
//    {
//        if (sliderVida != null)
//        {
//            sliderVida.maxValue = vida;
//            sliderVida.value = vida;
//        }
//    }

//    public void RecibirDanio(float d, bool skipBlink = false)
//    {
//        if (isDead) return;
//        if (!skipBlink && !isBlinking) StartCoroutine(ParpadeoMejorado());

//        vida -= d;
//        if (sliderVida != null) sliderVida.value = vida;
//        if (vida <= 0f) { vida = 0f; Morir(); }
//    }

//    public void RecibirDanioPorBala(BalaPlayer.TipoBala tb)
//    {
//        RecibirDanio(tb.ToString() == meshRenderers[0].material.color.ToString()
//                     ? danioBajo : danioAlto);
//    }

//    private IEnumerator ParpadeoMejorado()
//    {
//        isBlinking = true;
//        Color baseC = meshRenderers.Count > 0 ? meshRenderers[0].material.color : Color.white;
//        float half = blinkInterval * 0.5f;

//        for (int i = 0; i < blinkCount; i++)
//        {
//            meshRenderers.ForEach(r => r.material.color = Color.white);
//            yield return new WaitForSeconds(half);
//            meshRenderers.ForEach(r => r.material.color = baseC);
//            yield return new WaitForSeconds(half);
//        }
//        isBlinking = false;
//    }

//    private void Morir()
//    {
//        isDead = true;
//        HUDManager.Instance.AddInfoFragment(fragments);
//        FindObjectOfType<MissionManager>().RegisterKill(gameObject.tag);
//        Destroy(gameObject);
//    }
//}

//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;
//using System.Collections.Generic;

//[RequireComponent(typeof(SnakeController))]
//public class VidaJefe : MonoBehaviour
//{
//    public enum TipoEnemigo { Ametralladora = 0, Pistola = 1, Escopeta = 2 }

//    [Header("Configuración de vida")]
//    public float vida = 500f;
//    public Slider sliderVida;

//    [Header("Umbrales de vida (%)")]
//    [Tooltip("Porcentaje de vida para ir a columna 1")]
//    public float umbralPorcentaje80 = 80f;
//    [Tooltip("Porcentaje de vida para ir a columna 2")]
//    public float umbralPorcentaje60 = 60f;
//    [Tooltip("Porcentaje de vida para ir a columna 3")]
//    public float umbralPorcentaje40 = 40f;

//    [Header("Daño por bala")]
//    public float danioAlto = 20f;
//    public float danioBajo = 5f;

//    [Header("Parpadeo al recibir daño")]
//    public int blinkCount = 4;
//    public float blinkInterval = 0.1f;

//    [Header("Cambio de tipo")]
//    public float intervaloCambio = 10f;

//    [Header("Columnas de wrap")]
//    [Tooltip("Columna asignada para el 80%")]
//    public Transform columna1;
//    [Tooltip("Columna asignada para el 60%")]
//    public Transform columna2;
//    [Tooltip("Columna asignada para el 40%")]
//    public Transform columna3;

//    [Header("Spawn en columna")]
//    [Tooltip("Tiempo (seg) que el jefe permanece en la columna antes de generar enemigos")]
//    public float tiempoEnColumna = 5f;
//    [Tooltip("Cantidad total de enemigos a generar cada vez que expire el contador")]
//    public int totalEnemigos = 3;
//    [Tooltip("Prefabs de enemigos a instanciar")]
//    public GameObject[] enemyPrefabs;

//    [Header("Fragmentos al morir")]
//    [SerializeField] private int fragments = 200;

//    // Estado interno
//    private float vidaMaxima;
//    private bool isDead = false;
//    private float tiempoAcumulado = 0f;
//    private bool fue80 = false, fue60 = false, fue40 = false;
//    private bool isBlinking = false;
//    private int currentPhase = 0;  // 1,2 o 3 según la columna en la que esté

//    // Referencias
//    private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
//    private SnakeController snakeController;
//    [SerializeField] private CobraPoseController cobraPose;
//    [SerializeField] private ColumnWrapController columnWrap;

//    private IEnumerator Start()
//    {
//        vidaMaxima = vida;
//        snakeController = GetComponent<SnakeController>();
//        yield return null; // espera inicialización de segmentos

//        foreach (Transform seg in snakeController.Segmentos)
//        {
//            var rend = seg.GetComponent<MeshRenderer>();
//            if (rend != null) meshRenderers.Add(rend);
//        }

//        cobraPose = GetComponent<CobraPoseController>();
//        columnWrap = GetComponent<ColumnWrapController>();
//        cobraPose.enabled = true;
//        columnWrap.enabled = false;

//        ActualizarTipoYColor();
//        InicializarSlider();
//    }

//    private void Update()
//    {
//        if (isDead) return;

//        // Cambio de tipo periódico
//        tiempoAcumulado += Time.deltaTime;
//        if (tiempoAcumulado >= intervaloCambio)
//        {
//            tiempoAcumulado = 0f;
//            ActualizarTipoYColor();
//        }

//        // Comprobar umbrales de vida y arrancar fases
//        float pct = vida / vidaMaxima * 100f;

//        if (!fue80 && pct <= umbralPorcentaje80)
//        {
//            fue80 = true;
//            IniciarFaseColumna(1, columna1);
//        }
//        else if (!fue60 && pct <= umbralPorcentaje60)
//        {
//            fue60 = true;
//            IniciarFaseColumna(2, columna2);
//        }
//        else if (!fue40 && pct <= umbralPorcentaje40)
//        {
//            fue40 = true;
//            IniciarFaseColumna(3, columna3);
//        }
//    }

//    private void IniciarFaseColumna(int faseId, Transform targetCol)
//    {
//        // Marcar fase y desactivar pose
//        currentPhase = faseId;
//        if (cobraPose != null) cobraPose.enabled = false;

//        // Configurar y activar wrap
//        columnWrap.columna = targetCol;
//        columnWrap.enabled = true;

//        // Arrancar la rutina de spawn continuo para esta fase
//        StartCoroutine(EsperarWrapYSpawn(faseId));
//    }

//    private IEnumerator EsperarWrapYSpawn(int faseId)
//    {
//        // 1) Espera a que termine el wrap (ColumnWrapController se autodesactiva)
//        while (columnWrap.enabled)
//            yield return null;

//        // 2) Mientras siga en esta fase, esperar y spawnear repetidamente
//        while (currentPhase == faseId && !isDead)
//        {
//            // Esperar el tiempo en columna
//            float t = 0f;
//            while (t < tiempoEnColumna)
//            {
//                t += Time.deltaTime;
//                yield return null;
//            }

//            // Generar enemigos al azar de entre todo el array
//            for (int i = 0; i < totalEnemigos; i++)
//            {
//                if (enemyPrefabs == null || enemyPrefabs.Length == 0) break;

//                // Elige un prefab al azar
//                int idx = Random.Range(0, enemyPrefabs.Length);
//                GameObject prefab = enemyPrefabs[idx];

//                // Dirección aleatoria en XZ
//                Vector3 randomDir = Random.onUnitSphere;
//                randomDir.y = 0f;
//                randomDir.Normalize();

//                // Posición a 2 unidades del centro y 0.5 arriba
//                Vector3 pos = columnWrap.columna.position
//                            + randomDir * 2f
//                            + Vector3.up * 0.5f;

//                Instantiate(prefab, pos, Quaternion.identity);
//            }
//        }
//    }

//    private void ActualizarTipoYColor()
//    {
//        var tipo = (TipoEnemigo)Random.Range(0, System.Enum.GetValues(typeof(TipoEnemigo)).Length);
//        Color c = tipo == TipoEnemigo.Ametralladora ? Color.blue
//                 : tipo == TipoEnemigo.Pistola ? Color.red
//                 : Color.green;
//        meshRenderers.ForEach(r => r.material.color = c);
//    }

//    private void InicializarSlider()
//    {
//        if (sliderVida != null)
//        {
//            sliderVida.maxValue = vida;
//            sliderVida.value = vida;
//        }
//    }

//    public void RecibirDanio(float d, bool skipBlink = false)
//    {
//        if (isDead) return;
//        if (!skipBlink && !isBlinking) StartCoroutine(ParpadeoMejorado());

//        vida -= d;
//        if (sliderVida != null) sliderVida.value = vida;
//        if (vida <= 0f)
//        {
//            vida = 0f;
//            Morir();
//        }
//    }

//    public void RecibirDanioPorBala(BalaPlayer.TipoBala tb)
//    {
//        // Aquí puedes implementar tu lógica de daño según tipo de bala
//        RecibirDanio(tb.ToString() == meshRenderers[0].material.color.ToString() ? danioBajo : danioAlto,
//                     skipBlink: false);
//    }

//    private IEnumerator ParpadeoMejorado()
//    {
//        isBlinking = true;
//        Color baseC = meshRenderers.Count > 0 ? meshRenderers[0].material.color : Color.white;
//        float half = blinkInterval * 0.5f;

//        for (int i = 0; i < blinkCount; i++)
//        {
//            meshRenderers.ForEach(r => r.material.color = Color.white);
//            yield return new WaitForSeconds(half);
//            meshRenderers.ForEach(r => r.material.color = baseC);
//            yield return new WaitForSeconds(half);
//        }

//        isBlinking = false;
//    }

//    private void Morir()
//    {
//        isDead = true;
//        HUDManager.Instance.AddInfoFragment(fragments);
//        FindObjectOfType<MissionManager>().RegisterKill(gameObject.tag);
//        Destroy(gameObject);
//    }
//}
