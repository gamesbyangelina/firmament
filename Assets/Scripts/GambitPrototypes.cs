using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GambitPrototypes : MonoBehaviour
{
    public static GambitPrototypes instance;
    void Awake(){
        GambitPrototypes.instance = this;
    }
   
    /*
        Brian Bucklew's talk on ECS shows how objects are built from simple prototypes, and I've seen similar in
        Roguelike Celebration talks about ADOM and other RLs. My original idea was not to use Unity's built-in ECS,
        which would've let me use proper constructors and stuff, but since I use MonoBehaviours I can't really do
        that, so we have this somehwat clumsier approach to prototyping. 

        I imagine there's a nicer way of doing this - ScriptableObjects maybe, that MonoBehaviour-but-not class Unity
        has that I forget about all the time. This works fine here. If you wanted to make your thing bigger, you'd
        probably want a nicer way of defining these (one of the nice things about Brian's approach is that prototypes
        can be specified in XML and thus edited outside of code mode).
    */
    public FEntity SpawnPrototype(string name, int x, int y){
        FEntity res = null;

        GameObject go = new GameObject(name);
        go.tag = "Entity";
        res = go.AddComponent<FEntity>();
        res.Setup();

        if(name == "blood_spatter"){
            
            res.AddComponent("RenderComponent", "splatter_"+Random.Range(1,4), new Color32(126,37,83,255));
            res.GetComponent<RenderComponent>().SetSpriteLayer("Stage BG", 2);
            res.AddComponent("LocationComponent", x, y);
        }
        if(name == "fire"){
            res.AddComponent("RenderComponent", "fire", new Color32(255,163,0,255));
            res.GetComponent<RenderComponent>().SetSpriteLayer("Stage BG", 5);
            res.AddComponent("LocationComponent", x, y);
            res.AddComponent("FireComponent");
        }

        if(name == "grass"){
            res.AddComponent("RenderComponent", "grass"+Random.Range(1,4), new Color32(0,135,81,255));
            res.GetComponent<RenderComponent>().SetSpriteLayer("Stage BG", 0);
            res.AddComponent("LocationComponent", x, y);
            res.AddComponent("Flammable");
        }

        if(name == "armor"){
            res.AddComponent("NameComponent", "leather armor", "ffffff");
            res.AddComponent(res.gameObject.AddComponent<ArmorComponent>());
            res.AddComponent(res.gameObject.AddComponent<CanCarry>());
            res.AddComponent(res.gameObject.AddComponent<Equippable>());
            res.AddComponent("LocationComponent", x, y);
        }

        if(name == "sword"){
            res.AddComponent("NameComponent", "short sword", "ffffff");
            res.AddComponent(res.gameObject.AddComponent<MeleeWeaponComponent>());
            res.AddComponent(res.gameObject.AddComponent<CanCarry>());
            res.AddComponent(res.gameObject.AddComponent<Equippable>());
            res.AddComponent(res.gameObject.AddComponent<Throwable>());
            res.AddComponent("RenderComponent", "sword", new Color32(150,150,150,255));
            res.AddComponent("LocationComponent", x, y);
        }

        if(name == "healingpotion"){
            res.Setup();
            res.AddComponent("LocationComponent", x, y);
            res.AddComponent("NameComponent", "healing potion", "FF77A8");
            res.AddComponent("RenderComponent", "potion", new Color32(57, 87, 28, 255), 5);
            res.AddComponent("Throwable");
            res.AddComponent("CanCarry");
            res.AddComponent("LocationComponent");
            res.AddComponent("Drinkable");
            res.AddComponent("HealingPotionEffect");
            res.AddComponent("PotionComponent");
        }

        if(name == "skeleton_fighter"){
            res.name = "Skeleton";
            res.AddComponent("RenderComponent", "skeleton", (Color32) new Color(0.764f, 0.639f, 0.541f, 1f));
            res.AddComponent("LocationComponent", x, y);
            res.AddComponent(res.gameObject.AddComponent<MonsterComponent>());
            res.gameObject.GetComponent<MonsterComponent>().deathSpriteName = "pile_of_bones";
            res.AddComponent("NameComponent","skeleton warrior", "997577");
            res.AddComponent(res.gameObject.AddComponent<MeleeCombatComponent>());
            res.AddComponent(res.gameObject.AddComponent<MonsterAIComponent>());
            res.AddComponent("EquipSlotsComponent");
            res.AddComponent("InventoryComponent");
            res.AddComponent("StatBlockComponent");
            
            GameObject swordObj = new GameObject("Sword");
            swordObj.tag = "Entity";
            FEntity sword = swordObj.AddComponent<FEntity>();
            sword.Setup();
            sword.AddComponent("NameComponent", "short sword", "ffffff"); 
            sword.AddComponent("RenderComponent", "sword", new Color32(150,150,150,255));
            sword.AddComponent("LocationComponent");

            sword.AddComponent("CanCarry");
            sword.AddComponent("MeleeWeaponComponent");
            sword.AddComponent("RustyWeaponComponent");
            sword.AddComponent("Equippable");
            
            EquipSlotsComponent esc = res.GetComponent<EquipSlotsComponent>();
            esc.AddSlot("LeftHand");
            sword.PropagateEvent(new FEvent(FEventCodes.PICKED_UP, "carrier", res));
            res.PropagateEvent(new FEvent(FEventCodes.RECEIVE_ITEM, "item", sword));
            // res.PropagateEvent(new FEvent(FEventCodes.EQUIP_ITEM, "item", sword, "slotName", "LeftHand"));

        }

        return res;
    }

}
