using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThornsComponent : FComponent
{
    
    public int thornsAmount = 1;

    public override FEvent PropagateEvent(FEvent ev){
        if(ev.eventName == FEventCodes.ENTITY_RECEIVE_MELEE_ATTACK){
            FEntity attacker = ev.Get("attacker") as FEntity;
            attacker.PropagateEvent(new FEvent(FEventCodes.ENTITY_RECEIVE_MAGICAL_ATTACK, "damage", thornsAmount, "attacker", parentEntity, "element", Stat.ELEMENT.NONE));
        }
        return ev;
    }

}
