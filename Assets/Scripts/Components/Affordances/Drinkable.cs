    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drinkable : FComponent
{
    
    public override FEvent PropagateEvent(FEvent ev){
        if(ev.eventName == FEventCodes.GET_ENTITY_AFFORDANCES){
            List<string> affs = ev.Get("list") as List<string>;
            affs.Add("[d]rink");
        }
        return ev;
    }

}
