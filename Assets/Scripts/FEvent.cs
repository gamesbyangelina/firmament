using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FEvent 
{
    public string eventName;
    public Dictionary<string, object> data;
    
    public FEvent(string ename){
        this.eventName = ename;
        data = new Dictionary<string, object>();
    }

    public FEvent(string ename, params object[] d){
        this.eventName = ename;
        this.data = new Dictionary<string, object>();
        for(int i=0; i<d.Length; i+=2){
            this.data.Add((string)d[i], d[i+1]);
        }
    }

    public void Add(string key, object value){
        data.Add(key, value);
    }

    public object Get(string key){
        if(data.ContainsKey(key)){
            return data[key];
        }
        return null;
    }

    public void Set(string key, object value){
        if(data.ContainsKey(key))
            data[key] = value;
        else
            Add(key, value);
    }

}
