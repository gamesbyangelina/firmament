using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySlot
{
    
    //The thing we've equipped
    public FEntity slot;
    
    //e.g. "Left Hand"
    public string slotName;

    //e.g. Clothing
    public List<System.Type> requiredComponents;

    public EntitySlot(string slotName){
        this.slotName = slotName;
        requiredComponents = new List<System.Type>();
        this.slot = null;
    }

}
