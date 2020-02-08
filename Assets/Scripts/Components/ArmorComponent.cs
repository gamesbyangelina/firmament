using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorComponent : FComponent
{

    int damageReduction = 1;

    protected override void _Setup(){
        //Slightly higher than average priority
        priority = 99;
    }

    public override FEvent PropagateEvent(FEvent ev){
        if(ev.eventName == FEventCodes.ENTITY_RECEIVE_MELEE_ATTACK || ev.eventName == FEventCodes.ENTITY_RECEIVE_THROWN_ATTACK){
            int dmg = (int) ev.Get("damage");
            dmg -= damageReduction;
            dmg = dmg < 0 ? 0 : dmg;
            ev.Set("damage", dmg);
        }

        return ev;
    }

}
