using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireComponent : FComponent
{
    public override FEvent PropagateEvent(FEvent ev){
        if(ev.eventName == FEventCodes.END_TURN){
            //If nothing is on fire here, remove
            
            if(!GameMap.instance.HasComponentAt(typeof(BurningStatus), parentEntity)){
                //Destroy self
                ((GambitModel)GambitModel.instance).DestroyEntity(parentEntity);
            }
        }
        return ev;
    }
}
