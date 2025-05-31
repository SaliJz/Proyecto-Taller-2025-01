using UnityEngine;

/// <summary>
/// Genera enemigos aleatorios alrededor del segmento cabeza de la serpiente,
/// usando directamente la cabeza de SnakeController.
/// </summary>
public class SpawnEnemigosCercaCabeza : MonoBehaviour
{
    [Header("Referencia al SnakeController")]
    [Tooltip("Asigna aquí el componente SnakeController de tu serpiente")]
    public SnakeController snakeController;

    [Header("Configuración de spawn")]
    [Tooltip("Array de prefabs de enemigos posibles a instanciar.")]
    public GameObject[] prefabsEnemigos;

    [Tooltip("Cantidad de enemigos a generar en cada ola.")]
    public int enemigosPorOla = 3;

    [Tooltip("Tiempo (en segundos) entre cada ola de spawns.")]
    public float intervaloSpawn = 5f;

    [Tooltip("Radio (en unidades de mundo) alrededor de la cabeza donde pueden generarse los enemigos.")]
    public float radioSpawn = 3f;

    private float tiempoHastaProximoSpawn;

    void Start()
    {
        if (snakeController == null)
        {
            Debug.LogError("SpawnEnemigosCercaCabeza: No se ha asignado SnakeController.");
            enabled = false;
            return;
        }

        // Inicializar contador para el primer spawn
        tiempoHastaProximoSpawn = intervaloSpawn;
    }

    void Update()
    {
        // Asegurarse de que SnakeController ya tenga la cabeza instanciada
        if (snakeController.Segmentos == null || snakeController.Segmentos.Count == 0)
            return;

        // Reducir el contador según el tiempo transcurrido
        tiempoHastaProximoSpawn -= Time.deltaTime;
        if (tiempoHastaProximoSpawn <= 0f)
        {
            // Generar enemigos alrededor de la cabeza
            SpawnEnemigos();
            tiempoHastaProximoSpawn = intervaloSpawn;
        }
    }

    private void SpawnEnemigos()
    {
        // Obtener la cabeza directamente de SnakeController
        Transform cabeza = snakeController.Segmentos[0];
        if (cabeza == null)
            return;

        for (int i = 0; i < enemigosPorOla; i++)
        {
            if (prefabsEnemigos == null || prefabsEnemigos.Length == 0)
            {
                Debug.LogWarning("SpawnEnemigosCercaCabeza: No hay prefabs de enemigos asignados.");
                return;
            }

            // Elegir aleatoriamente uno de los prefabs disponibles
            int indicePrefab = Random.Range(0, prefabsEnemigos.Length);
            GameObject prefabElegido = prefabsEnemigos[indicePrefab];

            // Calcular posición aleatoria en un círculo de radio "radioSpawn" sobre el plano XZ,
            // manteniendo la Y de la cabeza
            Vector2 puntoAleatorio2D = Random.insideUnitCircle * radioSpawn;
            Vector3 posicionSpawn = new Vector3(
                cabeza.position.x + puntoAleatorio2D.x,
                cabeza.position.y,
                cabeza.position.z + puntoAleatorio2D.y
            );

            // Instanciar el enemigo en la posición calculada
            Instantiate(prefabElegido, posicionSpawn, Quaternion.identity);
        }
    }
}
