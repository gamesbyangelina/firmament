using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAIComponent : FComponent
{
    
    public enum AI_GOAL {HUNT_PLAYER};

    public int sightRange = 5;

    public override FEvent PropagateEvent(FEvent ev){
        if(ev.eventName == FEventCodes.ENTITY_DIED){
            Disable();
            return ev;
        }
        if(ev.eventName == FEventCodes.TAKE_TURN){
            //Can we see the player?
            if(((GambitModel)GambitModel.instance).CanSeePlayer(parentEntity, sightRange)){
                //Are we adjacent to the player?
                int dist = GameMap.instance.GridDistance(parentEntity, Model.instance.playerEntity);
                // Model.instance.StraightLineDistanceBetween(parentEntity, Model.instance.playerEntity);
                int[] loc = GameMap.instance.GetGridLocation(parentEntity);
                // Debug.Log("Distance to player (from "+loc[0]+","+loc[1]+"): "+dist);
                if(dist < 2){
                    //If so, attack
                    FEvent meleeAttack = parentEntity.PropagateEvent(new FEvent(FEventCodes.ENTITY_MELEE_ATTACK, "target", Model.instance.playerEntity, "damage", 2));
                    // Debug.Log("Adjacent to");
                }
                else{
                    List<MapTile> path = GameMap.instance.PathToFrom(parentEntity, Model.instance.playerEntity);
                    path.RemoveAt(path.Count-1);
                    if(path.Count > 0){
                        FEvent moveEvent = new FEvent(FEventCodes.ENTITY_MOVE, "x", path[0].x, "y", path[0].y);
                        parentEntity.PropagateEvent(moveEvent);
                        // Debug.Log("found a path");
                    }
                    else{
                        //Do nothing? We can see but not reach them.
                        // Debug.Log("Found no path");
                    }
                }
            }
            
            

        }
        return ev;
    }

}
