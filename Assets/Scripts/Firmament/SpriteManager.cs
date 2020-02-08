using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteManager : MonoBehaviour {

	public List<Sprite> allSprites;

	public Dictionary<string, Sprite> spriteMap;

	//Just display white blocks, for the open-source version.
	//Turn this off and put in your own sprites into the sprite manager.
	public bool exampleMode = true;

	public Sprite GetSpriteByName(string name){
		if(exampleMode){
			return spriteMap["wall_main"];
		}
		if(spriteMap.ContainsKey(name)){
			return spriteMap[name];
		}
		return null;
	}

	public static SpriteManager instance;
	void Awake(){
		SpriteManager.instance = this;

		spriteMap = new Dictionary<string, Sprite>();
		foreach(Sprite s in allSprites){
			spriteMap.Add(s.name, s);
		}
	}

}
