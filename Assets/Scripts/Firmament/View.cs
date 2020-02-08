using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class View : MonoBehaviour {


	[HideInInspector]
	public World world;

	virtual public void Init() {
        
    }

	virtual public void Redraw()
	{

	}

	virtual public void ShowText(string text){

	}

	virtual public void ClearText(){
		
	}

	virtual public void Ending(){
		
	}
}
