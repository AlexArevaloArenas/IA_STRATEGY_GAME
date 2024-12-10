using UnityEngine;
using System.Collections;

public class GridNode : IHeapItem<GridNode> {
	
	public bool walkable;
	public Vector3 worldPosition;
	public int gridX;
	public int gridY;
	public int movementPenalty;

	public int gCost;
	public int hCost;
	public GridNode parent;
	int heapIndex;

	public bool elevator;
    public bool slowMask;

    public GridNode(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, int _penalty) {
		walkable = _walkable;
		worldPosition = _worldPos;
		gridX = _gridX;
		gridY = _gridY;
		movementPenalty = _penalty;
		elevator = false;
		slowMask = false;
    }

	public int fCost {
		get {
			return gCost + hCost;
		}
	}

	public int HeapIndex {
		get {
			return heapIndex;
		}
		set {
			heapIndex = value;
		}
	}

	public int CompareTo(GridNode nodeToCompare) {
		int compare = fCost.CompareTo(nodeToCompare.fCost);
		if (compare == 0) {
			compare = hCost.CompareTo(nodeToCompare.hCost);
		}
		return -compare;
	}
}
