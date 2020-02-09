using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LocationComponent : FComponent
{
    
    /*
        One of the most fundamental components that almost everything has.

        This is one of the components that actually breaks containment quite a bit and reaches out to
        e.g. initiate tweening, or adjust positions. In an ideal world, we'd not do this, and instead
        we'd have a separate view/rendering step that updates the screen at the end of each turn. If
        something moved, it can do the tween then.

        I didn't do that here for a couple of reasons - one, it felt like overkill for this simple example,
        and two, the reliance on Monobehaviour objects containing both the sprite view stuff *and* the component
        logic stuff, meant that it was already kinda coupled together.
    */

    public int x;
    public int y;

    public bool carried;
    public FEntity carrier;

    public override void SetData(params object[] data){
        this.x = (int)data[0];
        this.y = (int)data[1];
    }

    protected override void _Setup(){
        priority = 99;
    }

    public override FEvent PropagateEvent(FEvent ev)
    {
        if(ev.eventName == FEventCodes.PICKED_UP){
            //Shadow the location component of the picker-upper
            carrier = ev.Get("carrier") as FEntity;
            carried = true;
            GameMap.instance.RemoveEntityAt(parentEntity);
        }

        if(ev.eventName == FEventCodes.THROWN){
            carried = false;
            carrier = null;
            this.x = (int) ev.data["x"];
            this.y = (int) ev.data["y"];

            transform.position = GameMap.instance.GetMapLocation(x, y);
            GameMap.instance.PutEntityAt(parentEntity, x, y);

            parentEntity.PropagateEvent(new FEvent(FEventCodes.POSITION_CHANGED, "x", x, "y", y));
        }
        if(ev.eventName == FEventCodes.DROP_ITEM_HERE){
            carried = false;
            carrier = null;
            this.x = (int) ev.data["x"];
            this.y = (int) ev.data["y"];

            Debug.Log("Dropping");

            transform.position = GameMap.instance.GetMapLocation(x, y);
            GameMap.instance.PutEntityAt(parentEntity, x, y);

            parentEntity.PropagateEvent(new FEvent(FEventCodes.POSITION_CHANGED, "x", x, "y", y));
        }

        if(ev.eventName == FEventCodes.GET_LOCATION){
            if(!carried){
                ev.Set("x", this.x);
                ev.Set("y", this.y);
            }
            else{
                Debug.Assert(carrier != parentEntity);
                FEvent subloc = carrier.PropagateEvent(new FEvent(FEventCodes.GET_LOCATION, "x", -1, "y", -1));
                ev.Set("x", (int)subloc.Get("x"));
                ev.Set("y", (int)subloc.Get("y"));
            }
        }
        if(ev.eventName == FEventCodes.ENTITY_SPAWN){
            this.x = (int) ev.data["x"];
            this.y = (int) ev.data["y"];

            transform.position = GameMap.instance.GetMapLocation(x, y);

            parentEntity.PropagateEvent(new FEvent(FEventCodes.POSITION_CHANGED, "x", x, "y", y));
        }
        else if(ev.eventName == FEventCodes.ENTITY_MOVE && !carried){
            int nx = (int) ev.data["x"];
            int ny = (int) ev.data["y"];

            transform.DOMove(GameMap.instance.GetMapLocation(nx, ny),  0.25f);
            GameMap.instance.ChangeMapLocation(parentEntity, nx, ny);

            this.x = nx;
            this.y = ny;

            parentEntity.PropagateEvent(new FEvent(FEventCodes.POSITION_CHANGED, "x", x, "y", y));
        }

        return ev;
    }

}
