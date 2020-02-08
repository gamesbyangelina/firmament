using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameComponent : FComponent
{
   
    public string _name;
    public string hexColor;



    override public void SetData(params object[] data){
        _name = (string)data[0];
        hexColor = (string)data[1];
    }

    public override FEvent PropagateEvent(FEvent ev){
        if(ev.eventName == FEventCodes.GET_ENTITY_NAME){
            FEvent mods = parentEntity.PropagateEvent(new FEvent(FEventCodes.GET_NAME_MODIFIERS, "prefixes", new List<string>(), "suffixes", new List<string>()));
            string pref = "";
            foreach(string s in mods.Get("prefixes") as List<string>){
                pref += s+" ";
            }
            string suff = "";
            foreach(string s in mods.Get("suffixes") as List<string>){
                suff += s+" ";
            }
            ev.Set("name", pref+_name+suff);
            ev.Set("namecolor", hexColor);
            ev.Set("fullstring", pref+"<#"+hexColor+">"+_name+"<#ffffff>"+suff);
        }
        if(ev.eventName == FEventCodes.GET_VISIBLE_LIST){
            FEvent mods = parentEntity.PropagateEvent(new FEvent(FEventCodes.GET_NAME_MODIFIERS, "prefixes", new List<string>(), "suffixes", new List<string>()));
            string pref = "";
            foreach(string s in mods.Get("prefixes") as List<string>){
                pref += s+" ";
            }
            string suff = "";
            foreach(string s in mods.Get("suffixes") as List<string>){
                suff += s+" ";
            }
            if(suff.Length > 0)
                suff = " "+suff;
            List<string> list = ev.Get("list") as List<string>;
            list.Add(pref+"<#"+hexColor+">"+_name+"<#ffffff>"+suff);
            ev.Set("list", list);
        }
        return ev;
    }


}
