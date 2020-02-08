using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingPotionEffect : FComponent
{
   
    public override FEvent PropagateEvent(FEvent ev){
        if(ev.eventName == FEventCodes.DRINK){
            //INGEST
            FEntity drinker = ev.Get("drinker") as FEntity;
            drinker.PropagateEvent(new FEvent(FEventCodes.HEAL_HP, "amount", 5));
        }
        else if(ev.eventName == FEventCodes.CONTAINER_BREAK){
            //BROKE
            GameMap.instance.PropagateEventToTile(new FEvent(FEventCodes.HEAL_HP, "amount", 5), parentEntity, true);
        }
        return ev;
    }

}
