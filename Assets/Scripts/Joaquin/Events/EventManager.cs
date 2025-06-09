using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    [System.Serializable]
    public class EventData
    {
        [SerializeField] private GameObject eventRoot;
        [SerializeField] private Sprite eventIcon;
        [SerializeField] private string eventName;

        public GameObject EventRoot => eventRoot;
        public Sprite EventIcon => eventIcon;
        public string EventName => eventName;
    }

    [SerializeField] private EventData[] events;
    [SerializeField] private float timeBetweenEvents = 60f;

    private float eventTimer;
    private int currentEventIndex = -1;

    void Start()
    {
        eventTimer = timeBetweenEvents;
    }

    void OnEnable()
    {
        if (events == null || events.Length == 0)
        {
            Debug.LogWarning("[EventManager] No eventos asignados.");
            return;
        }
        ActivateRandomEvent();
    }

    void Update()
    {
        if (timeBetweenEvents == 0f)
        {
            ActivateRandomEvent();
            return;
        }

        eventTimer -= Time.deltaTime;

        if (eventTimer <= 0)
        {
            ActivateRandomEvent();
            eventTimer = timeBetweenEvents;
        }
    }

    void ActivateRandomEvent()
    {
        if (currentEventIndex != -1)
        {
            DeactivateEvent(currentEventIndex);
        }

        currentEventIndex = Random.Range(0, events.Length);
        EventData selected = events[currentEventIndex];

        selected.EventRoot.SetActive(true);

        if (HUDManager.Instance != null)
        {
            if (selected.EventIcon == null)
            {
                Debug.LogError("El ícono del evento es null.");
            }
            else
            {
                Debug.Log("Evento con ícono: " + selected.EventIcon.name);
                HUDManager.Instance.UpdateIcon(selected.EventIcon);
            }
        }

        Debug.Log("Activated event: " + selected.EventName);
    }

    void DeactivateEvent(int index)
    {
        EventData oldEvent = events[index];

        oldEvent.EventRoot.SetActive(false);

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.HideIcon();
        }

        Debug.Log("Deactivated event: " + oldEvent.EventName);
    }
}