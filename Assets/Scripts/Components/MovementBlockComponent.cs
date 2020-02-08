using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementBlockComponent : FComponent
{


    public override FEvent PropagateEvent(FEvent ev){
        if(ev.eventName == FEventCodes.GAME_START){
            int[] loc = GameMap.instance.GetGridLocation(parentEntity);
            GameMap.instance.BlockTile(loc[0], loc[1]);
        }
        if(ev.eventName == FEventCodes.ENTITY_TRY_MOVE_INTO){
            ev.Set("moveCancelled", true);
            return ev;
        }
        return ev;
    }
}
