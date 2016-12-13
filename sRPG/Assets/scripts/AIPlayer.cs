using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading; 

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
//		float AP = base.attPower;
		float AP = Mathf.Max(0, (int)Mathf.Floor(base.damageBase + Random.Range(0, base.damageRollSides)) - opponent.damageReduction);
		float TAP = opponent.attPower/10f;
		float THP = opponent.HP;
		AP = AP / 12f;
		THP = THP / 25f;
		List<Tile> nearbyTiles= TileHighlight.findRange (GameManager.instance.map [(int)opponent.gridPosition.x] [(int)opponent.gridPosition.y], specialRange, true);
		List<Tile> inRangeTiles= TileHighlight.findRange (GameManager.instance.map [(int)base.gridPosition.x] [(int)base.gridPosition.y], (int)base.attackRange, true);
		float amount = 0;
		foreach (Tile t in nearbyTiles) {
			foreach (Player tmp in GameManager.instance.players.Where(x => x.GetType() != typeof(UserPlayer))){
				if (t.gridPosition == tmp.gridPosition)
					amount = amount + 0.25f;
			}
		}
		int enemyNum = 0;
		foreach (Tile t in inRangeTiles) {
			foreach (Player tmp in GameManager.instance.players.Where(x => x.GetType() != typeof(AIPlayer))){
				if (t.gridPosition != tmp.gridPosition)
					enemyNum += enemyNum;
			}
		}
		bool InRange = false;
		foreach (Tile t in inRangeTiles) {
			if (opponent.gridPosition == t.gridPosition){
				InRange = true;
			}
		}

		float attScore;
		if (enemyNum == 0 && !InRange ) {
			attScore = 0;
		} else { 
			attScore = (1 + TAP * 1f) * (1 + amount * 1) / (1 + THP * 1f) ;
		}
		return attScore;
	}

	public float moveScore(Player opponent, int specialRange)
	{
		//int AP = base.damageBase;
//		float AP = base.attPower;
		float TAP = opponent.attPower/10f;
		float THP = opponent.HP/25f;
		float AP = Mathf.Max(0, (float)Mathf.Floor(base.damageBase + Random.Range(0, base.damageRollSides)) - (float)opponent.damageReduction);
		float oneshot  = (float)Mathf.Max (0.0f,(THP *25f- AP));
		Tile tmpTile = GameManager.instance.map [(int)opponent.gridPosition.x] [(int)opponent.gridPosition.y];
		Tile originTile = GameManager.instance.map [(int)base.gridPosition.x] [(int)base.gridPosition.y];
		List<Tile> path = new List<Tile>();
//		tmpTile.path = TileHighlight.AstarFindpath (originTile, tmpTile, GameManager.instance.players.Where(x => x.gridPosition != gridPosition).Select(x => x.gridPosition).ToArray()); // a star return a path.
		tmpTile.path = TilePathFinder.FindPath (originTile,tmpTile, GameManager.instance.players.Where(x => x.gridPosition != gridPosition && x.gridPosition != opponent.gridPosition).Select(x => x.gridPosition).ToArray());

		int Distance = TileHighlight.GetCost(tmpTile);
		List<Tile> nearbyTiles= TileHighlight.findRange (GameManager.instance.map [(int)opponent.gridPosition.x] [(int)opponent.gridPosition.y], specialRange, true);
		float alleyAmount = 0;
		float enemyAmount = 0;
		foreach (Tile t in nearbyTiles) {
			foreach (Player tmp in GameManager.instance.players.Where(x => x.GetType() != typeof(AIPlayer))){
				if (t.gridPosition == tmp.gridPosition)
					enemyAmount = enemyAmount + 0.25f;
			}
			foreach (Player tmp in GameManager.instance.players.Where(x => x.GetType() != typeof(UserPlayer))){
				if (t.gridPosition == tmp.gridPosition)
					alleyAmount = alleyAmount + 0.25f;
			}
		}
		float moveScore;
		if (oneshot <= 0)
			moveScore = (1 +0.3f * TAP) * (1 + alleyAmount*0.1f) / ((1 + THP * 0.8f) * (1 + enemyAmount*0.1f) * 0.7f * (Distance))+2f;  
		else
			moveScore = (1 +0.3f * TAP) * TAP * (1 + alleyAmount*0.1f) / ((1 + THP * 0.8f) * (1 + enemyAmount*0.1f) * 0.7f * (Distance))+1f;
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
			List<Tile> attacktilesInRange = TileHighlight.findRange(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], attackRange, true);
			//List<Tile> movementToAttackTilesInRange = TileHighlight.findRange(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], movementPerActionPoint + attackRange);
			List<Tile> movementTilesInRange = TileHighlight.findRange(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], movementPerActionPoint + 1000);

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

			var opponentsInattRange = attacktilesInRange.Select(x => GameManager.instance.players.Where (y => y.GetType() != typeof(AIPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPosition).Count () > 0 ? GameManager.instance.players.Where(y => y.gridPosition == x.gridPosition).First() : null).ToList();

			//Player opponent = opponentsInRange.OrderBy (x => x != null ? -x.HP : 1000).First ();
			Player attopponent = opponentsInattRange.OrderBy (x => x != null? -x.myattScore : 1000).First ();

			var opponentsInmoveRange = movementTilesInRange.Select(x => GameManager.instance.players.Where (y => y.GetType() != typeof(AIPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPosition).Count () > 0 ? GameManager.instance.players.Where(y => y.gridPosition == x.gridPosition).First() : null).ToList();
			//Player opponent = opponentsInRange.OrderBy (x => x != null ? -x.HP : 1000).ThenBy (x => x != null ? TilePathFinder.FindPath(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y],GameManager.instance.map[(int)x.gridPosition.x][(int)x.gridPosition.y]).Count() : 1000).First ();

			Player moveopponent = opponentsInmoveRange.OrderBy (x => x != null? -x.mymoveScore : 1000).First ();

			bool isTheattTarget = false;

			Debug.Log (moveopponent.mymoveScore);
			if (((attopponent != null) && (attopponent.myattScore >= moveopponent.mymoveScore))||attopponent == moveopponent)
				isTheattTarget = true;
			Thread.Sleep(2000);
			//attack if in range and with lowest HP
			if (isTheattTarget && attacktilesInRange.Where(x => GameManager.instance.players.Where (y => y.GetType() != typeof(AIPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPosition).Count() > 0).Count () > 0) {
//				GameManager.instance.removeTileHighlights();
				moving = false;
				attacking = true;
				GameManager.instance.highlightTilesAt(gridPosition, Color.red, attackRange);
				//base.TurnOnGUI ();
				GameManager.instance.attackWithCurrentPlayer(GameManager.instance.map[(int)attopponent.gridPosition.x][(int)attopponent.gridPosition.y]);

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
			else if (!moving && movementTilesInRange.Where(x => GameManager.instance.players.Where (y => y.GetType() != typeof(AIPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPosition).Count() > 0).Count () > 0) 
			{
				
				GameManager.instance.removeTileHighlights();
				moving = true;
				attacking = false;
				GameManager.instance.highlightTilesAt(gridPosition, Color.blue, movementPerActionPoint, false);
//				Thread.Sleep(2000);
				List<Tile> path = TilePathFinder.FindPath (GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y],GameManager.instance.map[(int)moveopponent.gridPosition.x][(int)moveopponent.gridPosition.y], GameManager.instance.players.Where(x => x.gridPosition != gridPosition && x.gridPosition != moveopponent.gridPosition).Select(x => x.gridPosition).ToArray());
				if (path.Count () > 1) {
					List<Tile> actualMovement = TileHighlight.findRange (GameManager.instance.map [(int)gridPosition.x] [(int)gridPosition.y], movementPerActionPoint, GameManager.instance.players.Where (x => x.gridPosition != gridPosition).Select (x => x.gridPosition).ToArray ());
					path.Reverse ();
//					Thread.Sleep(2000);
					if (path.Where(x => actualMovement.Contains(x)).Count() > 0) 
					{
//						Thread.Sleep(2000);
						GameManager.instance.moveCurrentPlayer (path.Where (x => actualMovement.Contains (x)).First ());
					}
				}
			}
//			Thread.Sleep(2000);

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
