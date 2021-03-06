﻿// using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Model : MonoBehaviour {

	/*
		The reason there's something separate here called Gambit is that this example was built on
		another library I'd been building up for simple turn-based grid-based games. It's not got much
		code in but it's helpful to me. You can safely ignore this class really, it's just to do with
		organising the top-level and the model. GambitModel has specific stuff like the dungeon example in.
	*/

	public string backgroundLayerName;
	public string mainLayerName;
	public string foregroundLayerName;

	public FEntity playerEntity;
	public SpriteManager spriteManager;

	public static Model instance;
	void Awake(){
		Model.instance = this;
	}

	//List of all entities in the game
	protected List<FEntity> entityList;
	public int worldWidth = 10;
	public int worldHeight = 10;

	public GameMap map;

	public virtual void SetupGame(){

	}

	public void RemoveFromWorld(FEntity entity){
		map.RemoveEntityAt(entity);
	}

	public int StraightLineDistanceBetween(FEntity e1, FEntity e2){
		// LocationComponent l1 = e1.GetComponent<LocationComponent>();
		// LocationComponent l2 = e2.GetComponent<LocationComponent>();
		FEvent loc1 = e1.PropagateEvent(new FEvent(FEventCodes.GET_LOCATION, "x", -1, "y", -1));
		FEvent loc2 = e2.PropagateEvent(new FEvent(FEventCodes.GET_LOCATION, "x", -1, "y", -1));
		
		return Mathf.Max(Mathf.Abs(((int)loc1.Get("x")-((int)loc2.Get("x")))), ((int)loc1.Get("y")-((int)loc2.Get("y"))));
		// return (e1.gameObject.transform.position-e2.gameObject.transform.position).magnitude;
	}

	void Start(){
		SetupGame();
	}

}
