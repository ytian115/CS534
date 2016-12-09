using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AIPlayer : Player {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	public override void Update () {
		if (GameManager.instance.players[GameManager.instance.currentPlayerIndex] == this) {
			transform.GetComponent<Renderer>().material.color = Color.green;
		} else {
			transform.GetComponent<Renderer>().material.color = Color.white;
		}
		base.Update();
	}

	//calculate the scores
	public float attackScore(Player opponent, int specialRange)
	{
		int AP = base.damageBase;
		int TAP = opponent.damageBase;
		int THP = opponent.HP;
		List<Tile> nearbyTiles= TileHighlight.FindHighlight (GameManager.instance.map [(int)opponent.gridPosition.x] [(int)opponent.gridPosition.y], specialRange, true);
		int amount = 0;
		foreach (Tile t in nearbyTiles) {
			foreach (Player tmp in GameManager.instance.players.Where(x => x.GetType() != typeof(UserPlayer))){
				if (t.gridPosition == tmp.gridPosition)
					amount++;
			}
		}
		float attScore = 0.5f * AP * TAP * (1 + amount*0.1f) / THP;
		return attScore;
	}

	public float moveScore(Player opponent, int specialRange)
	{
		int AP = base.damageBase;
		int TAP = opponent.damageBase;
		int THP = opponent.HP;
		Tile tmpTile = GameManager.instance.map [(int)opponent.gridPosition.x] [(int)opponent.gridPosition.y];
		Tile originTile = GameManager.instance.map [(int)base.gridPosition.x] [(int)base.gridPosition.y];
		List<Tile> path = new List<Tile>();
		tmpTile.path = TileHighlight.AstarFindpath (originTile, tmpTile, GameManager.instance.players.Where(x => x.gridPosition != gridPosition).Select(x => x.gridPosition).ToArray()); // a star return a path.
		int Distance = TileHighlight.GetCost(tmpTile);
		List<Tile> nearbyTiles= TileHighlight.FindHighlight (GameManager.instance.map [(int)opponent.gridPosition.x] [(int)opponent.gridPosition.y], specialRange, true);
		int alleyAmount = 0;
		int enemyAmount = 0;
		foreach (Tile t in nearbyTiles) {
			foreach (Player tmp in GameManager.instance.players.Where(x => x.GetType() != typeof(AIPlayer))){
				if (t.gridPosition == tmp.gridPosition)
					enemyAmount++;
			}
			foreach (Player tmp in GameManager.instance.players.Where(x => x.GetType() != typeof(UserPlayer))){
				if (t.gridPosition == tmp.gridPosition)
					alleyAmount++;
			}
		}
		float moveScore = 0.5f * AP * TAP * (1 + alleyAmount*0.1f) / (THP * (1 + enemyAmount*0.1f) * Distance);
		return moveScore;

	}


	public override void TurnUpdate ()
	{
		if (positionQueue.Count > 0) {
			transform.position += (positionQueue[0] - transform.position).normalized * moveSpeed * Time.deltaTime;
			
			if (Vector3.Distance(positionQueue[0], transform.position) <= 0.1f) {
				transform.position = positionQueue[0];
				positionQueue.RemoveAt(0);
				if (positionQueue.Count == 0) {
					actionPoints--;
				}
			}
			
		} else {
			//priority queue
			List<Tile> attacktilesInRange = TileHighlight.FindHighlight(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], attackRange, true);
			//List<Tile> movementToAttackTilesInRange = TileHighlight.FindHighlight(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], movementPerActionPoint + attackRange);
			List<Tile> movementTilesInRange = TileHighlight.FindHighlight(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], movementPerActionPoint + 1000);

			//new strategy
			float maxattScore = 0;
			string maxattIndexname = "";
			float maxmoveScore = 0;
			string maxmoveIndexname = "";
			foreach (Player tmp in GameManager.instance.players.Where(x => x.GetType() != typeof(AIPlayer))) {
				tmp.myattScore = attackScore (tmp, 3);
				if (tmp.myattScore > maxattScore) {
					maxattScore = tmp.myattScore;
					maxattIndexname = tmp.playerName;
				}
				tmp.mymoveScore = moveScore (tmp, 3);
				if (tmp.mymoveScore > maxmoveScore) {
					maxmoveScore = tmp.mymoveScore;
					maxmoveIndexname = tmp.playerName;
				}
			}
			Debug.Log (maxattScore);
			Debug.Log (maxattIndexname);
			Debug.Log (maxmoveScore);
			Debug.Log (maxmoveIndexname);

			//attack if in range and with lowest HP
			if (attacktilesInRange.Where(x => GameManager.instance.players.Where (y => y.GetType() != typeof(AIPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPosition).Count() > 0).Count () > 0) {
				var opponentsInRange = attacktilesInRange.Select(x => GameManager.instance.players.Where (y => y.GetType() != typeof(AIPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPosition).Count () > 0 ? GameManager.instance.players.Where(y => y.gridPosition == x.gridPosition).First() : null).ToList();
				Player opponent = opponentsInRange.OrderBy (x => x != null ? -x.HP : 1000).First ();

				GameManager.instance.removeTileHighlights();
				moving = false;
				attacking = true;
				GameManager.instance.highlightTilesAt(gridPosition, Color.red, attackRange);
				//System.Threading.Thread.Sleep(5000);
				//base.TurnOnGUI ();
				GameManager.instance.attackWithCurrentPlayer(GameManager.instance.map[(int)opponent.gridPosition.x][(int)opponent.gridPosition.y]);
			}
			//move toward nearest attack range of opponent
//			else if (!moving && movementToAttackTilesInRange.Where(x => GameManager.instance.players.Where (y => y.GetType() != typeof(AIPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPosition).Count() > 0).Count () > 0) {
//				var opponentsInRange = movementToAttackTilesInRange.Select(x => GameManager.instance.players.Where (y => y.GetType() != typeof(AIPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPosition).Count () > 0 ? GameManager.instance.players.Where(y => y.gridPosition == x.gridPosition && y.HP > 0).First() : null).ToList();
//				Player opponent = opponentsInRange.OrderBy (x => x != null ? -x.HP : 1000).ThenBy (x => x != null ? TilePathFinder.FindPath(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y],GameManager.instance.map[(int)x.gridPosition.x][(int)x.gridPosition.y]).Count() : 1000).First ();
//
//				GameManager.instance.removeTileHighlights();
//				moving = true;
//				attacking = false;
//				GameManager.instance.highlightTilesAt(gridPosition, Color.blue, movementPerActionPoint, false);
//
//				List<Tile> path = TilePathFinder.FindPath (GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y],GameManager.instance.map[(int)opponent.gridPosition.x][(int)opponent.gridPosition.y], GameManager.instance.players.Where(x => x.gridPosition != gridPosition && x.gridPosition != opponent.gridPosition).Select(x => x.gridPosition).ToArray());
//				if (path.Count() > 1) { 
//					GameManager.instance.moveCurrentPlayer(path[(int)Mathf.Max(0, path.Count - 1 - attackRange)]);
//				}
//			}
			//move toward nearest opponent
			else if (!moving && movementTilesInRange.Where(x => GameManager.instance.players.Where (y => y.GetType() != typeof(AIPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPosition).Count() > 0).Count () > 0) {
				var opponentsInRange = movementTilesInRange.Select(x => GameManager.instance.players.Where (y => y.GetType() != typeof(AIPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPosition).Count () > 0 ? GameManager.instance.players.Where(y => y.gridPosition == x.gridPosition).First() : null).ToList();
				Player opponent = opponentsInRange.OrderBy (x => x != null ? -x.HP : 1000).ThenBy (x => x != null ? TilePathFinder.FindPath(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y],GameManager.instance.map[(int)x.gridPosition.x][(int)x.gridPosition.y]).Count() : 1000).First ();

				GameManager.instance.removeTileHighlights();
				moving = true;
				attacking = false;
				GameManager.instance.highlightTilesAt(gridPosition, Color.blue, movementPerActionPoint, false);
				
				List<Tile> path = TilePathFinder.FindPath (GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y],GameManager.instance.map[(int)opponent.gridPosition.x][(int)opponent.gridPosition.y], GameManager.instance.players.Where(x => x.gridPosition != gridPosition && x.gridPosition != opponent.gridPosition).Select(x => x.gridPosition).ToArray());
				if (path.Count() > 1) {
					List<Tile> actualMovement = TileHighlight.FindHighlight(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], movementPerActionPoint, GameManager.instance.players.Where(x => x.gridPosition != gridPosition).Select(x => x.gridPosition).ToArray());
					path.Reverse();
					if (path.Where(x => actualMovement.Contains(x)).Count() > 0) GameManager.instance.moveCurrentPlayer(path.Where (x => actualMovement.Contains(x)).First());
				}
			}
		}

//		if (!attacking && !moving) {
//			actionPoints = 2;		
//			GameManager.instance.nextTurn();
//			return;
//		}

		if (actionPoints <= 1 && (attacking || moving)) {
			moving = false;
			attacking = false;			
		}
		base.TurnUpdate ();
	}
	
	public override void TurnOnGUI () {
		base.TurnOnGUI ();
	}
}
