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

    private List<GameObject> targets = new List<GameObject>();
    public List<Vector3> targetMagePositions = new List<Vector3>();
    public List<Vector3> targetKnightPositions = new List<Vector3>();
    public List<Vector3> targetArcherPositions = new List<Vector3>();
    public List<Vector3> targetPawnPositions = new List<Vector3>();

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
        targetMagePositions.Clear();
        targetKnightPositions.Clear();
        targetArcherPositions.Clear();
        targetPawnPositions.Clear();

        foreach (var target in targets)
        {
            Unit unit = target.GetComponent<Unit>();
            if (unit != null && unit.visible){
                switch (unit.type)
                {
                    case UnitType.Mage:
                        targetMagePositions.Add(target.transform.position);
                        break;
                    case UnitType.Knight:
                        targetKnightPositions.Add(target.transform.position);
                        break;
                    case UnitType.Archer:
                        targetArcherPositions.Add(target.transform.position);
                        break;
                    case UnitType.Pawn:
                        targetPawnPositions.Add(target.transform.position);
                        break;
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        targets.Add(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        targets.Remove(other.gameObject);
    }

    void OnDrawGizmos()
    {
        //Gizmos.color = IsTargetInRange ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}