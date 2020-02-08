using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementalResistance : FComponent
{

    public Stat.ELEMENT eLEMENT;
    public float modifier;
    
    public override void SetData(params object[] data){
        eLEMENT = (Stat.ELEMENT) data[0];
        modifier = (float) data[1];
    }

    public override FEvent PropagateEvent(FEvent ev){
        if(ev.eventName == FEventCodes.ENTITY_RECEIVE_MAGICAL_ATTACK){
            if(ev.data.ContainsKey("type")){
                if((Stat.ELEMENT)ev.Get("type") == eLEMENT){
                    int dmg = (int) ev.Get("damage");
                    dmg = (int)Mathf.Round((float)dmg * modifier);
                    ev.Set("damage", dmg);
                }
            }
        }
        return ev;
    }

}
