using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FComponent : MonoBehaviour
{

   public bool componentEnabled = true;
   public void Disable(){
      componentEnabled = false;
   }
   public void Enable(){
      componentEnabled = true;
   }

   public int priority = 100;

   [HideInInspector]
   public bool setup = false;

   [HideInInspector]
   public FEntity parentEntity;

   public string You(string message){
      FEvent name = parentEntity.PropagateEvent(new FEvent(FEventCodes.GET_ENTITY_NAME));
      return (name.Get("fullstring") as string)+" "+message;
   }

   public void Setup(){
      _Setup();
      setup = true;
   }

   virtual public void SetData(params object[] data){
   }

   protected virtual void _Setup(){

   }

   public virtual FEvent PropagateEvent(FEvent ev){
      return ev;
   }
}
