using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageLog : MonoBehaviour
{

    List<string> history;

    public TMPro.TextMeshProUGUI log;

    string line1 = "";
    string line2 = "";
    string line3 = "";

    public bool verbose = false;

    public static MessageLog instance;
    void Awake(){
        MessageLog.instance = this;
        history = new List<string>();
    }    

    public void Log(string message){
        if(verbose) Debug.Log("Log: "+message);
        
        if(line1.Length > 0)
            history.Add(line1);
        line1 = line2;
        line2 = line3;
        line3 = message;

        log.text = "Turn "+turn+": "+line1+"\n"+line2+"\n"+line3;
    }

    public static int turn = 1;


}
