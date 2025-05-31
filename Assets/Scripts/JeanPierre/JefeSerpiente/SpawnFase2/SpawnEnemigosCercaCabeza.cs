using UnityEngine;

/// <summary>
/// Genera enemigos aleatorios alrededor de un Transform "cabeza",
/// cada cierto intervalo y en cantidad definida, usando un arreglo de prefabs.
/// </summary>
public class SpawnEnemigosCercaCabeza : MonoBehaviour
{
    [Header("Referencia al segmento cabeza de la serpiente")]
    [Tooltip("Asigna aqu� el Transform del segmento cabeza de tu serpiente (por ejemplo, el GameObject que usa el prefabCabeza).")]
    public Transform cabeza;

    [Header("Configuraci�n de spawn")]
    [Tooltip("Array de prefabs de enemigos posibles a instanciar.")]
    public GameObject[] prefabsEnemigos;

    [Tooltip("Cantidad de enemigos a generar en cada ola.")]
    public int enemigosPorOla = 3;

    [Tooltip("Tiempo (en segundos) entre cada ola de spawns.")]
    public float intervaloSpawn = 5f;

    [Tooltip("Radio (en unidades de mundo) alrededor de la cabeza donde pueden generarse los enemigos.")]
    public float radioSpawn = 3f;

    // Contador interno para controlar cu�ndo tocar� spawnear la siguiente ola
    private float tiempoHastaProximoSpawn;

    void Start()
    {
        // Inicializamos el contador para que en X segundos (intervaloSpawn) se genere la primera ola.
        tiempoHastaProximoSpawn = intervaloSpawn;
    }

    void Update()
    {
        // Si no hay referencia al Transform de la cabeza, salimos para no lanzar errores.
        if (cabeza == null)
            return;

        // Reducimos el contador seg�n el tiempo transcurrido
        tiempoHastaProximoSpawn -= Time.deltaTime;
        if (tiempoHastaProximoSpawn <= 0f)
        {
            // Cuando llega a 0 (o menos), generamos la ola de enemigos y reiniciamos el contador
            SpawnEnemigos();
            tiempoHastaProximoSpawn = intervaloSpawn;
        }
    }

    private void SpawnEnemigos()
    {
        // Por cada enemigo que queramos en la ola...
        for (int i = 0; i < enemigosPorOla; i++)
        {
            if (prefabsEnemigos == null || prefabsEnemigos.Length == 0)
            {
                Debug.LogWarning("SpawnEnemigosCercaCabeza: No hay prefabs de enemigos asignados.");
                return;
            }

            // Elegimos aleatoriamente uno de los prefabs disponibles
            int indicePrefab = Random.Range(0, prefabsEnemigos.Length);
            GameObject prefabElegido = prefabsEnemigos[indicePrefab];

            // Calculamos una posici�n aleatoria en un c�rculo de radio "radioSpawn" sobre el plano XZ,
            // manteniendo la misma Y de la cabeza.
            Vector2 puntoAleatorio2D = Random.insideUnitCircle * radioSpawn;
            Vector3 posicionSpawn = new Vector3(
                cabeza.position.x + puntoAleatorio2D.x,
                cabeza.position.y,
                cabeza.position.z + puntoAleatorio2D.y
            );

            // Instanciamos el enemigo en la posici�n calculada, sin rotaci�n especial (Quaternion.identity)
            Instantiate(prefabElegido, posicionSpawn, Quaternion.identity);
        }
    }
}
