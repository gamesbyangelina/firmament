using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BleedingStatus : FComponent
{
    int damagePerTurn = 1;

    protected override void _Setup(){
        priority = 50;
    }

    public override FEvent PropagateEvent(FEvent ev){
        if(ev.eventName == FEventCodes.ENTITY_MOVE){
            parentEntity.PropagateEvent(new FEvent(FEventCodes.LOSE_HP, "damage", 1));
            ((GambitModel)GambitModel.instance).SpawnPrototypeAtEntity("blood_spatter", parentEntity);
        }
        return ev;
    }

}
