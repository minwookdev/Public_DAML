// ReSharper disable InvalidXmlDocComment
/// CODER	      :	MINWOOK KIM
/// MODIFIED DATE : 2023. 05. 04
/// IMPLEMENTATION: A* Pathfinding 노드 정의 클래스 (구역)
using UnityEngine;
using System;
// ReSharper disable IdentifierTypo

/// NOTE:
///
namespace CoffeeCat.Pathfinding2D {
	[Serializable]
	public class Node : IHeapItem<Node> {
		public bool IsMoveable;
		public Vector2 WorldPosition;
		public int GridX, GridY;
		// 지형에 따른 이동 패널티 (길, 잔디, 진흙 등 정의)
		public int MovementPenalty; 

		public int gCost;
		public int hCost;
		public Node parent;
		public int heapIndex;

		public int fCost {
			get => gCost + hCost;
		}

		public int HeapIndex {
			get => heapIndex;
			set => heapIndex = value;
		}
		
		public Node(bool isMoveable, Vector2 worldPosition, int gridX, int gridY, int penalty) {
			this.IsMoveable = isMoveable;
			this.WorldPosition = worldPosition;
			this.GridX = gridX;
			this.GridY = gridY;
			this.MovementPenalty = penalty;
		}
        
		public int CompareTo(Node nodeToCompare) {
			int compare = fCost.CompareTo(nodeToCompare.fCost);
			if (compare == 0) {
				compare = hCost.CompareTo(nodeToCompare.hCost);
			}
			return -compare;
		}
	}
}
