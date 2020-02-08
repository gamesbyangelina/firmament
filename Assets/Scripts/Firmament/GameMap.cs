using System.Collections;
using System.Collections.Generic;  
using UnityEngine;

public class GameMap : MonoBehaviour
{

    public static GameMap instance;
    void Awake(){
        GameMap.instance = this;
    }

    [Header("Prefabs")]
    public MapTile mapTilePrefab;

    [Header("Config")]
    public float widthSpacingFactor = 2f/3f;
    public float heightSpacingFactor = 1;
    
    public MapTile[] maps;

    [HideInInspector]
    public int width;
    [HideInInspector]
    public int height;


    public Dictionary<string, List<FEntity>> entityLocationMap;
    // public Dictionary<string, List<FEntity>> pendingAdditions;

    public void ClearMap(){
        entityLocationMap = new Dictionary<string, List<FEntity>>();
    }

    public void CreateGameMap(int w, int h){
        if(maps != null){
            for(int i=0; i<maps.Length; i++){
                Destroy(maps[i].gameObject);
            }
        }

        this.width = w;
        this.height = h;
        maps = new MapTile[width*height];

        for(int i=0; i<width; i++){
            for(int j=0; j<height; j++){
                maps[i+j*width] = Instantiate(mapTilePrefab);
                maps[i+j*width].transform.parent = transform;
                maps[i+j*width].transform.position = GetMapLocation(i,j);
                maps[i+j*width].GetComponent<ClickableTile>().SetPosition(i,j);
                maps[i+j*width].x = i;
                maps[i+j*width].y = j;
                if(UnityEngine.Random.Range(0f, 1f) < 0.15f){
                    maps[i+j*width].spriteRenderer.sprite = SpriteManager.instance.GetSpriteByName("tile_cracked_"+ UnityEngine.Random.Range(1, 4));
                }
            }
        }

        for(int i=0; i<width; i++){
            for(int j=0; j<height; j++){
                int index = (i+j*width);
                for(int x=-1; x<2; x++){
                    for(int y=-1; y<2; y++){
                        int dx = i+x;
                        int dy = y+j;
                        int dindex = dx+(dy*width);
                        if(dx < 0 || dy < 0 || dx >= width || dy >= height){
                            continue;
                        }
                        if(x == 0 && y == 0) continue;

                        maps[index].nbs.Add(maps[dindex]);
                        maps[dindex].nbs.Add(maps[index]);
                    }
                }
            }
        }

        entityLocationMap = new Dictionary<string, List<FEntity>>();
        // pendingAdditions = new Dictionary<string, List<FEntity>>();
    }

    public void ClearFloorAt(int x, int y){
        maps[x+y*width].spriteRenderer.color = new Color(0,0,0,0);
    }

    public MapTile GetTile(int x, int y){
        return maps[x+(y*width)];
    }

    // public void ProcessPendingAdditions(){
    //     foreach(string key in pendingAdditions.Keys){
    //         foreach(FEntity value in pendingAdditions[key]){
    //             if(entityLocationMap.ContainsKey(key)){
    //                 entityLocationMap[key].Add(value);
    //             }
    //             else{
    //                 List<FEntity> fes = new List<FEntity>();
    //                 fes.Add(value);
    //                 entityLocationMap.Add(key, fes);
    //             }
    //         }
    //     }
    //     pendingAdditions = new Dictionary<string, List<FEntity>>();
    // }

    public void PutEntityAt(FEntity entity, int x, int y){
        string key = x+","+y;
        // Debug.Log("Added "+entity.name+" to "+x+","+y);
        if(entityLocationMap.ContainsKey(key)){
            List<FEntity> fes = entityLocationMap[key];
            fes.Add(entity);
        }
        else{
            List<FEntity> fes = new List<FEntity>();
            fes.Add(entity);
            entityLocationMap.Add(key, fes);
        }
    }

    public void BlockTile(int x, int y){
        MapTile tile = maps[x+y*width];
        foreach(MapTile t in tile.nbs){
            t.nbs.Remove(tile);
        }
        tile.blocked = true;
        tile.nbs = new List<MapTile>();
    }

    public void PutEntityAt(FEntity entity, FEntity location){
        int[] loc = GetGridLocation(location);
        PutEntityAt(entity, loc[0], loc[1]);
    }
    public int[] GetGridLocation(FEntity entity){
        FEvent loc = entity.PropagateEvent(new FEvent(FEventCodes.GET_LOCATION, "x", -1, "y", -1));
        return new int[]{(int) loc.Get("x"), (int) loc.Get("y")};

    }

