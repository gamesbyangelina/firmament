using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryComponent : FComponent
{

    public List<FEntity> contents;
    public bool concealed = false;

    protected override void _Setup(){
        contents = new List<FEntity>();
    }
    
    public override FEvent PropagateEvent(FEvent ev){
        
        if(ev.eventName == FEventCodes.TRY_PICK_UP_OBJECT){
            FEntity entity = ev.Get("object") as FEntity;
            if(contents.Contains(entity)){
                contents.Remove(entity);
            }
        }
        if(ev.eventName == FEventCodes.LOSE_ITEM || ev.eventName == FEventCodes.CONSUME_ITEM || ev.eventName == FEventCodes.EQUIP_ITEM){
            FEntity entity = ev.Get("item") as FEntity;
            if(contents.Contains(entity)){
                contents.Remove(entity);
            }
        }
        if(ev.eventName == FEventCodes.RECEIVE_ITEM || ev.eventName == FEventCodes.UNEQUIP_ITEM){
            string name = (ev.Get("item") as FEntity).PropagateEvent(new FEvent(FEventCodes.GET_ENTITY_NAME)).Get("fullstring") as string;
            // Debug.Log("Added item to my inventory: "+name);
            FEntity entity = (FEntity) ev.Get("item");
            contents.Add(entity);
            return ev;
        }
        else if(ev.eventName == FEventCodes.GET_VISIBLE_LIST || ev.eventName == FEventCodes.GET_ALL_PICKABLES){
            //nothing to do here because we probably can't see our contents
            //q: is this a container's property? i'm not really sure.
            if(concealed)
                return ev;
            else{
                foreach(FEntity ent in contents){
                    ev = ent.PropagateEvent(ev);
                }
            }
        }
        else if(ev.eventName == FEventCodes.GET_CARRIED_LIST){
            List<FEntity> list = ev.Get("list") as List<FEntity>;
            foreach(FEntity en in contents){
                list.Add(en);
            }
            ev.Set("list", list);
        }
        else{
            foreach(FEntity fe in contents){
                //Hmm.
            }
        }

        return ev;
    }

}
