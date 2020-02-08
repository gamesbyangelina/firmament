using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanCarry : FComponent
{
   
    public override FEvent PropagateEvent(FEvent ev){

        if(ev.eventName == FEventCodes.GET_ALL_PICKABLES){
            List<FEntity> l = ev.Get("items") as List<FEntity>;
            l.Add(parentEntity);
            ev.Set("items", l);
        }
        return ev;
    }

}
