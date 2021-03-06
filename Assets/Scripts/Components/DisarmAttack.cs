﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisarmAttack : FComponent
{
    
    //Most of the probabilities in the example are 100% because I prefer that kind of thing.
    public float disarmChance = 1f;

    public override FEvent PropagateEvent(FEvent ev){
        if(ev.eventName == FEventCodes.ENTITY_MELEE_ATTACK){
            if(Random.Range(0f, 1f) < disarmChance){
                (ev.Get("target") as FEntity).PropagateEvent(new FEvent(FEventCodes.DISARM_WEAPON));
            }
        }
        return ev;
    }

}
