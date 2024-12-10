using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

public class Grid : MonoBehaviour {

	public bool displayGridGizmos;
	public LayerMask unwalkableMask;
    public LayerMask elevatorMask;
    public LayerMask slowMask;
    public Vector2 gridWorldSize;
	public float nodeRadius;
	public TerrainType[] walkableRegions;
	public int obstacleProximityPenalty = 10;
	Dictionary<int,int> walkableRegionsDictionary = new Dictionary<int, int>();
	LayerMask walkableMask;

    GridNode[,] grid;

	float nodeDiameter;
	float diagonalDistance;
	float horizontalDitance;



    public int gridSizeX, gridSizeY;

	int penaltyMin = int.MaxValue;
	int penaltyMax = int.MinValue;

	void Awake() {
		nodeDiameter = nodeRadius*2;
        horizontalDitance = nodeDiameter;
        diagonalDistance = Mathf.Sqrt(Mathf.Pow(nodeDiameter,2)+ Mathf.Pow(nodeDiameter, 2));

        gridSizeX = Mathf.RoundToInt(gridWorldSize.x/nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y/nodeDiameter);

		foreach (TerrainType region in walkableRegions) {
			walkableMask.value |= region.terrainMask.value;
			walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value,2),region.terrainPenalty);
		}

		CreateGrid();
	}

	public int MaxSize {
		get {
			return gridSizeX * gridSizeY;
		}
	}

	void CreateGrid() {
		grid = new GridNode[gridSizeX,gridSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x/2 - Vector3.forward * gridWorldSize.y/2;

		for (int x = 0; x < gridSizeX; x ++) {
			for (int y = 0; y < gridSizeY; y ++) {
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
				bool walkable = !(Physics.CheckSphere(worldPoint,nodeRadius,unwalkableMask));

				int movementPenalty = 0;
                //float currentY = worldPoint.y;

                //Check terrain type
                Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
				RaycastHit hit;
				bool elevator = false;
                bool slow = false;


                if (Physics.Raycast(ray,out hit, 100, walkableMask)) {

                    //Assign Height
                    
					if (hit.point.y > this.transform.position.y) {
                        worldPoint.y = hit.point.y;
                    }

					//Fundamental node priority
					walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
				}

				if (!walkable) {
					movementPenalty += obstacleProximityPenalty;
				}

                if (Physics.CheckSphere(worldPoint, nodeRadius, elevatorMask))
                {
                    elevator = true;
                    //Debug.Log(elevator);

                }

                if (Physics.CheckSphere(worldPoint, nodeRadius, slowMask))
                {
                    slow = true;
					worldPoint.y = 0;
                    //Debug.Log(elevator);

                }

                grid[x,y] = new GridNode(walkable,worldPoint, x,y, movementPenalty);
                grid[x, y].elevator = elevator;
                grid[x, y].slowMask = slow;
                //Debug.Log(grid[x, y].worldPosition.y);

            }
		}

		BlurPenaltyMap (3);

		//Create borders between Grid Floors
		SetUnwalkableBordersBetweenFloors();


    }

	void BlurPenaltyMap(int blurSize) {
		int kernelSize = blurSize * 2 + 1;
		int kernelExtents = (kernelSize - 1) / 2;

		int[,] penaltiesHorizontalPass = new int[gridSizeX,gridSizeY];
		int[,] penaltiesVerticalPass = new int[gridSizeX,gridSizeY];

		for (int y = 0; y < gridSizeY; y++) {
			for (int x = -kernelExtents; x <= kernelExtents; x++) {
				int sampleX = Mathf.Clamp (x, 0, kernelExtents);
				penaltiesHorizontalPass [0, y] += grid [sampleX, y].movementPenalty;
			}

			for (int x = 1; x < gridSizeX; x++) {
				int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridSizeX);
				int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridSizeX-1);

				penaltiesHorizontalPass [x, y] = penaltiesHorizontalPass [x - 1, y] - grid [removeIndex, y].movementPenalty + grid [addIndex, y].movementPenalty;
			}
		}
			
		for (int x = 0; x < gridSizeX; x++) {
			for (int y = -kernelExtents; y <= kernelExtents; y++) {
				int sampleY = Mathf.Clamp (y, 0, kernelExtents);
				penaltiesVerticalPass [x, 0] += penaltiesHorizontalPass [x, sampleY];
			}

			int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass [x, 0] / (kernelSize * kernelSize));
			grid [x, 0].movementPenalty = blurredPenalty;

			for (int y = 1; y < gridSizeY; y++) {
				int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridSizeY);
				int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridSizeY-1);

				penaltiesVerticalPass [x, y] = penaltiesVerticalPass [x, y-1] - penaltiesHorizontalPass [x,removeIndex] + penaltiesHorizontalPass [x, addIndex];
				blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass [x, y] / (kernelSize * kernelSize));
				grid [x, y].movementPenalty = blurredPenalty;

				if (blurredPenalty > penaltyMax) {
					penaltyMax = blurredPenalty;
				}
				if (blurredPenalty < penaltyMin) {
					penaltyMin = blurredPenalty;
				}
			}
		}

	}

	private void SetUnwalkableBordersBetweenFloors()
	{
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
				if (grid[x, y].elevator == true)
				{
                    grid[x, y].walkable = true;

                }
				else
				{
                    float height = grid[x, y].worldPosition.y;

                    foreach (GridNode node in GetNeighbours(grid[x, y]))
                    {
                        if (height > node.worldPosition.y && !grid[x, y].slowMask)
                        {
                            grid[x, y].walkable = false;

                        }
						if (node.elevator == true)
						{
                            grid[x, y].walkable = true;
                        }
                    }
                }
            }
        }
    }

	public List<GridNode> GetNeighbours(GridNode node) {
		List<GridNode> neighbours = new List<GridNode>();

		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				if (x == 0 && y == 0)
					continue;

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
					neighbours.Add(grid[checkX,checkY]);
				}
			}
		}

		return neighbours;
	}

	public GridNode NodeFromWorldPoint(Vector3 worldPosition) {
		float percentX = (worldPosition.x + gridWorldSize.x/2) / gridWorldSize.x;
		float percentY = (worldPosition.z + gridWorldSize.y/2) / gridWorldSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((gridSizeX-1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY-1) * percentY);
		return grid[x,y];
	}

	void OnDrawGizmos() {
		Gizmos.DrawWireCube(transform.position,new Vector3(gridWorldSize.x,1,gridWorldSize.y));
		if (grid != null && displayGridGizmos) {
			foreach (GridNode n in grid) {

				Gizmos.color = Color.Lerp (Color.white, Color.black, Mathf.InverseLerp (penaltyMin, penaltyMax, n.movementPenalty));
				Gizmos.color = (n.walkable)?Gizmos.color:Color.red;
				Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter));
			}
		}
	}

	[System.Serializable]
	public class TerrainType {
		public LayerMask terrainMask;
		public int terrainPenalty;
	}

	/*
	public float NodeDistance(GridNode node1, GridNode node2)
	{
        bool[,] revisedNodes = new bool[gridSizeX, gridSizeY];
        for (int i = 0; i < gridSizeX; i++)
        {
            for (int j = 0; j < gridSizeY; j++)
            {
                revisedNodes[i, j] = false;
            }
        }
        return 
	}

    public float DistanceNodeSearch(GridNode node, GridNode targetNode, float distance, bool[,] revisedNodeMatrix)
    {
		if ()
		{

		}

        if (!revisedNodeMatrix[node.gridX, node.gridY] && range < 0)
        {
            revisedNodeMatrix[node.gridX, node.gridY] = true;

            // Exploramos los vecinos del nodo actual
            foreach (GridNode nn in GetNeighbours(node))
            {
                // Realizamos búsqueda recursiva con una profundidad reducida
                float plusDistance = Vector3.Distance(nn.worldPosition, node.worldPosition);
                DistanceNodeSearch(nn, distance+plusDistance, revisedNodeMatrix);
            }

        }

    }
	*/
}