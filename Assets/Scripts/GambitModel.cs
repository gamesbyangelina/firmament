using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GambitModel : Model
{

	public FEntity objectEntityPrefab;
	public FEntity itemEntityPrefab;
	public FEntity actorEntityPrefab;

	bool[,] map_draft;

	public void Print(bool[,] m){
		string res = "";
		for(int i=0; i<m.GetLength(0); i++){
			string line = "";
			for(int j=0; j<m.GetLength(1); j++){
				line += m[i,j] ? 1 : 0;
			}
			res += line+"\n";
		}
		Debug.Log(res);
	}

	public override void SetupGame(){
		this.map.CreateGameMap(worldWidth, worldHeight);
		
		entityList = new List<FEntity>();
		ClearSpawnRequests();

		SetupDungeon();
		
		ProcessSpawnRequests();
		ClearSpawnRequests();
		UpdateInterface();
		
		StartCoroutine("PostVision");		
	}

	public void SetupDungeon(){
		CAMap(0.465f, 2, 4, 5);
		Print(map_draft);
		List<MapTile> region = FindLargestRegion(map_draft);

		playerEntity = Instantiate(actorEntityPrefab);
		playerEntity.Setup();
		RenderComponent rc = playerEntity.GetComponent<RenderComponent>();
		rc.SetSprite(spriteManager.GetSpriteByName("knight"));
		rc.GetComponent<RenderComponent>().SetColor(new Color(0.964f, 0.839f, 0.741f, 1f));
		playerEntity.AddComponent(playerEntity.gameObject.AddComponent<MeleeCombatComponent>());
		playerEntity.AddComponent(playerEntity.gameObject.AddComponent<PlayerComponent>());
		NameComponent nc = playerEntity.GetComponent<NameComponent>();
		nc.SetData("Player", "c3a38a");
		playerEntity.AddComponent(nc);
		playerEntity.AddComponent("OrganicCreature");
		
		CameraTrackObject.instance.trackObject = playerEntity.gameObject;
		
		EquipSlotsComponent esc = playerEntity.GetComponent<EquipSlotsComponent>();
		esc.AddSlot("LeftHand");
		esc.AddSlot("Chest");
		
		entityList.Add(playerEntity);
		MapTile start = region[Random.Range(0, region.Count)];
		map.PutEntityAt(playerEntity, start.x, start.y);
		playerEntity.PropagateEvent(new FEvent(FEventCodes.ENTITY_SPAWN, "x", start.x, "y", start.y));

		FEntity sword = GambitPrototypes.instance.SpawnPrototype("sword", -1, -1);
		sword.PropagateEvent(new FEvent(FEventCodes.PICKED_UP, "carrier", playerEntity));
		playerEntity.PropagateEvent(new FEvent(FEventCodes.RECEIVE_ITEM, "item", sword));
		playerEntity.PropagateEvent(new FEvent(FEventCodes.EQUIP_ITEM, "item", sword, "slotName", "LeftHand"));
	
		FEntity armor = GambitPrototypes.instance.SpawnPrototype("armor", -1, -1);
		//Player can't die.
		armor.AddComponent("InvulnerableComponent");
		armor.PropagateEvent(new FEvent(FEventCodes.PICKED_UP, "carrier", playerEntity));
		playerEntity.PropagateEvent(new FEvent(FEventCodes.RECEIVE_ITEM, "item", armor));
		playerEntity.PropagateEvent(new FEvent(FEventCodes.EQUIP_ITEM, "item", armor, "slotName", "Chest"));
		
		nonPlayerAgents = new List<FEntity>();
		
		for(int i=0; i<10; i++){
			start = region[Random.Range(0, region.Count)];
			SpawnPrototypeAt("skeleton_fighter", start.x, start.y);
		}
		
		for(int i=0; i<5; i++){
			start = region[Random.Range(0, region.Count)];
			SpawnPrototypeAt("healingpotion", start.x, start.y);
		}

		for(int i=0; i<20; i++){
			start = region[Random.Range(0, region.Count)];
			SpawnPrototypeAt("grass", start.x, start.y);
		}
	}

	IEnumerator PostVision(){
		yield return new WaitForEndOfFrame();
		GameMap.instance.UpdateVisionCone(playerEntity);
	}

	
	public void EndTurn(){	
		UpdateInterface();

		markedForRemoval = new List<FEntity>();
		FEntity entity = null;
		for(int i=0; i<nonPlayerAgents.Count; i++){
			entity = nonPlayerAgents[i];
			entity.PropagateEvent(new FEvent(FEventCodes.TAKE_TURN));
		}

		//Everyone's acted, now:
		for(int i=0; i<nonPlayerAgents.Count; i++){
			entity = nonPlayerAgents[i];
			entity.PropagateEvent(new FEvent(FEventCodes.END_TURN));
		}
		playerEntity.PropagateEvent(new FEvent(FEventCodes.END_TURN));

		for(int i=0; i<markedForRemoval.Count; i++){
			entity = markedForRemoval[i];
			if(entityList.Contains(entity))
				entityList.Remove(entity);
			if(nonPlayerAgents.Contains(entity))
				nonPlayerAgents.Remove(entity);
			Destroy(entity.gameObject);
		}
		ProcessSpawnRequests();
		ClearSpawnRequests();

		GameMap.instance.UpdateVisionCone(playerEntity);

		MessageLog.turn++;
	}

	public List<MapTile> FindLargestRegion(bool[,] bmap){
		List<MapTile> res = new List<MapTile>();

		for(int i=0; i<worldWidth; i++){
			for(int j=0; j<worldHeight; j++){
				GameMap.instance.maps[i+j*worldWidth].flag = false;
			}
		}

		for(int i=0; i<worldWidth; i++){
			for(int j=0; j<worldHeight; j++){
				if(!bmap[i,j] && !GameMap.instance.maps[i+j*worldWidth].flag){
					MapTile root = GameMap.instance.maps[i+j*worldWidth];
					List<MapTile> region = new List<MapTile>();
					List<MapTile> openList = new List<MapTile>(); openList.Add(root);
					while(openList.Count > 0){
						MapTile t = openList[0]; openList.RemoveAt(0);
						t.flag = true;
						region.Add(t);
						foreach(MapTile n in t.nbs){
							if(!n.flag && !openList.Contains(n) && !bmap[n.x, n.y] && !n.blocked){
								openList.Add(n);
							}
						}
					}
					if(region.Count > res.Count){
						res = region;
						Debug.Log("new largest region: "+region.Count);
					}
				}
			}
		}

		return res;
	}

	public void CAMap(float irand, int iter, int min, int max){
		bool[,] bmap = new bool[worldWidth, worldHeight];
		for(int i=0; i<worldWidth; i++){
			for(int j=0; j<worldHeight; j++){
				bmap[i,j] = Random.Range(0f, 1f) < irand;
			}
		}

		for(int it=0; it<iter; it++){
			bool[,] nmap = new bool[worldWidth, worldHeight];
			for(int i=0; i<worldWidth; i++){
				for(int j=0; j<worldHeight; j++){
					int n = cnt_nbs(bmap,i,j);
					if(!bmap[i,j]){
						nmap[i,j] = n > 4;
					}
					else{
						nmap[i,j] = n >= 4;
					}
				}
			}	
			bmap = nmap;
		}

		for(int i=0; i<worldWidth; i++){
			for(int j=0; j<worldHeight; j++){
				if(bmap[i,j] || i == 0 || j == 0 || i == worldWidth-1 || j == worldHeight-1){
					bmap[i,j] = true;
					FEntity wall = Instantiate(objectEntityPrefab);
					wall.Setup();
					wall.name = "Wall";
					string spr = "wall_main";
					if(j > 0 && !bmap[i,j-1]){
						spr = "rockwall"+Random.Range(1, 4);
					}
					if(i == 0 || i == worldWidth-1){
						spr = "wall_main";
					}
					wall.GetComponent<RenderComponent>().SetData(spr, new Color32(146,126,106,255));
					wall.AddComponent("MovementBlockComponent");
					wall.AddComponent("BlockVisionComponent");
					entityList.Add(wall);
					map.PutEntityAt(wall, i,j);
					map.ClearFloorAt(i,j);
					wall.PropagateEvent(new FEvent(FEventCodes.ENTITY_SPAWN, "x", i, "y", j));
				}
			}
		}

		map_draft = bmap;
	}

	public int cnt_nbs(bool[,] map, int x, int y){
		int n = 0;
		for(int i=-1; i<2; i++){
			for(int j=-1; j<2; j++){
				if(i != 0 || j != 0){
					int dx = i+x; int dy = j+y;
					if(dx >= 0 && dy >= 0 && dx < map.GetLength(0) && dy < map.GetLength(1)){
						if(map[dx,dy])
							n++;
					}
					else
						n++;
				}
			}
		}
		return n;
	}

	public void AddRoom(int x, int y, int w, int h){
		for(int i=x; i<x+w; i++){
			FEntity wall = Instantiate(objectEntityPrefab);
			wall.Setup();
			wall.name = "Wall";
			wall.GetComponent<RenderComponent>().SetData("wall_bottom", new Color32(146,126,106,255));
			wall.AddComponent("MovementBlockComponent");
			wall.AddComponent("BlockVisionComponent");
			entityList.Add(wall);
			map.PutEntityAt(wall, i,y);
			map.ClearFloorAt(i,y);
			wall.PropagateEvent(new FEvent(FEventCodes.ENTITY_SPAWN, "x", i, "y", y));

			wall = Instantiate(objectEntityPrefab);
			wall.Setup();
			wall.name = "Wall";
			wall.GetComponent<RenderComponent>().SetData("wall_bottom", new Color32(146,126,106,255));
			wall.AddComponent("MovementBlockComponent");
			wall.AddComponent("BlockVisionComponent");
			entityList.Add(wall);
			map.PutEntityAt(wall, i, y+h-1);
			map.ClearFloorAt(i,y+h-1);
			wall.PropagateEvent(new FEvent(FEventCodes.ENTITY_SPAWN, "x", i, "y", y+h-1));
		}
		for(int i=y; i<y+h; i++){
			FEntity wall = Instantiate(objectEntityPrefab);
			wall.Setup();
			wall.name = "Wall";
			wall.GetComponent<RenderComponent>().SetData("wall_bottom", new Color32(146,126,106,255));
			wall.AddComponent("MovementBlockComponent");
			wall.AddComponent("BlockVisionComponent");
			entityList.Add(wall);
			map.PutEntityAt(wall, x,i);
			map.ClearFloorAt(x,i);
			wall.PropagateEvent(new FEvent(FEventCodes.ENTITY_SPAWN, "x", x, "y", i));

			wall = Instantiate(objectEntityPrefab);
			wall.Setup();
			wall.name = "Wall";
			wall.GetComponent<RenderComponent>().SetData("wall_bottom", new Color32(146,126,106,255));
			wall.AddComponent("MovementBlockComponent");
			wall.AddComponent("BlockVisionComponent");
			entityList.Add(wall);
			map.PutEntityAt(wall, x+w-1,i);
			map.ClearFloorAt(x+w-1,i);
			wall.PropagateEvent(new FEvent(FEventCodes.ENTITY_SPAWN, "x", x+w-1, "y", i));
		}
	}

	public void DestroyEntity(FEntity entity){
		map.RemoveEntityAt(entity);
		markedForRemoval.Add(entity);
	}

	
	public void ShowInventory(){
		FEvent getInventory = playerEntity.PropagateEvent(new FEvent(FEventCodes.GET_CARRIED_LIST, "list", new List<FEntity>()));
		List<FEntity> inventoryOptions = getInventory.Get("list") as List<FEntity>;
		FEvent wieldedList = playerEntity.PropagateEvent(new FEvent(FEventCodes.GET_WIELDED_LIST, "list", new List<FEntity>()));
		inventory = new List<FEntity>();
		inventory.AddRange(wieldedList.Get("list") as List<FEntity>);
		inventory.AddRange(inventoryOptions);
		ItemListWindow.instance.OpenWindow("Inventory", "Number: Select\tESC: Close", inventory);
	}

	public List<FEntity> inventory;
	public FEntity selectedInventoryItem;
	public int selectedIndex;

	public void SelectInventoryItem(int item){
		
		if(item < 1 || item > inventory.Count){
			return;
		}
		else{
			//Get affordances
			FEvent getaffs = inventory[item-1].PropagateEvent(new FEvent(FEventCodes.GET_ENTITY_AFFORDANCES, "list", new List<string>())); 
			List<string> affs = getaffs.Get("list") as List<string>;
			string suff = "";
			for(int i=0; i<affs.Count; i++){
				if(i == 0) suff += " (";
				suff += affs[i];
				if(i < affs.Count-1)
					suff+= ", ";
				else
					suff+=")";
			}
			selectedIndex = item;
			//Select the item
			ItemListWindow.instance.SelectItem(item-1, suff);
			selectedInventoryItem = inventory[item-1];
		}
	}

	public void EquipSelected(){
		if(selectedInventoryItem == null || !selectedInventoryItem.HasComponent(typeof(Equippable)))
			return;
		playerEntity.PropagateEvent(new FEvent(FEventCodes.EQUIP_ITEM, "item", selectedInventoryItem));
		SelectInventoryItem(selectedIndex);
		UpdateInterface();
	}

	public void UnequipSelected(){
		if(selectedInventoryItem == null || !selectedInventoryItem.HasComponent(typeof(Equippable)))
			return;
		playerEntity.PropagateEvent(new FEvent(FEventCodes.UNEQUIP_ITEM, "item", selectedInventoryItem));
		SelectInventoryItem(selectedIndex);
		UpdateInterface();
	}

	public void DrinkSelected(){
		if(selectedInventoryItem == null || !selectedInventoryItem.HasComponent(typeof(Drinkable)))
			return;
		
		string _n = selectedInventoryItem.PropagateEvent(new FEvent(FEventCodes.GET_ENTITY_NAME)).Get("name") as string;
		MessageLog.instance.Log("You drink the "+_n+".");

		FEvent drink = selectedInventoryItem.PropagateEvent(new FEvent(FEventCodes.DRINK, "drinker", playerEntity, "consumed", false));
		if((bool)drink.Get("consumed")){
			inventory.Remove(selectedInventoryItem);
			playerEntity.PropagateEvent(new FEvent(FEventCodes.CONSUME_ITEM, "item", selectedInventoryItem));
		}
		selectedInventoryItem = null;
		selectedIndex = -1;

		ItemListWindow.instance.RefreshWindow(inventory);
	}

	public void ThrowSelected(){
		if(selectedInventoryItem == null || !selectedInventoryItem.HasComponent(typeof(Throwable)))
			return;
		Controller.instance.EnterTileSelection(ThrowAt);
		ItemListWindow.instance.CloseWindow();
	}

	public void ThrowAt(int x, int y){
		//Throw event
		playerEntity.PropagateEvent(new FEvent(FEventCodes.LOSE_ITEM, "item", selectedInventoryItem));
		selectedInventoryItem.PropagateEvent(new FEvent(FEventCodes.THROWN, "x", x, "y", y));
		//selectedInventoryItem
		Controller.instance.ExitTileSelection();
	}

	public void PickUpItemHere(){
		pickOptions = GameMap.instance.GetPickUppableItemsAt(playerEntity);
		ItemListWindow.instance.OpenWindow("Pick Up Item", "Number: Select Item\tESC: Close", pickOptions);
	}

	public List<FEntity> pickOptions;

	public bool SelectPickup(int item){
		if(item < 1 || item > pickOptions.Count){
			return false;
		}
		else{ 
			//Take the item
			GameMap.instance.PropagateEventToTile(new FEvent(FEventCodes.TRY_PICK_UP_OBJECT, "object", pickOptions[item-1]), playerEntity, true);
			pickOptions[item-1].PropagateEvent(new FEvent(FEventCodes.PICKED_UP, "carrier", playerEntity));
			playerEntity.PropagateEvent(new FEvent(FEventCodes.RECEIVE_ITEM, "item", pickOptions[item-1]));
			//Close the window
			ItemListWindow.instance.CloseWindow();
			UpdateInterface();
			return true;
		}
	}

	public bool TryMovePlayerRelative(int dx, int dy){
		FEvent loc = playerEntity.PropagateEvent(new FEvent(FEventCodes.GET_LOCATION, "x", -1, "y", -1));
		return TryMovePlayer(dx+((int)loc.Get("x")), dy+((int)loc.Get("y")));
	}

	public bool TryMovePlayer(int tox, int toy){
		return TryMoveEntity(playerEntity, tox, toy);
	}

	public bool TryMoveEntityRelative(FEntity entity, int dx, int dy){
		FEvent loc = entity.PropagateEvent(new FEvent(FEventCodes.GET_LOCATION, "x", -1, "y", -1));
		return TryMoveEntity(entity, dx+((int)loc.Get("x")), dy+((int)loc.Get("y")));
	}

	public bool CanSeePlayer(FEntity entity, int maxDistance){
		Vector2 actualPosition = GameMap.instance.GetMapLocation(entity);
		Vector2 actualPlayerPosition = GameMap.instance.GetMapLocation(playerEntity);
		Vector2 dist = actualPlayerPosition-actualPosition;
		if(dist.magnitude > maxDistance){
			return false;
		}
		if(Physics2D.Raycast(actualPosition, dist, dist.magnitude, 1 << 8).collider == null){
			return true;
		}

		return false;
	}

	List<FEntity> nonPlayerAgents = new List<FEntity>();

	
	public TMPro.TextMeshProUGUI hpText;
	public TMPro.TextMeshProUGUI wieldingText;
	public TMPro.TextMeshProUGUI onGroundText;

	public void UpdateInterface(){
		StatBlockComponent stats = playerEntity.GetComponent<StatBlockComponent>();
		hpText.text = "HP: "+stats.hp+"/"+stats.max_hp;

		FEvent list = playerEntity.PropagateEvent(new FEvent(FEventCodes.GET_WIELDED_LIST, "list", new List<FEntity>()));
		List<FEntity> items = list.Get("list") as List<FEntity>;
		string listOfItems = "";
		for(int i=0; i<items.Count; i++){
			string nameOfItem = items[i].PropagateEvent(new FEvent(FEventCodes.GET_ENTITY_NAME)).Get("name") as string;
			listOfItems += nameOfItem;
			if(i < items.Count-1)
				listOfItems += ", ";
		}
		wieldingText.text = "Equipped: "+listOfItems;

		FEvent groundList = GameMap.instance.PropagateEventToTile(new FEvent(FEventCodes.GET_VISIBLE_LIST, "list", new List<string>()), playerEntity);
		List<string> onFloor = groundList.Get("list") as List<string>;
		string listOnFloor = "";
		for(int i=0; i<onFloor.Count; i++){
			listOnFloor += onFloor[i];
			if(i < onFloor.Count-1)
				listOnFloor += ", ";
		}
		onGroundText.text = "Here: "+listOnFloor;
	}

	List<FEntity> markedForRemoval;


	public bool TryMoveEntity(FEntity ch, int tox, int toy){
        FEvent moveIntoEvent = new FEvent(FEventCodes.ENTITY_TRY_MOVE_INTO);
        moveIntoEvent.Add("x", tox);
        moveIntoEvent.Add("y", toy);
		moveIntoEvent.Add("entity", ch);
        moveIntoEvent.Add("moveCancelled", false);

		FEvent loc1 = ch.PropagateEvent(new FEvent(FEventCodes.GET_LOCATION, "x", -1, "y", -1));
		int dist = Mathf.Max(Mathf.Abs((int)loc1.Get("x")-tox), Mathf.Abs((int)loc1.Get("y")-toy));
		if(dist > 1){
			Debug.Log(loc1.Get("x")+","+loc1.Get("y"));
			List<MapTile> tiles = GameMap.instance.PathToFrom((int)loc1.Get("x"), (int)loc1.Get("y"), tox, toy);
			Debug.Log(tiles[0].x+","+tiles[0].y);
			return TryMoveEntity(ch,tiles[0].x, tiles[0].y);
		}

        List<FEntity> entitiesAtNewLocation = map.GetEntitiesAt(tox, toy);
        foreach(FEntity entity in entitiesAtNewLocation){
            moveIntoEvent = entity.PropagateEvent(moveIntoEvent);
        }

        if((bool)moveIntoEvent.Get("moveCancelled")){
            //Nope
			if(moveIntoEvent.Get("moveBlockedByEntity") != null){
				//End turn
				FEntity entity = (FEntity) moveIntoEvent.Get("blockingEntity");
				FEvent meleeAttack = ch.PropagateEvent(new FEvent(FEventCodes.ENTITY_MELEE_ATTACK, "target", entity, "damage", 0));
				
			}
			EndTurn();
            return false;
        }

        FEvent moveEvent = new FEvent(FEventCodes.ENTITY_MOVE);
        moveEvent.Add("x", tox);
        moveEvent.Add("y", toy);

        moveEvent = ch.PropagateEvent(moveEvent);
		EndTurn();

        return true;
    }

	List<FEntity> spawnRequests;
	List<int[]> spawnLocationRequests;

	public void ClearSpawnRequests(){
		spawnRequests = new List<FEntity>();
		spawnLocationRequests = new List<int[]>();
	}

	public void SpawnPrototypeAt(string prototype, int x, int y){
		FEntity proto = GambitPrototypes.instance.SpawnPrototype(prototype, x, y);
		spawnRequests.Add(proto);
		spawnLocationRequests.Add(new int[]{x,y});
	}

	public void SpawnPrototypeAtEntity(string prototype, FEntity location){
		int[] loc = GameMap.instance.GetGridLocation(location);
		SpawnPrototypeAt(prototype, loc[0], loc[1]);
	}

	public void ProcessSpawnRequests(){
		for(int i=0; i<spawnRequests.Count; i++){
			GameMap.instance.PutEntityAt(spawnRequests[i], spawnLocationRequests[i][0], spawnLocationRequests[i][1]);
			entityList.Add(spawnRequests[i]);
			nonPlayerAgents.Add(spawnRequests[i]);
			spawnRequests[i].PropagateEvent(new FEvent(FEventCodes.ENTITY_SPAWN, "x", spawnLocationRequests[i][0], "y", spawnLocationRequests[i][1]));
		}
	}

}
