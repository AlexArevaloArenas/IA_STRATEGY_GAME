using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using UnityEditor.Experimental.GraphView;
using UnityEngine.SocialPlatforms;
using Unity.VisualScripting;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.TestTools;

public class PathRequestManager : MonoBehaviour {

	Queue<PathResult> results = new Queue<PathResult>();

	static PathRequestManager instance;
	Pathfinding pathfinding;

	void Awake() {
		instance = this;
		pathfinding = GetComponent<Pathfinding>();
	}

	void Update() {
		if (results.Count > 0) {
			int itemsInQueue = results.Count;
			lock (results) {
				for (int i = 0; i < itemsInQueue; i++) {
					PathResult result = results.Dequeue ();
					result.callback (result.path, result.success);
				}
			}
		}
	}

	public static void RequestPath(PathRequest request) {
		ThreadStart threadStart = delegate {
			instance.pathfinding.FindPath (request, instance.FinishedProcessingPath);
		};
		threadStart.Invoke ();
	}

	public void FinishedProcessingPath(PathResult result) {
		lock (results) {
			results.Enqueue (result);
        }
    }

    //Method to find all enemies
    private static Unit[] FindAllEnemies() {
    GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
    List<Unit> enemyUnits = new List<Unit>();

    foreach (GameObject enemyObject in enemyObjects) {
        Unit unit = enemyObject.GetComponent<Unit>();
        if (unit != null) {
            enemyUnits.Add(unit);
        }
    }

    return enemyUnits.ToArray();
}

    public static Unit[] FindEnemiesAvailable(Vector3 pos, float moveRange, float attackRange)
    {
		List<Unit> attacktableEnemies = new List<Unit>();
		Unit[] enemies = FindAllEnemies();

        bool[,] revisedNodes = new bool[instance.pathfinding.grid.gridSizeX, instance.pathfinding.grid.gridSizeY];
        for (int i = 0; i < instance.pathfinding.grid.gridSizeX; i++)
        {
            for (int j = 0; j < instance.pathfinding.grid.gridSizeY; j++)
            {
                revisedNodes[i, j] = false;
            }
        }

        instance.pathfinding.FindAvailableEnemies(instance.pathfinding.grid.NodeFromWorldPoint(pos), moveRange, attackRange, ref attacktableEnemies, revisedNodes, enemies);
		
		return attacktableEnemies.ToArray();
    }

	public static GridNode[] FindBestAttackPlaces(Vector3 pos, float moveRange, Unit targetUnit, List<Unit> enemies, UnitType unitType)
    {
        bool[,] revisedNodes = new bool[instance.pathfinding.grid.gridSizeX, instance.pathfinding.grid.gridSizeY];
        for (int i = 0; i < instance.pathfinding.grid.gridSizeX; i++)
        {
            for (int j = 0; j < instance.pathfinding.grid.gridSizeY; j++)
            {
                revisedNodes[i, j] = false;
            }
        }

        return instance.pathfinding.GetBestAttackPlaces(
            instance.pathfinding.grid.NodeFromWorldPoint(pos),
            moveRange,
            targetUnit,
            enemies,
            revisedNodes,
            unitType
        );
    }

    public static GridNode[] FindSafePlaces(Vector3 pos, float moveRange, List<Unit> enemies)
    {
        bool[,] revisedNodes = new bool[instance.pathfinding.grid.gridSizeX, instance.pathfinding.grid.gridSizeY];
        for (int i = 0; i < instance.pathfinding.grid.gridSizeX; i++)
        {
            for (int j = 0; j < instance.pathfinding.grid.gridSizeY; j++)
            {
                revisedNodes[i, j] = false;
            }
        }

        return instance.pathfinding.GetSafePlaces(
            instance.pathfinding.grid.NodeFromWorldPoint(pos),
            moveRange,
            enemies,
            revisedNodes
        );
    }

    public static bool _IsPlaceAvailable(Vector3 punto,float range)
    {
        bool[,] revisedNodes = new bool[instance.pathfinding.grid.gridSizeX, instance.pathfinding.grid.gridSizeY];
        for (int i = 0; i < instance.pathfinding.grid.gridSizeX; i++)
        {
            for (int j = 0; j < instance.pathfinding.grid.gridSizeY; j++)
            {
                revisedNodes[i, j] = false;
            }
        }
        return instance.pathfinding.IsPlaceAvailable(instance.pathfinding.grid.NodeFromWorldPoint(punto), range, punto, revisedNodes);
    }
}
public struct PathResult {
	public Vector3[] path;
	public bool success;
	public Action<Vector3[], bool> callback;

	public PathResult (Vector3[] path, bool success, Action<Vector3[], bool> callback)
	{
		this.path = path;
		this.success = success;
		this.callback = callback;
	}

}

public struct PathRequest {
	public Vector3 pathStart;
	public Vector3 pathEnd;
	public Action<Vector3[], bool> callback;

	public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback) {
		pathStart = _start;
		pathEnd = _end;
		callback = _callback;
	}

}
