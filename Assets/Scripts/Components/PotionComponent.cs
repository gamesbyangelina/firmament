using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionComponent : FComponent
{

    protected override void _Setup(){
        priority = 500;
    }
    
    public override FEvent PropagateEvent(FEvent ev){
        if(ev.eventName == FEventCodes.THROWN){
            //Break
            string name = parentEntity.PropagateEvent(new FEvent(FEventCodes.GET_ENTITY_NAME)).Get("name") as string;
            MessageLog.instance.Log("The "+name+" smashes!");

            GameMap.instance.RemoveEntityAt(parentEntity);
            parentEntity.PropagateEvent(new FEvent(FEventCodes.CONTAINER_BREAK));
            Destroy(gameObject);    
        }
        if(ev.eventName == FEventCodes.DRINK){
            ev.Set("consumed", true);
            Destroy(gameObject);    
        }
        
        return ev;
    }

}
