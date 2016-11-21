using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileRange : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

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
        for (int i=(int)leftMax;i<(int)rightMax;i++)
        {
            for(int j = (int)upMax; j < (int)downMax; j++)
            {
                Tile tmpTile = new Tile();
                tmpTile.gridPosition.x = i;
                tmpTile.gridPosition.y = j;

                List<Tile> path = new List<Tile>();
               // tmpTile.path = AStar(originTile,tmpTile) // a star return a path.
              // tmpTile.cost = ???
              if(tmpTile.cost <= movementPoints)
                {
                    result.Add(tmpTile);
                }
            }
        }

        return result;
    }
}
