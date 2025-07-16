//using System.Collections;
//using UnityEngine;

//public class GameSequenceManager : MonoBehaviour
//{
//    [Header("Habilitar Secuencia")]
//    [Tooltip("Si es true, la secuencia se ejecutar� al arrancar.")]
//    public bool sequenceEnabled = true;

//    [Header("Referencias a Controladores")]
//    [Tooltip("Arrastra aqu� el componente AnimacionMuerteCobraEpic")]
//    public AnimacionMuerteCobraEpic epicController;
//    [Tooltip("Arrastra aqu� el componente SnakeDissolveController")]
//    public SnakeDissolveController dissolveController;
//    [Tooltip("Arrastra aqu� el componente GeneradorUnicoDeEnemigosAleatoriosYUbicaciones")]
//    public GeneradorUnicoDeEnemigosAleatoriosYUbicaciones generatorController;
//    [Tooltip("Arrastra aqu� el componente UltraSmoothBlackScreenSceneTransitionFadeController")]
//    public UltraSmoothBlackScreenSceneTransitionFadeController fadeController;

//    [Header("Tiempos de Secuencia (segundos)")]
//    [Tooltip("Tiempo que transcurre tras activar la pose �pica antes de disolver/generadores")]
//    public float delayAfterEpic = 2f;
//    [Tooltip("Tiempo que transcurre tras disolver/generadores antes del fundido")]
//    public float delayAfterDissolve = 1.5f;

//    private void Start()
//    {
//        if (sequenceEnabled)
//            StartCoroutine(RunGameSequence());
//        else
//            Debug.Log("GameSequenceManager: la secuencia est� deshabilitada (sequenceEnabled = false).");
//    }

//    private IEnumerator RunGameSequence()
//    {
//        // Asegura que todos los controladores est�n habilitados
//        if (epicController != null) epicController.enabled = true;
//        if (dissolveController != null) dissolveController.enabled = true;
//        if (generatorController != null) generatorController.enabled = true;
//        if (fadeController != null) fadeController.enabled = true;

//        // 1) Activar animaci�n �pica de la cobra
//        if (epicController != null)
//            epicController.activarEpic = true;
//        else
//            Debug.LogWarning("GameSequenceManager: falta asignar epicController.");

//        yield return new WaitForSeconds(delayAfterEpic);

//        // 2) Activar disoluci�n y detener spawn de enemigos
//        if (dissolveController != null)
//            dissolveController.dissolveActive = true;
//        else
//            Debug.LogWarning("GameSequenceManager: falta asignar dissolveController.");

//        if (generatorController != null)
//            generatorController.stopSpawning = true;
//        else
//            Debug.LogWarning("GameSequenceManager: falta asignar generatorController.");

//        yield return new WaitForSeconds(delayAfterDissolve);

//        // 3) Activar fundido a negro y cambio de escena
//        if (fadeController != null)
//            fadeController.triggerSceneTransitionFade = true;
//        else
//            Debug.LogWarning("GameSequenceManager: falta asignar fadeController.");
//    }
//}
