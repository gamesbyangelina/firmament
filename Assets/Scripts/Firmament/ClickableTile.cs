using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ClickableTile : MonoBehaviour {

	// Color c_in;
	// Color c_out;

	void Start(){
		// c_in = bg.color;
		// c_out = bg.color;
		// c_in.a = 0.25f;
	}

	public void SetPosition(int _x, int _y){
		this.x = _x;
		this.y = _y;
	}

	public int x, y;

	// public SpriteRenderer bg;

	void OnMouseDown(){
		// Debug.Log("Clicked "+x+","+y);
		Controller.instance.OnTileClicked(this);
	}

	void OnMouseOver(){
		Controller.instance.TileHoverIn(x,y);
		// bg.DOColor(c_in, 0.2f);
	}
	void OnMouseExit(){
		Controller.instance.TileHoverOut(x,y);
		// bg.DOColor(c_out, 0.2f);
	}

}
