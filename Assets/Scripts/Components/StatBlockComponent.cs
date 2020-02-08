using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatBlockComponent : FComponent
{
    public int hp = 5;
    public int max_hp = 5;
    public bool isOrganic = false;

    protected override void _Setup(){
        this.priority = 10000;
    }

    public override FEvent PropagateEvent(FEvent ev){
        if(ev.eventName == FEventCodes.HEAL_HP && isOrganic){
            FEvent nameEvent = parentEntity.PropagateEvent(new FEvent(FEventCodes.GET_ENTITY_NAME,"name", "????"));
            string ourName = (string)nameEvent.Get("fullstring");
            int amt = (int)ev.Get("amount");
            hp = Mathf.Min(max_hp, hp+amt);
            MessageLog.instance.Log(ourName+" heals "+amt+" HP.");
        }
        if(ev.eventName == FEventCodes.LOSE_HP){
            FEvent nameEvent = parentEntity.PropagateEvent(new FEvent(FEventCodes.GET_ENTITY_NAME,"name", "????"));
            string ourName = (string)nameEvent.Get("fullstring");
            int dmg = (int)ev.Get("damage");

            this.hp -= dmg;
            MessageLog.instance.Log(ourName+" takes "+dmg+" damage.");

            if(hp < 0){
                parentEntity.PropagateEvent(new FEvent(FEventCodes.ENTITY_WOULD_DIE));
            }
        }
        if(ev.eventName == FEventCodes.ENTITY_RECEIVE_MELEE_ATTACK){
            FEvent nameEvent = parentEntity.PropagateEvent(new FEvent(FEventCodes.GET_ENTITY_NAME,"name", "????"));
            string ourName = (string)nameEvent.Get("fullstring");
            int dmg = (int)ev.Get("damage");

            this.hp -= dmg;
            MessageLog.instance.Log(ourName+" takes "+dmg+" damage.");

            if(hp < 0){
                parentEntity.PropagateEvent(new FEvent(FEventCodes.ENTITY_WOULD_DIE));
            }
        }
        if(ev.eventName == FEventCodes.ENTITY_RECEIVE_THROWN_ATTACK){
            FEvent nameEvent = parentEntity.PropagateEvent(new FEvent(FEventCodes.GET_ENTITY_NAME,"name", "????"));
            string ourName = (string)nameEvent.Get("fullstring");
            int dmg = (int)ev.Get("damage");

            this.hp -= dmg;
            MessageLog.instance.Log(ourName+" takes "+dmg+" damage.");

            if(hp < 0){
                parentEntity.PropagateEvent(new FEvent(FEventCodes.ENTITY_WOULD_DIE));
            }

            //Because this is a thrown item, we absorbed the damage
            ev.Set("damage", 0);
        }
        if(ev.eventName == FEventCodes.ENTITY_RECEIVE_MAGICAL_ATTACK){
            FEvent nameEvent = parentEntity.PropagateEvent(new FEvent(FEventCodes.GET_ENTITY_NAME,"name", "????"));
            string ourName = (string)nameEvent.Get("fullstring");
            int dmg = (int)ev.Get("damage");

            this.hp -= dmg;
            MessageLog.instance.Log(ourName+" takes "+dmg+" damage.");

            if(hp < 0){
                parentEntity.PropagateEvent(new FEvent(FEventCodes.ENTITY_WOULD_DIE));
            }
        }
        if(ev.eventName == FEventCodes.ENTITY_WOULD_DIE){
            FEvent nameEvent = parentEntity.PropagateEvent(new FEvent(FEventCodes.GET_ENTITY_NAME,"name", "????"));
            string ourName = (string)nameEvent.Get("fullstring");
            //Print a message to the UI
            MessageLog.instance.Log(ourName+" dies!");

            parentEntity.PropagateEvent(new FEvent(FEventCodes.ENTITY_DIED));
        }
        return ev;
    }

}
