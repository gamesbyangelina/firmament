using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterComponent : FComponent
{

    public string deathSpriteName;
    public bool dead = false;

    public override FEvent PropagateEvent(FEvent ev){
        if(dead && ev.eventName == FEventCodes.GET_NAME_MODIFIERS){
            List<string> ss = (ev.Get("suffixes") as List<string>);
            ss.Add("(dead)");
            ev.Set("suffixes", ss);
        }

        if(dead) return ev;

        if(ev.eventName == FEventCodes.ENTITY_TRY_MOVE_INTO){
            FEntity mover = (FEntity) ev.Get("entity");

            //If the player tries to move into us, fight them
            if(mover.HasComponent(typeof(PlayerComponent))){
                //Request attack
                ev.Set("moveBlockedByEntity", true);
                ev.Set("moveCancelled", true);
                ev.Set("blockingEntity", this.parentEntity);
                return ev;
            }
            else{
                ev.Set("moveCancelled", true);
            }
        }
        else if(ev.eventName == FEventCodes.ENTITY_DIED){
            //Change the avatar
            dead = true;
            parentEntity.PropagateEvent(new FEvent(FEventCodes.CHANGE_SPRITE, "spriteName", deathSpriteName));
            parentEntity.PropagateEvent(new FEvent(FEventCodes.CHANGE_SPRITE_LAYER, "spriteLayer", Model.instance.backgroundLayerName));
        }

        return ev;
    }
}
