using System;
using System.Collections.Generic;
//using Malevolent;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Sensor : MonoBehaviour
{
    [SerializeField] float detectionRadius = 5f;
    //[SerializeField] float timerInterval = 1f;

    SphereCollider detectionRange;

    public event Action OnTargetChanged = delegate { };

    public struct unitData {
        public string id;
        public Vector3 position;
        public UnitType type;
        public float maxHealth;
        public float currentHealth;
        public unitData(string id, Vector3 position, UnitType type, (float, float) health) {
            this.id = id;
            this.position = position;
            this.type = type;
            this.currentHealth = health.Item1;
            this.maxHealth = health.Item2;
        }
    };

    private List<GameObject> foundTargets = new List<GameObject>();
    public List<unitData> seenTargets = new List<unitData>();

    void Awake()
    {
        detectionRange = GetComponent<SphereCollider>();
        detectionRange.isTrigger = true;
        detectionRange.radius = detectionRadius;
    }

    /*void Start()
    {
        timer = new CountdownTimer(timerInterval);
        timer.OnTimerStop += () => {
            UpdateTargetPosition(target.OrNull());
            timer.Start();
        };
        timer.Start();
    }*/

    /*void Update()
    {
        timer.Tick(Time.deltaTime);
    }*/

    void UpdateTargetPositions()
    {
        seenTargets.Clear();

        foreach (var target in foundTargets)
        {
            Unit unit = target.GetComponent<Unit>();
            if (unit != null && unit.visible){
                seenTargets.Add(new unitData( target.GetInstanceID().ToString(),
                                                target.transform.position,
                                                unit.type,
                                                (unit.GetHealth(), unit.MaxHealth)));
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        foundTargets.Add(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        foundTargets.Remove(other.gameObject);
    }

    void OnDrawGizmos()
    {
        //Gizmos.color = IsTargetInRange ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}