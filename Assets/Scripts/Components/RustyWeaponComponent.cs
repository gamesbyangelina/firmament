﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RustyWeaponComponent : FComponent
{

    int penalty = 1;
    
    public override FEvent PropagateEvent(FEvent ev){
        if(ev.eventName == FEventCodes.ENTITY_MELEE_ATTACK){
            int dmg = ev.Get("damage") == null ? 0 : (int) ev.Get("damage");
            dmg -= penalty;
            ev.Set("damage", dmg);
        }
        if(ev.eventName == FEventCodes.GET_NAME_MODIFIERS){
            List<string> ps = (ev.Get("prefixes") as List<string>);
            ps.Add("<#AB5236>rusty<#ffffff>");
            ev.Set("prefixes", ps);
        }
        return ev;
    }

}
