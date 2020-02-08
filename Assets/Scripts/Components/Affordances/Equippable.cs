using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equippable : FComponent
{

    public bool equipped = false;
    
    public override FEvent PropagateEvent(FEvent ev){
        if(ev.eventName == FEventCodes.WAS_EQUIPPED){
            equipped = true;
        }
        if(ev.eventName == FEventCodes.WAS_UNEQUIPPED){
            equipped = false;
        }
        if(ev.eventName == FEventCodes.GET_ENTITY_AFFORDANCES){
            List<string> affs = ev.Get("list") as List<string>;
            if(!equipped)
                affs.Add("[e]quip");
            else
                affs.Add("[u]nequip");
        }
        return ev;
    }

}
