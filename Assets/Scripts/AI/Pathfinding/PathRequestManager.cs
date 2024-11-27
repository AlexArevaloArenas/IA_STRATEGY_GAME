﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using UnityEditor.Experimental.GraphView;
using UnityEngine.SocialPlatforms;
using Unity.VisualScripting;

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
    public static Unit[] FindEnemiesAvailable(Vector3 pos, float moveRange, float attackRange)
    {
		List<Unit> attacktableEnemies = new List<Unit>();
		Unit[] enemies = new Unit[1];

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
