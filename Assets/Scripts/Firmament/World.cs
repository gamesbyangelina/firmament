using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class World : ScriptableObject {

    public static World instance;
	void Awake(){
		World.instance = this;
	}

    public WorldObject[,,] map;
   
    public WorldObject player;

    public int height;
    public int width;

    // public void AddBG(WorldObject ob, int x, int y){
    //     trails[x, y] = ob;
    // }

    public void AddObject(WorldObject o, int l=1){
        AddObject(o, o.x, o.y, l);
    }

    public void AddObject(WorldObject o, int x, int y, int l=0, bool forceDestroy=false){
        // Debug.Assert(map[l,x,y] == null);
        if(map[l,x,y] != null){
            // Debug.Log("Tried to overwrite "+map[l,x,y].gameSpaceObject.mainSprite.sprite.name);
            if(forceDestroy){
                DestroyObjectAt(x, y, l);
            }
            else{
                return;
            }
        }

        map[l, x, y] = o;
        o.x = x;
        o.y = y;

        Debug.Assert(map[l,x,y] == o);
        Debug.Assert(map[l,o.x,o.y] == o);

        GameSpaceObject gso = GameSpaceView.instance.CreateWorldSpaceObject(o, "PlayLayer", l);
        gso.GetComponent<SpriteRenderer>().sortingOrder = (l+1)*2;
    }

    public WorldObject GetObject(int x, int y, int l=1){
        return map[l, x, y];
    }

    public World(int layers, int width, int height){
        this.map = new WorldObject[layers, width, height];

        WorldObject tileOutline = new WorldObject("", "", "", Pal.instance.world_color);
        GameObject tileParent = new GameObject("Tile Bases");
        
        this.width = width;
        this.height = height;
        for (int i = 0; i < width; i++){
            for (int j = 0; j < height; j++){
                tileOutline = new WorldObject("", "", "", Pal.instance.world_color);
                tileOutline.x = i;
                tileOutline.y = j;
                GameSpaceView.instance.CreateBaseTile(tileOutline);
                tileOutline.gameSpaceObject.transform.parent = tileParent.transform;
            }
        }
    }

    internal void ChangeLayer(int x, int y, int layerFrom, int layerTo)
    {
        WorldObject wo = map[layerFrom,x,y];
        map[layerFrom,x,y] = null;
        map[layerTo,x,y] = wo;
    }

    internal void DestroyGameObject(GameObject o)
    {
        Destroy(o);
    }


    

    internal void DestroyObjectAt(int x, int y, int layer=1, bool fade=false)
    {
        
        WorldObject o = map[layer, x, y];
        if(o == null) return;
        
        if(fade){
            o.gameSpaceObject.mainSprite.DOFade(0f, 0.5f);
        }
        else{
            o.DestroyWorldObject(this);
        }
        map[layer,x,y] = null;
        
    }

    public void UpdateWorldLocation(WorldObject o, int x, int y, int l=1){
        map[l, o.x, o.y] = null;
        map[l, x, y] = o;
    }

    public int MoveRelative(WorldObject o, int dx, int dy, int l=0){
        int nx = o.x + dx;
        int ny = o.y + dy;

        if (nx < 0 || ny < 0 || nx > width - 1 || ny > height - 1) {
            return -1;
        }
        else if (map[l, nx, ny] != null && map[l,nx, ny].blocks){
            return 1;
        }
        else{
            map[l, o.x, o.y] = null;
            map[l, nx, ny] = o;
            o.x = nx;
            o.y = ny;
        }

        return 0;
    }

}
