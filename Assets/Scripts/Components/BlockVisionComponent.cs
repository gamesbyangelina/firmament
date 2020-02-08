using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockVisionComponent : FComponent
{
    
    protected override void _Setup(){
        BoxCollider2D bc = parentEntity.gameObject.AddComponent<BoxCollider2D>();
        parentEntity.gameObject.layer = 8;
    }

}
