using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganicCreature : FComponent
{

    /*
        This is one of those components that doesn't really mean anything - I didn't want "bleedable" 
        to be a component, basically, and I figured there'd be similar stuff you'd have here too, like
        "requires food" or "can be poisoned".

        Dwarf Fortress is an example of a richer version of this - things can bleed if they have veins
        and a heart, for instance (I don't know if that's how DF does it but it does model organs
        and fluids, so you might do it like that). Just having a CanBleed component would also work, 
        or a StatusEffect component that just lists all their resistances and vulnerabilities.
        This is the thing with ECS, there's like 100 ways to do something and they *all* sound wrong.
    */

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
