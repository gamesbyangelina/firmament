using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganicCreature : FComponent
{
    public override FEvent PropagateEvent(FEvent ev){
        if(ev.eventName == FEventCodes.INFLICT_BLEEDING){
            if(parentEntity.HasComponent(typeof(BleedingStatus))){
                return ev;
            }
            else{
                parentEntity.AddComponent("BleedingStatus");
            }
        }
        return ev;
    }
}