    public void RemoveEntityAt(FEntity entity){
        FEvent loc = entity.PropagateEvent(new FEvent(FEventCodes.GET_LOCATION, "x", -1, "y", -1));

        string key = ((int)loc.Get("x"))+","+((int)loc.Get("y"));
        if(!entityLocationMap.ContainsKey(key)){
            // Debug.Log("Could not find item");
            return;
        }
        entityLocationMap[key].Remove(entity);
    }

    public List<FEntity> GetEntitiesAt(int x, int y){
        string key = x+","+y;
        if(!entityLocationMap.ContainsKey(key)){
            return new List<FEntity>();
        }
        return entityLocationMap[key];
    }

    public Vector3 GetMapLocation(int x, int y){
        return new Vector3(x*widthSpacingFactor, y*heightSpacingFactor, 0);
    }

    public Vector3 GetMapLocation(FEntity entity){
        FEvent loc = entity.PropagateEvent(new FEvent(FEventCodes.GET_LOCATION, "x", -1, "y", -1));
        return GetMapLocation(((int)loc.Get("x")),((int)loc.Get("y")));
    }

    public Vector3 GetMapOffset(int x, int y){
        //this is a dupe for now but we may have location use a base offset, this would be without that base offset
        return new Vector3(x*widthSpacingFactor, y*heightSpacingFactor, 0);
    }

    public void ChangeMapLocation(FEntity entity, int nx, int ny){
        RemoveEntityAt(entity);
        PutEntityAt(entity, nx, ny);
    }

    public bool HasComponentAt(System.Type component, FEntity entity){
        int[] loc = GetGridLocation(entity);
        bool res = false;
        foreach(FEntity e in entityLocationMap[loc[0]+","+loc[1]]){
            res = res || e.HasComponent(component);
        }
        return res;
    }

    public FEvent PropagateEventAOE(FEvent ev, FEntity entity, int range, float chance=1f, bool exclude=true){
        int[] loc = GetGridLocation(entity);
        FEvent res = ev;
        for(int i=-range; i<range+1; i++){
            for(int j=-range; j<range+1; j++){
                if(Random.Range(0f, 1f) >= chance) continue;
                if(i == 0 && j == 0){
                    res = PropagateEventToTile(ev, entity, exclude);
                }
                else{
                    if(loc[0]+i >=0 && loc[0]+i < width && loc[1]+j >= 0 && loc[1]+j < height)
                        PropagateEventToTile(ev, loc[0]+i, loc[1]+j);
                }
            }
        }
        return res;
    }
    
    public FEvent PropagateEventToTile(FEvent ev, int x, int y){
        string key = x+","+y;
        if(!entityLocationMap.ContainsKey(key)){
            Debug.LogWarning("Tried to send an event to a location that doesn't exist ("+x+","+y+").");
            return ev;
        }

        foreach(FEntity e in entityLocationMap[key]){
            e.PropagateEvent(ev);
        }
        return ev;
    }

    public FEvent PropagateEventToTile(FEvent ev, FEntity entity, bool exclude=true){
        string key = GetPosition(entity);
        if(!entityLocationMap.ContainsKey(key)){
            Debug.LogError("Tried to send an event to an entity's location, but that location doesn't exist.");
            return ev;
        }
        foreach(FEntity e in entityLocationMap[key]){
            if(e == entity && exclude) continue;
            
            e.PropagateEvent(ev);
        }
        return ev;
    }

    public string GetPosition(FEntity entity){
        FEvent loc = entity.PropagateEvent(new FEvent(FEventCodes.GET_LOCATION, "x", -1, "y", -1));
        return ((int)loc.Get("x"))+","+((int)loc.Get("y"));
    }

    internal List<FEntity> GetPickUppableItemsAt(FEntity entity, bool excludeSelf = true)
    {
        string key = GetPosition(entity);

        List<FEntity> res = new List<FEntity>();
        foreach(FEntity fe in entityLocationMap[key]){
            //We probably never want to pick stuff up off the thing we're looking at?
            if(excludeSelf && fe == entity) continue;

            FEvent takeables = fe.PropagateEvent(new FEvent(FEventCodes.GET_ALL_PICKABLES, "items", new List<FEntity>()));
            foreach(FEntity item in takeables.Get("items") as List<FEntity>){
                res.Add(item);
            }
        }
        return res;
    }

