using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TilePathFinder : MonoBehaviour {
	public static List<Tile> FindPath(Tile originTile, Tile destinationTile) {
		return FindPath(originTile, destinationTile, new Vector2[0]);
	}

	public static List<Tile> FindPath(Tile startTile, Tile targetTile, Vector2[] occupied) {
		return targetTile.path;
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

			if (currentTile = targetTile) {
				//find the path from startTile by using tile.parent
				Tile tmpcurTile = currentTile;
				List<Tile> tmppath = new List<Tile> ();
				while (tmpcurTile.parent != startTile) {
					tmppath.Add (tmpcurTile);
					tmpcurTile = tmpcurTile.parent;
				}
				tmppath.Add (startTile);
				tmppath.Reverse();

				//targetTile.path = tmppath;

				return tmppath;
			}

			foreach (Tile neighbor in currentTile.neighbors) {
				if (neighbor.impassible || closedSet.Contains(neighbor) || occupied.Contains(neighbor.gridPosition)) continue;

				int newMovecost2Neighbor = currentTile.gCost + GetDistance (currentTile, neighbor);
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

//	public static List<Tile> FindPath(Tile originTile, Tile destinationTile, Vector2[] occupied) {
//		List<Tile> closed = new List<Tile>();
//		List<TilePath> open = new List<TilePath>();
//		
//		TilePath originPath = new TilePath();
//		originPath.addTile(originTile);
//		
//		open.Add(originPath);
//		
//		while (open.Count > 0) {
//			//open = open.OrderBy(x => x.costOfPath).ToList();
//			TilePath current = open[0];
//			open.Remove(open[0]);
//			
//			if (closed.Contains(current.lastTile)) {
//				continue;
//			} 
//			if (current.lastTile == destinationTile) {
//				current.listOfTiles.Distinct();
//				current.listOfTiles.Remove(originTile);
//				return current.listOfTiles;
//			}
//			
//			closed.Add(current.lastTile);
//			
//			foreach (Tile t in current.lastTile.neighbors) {
//				if (t.impassible || occupied.Contains(t.gridPosition)) continue;
//				TilePath newTilePath = new TilePath(current);
//				newTilePath.addTile(t);
//				open.Add(newTilePath);
//			}
//		}
//		return null;
//	}

	//calculate the h(x)
	public static int GetDistance (Tile tileA, Tile tileB) {
		return ((int)Mathf.Abs (tileA.gridPosition.x - tileB.gridPosition.x) + (int)Mathf.Abs (tileA.gridPosition.y - tileB.gridPosition.y));
	}
}
