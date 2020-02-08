using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderComponent : FComponent
{
    SpriteRenderer spriteRenderer;

    public override void SetData(params object[] data){
        SetSprite(data[0] as string);
        SetColor((Color32) data[1]);
        if(data.Length > 2){
            spriteRenderer.sortingOrder = (int)data[2];
        }
    }

    protected override void _Setup(){
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if(spriteRenderer == null){
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.sortingLayerName = Model.instance.mainLayerName;
        spriteRenderer.sortingOrder = 10;
    }

    public void SetSprite(Sprite sprite){
        spriteRenderer.sprite = sprite;       
    }

    public void SetSprite(string spriteName){
        spriteRenderer.sprite = SpriteManager.instance.GetSpriteByName(spriteName);
    }

    public void SetSpriteLayer(string layer, int order=0){
        spriteRenderer.sortingLayerName = layer;
        spriteRenderer.sortingOrder = order;
    }

    public void SetColor(Color color){
        spriteRenderer.color = color;
    }

    public override FEvent PropagateEvent(FEvent ev){
        if(ev.eventName == FEventCodes.PICKED_UP){
            spriteRenderer.enabled = false;
        }
        if(ev.eventName == FEventCodes.THROWN || ev.eventName == FEventCodes.DROP_ITEM_HERE){
            spriteRenderer.enabled = true;
        }
        if(ev.eventName ==  FEventCodes.CHANGE_SPRITE){
            SetSprite((string)ev.Get("spriteName"));
        }
        if(ev.eventName == FEventCodes.CHANGE_SPRITE_LAYER){
            SetSpriteLayer((string)ev.Get("spriteLayer"));
        }
        return ev;
    }

    
}
