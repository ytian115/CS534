using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TileHighlight {
	
	public TileHighlight () {
		
	}

	public static List<Tile> FindHighlight(Tile originTile, int movementPoints) {
		return FindHighlight(originTile, movementPoints, new Vector2[0], false);
	}
	public static List<Tile> FindHighlight(Tile originTile, int movementPoints, bool staticRange) {
		return FindHighlight(originTile, movementPoints, new Vector2[0], staticRange);
	}
	public static List<Tile> FindHighlight(Tile originTile, int movementPoints, Vector2[] occupied) {
		return FindHighlight(originTile, movementPoints, occupied, false);
	}

//	public static List<Tile> FindHighlight(Tile originTile, int movementPoints, Vector2[] occupied, bool staticRange) {
//		List<Tile> closed = new List<Tile>();
//		List<TilePath> open = new List<TilePath>();
//		
//		TilePath originPath = new TilePath();
//		if (staticRange) originPath.addStaticTile(originTile);
//		else originPath.addTile(originTile);
//		
//		open.Add(originPath);
//		
//		while (open.Count > 0) {
//			TilePath current = open[0];
//			open.Remove(open[0]);
//			
//			if (closed.Contains(current.lastTile)) {
//				continue;
//			} 
//			if (current.costOfPath > movementPoints + 1) {
//				continue;
//			}
//			
//			closed.Add(current.lastTile);
//			
//			foreach (Tile t in current.lastTile.neighbors) {	
//				if (t.impassible || occupied.Contains(t.gridPosition)) continue;
//				TilePath newTilePath = new TilePath(current);
//				if (staticRange) newTilePath.addStaticTile(t);
//				else newTilePath.addTile(t);
//				open.Add(newTilePath);
//			}
//		}
//		closed.Remove(originTile);
//		closed.Distinct();
//		return closed;
//	}
	public static List<Tile> FindHighlight(Tile originTile, int movementPoints, Vector2[] occupied, bool staticRange)
	{
		List<Tile> closed = new List<Tile>();
		List<Tile> open = new List<Tile>();
		List<Tile> result = new List<Tile>();
		//int i = Tile.x - movementPoints;
		float leftMax = Mathf.Max(originTile.gridPosition.x - movementPoints, 0) ;
		float rightMax = Mathf.Min(originTile.gridPosition.x + movementPoints, GameManager.instance.mapSize -1);
		float upMax = Mathf.Max(originTile.gridPosition.y - movementPoints, 0);
		float downMax = Mathf.Min(originTile.gridPosition.y + movementPoints, GameManager.instance.mapSize - 1);
		for (int i=(int)leftMax;i<=(int)rightMax;i++)
		{
			for(int j = (int)upMax; j <= (int)downMax; j++)
			{
				Tile tmpTile = new Tile();
				//tmpTile.gridPosition.x = i;
				//tmpTile.gridPosition.y = j;
				tmpTile = GameManager.instance.map[i][j];
				if ((originTile.gridPosition.x == tmpTile.gridPosition.x) && (originTile.gridPosition.y == tmpTile.gridPosition.y))
					continue;
				
				List<Tile> path = new List<Tile>();
				tmpTile.path = AstarFindpath (originTile, tmpTile, occupied); // a star return a path.
				tmpTile.cost = GetCost(tmpTile);
				if(tmpTile.cost <= movementPoints)
				{
					result.Add(tmpTile);
				}
			}
		}
		//result.Distinct ();
		return result;
	}

	//calculate the cost of real path
	public static int GetCost (Tile currentTile) {
		int realCost = 0;
		if (currentTile.path == null)
			return 9999;
		//if (currentTile.path.Count <= 1)
		//	return 0;
		for (int i = 1; i < currentTile.path.Count; i++){
		//while (currentTile.path[i] != null) {
			realCost += currentTile.path [i].movementCost;
		}
		Debug.Log (currentTile.gridPosition);
		if (realCost > 0) Debug.Log (realCost);
		return realCost;
	}

	public static int GetDistance (Tile tileA, Tile tileB) {
		return ((int)Mathf.Abs (tileA.gridPosition.x - tileB.gridPosition.x) + (int)Mathf.Abs (tileA.gridPosition.y - tileB.gridPosition.y));
	}

	//find path by using Astar
	public static List<Tile> AstarFindpath(Tile startTile, Tile targetTile, Vector2[] occupied) {
		List<Tile> openSet = new List<Tile> ();
		HashSet<Tile> closedSet = new HashSet<Tile> ();
		openSet.Add (startTile);

		while (openSet.Count > 0) {
			Tile currentTile = openSet [0];
			for (int i = 1; i < openSet.Count; i++) {
				if (openSet [i].fCost < currentTile.fCost || openSet [i].fCost == currentTile.fCost && openSet [i].hCost < currentTile.hCost) {
					currentTile = openSet [i];
				}
			}
			//remove current from Openlist and add it to Closelist
			openSet.Remove (currentTile);
			closedSet.Add (currentTile);

			if (currentTile.gridPosition == targetTile.gridPosition) {
				//find the path from startTile by using tile.parent
				Tile tmpcurTile = new Tile();
				tmpcurTile	= currentTile;
				List<Tile> tmppath = new List<Tile> ();
				while (tmpcurTile != startTile) {
					tmppath.Add (tmpcurTile);
					tmpcurTile = tmpcurTile.parent;
				}
				tmppath.Add (startTile);
				tmppath.Reverse();

				//targetTile.path = tmppath;

				return tmppath;
			}

			currentTile.generateNeighbors ();
			foreach (Tile neighbor in currentTile.neighbors) {
				if (neighbor.impassible || closedSet.Contains(neighbor) || occupied.Contains(neighbor.gridPosition)) continue;

				//int newMovecost2Neighbor = currentTile.gCost + GetDistance (currentTile, neighbor);
				int newMovecost2Neighbor = currentTile.gCost + neighbor.movementCost;
				if (newMovecost2Neighbor < neighbor.gCost || !openSet.Contains (neighbor)) {
					neighbor.gCost = newMovecost2Neighbor;
					neighbor.hCost = GetDistance (neighbor, targetTile);
					neighbor.parent = currentTile;

					if (!openSet.Contains (neighbor))
						openSet.Add (neighbor);
				}
			}
		}//end while

		//did not find a path
		return null;
	}
}
