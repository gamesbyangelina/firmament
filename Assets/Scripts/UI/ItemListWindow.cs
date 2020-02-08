using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ItemListWindow : MonoBehaviour
{

    public static ItemListWindow instance;
    void Awake(){
        ItemListWindow.instance = this;
    }

    public TMPro.TextMeshProUGUI title;
    public TMPro.TextMeshProUGUI controls;
    public RectTransform listParent;
    public RectTransform windowParent;
    public TMPro.TextMeshProUGUI itemPrefab;

    bool open;
    List<FEntity> entityList;
    List<TMPro.TextMeshProUGUI> texts;

    public delegate void Callback(FEntity selected); 

    public void OpenWindow(string titletxt, string controlstxt, List<FEntity> entities){
        title.text = titletxt;
        controls.text = controlstxt;
        RefreshWindow(entities);
        windowParent.DOAnchorPosY(0, 0.25f);        
    }

    public void RefreshWindow(List<FEntity> entities){
        int num = 1;
        entityList = entities;

        if(texts != null){
            for(int i=0; i<texts.Count; i++){
                Destroy(texts[i].gameObject);
            }
        }

        texts = new List<TMPro.TextMeshProUGUI>();
        
        foreach(FEntity entity in entities){
            TMPro.TextMeshProUGUI textEntry = Instantiate(itemPrefab);
            texts.Add(textEntry);
            FEvent name = entity.PropagateEvent(new FEvent(FEventCodes.GET_ENTITY_NAME));
            textEntry.text = num+". "+name.Get("fullstring") as string;
            textEntry.rectTransform.parent = listParent;
            textEntry.color = normalColor;
            num++;
        }
    }

    public Color selectedColor;
    public Color normalColor;

    int selected = -1;
    public void SelectItem(int item, string suffix=""){
        if(selected > -1){
            texts[selected].color = normalColor;
            FEvent name = entityList[selected].PropagateEvent(new FEvent(FEventCodes.GET_ENTITY_NAME));
            texts[selected].text = (selected+1)+". "+name.Get("fullstring") as string;
        }
        selected = item;
        texts[selected].color = selectedColor;
        texts[selected].text = texts[selected].text + suffix;
    }

    public void CloseWindow(){
        windowParent.DOAnchorPosY(700, 0.25f);  
        selected = -1;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
