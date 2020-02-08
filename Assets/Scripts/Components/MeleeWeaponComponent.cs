using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeaponComponent : FComponent
{
   
    int damage = 3;

    public override FEvent PropagateEvent(FEvent e){
        if(e.eventName == FEventCodes.ENTITY_MELEE_ATTACK){
            int dmg = e.Get("damage") == null ? 0 : (int)e.Get("damage");
            dmg += damage;
            e.Set("damage", dmg);
        }
        if(e.eventName == FEventCodes.THROWN){
            //Collect a damage event as if we were attacking?
            FEvent attack = parentEntity.PropagateEvent(new FEvent(FEventCodes.ENTITY_MELEE_ATTACK));
            GameMap.instance.PropagateEventToTile(new FEvent(FEventCodes.ENTITY_RECEIVE_THROWN_ATTACK, "attacker", this.parentEntity, "damage", (int)attack.Get("damage")), parentEntity, true);

            
        }
        return e;
    }

}
