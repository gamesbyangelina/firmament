using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flammable : FComponent
{
    
    public override FEvent PropagateEvent(FEvent ev){
        if(ev.eventName == FEventCodes.SET_FIRE_TO){
            if(!parentEntity.HasComponent(typeof(BurningStatus))){
                parentEntity.AddComponent("BurningStatus");
                if(!GameMap.instance.HasComponentAt(typeof(FireComponent), parentEntity)){
                    ((GambitModel)GambitModel.instance).SpawnPrototypeAtEntity("fire", parentEntity);
                }
            }
        }
        return ev;
    }

}
