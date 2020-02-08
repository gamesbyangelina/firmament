using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : MonoBehaviour
{
   public int x;
   public int y;
   public float estimateScoreToThere;
   public float scoreToHere;
   public MapTile pathHelper;
   public bool blocked = false;
   public bool flag = false;

   public SpriteRenderer coverRenderer;
   public SpriteRenderer spriteRenderer;
   
   public List<MapTile> nbs = new List<MapTile>();
   public void Show(){
      coverRenderer.color = new Color32(0,0,0,0);
   }

   public void Hide(){
      coverRenderer.color = new Color32(0,0,0,255);
   }

   public override bool Equals(object other){
      MapTile m = (MapTile)other;
      return m.x == this.x && m.y == this.y;
   }

}
