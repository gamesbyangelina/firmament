using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingPotionEffect : FComponent
{
   /*   
    Potions were one of the last things I implemented before stopping on the example. This is a good example of a branch
    point for detail in an ECS. A potion could be a container filled with a liquid, and the liquid has effects when it 
    touches things (the floor, a human, a zombie). However that felt too detailed/granular, so instead potions are things
    that can be drunk or thrown, and then apply affects to the things they're drunk by or thrown at. So you can't find 
    a fountain of healing liquid or find someone who cries tears of healing (or maybe you could but it'd be another case
    rather than reusing the same healing liquid in a new context).
   */
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
