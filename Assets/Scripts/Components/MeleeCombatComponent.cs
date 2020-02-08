using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeCombatComponent : FComponent
{

    protected override void _Setup(){
        //Appears later in the queue so it attacks after everything has added modifiers
        this.priority = 1000;
    }
    public override FEvent PropagateEvent(FEvent ev){
        if(ev.eventName == FEventCodes.ENTITY_MELEE_ATTACK){
            FEntity target = (FEntity) ev.Get("target");

            FEvent nameEvent = parentEntity.PropagateEvent(new FEvent(FEventCodes.GET_ENTITY_NAME,"name", "????"));
            string ourName = (string)nameEvent.Get("fullstring");
            nameEvent = target.PropagateEvent(new FEvent(FEventCodes.GET_ENTITY_NAME,"name", "????"));
            string theirName = (string)nameEvent.Get("fullstring");

            MessageLog.instance.Log(ourName+" attacks the "+theirName+"!");
            

            target.PropagateEvent(new FEvent(FEventCodes.ENTITY_RECEIVE_MELEE_ATTACK, "attacker", this.parentEntity, "damage", (int)ev.Get("damage")));

            
        }
        return ev;
    }

}
