using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InflictsBleeding : FComponent
{
    
    public override FEvent PropagateEvent(FEvent ev){

        if(ev.eventName == FEventCodes.ENTITY_MELEE_ATTACK){
            FEntity target = ev.Get("target") as FEntity;
            target.PropagateEvent(new FEvent(FEventCodes.INFLICT_BLEEDING));
        }

        return ev;
    }
    
}
