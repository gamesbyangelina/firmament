using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pal : MonoBehaviour {

	public static Pal instance;
	void Awake(){
		Pal.instance = this;
	}

	public Color background_color;
	public Color interactable_color;
	public Color world_color;
	public Color world_color_mute;

	public Color player_color;
	
}
