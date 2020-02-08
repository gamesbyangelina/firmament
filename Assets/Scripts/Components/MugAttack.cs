using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MugAttack : FComponent
{
    
    public float mugChance = 1f;

    public override FEvent PropagateEvent(FEvent ev){
        if(ev.eventName == FEventCodes.ENTITY_MELEE_ATTACK && Random.Range(0f, 1f) < mugChance){
            FEntity target = (FEntity) ev.Get("target");
            List<FEntity> targetCarries = target.PropagateEvent(new FEvent(FEventCodes.GET_CARRIED_LIST, "list", new List<FEntity>())).Get("list") as List<FEntity>;
            if(targetCarries.Count > 0){
                FEntity stolenItem = targetCarries[Random.Range(0, targetCarries.Count)];
                //Take the item
                target.PropagateEvent(new FEvent(FEventCodes.TRY_PICK_UP_OBJECT, "object", stolenItem));
                stolenItem.PropagateEvent(new FEvent(FEventCodes.PICKED_UP, "carrier", parentEntity));
                parentEntity.PropagateEvent(new FEvent(FEventCodes.RECEIVE_ITEM, "item", stolenItem));

                Debug.Log("Stolen item");
                // MessageLog.instance.Log("")
            }
        }
        return ev;
    }

}
