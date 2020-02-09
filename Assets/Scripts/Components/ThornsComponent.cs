using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThornsComponent : FComponent
{
    
    /*
        When I was a kid (ok still now) I was such a sucker for thorns abilities. There's a thorns aura in 
        Diablo 2 I think? I slammed so many points into that. I have no idea why I thought the centrepiece
        of my strategy should be being punched in the face.
    */

    public int thornsAmount = 1;

    public override FEvent PropagateEvent(FEvent ev){
        if(ev.eventName == FEventCodes.ENTITY_RECEIVE_MELEE_ATTACK){
            FEntity attacker = ev.Get("attacker") as FEntity;
            attacker.PropagateEvent(new FEvent(FEventCodes.ENTITY_RECEIVE_MAGICAL_ATTACK, "damage", thornsAmount, "attacker", parentEntity, "element", Stat.ELEMENT.NONE));
        }
        return ev;
    }

}