    public void UpdateVisionCone(FEntity visionEntity){
        return;
        Vector3 actualPosition = GetMapLocation(visionEntity);
        Collider2D[] cs = Physics2D.OverlapCircleAll(actualPosition, 4, 1 << 9);
        foreach(Collider2D c in cs){
            Vector2 meetingPoint = c.transform.position-actualPosition;
            RaycastHit2D tch = Physics2D.Raycast(actualPosition, meetingPoint, meetingPoint.magnitude, 1 << 8);
            if(tch.collider == null){
                c.gameObject.GetComponent<MapTile>().Show();
            }   
            else{
                int[] loc = GetGridLocation(tch.collider.gameObject.GetComponent<FEntity>());
                maps[loc[0]+(loc[1]*width)].Show();
            } 
            
            // Vector3 d = visionEntity.transform.position-c.transform.position;
            // RaycastHit2D rch = Physics2D.Raycast(c.transform.position, d, d.magnitude, 1 << 8);
            // if(rch.collider == null){
            //     c.gameObject.GetComponent<MapTile>().Show();
            // }
            // else{
            //     RaycastHit2D[] css = Physics2D.RaycastAll(c.transform.position, d, d.magnitude, 1 << 8);
            //     if(css.Length == 1){
            //         int[] loc = GetGridLocation(rch.collider.gameObject.GetComponent<FEntity>());
            //         maps[loc[0]+(loc[1]*width)].Show();
            //     }
            // }
        }
    }

    public List<MapTile> PathToFrom(FEntity from, FEntity to){
        int[] lf = GetGridLocation(from);
        int[] tf = GetGridLocation(to);
        return PathToFrom(lf[0], lf[1], tf[0], tf[1]);
    }

    public List<MapTile> PathToFrom(int fx, int fy, int tx, int ty){

        List<MapTile> openList = new List<MapTile>();
        List<MapTile> closedList = new List<MapTile>();

        MapTile end = maps[tx+ty*width];

        MapTile start = maps[fx+fy*width];
        start.scoreToHere = 0;
        start.estimateScoreToThere = Manhattan(fx, fy, tx, ty);
        start.pathHelper = null;
        openList.Add(maps[fx+fy*width]);

        int iter = 0;
        while(openList.Count > 0){
            iter++;
            MapTile tile = openList[0];
            openList.RemoveAt(0);
            closedList.Add(tile);


            if(tile.Equals(end)){
                //return the path
                List<MapTile> res = new List<MapTile>();
                res.Add(tile);
                while(tile.pathHelper != null){
                    tile = tile.pathHelper;
                    res.Add(tile);
                }
                res.RemoveAt(res.Count-1);
                res.Reverse();
                // Debug.Log("found in "+iter+" iterations");
                return res;
            }

            for(int i=0; i<tile.nbs.Count; i++){
                MapTile nb = tile.nbs[i];
                if(closedList.Contains(nb)){
                    continue;
                }
                if(!openList.Contains(nb)){
                    openList.Add(nb);
                    nb.pathHelper = tile;
                    nb.scoreToHere = tile.scoreToHere+Mathf.Min(1.41f, Manhattan(tile.x, tile.y, nb.x, nb.y));
                    nb.estimateScoreToThere = Manhattan(nb.x, nb.y, tx, ty);
                }
                else{
                    if(tile.scoreToHere+1 < nb.scoreToHere){
                        nb.pathHelper = tile;
                        nb.scoreToHere = tile.scoreToHere+Mathf.Min(1.41f, Manhattan(tile.x, tile.y, nb.x, nb.y));
                        nb.estimateScoreToThere = Manhattan(nb.x, nb.y, tx, ty);
                    }
                }
            }

            openList.Sort(delegate(MapTile m1, MapTile m2){
                return (m1.scoreToHere+m1.estimateScoreToThere).CompareTo(m2.scoreToHere+m2.estimateScoreToThere);
            });
            // Debug.Log(openList[0].scoreToHere);
            // Debug.Log(openList[openList.Count-1].scoreToHere);
            // Debug.Log("--");
        }

        return new List<MapTile>();
    }

    public int GridDistance(FEntity f1, FEntity f2){
        int[] loc1 = GetGridLocation(f1);
        int[] loc2 = GetGridLocation(f2);
        return Mathf.Max(Mathf.Abs(loc1[0]-loc2[0]), Mathf.Abs(loc1[1]-loc2[1]));
    }

    public int Manhattan(FEntity f1, FEntity f2){
        int[] loc1 = GetGridLocation(f1);
        int[] loc2 = GetGridLocation(f2);
        return Manhattan(loc1[0], loc1[1], loc2[0], loc2[1]);
    }
    
    public int Manhattan(int x1, int y1, int x2, int y2){
        return Mathf.Abs(x1-x2) + Mathf.Abs(y1-y2);
    }

}
