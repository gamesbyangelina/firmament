using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvulnerableComponent : FComponent
{
    
    public override FEvent PropagateEvent(FEvent ev){

        if(ev.eventName == FEventCodes.ENTITY_RECEIVE_MELEE_ATTACK){
            ev.Set("damage", 0);
        }

        return ev;
    }
    
}
