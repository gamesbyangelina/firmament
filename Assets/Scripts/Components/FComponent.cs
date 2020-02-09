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

   /*
      When we propagate messages to objects, components receive them according to their priority
      order. This is so we can do stuff like have components that 'interrupt' a message and change 
      it or stop it propagating altogether. 
   */
   public int priority = 100;

   [HideInInspector]
   public bool setup = false;

   [HideInInspector]
   public FEntity parentEntity;

   /*
      A neat thing I saw in Alexei Peper's Nethack talk, which is that Nethack's print command is
      called "You" so the code reads You("kill the beast"). I thought that was really cute.
   */
   public string You(string message){
      FEvent name = parentEntity.PropagateEvent(new FEvent(FEventCodes.GET_ENTITY_NAME));
      return (name.Get("fullstring") as string)+" "+message;
   }

   public void Setup(){
      _Setup();
      setup = true;
   }

   /*
      Rather than setting a load of fields, this lets us cram all the initialisation for the component
      into a single method. It's essentially like a constructor, but since FComponents are MonoBehaviours 
      they don't have constructors, so we do this instead. There's probably a nicer/fancier way to set 
      this up, but this is how my hacky thought process decided to do it here.
   */
   virtual public void SetData(params object[] data){
   }

   /*
      Override this if you want your component to do stuff when it's added to the object. Useful for
      initialising things that aren't given to you, setting up data structures, or propagating events
      based on your existence (for example, the creation of fire might trigger events in the instant it appears).
   */
   protected virtual void _Setup(){

   }

   public virtual FEvent PropagateEvent(FEvent ev){
      return ev;
   }
}
