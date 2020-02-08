using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurningStatus : FComponent
{
    
    public int damage = 1;
    public float fireChance = 0.15f;

    bool startedBurning = true;

    public override FEvent PropagateEvent(FEvent ev){
        if(ev.eventName == FEventCodes.END_TURN){
            if(startedBurning){
                startedBurning = false;
                return ev;
            }
            //Take fire damage
            parentEntity.PropagateEvent(new FEvent(FEventCodes.ENTITY_RECEIVE_MAGICAL_ATTACK, "damage", damage, "type", Stat.ELEMENT.FIRE));
            //Chance to set tile on fire
            GameMap.instance.PropagateEventAOE(new FEvent(FEventCodes.SET_FIRE_TO), parentEntity, 1, fireChance);

            
        }
        if(ev.eventName == FEventCodes.ENTITY_MOVE){
            if(!GameMap.instance.HasComponentAt(typeof(FireComponent), parentEntity)){
                // ((GambitModel)GambitModel.instance).SpawnPrototypeAtEntity("fire", parentEntity);
            }
        }
        return ev;
    }

}
