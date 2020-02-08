using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GameSpaceView : View {

	public static GameSpaceView instance;
	void Awake(){
		GameSpaceView.instance = this;

		allObjects = new List<GameSpaceObject>();
	}

	public enum CameraType {SNAP, TWEEN, STATIC};

	[Header("Spacing and Placement")]
	public Vector3 origin;
	public Vector3 tileSpacing;

	[Header("Camera Setup")]
	public CameraType cameraType;
	public GameSpaceObject trackTarget;

	[Header("Transition Effects")]
	public SpriteRenderer backgroundWash;
	public Color color_shallow;
	public Color color_deep;
	public Color fadeInColor;
	public UnityEngine.UI.Image fadeInCover;


	[Header("Prefabs")]
	public GameObject tilePrefab;
	public ClickableTile clickableTilePrefab;

	List<GameSpaceObject> allObjects;

	public void FadeOut(float len){
		fadeInCover.DOColor(fadeInColor, len);
	}
	override public void Init(){
		if(fadeInCover != null){
			fadeInCover.color = fadeInColor;
			Color to = new Color(0,0,0,0);
			fadeInCover.DOColor(to, 0.4f);
		}
	}

	void FixedUpdate(){
		if(trackTarget != null){
			Camera.main.transform.position = new Vector3(trackTarget.transform.position.x, trackTarget.transform.position.y, -10);
		}
	}

	public void CameraTrackObject(WorldObject obj){
		trackTarget = obj.gameSpaceObject;
	}

	public void CreateBaseTile(WorldObject obj){
		CreateWorldSpaceObject(obj, "Base", 0);

		ClickableTile tile = Instantiate(clickableTilePrefab);
		tile.transform.position = obj.gameSpaceObject.transform.position + new Vector3(0, 0, 10);
		tile.transform.parent = transform;
		tile.x = obj.x;
		tile.y = obj.y;
		
	}

	public GameSpaceObject CreateWorldSpaceObject(WorldObject obj, string layer = "PlayLayer", int worldLayer=0){
		GameSpaceObject gameSpaceObject = Instantiate(tilePrefab).GetComponent<GameSpaceObject>();

		//Setup sprite
		SpriteRenderer sr = gameSpaceObject.GetComponent<SpriteRenderer>();
		sr.sprite = SpriteManager.instance.GetSpriteByName(obj.spriteName);
		gameSpaceObject.name = obj.spriteName;
		sr.color = obj.color;

		sr.sortingLayerName = layer;
		sr.sortingOrder = worldLayer+1;

		obj.SetGameSpaceObject(gameSpaceObject);

		gameSpaceObject.transform.position = GetWorldSpaceLocation(obj.x, obj.y);

		allObjects.Add(gameSpaceObject);

		return gameSpaceObject;
	}

	public void MoveObject(WorldObject obj, int tox, int toy, bool immediate = false){
		if(immediate){
			obj.gameSpaceObject.transform.position = GetWorldSpaceLocation(tox, toy);
		}
		else{
			obj.gameSpaceObject.transform.DOMove(GetWorldSpaceLocation(tox, toy), 0.4f);
		}
	}

	public Vector3 GetWorldSpaceLocation(int x, int y){
		return origin + new Vector3(x * tileSpacing.x, y * tileSpacing.y, 0);
	}

	public Vector3 GetScreenSpaceLocation(int x, int y){
		return Camera.main.WorldToScreenPoint(origin + new Vector3(x * tileSpacing.x, y * tileSpacing.y, 0));
	}

}
