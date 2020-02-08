using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipSlotsComponent : FComponent
{
    
    Dictionary<string, EntitySlot> slots;

    protected override void _Setup(){
        slots = new Dictionary<string, EntitySlot>();
    }

    public void AddSlot(string slotName, params System.Type[] requirements){
        slots.Add(slotName, new EntitySlot(slotName));
    }

    bool EquipInSlot(string slotName, FEntity entity){
        // Debug.LogWarning("Warning: we aren't checking requirements");

        //no slot of that name
        if(!slots.ContainsKey(slotName)){
            return false;
        }
        //slot already has something in it
        if(slots[slotName].slot != null){
            return false;
        }
        //already equipped elsewhere
        // Debug.Log(entity.GetComponent<NameComponent>()._name);
        if(entity.GetComponent<Equippable>().equipped)
            return false;

        slots[slotName].slot = entity;

        return true;
    }

    public override FEvent PropagateEvent(FEvent ev){
        if(ev.eventName == FEventCodes.GET_NAME_MODIFIERS ||
        ev.eventName == FEventCodes.GET_LOCATION){
            return ev;
        }

        if(ev.eventName == FEventCodes.ENTITY_DIED){
            foreach(EntitySlot slot in slots.Values){
                if(slot.slot != null){
                    parentEntity.PropagateEvent(new FEvent(FEventCodes.UNEQUIP_ITEM, "item", slot.slot));
                }
            }
        }

        if(ev.eventName == FEventCodes.DISARM_WEAPON){
            List<FEntity> weapons = new List<FEntity>();
            //What can be disarmed?
            foreach(EntitySlot slot in slots.Values){
                if(slot.slot != null && slot.slot.HasComponent(typeof(MeleeWeaponComponent)))
                    weapons.Add(slot.slot);
            }

            if(weapons.Count > 0){
                FEntity disarmTarget = weapons[Random.Range(0, weapons.Count)];
                parentEntity.PropagateEvent(new FEvent(FEventCodes.UNEQUIP_ITEM, "item", disarmTarget));

                FEvent lev = parentEntity.PropagateEvent(new FEvent(FEventCodes.GET_LOCATION, "x", -1, "y", -1));
                parentEntity.PropagateEvent(new FEvent(FEventCodes.LOSE_ITEM, "item", disarmTarget, "x", (int)lev.Get("x"), "y", (int)lev.Get("y")));
                disarmTarget.PropagateEvent(new FEvent(FEventCodes.DROP_ITEM_HERE, "item", disarmTarget, "x", (int)lev.Get("x"), "y", (int)lev.Get("y")));

                FEvent name = disarmTarget.PropagateEvent(new FEvent(FEventCodes.GET_ENTITY_NAME));
                MessageLog.instance.Log(You("drop the "+name.Get("fullstring") as string));
            }

            return ev;
        }
        
        if(ev.eventName == FEventCodes.ENTITY_SPAWN || ev.eventName == FEventCodes.GET_ENTITY_NAME){
            //Ok so we clearly need better logic to determine what events propagate to children.
            return ev;
        }

        foreach(EntitySlot slot in slots.Values){
            if(slot.slot != null)
                ev = slot.slot.PropagateEvent(ev);
        }

        // if(ev.eventName == FEventCodes.TRY_PICK_UP_OBJECT){
        //     FEntity entity = ev.Get("object") as FEntity;
        //     foreach(EntitySlot slot in slots.Values){
        //         if(slot.slot != null && slot.slot == entity){
        //             slot.slot = null;
        //             entity.PropagateEvent(new FEvent(FEventCodes.WAS_UNEQUIPPED));
        //         }
        //     }
        // }

        if(ev.eventName == FEventCodes.EQUIP_ITEM){
            string sn = ev.Get("slotName") as string;
            if(sn == null){
                Debug.LogWarning("No slot provided for equip request");
                foreach(EntitySlot s in slots.Values){
                    if(s.slot == null){
                        sn = s.slotName;
                    }
                }
            }
            if(sn == null){
                return ev;
            }

            FEntity en = ev.Get("item") as FEntity;
            if(EquipInSlot(sn, en))
                en.PropagateEvent(new FEvent(FEventCodes.WAS_EQUIPPED));
        }
        if(ev.eventName == FEventCodes.UNEQUIP_ITEM){
            FEntity en = ev.Get("item") as FEntity;
            foreach(EntitySlot slot in slots.Values){
                if(slot.slot != null && slot.slot == en){
                    slot.slot = null;
                    en.PropagateEvent(new FEvent(FEventCodes.WAS_UNEQUIPPED));
                }
            }
        }
        //This is actually identical to unequipping it, but I duplicated it for clarity >_>
        if(ev.eventName == FEventCodes.LOSE_ITEM){
            FEntity en = ev.Get("item") as FEntity;
            foreach(EntitySlot slot in slots.Values){
                if(slot.slot != null && slot.slot == en){
                    slot.slot = null;
                    en.PropagateEvent(new FEvent(FEventCodes.WAS_UNEQUIPPED));
                }
            }
        }

        if(ev.eventName == FEventCodes.GET_WIELDED_LIST){
            List<FEntity> names = (List<FEntity>) ev.Get("list");
            foreach(EntitySlot slot in slots.Values){
                if(slot.slot == null) continue;
                names.Add(slot.slot);
            }
            ev.Set("list", names);
        }

        return ev;
    }

}
