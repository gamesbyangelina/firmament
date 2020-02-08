using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObject {

    public string description;
    //Higher is better
    public int descriptionPriority;
    public GameSpaceObject gameSpaceObject;
    
    public bool blocks = true;
    public string symbol = "-";
    public string name = "";
    public string spriteName = "";
    public Color color;
    // public string description = "Warm air blows in from some indeterminate direction.";

    public int x;
    public int y;

    public enum DAMAGETYPE {EXPLOSIVE, FIRE, ELECTRICAL, IMPACT, SHOT};

    public virtual void ReceiveDamage(DAMAGETYPE damage, WorldObject origin=null){

    }
    
    virtual public void DestroyWorldObject(World world){

        world.DestroyGameObject(gameSpaceObject.gameObject);
    }

    public virtual bool HookTargetCheck(WorldObject h){
        return true;
    }

    public virtual bool OnHook(){
        return true;
    }

    public bool hidden = false;

    public void Hide(){
        hidden = true;
        gameSpaceObject.mainSprite.enabled = false;
    }

    public void Show(){
        hidden = false;
        gameSpaceObject.mainSprite.enabled = true;
    }


    public WorldObject(){

    }

    public WorldObject(string name, string spriteName, string description, Color c){
        this.name = name;
        this.spriteName = spriteName;
        this.description = description;
        this.color = c;
    }

    public WorldObject(string name, string symbol, string description){
        this.name = name;
        this.symbol = symbol;
        this.description = description;
    }

    public virtual void OnCollision(WorldObject wo){
        
    }

    public virtual void OnEnter(WorldObject wo){

    }

    public virtual void OnExit(WorldObject wo){

    }

    public virtual void UpdateWorldLocation(int x, int y){
        this.x = x;
        this.y = y;
    }

    public virtual void SetGameSpaceObject(GameSpaceObject obj){
        this.gameSpaceObject = obj;
    }

    internal List<Point> GetNeighbours(Point p){
		List<Point> res = new List<Point>();

		for(int i=-1; i<2; i++){
			for(int j=-1; j<2; j++){
				if(i == 0 && j == 0)
					continue;
				int dx = p.x+i; int dy = p.y+j;
				if(dx < 0 || dy < 0 || dx > Model.instance.worldWidth-1 || dy > Model.instance.worldHeight-1){
					continue;
				}

				// if(World.instance.map[Model.L_PLAY,dx,dy] != null && World.instance.map[Model.L_PLAY, dx, dy].blocks){
				// 	continue;
				// }
				res.Add(new Point(dx,dy,p.g+1));
			}
		}

		return res;
    }

    internal List<Point>  GetAdjacentSpace(Point p){
		List<Point> res = new List<Point>();

		for(int i=-1; i<2; i++){
			for(int j=-1; j<2; j++){
				if(i == 0 && j == 0)
					continue;
				int dx = p.x+i; int dy = p.y+j;

				if(dx < 0 || dy < 0 || dx > Model.instance.worldWidth-1 || dy > Model.instance.worldHeight-1){
					continue;
				}

				// if(World.instance.map[Model.L_PLAY,dx,dy] != null && World.instance.map[Model.L_PLAY, dx, dy].spriteName == "wall"){
				// 	continue;
				// }
				res.Add(new Point(dx,dy,p.g+1));
			}
		}

		return res;
    }

}
