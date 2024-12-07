using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEditor.Experimental.GraphView;
using static UnityEditor.PlayerSettings;
using Unity.VisualScripting;

public class Pathfinding : MonoBehaviour {

	public Grid grid;
	public LayerMask enemyUnits;
	public LayerMask playerUnits;

	public GameObject debugBola;

	
	void Awake() {
		grid = GetComponent<Grid>();
	}
	

	public void FindPath(PathRequest request, Action<PathResult> callback) {
		
		Stopwatch sw = new Stopwatch();
		sw.Start();
		
		Vector3[] waypoints = new Vector3[0];
		bool pathSuccess = false;

        GridNode startNode = grid.NodeFromWorldPoint(request.pathStart);
        GridNode targetNode = grid.NodeFromWorldPoint(request.pathEnd);
		startNode.parent = startNode;
		
		
		if (startNode.walkable && targetNode.walkable) {
			Heap<GridNode> openSet = new Heap<GridNode>(grid.MaxSize);
			HashSet<GridNode> closedSet = new HashSet<GridNode>();
			openSet.Add(startNode);
			
			while (openSet.Count > 0) {
                GridNode currentNode = openSet.RemoveFirst();
				closedSet.Add(currentNode);
				
				if (currentNode == targetNode) {
					sw.Stop();
					//print ("Path found: " + sw.ElapsedMilliseconds + " ms");
					pathSuccess = true;
					break;
				}
				
				foreach (GridNode neighbour in grid.GetNeighbours(currentNode)) {
					if (!neighbour.walkable || closedSet.Contains(neighbour)) {
						continue;
					}
					
					int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) + neighbour.movementPenalty;
					if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) {
						neighbour.gCost = newMovementCostToNeighbour;
						neighbour.hCost = GetDistance(neighbour, targetNode);
						neighbour.parent = currentNode;
						
						if (!openSet.Contains(neighbour))
							openSet.Add(neighbour);
						else 
							openSet.UpdateItem(neighbour);
					}
				}
			}
		}
		if (pathSuccess) {
			waypoints = RetracePath(startNode,targetNode);
			pathSuccess = waypoints.Length > 0;
		}
		callback (new PathResult (waypoints, pathSuccess, request.callback));
		
	}
		
	
	Vector3[] RetracePath(GridNode startNode, GridNode endNode) {
		List<GridNode> path = new List<GridNode>();
        GridNode currentNode = endNode;
		
		while (currentNode != startNode) {
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}
		Vector3[] waypoints = SimplifyPath(path);
		Array.Reverse(waypoints);
		return waypoints;
		
	}
	
	Vector3[] SimplifyPath(List<GridNode> path) {
		List<Vector3> waypoints = new List<Vector3>();
		Vector2 directionOld = Vector2.zero;
		
		for (int i = 1; i < path.Count; i ++) {
			Vector2 directionNew = new Vector2(path[i-1].gridX - path[i].gridX,path[i-1].gridY - path[i].gridY);
			if (directionNew != directionOld) {
				waypoints.Add(path[i].worldPosition);
			}
			directionOld = directionNew;
		}
		return waypoints.ToArray();
	}
	
	int GetDistance(GridNode nodeA, GridNode nodeB) {
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
		
		if (dstX > dstY)
			return 14*dstY + 10* (dstX-dstY);
		return 14*dstX + 10 * (dstY-dstX);
	}
	
	

	//EXTRAS: find safest nodes
	public GridNode[] SafestNodesAvailable(GameObject unit,GameObject enemies, int nodeQuantity, int distance)
	{
        GridNode[] safestNodes = new GridNode[nodeQuantity];
		GridNode currentNode = grid.NodeFromWorldPoint(unit.transform.position);

		for (int i = 0;i<distance; i++) { 
			
		}

		return safestNodes;
    }

	public void FindAvailableEnemies(GridNode node,float moveRange, float attackRange, ref List<Unit> attackableEnemies, bool[,] revisedNodeMatrix, Unit[] enemies)
    {
		if (enemies == null || enemies.Length == 0) return; 

        if (!revisedNodeMatrix[node.gridX, node.gridY] && moveRange > 0)
        {
            revisedNodeMatrix[node.gridX, node.gridY] = true;

			foreach (Unit u in enemies) {
                //UnityEngine.Debug.DrawRay(node.worldPosition, (u.transform.position - node.worldPosition).normalized * attackRange, Color.red, 2, false);
				RaycastHit hit;
                if (Physics.Raycast(node.worldPosition, (u.transform.position - node.worldPosition).normalized, out hit, attackRange, playerUnits))
				{
					UnityEngine.Debug.Log("HIT NAME: " + hit.collider.gameObject.name);
					if (hit.collider.transform.parent.CompareTag("Player"))
					{
                        if (!attackableEnemies.Contains(u))
                        {
                            attackableEnemies.Add(u);
                        }
                    }
                    
                }
            }

            // Exploramos los vecinos del nodo actual
			foreach (GridNode nn in grid.GetNeighbours(node))
            {
				// Realizamos búsqueda recursiva con una profundidad reducida
				float distance = Vector3.Distance(nn.worldPosition,node.worldPosition);
                if (moveRange - distance > 0) // Add this check to prevent infinite recursion
				{
					
					FindAvailableEnemies(nn, moveRange - distance, attackRange, ref attackableEnemies, revisedNodeMatrix, enemies);
				}
            }

        }

		UnityEngine.Debug.Log("ATTACKABLE ENEMIES: " + attackableEnemies.Count);

    }

	
    public GridNode[] GetSafePlaces(GridNode node, float moveRange, List<Unit> enemies, bool[,] revisedNodeMatrix)
	{
		List<GridNode> safePlaces = new List<GridNode>();
		float minDamage = Mathf.Infinity; 

		FindSafePlaces(node, moveRange, enemies, revisedNodeMatrix, ref safePlaces, ref minDamage);

		if(enemies.Count == 0)
		{
            foreach (GridNode n in safePlaces)
            {

				Instantiate(debugBola, n.worldPosition, Quaternion.identity);
            }
        }

		return safePlaces.ToArray();
	}

	private void FindSafePlaces(GridNode node, float moveRange, List<Unit> enemies, bool[,] revisedNodeMatrix, ref List<GridNode> safePlaces, ref float minDamage)
	{
		if (!revisedNodeMatrix[node.gridX, node.gridY] && moveRange > 0)
		{
			revisedNodeMatrix[node.gridX, node.gridY] = true;

			float damage = CalculatePotentialDamage(node, enemies);

			if (damage < minDamage) // New minimum damage
			{
				minDamage = damage;
				safePlaces.Clear();
				safePlaces.Add(node);
			}
			else if (damage == minDamage) // Same damage as minimum
			{
				safePlaces.Add(node);
			}

			List<GridNode> neighbors = grid.GetNeighbours(node);
			neighbors.Sort((a, b) => b.worldPosition.y.CompareTo(a.worldPosition.y)); // Prioritize higher Y nodes

			foreach (GridNode nn in neighbors)
			{
				if (nn.walkable)
				{
                    float distance = Vector3.Distance(nn.worldPosition, node.worldPosition);
                    FindSafePlaces(nn, moveRange - distance, enemies, revisedNodeMatrix, ref safePlaces, ref minDamage);
                }
				
			}
		}
	}

	private float CalculatePotentialDamage(GridNode node, List<Unit> enemies) //raycast from enemy to player
	{
		float totalDamage = 0f;

		foreach (Unit enemy in enemies)
		{
			Vector3 direction = (enemy.transform.position - node.worldPosition).normalized;
			if (Physics.Raycast(enemy.transform.position, direction, enemy.AttackRange, playerUnits))
			{
				totalDamage += enemy.AttackDamage;
			}
		}

		return totalDamage;
	}

	public GridNode[] GetBestAttackPlaces(GridNode node, float moveRange, Unit targetUnit, List<Unit> enemies, bool[,] revisedNodeMatrix, UnitType unitType)
	{
		List<GridNode> bestAttackPlaces = new List<GridNode>();
		float minDamage = Mathf.Infinity;

		FindBestAttackPlace(node, moveRange, targetUnit, enemies, revisedNodeMatrix, ref bestAttackPlaces, ref minDamage, unitType);

		return bestAttackPlaces.ToArray();
	}

    private void FindBestAttackPlace(GridNode node, float moveRange, Unit targetUnit, List<Unit> enemies, bool[,] revisedNodeMatrix, ref List<GridNode> bestAttackPlaces, ref float minDamage, UnitType unitType)
	{
		if (!revisedNodeMatrix[node.gridX, node.gridY] && moveRange > 0)
		{
			revisedNodeMatrix[node.gridX, node.gridY] = true;

			float damage = CalculatePotentialDamage(node, enemies); //Same as safe places

			if (CanAttackTarget(node, targetUnit) && damage < minDamage) //new minimum damage (take damage from enemy)
			{
				minDamage = damage;
				bestAttackPlaces.Clear();
				bestAttackPlaces.Add(node);
			}
			else if (CanAttackTarget(node, targetUnit) && damage == minDamage) //another minimum damage
			{
				bestAttackPlaces.Add(node);
			}

			List<GridNode> neighbors = grid.GetNeighbours(node);
			if (unitType != UnitType.Knight) //If unit is knight, it makes no sense to attack from above
			{
				neighbors.Sort((a, b) => b.worldPosition.y.CompareTo(a.worldPosition.y)); // Prioritize higher Y nodes
			}

			foreach (GridNode nn in neighbors)
			{
				float distance = Vector3.Distance(nn.worldPosition, node.worldPosition);
				FindBestAttackPlace(nn, moveRange - distance, targetUnit, enemies, revisedNodeMatrix, ref bestAttackPlaces, ref minDamage, unitType);
			}
		}

		
	}

	private bool CanAttackTarget(GridNode node, Unit targetUnit)
	{
		Vector3 direction = (targetUnit.transform.position - node.worldPosition).normalized;
		return Physics.Raycast(node.worldPosition, direction, targetUnit.AttackRange, enemyUnits);
	}

	public bool IsPlaceAvailable(GridNode currentNode,float moveRange,Vector3 target, bool[,] revisedNodeMatrix)
	{
        // Si el nodo actual es el mismo que el nodo objetivo, devolver true
        if (currentNode == grid.NodeFromWorldPoint(target))
        {
            return true;
        }

        // Si el rango de movimiento se ha agotado, devolver false
        if (moveRange <= 0)
        {
            return false;
        }

        // Obtener los vecinos del nodo actual
        List<GridNode> neighbors = grid.GetNeighbours(currentNode);

        if (neighbors.Count > 0)
        {
            foreach (GridNode nn in neighbors)
            {
                // Asegúrate de que el nodo no haya sido visitado previamente
                if (revisedNodeMatrix[nn.gridX, nn.gridY]) continue;

                // Marca el nodo como visitado
                revisedNodeMatrix[nn.gridX, nn.gridY] = true;

                // Calcula la distancia al vecino
                float distance = Vector3.Distance(nn.worldPosition, currentNode.worldPosition);

                // Llamada recursiva para comprobar si el objetivo es alcanzable desde este vecino
                if (IsPlaceAvailable(nn, moveRange - distance, target, revisedNodeMatrix))
                {
                    return true;
                }
            }
        }

        // Si no hay vecinos disponibles o ninguno lleva al objetivo, devolver false
        return false;

    }



}
