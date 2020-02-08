using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FEntity : MonoBehaviour
{

    public List<FComponent> components;

    public virtual void Setup(){
        components = new List<FComponent>();
        ScanForComponents();
    }

    public void ScanForComponents(){
        foreach(FComponent comp in GetComponents(typeof(FComponent))){
            if(!comp.setup){
                AddComponent(comp);
            }
        }
    }

    public bool HasComponent(System.Type type){
        return GetComponent(type) != null;
    }

    public void AddComponent(string name, params object[] data){
        if(HasComponent(System.Type.GetType(name))){
            return;
        }
        FComponent fc = (FComponent) gameObject.AddComponent(System.Type.GetType(name));
        AddComponent(fc);
        if(data.Length > 0)
            fc.SetData(data);
    }

    public void AddComponent(FComponent component){
        this.components.Add(component);
        component.parentEntity = this;
        component.Setup();
        components.Sort(delegate(FComponent f1, FComponent f2){
            return f1.priority.CompareTo(f2.priority);
        });
        // Debug.Log(components[0].priority);
    }

    public FEvent PropagateEvent(FEvent ev){
        for(int i=0; i<components.Count; i++){
            FComponent comp = components[i];
            if(comp.componentEnabled)
                ev = comp.PropagateEvent(ev);
        }

        return ev;
    }
    
}
