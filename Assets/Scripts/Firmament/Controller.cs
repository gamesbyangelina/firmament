using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class Controller : MonoBehaviour {

    public static Controller instance;
    void Awake(){
        Controller.instance = this;
    }

	public Model model;

	[HideInInspector]
	public GameSpaceView view;

    public bool showingPopup = false;

    public bool skipIntro = false;

    public bool lit = false;

	void Start() {
        Sinput.buttonRepeatWait = 0f;
		Sinput.buttonRepeat = 0.25f;
    }

    public SpriteRenderer tileSelectionSprite;

    void FixedUpdate(){
        Vector3 lookAt = (Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }


    internal void TileHoverOut(int x, int y)
    {
        
    }

    internal void TileHoverIn(int x, int y)
    {   
        if(tileSelectionMode){
            tileSelectionSprite.transform.position = GameMap.instance.GetMapLocation(x, y);
        }
    }

    public void OnTileClicked(ClickableTile t){
        if(tileSelectionMode){
            tileSelectionCallback(t.x, t.y);
        }
        else{
            ((GambitModel)GambitModel.instance).TryMovePlayer(t.x, t.y);
        }
    }

    public bool SUSPEND_GAMEPLAY = false;
    public void Nothing(){}

	public void Update() {
        if(noInput)
            return;

        if(SUSPEND_GAMEPLAY){
            return;
        }

        int mix = 0; int miy = 0;
        if(Sinput.GetButtonDown("UL")){
            mix = -1; miy = 1;
        }
        if(Sinput.GetButtonDown("U")){
            miy = 1;
        }
        if(Sinput.GetButtonDown("UR")){
            mix = 1; miy = 1;
        }
        if(Sinput.GetButtonDown("L")){
            mix = -1;
        }
        if(Sinput.GetButtonDown("R")){
            mix = 1;
        }
        if(Sinput.GetButtonDown("DL")){
            mix = -1; miy = -1;
        }
        if(Sinput.GetButtonDown("D")){
            miy = -1;
        }
        if(Sinput.GetButtonDown("DR")){
            mix = 1; miy = -1;
        }
        if(mix != 0 || miy != 0){
            if(tileSelectionMode){
                tileSelectionSprite.transform.position += GameMap.instance.GetMapOffset(mix, miy);
            }
            else{
                ((GambitModel)GambitModel.instance).TryMovePlayerRelative(mix, miy);
            }
        }

        if(Sinput.GetButtonDown("PICKUP")){
            ((GambitModel)GambitModel.instance).PickUpItemHere();
            showingPickScreen = true;
        }
        if(Sinput.GetButtonDown("INVENTORY")){
            ((GambitModel)GambitModel.instance).ShowInventory();
            showingInventory = true;
        }
        for(int i=1; i<6; i++){
            if(Sinput.GetButtonDown(""+i)){
                if(showingPickScreen){
                    if(((GambitModel)GambitModel.instance).SelectPickup(i)){
                        showingPickScreen = false;
                    }
                }
                if(showingInventory){
                    ((GambitModel)GambitModel.instance).SelectInventoryItem(i);
                }
            }
        }
        if(Sinput.GetButtonDown("Cancel")){
            if(showingPickScreen){
                showingPickScreen = false;
            }
            if(showingInventory){
                showingInventory = false;
            }
            ItemListWindow.instance.CloseWindow();
        }

        if(showingInventory && Sinput.GetButtonDown("EQUIP")){
            ((GambitModel)GambitModel.instance).EquipSelected();
        }
        else if(showingInventory && Sinput.GetButtonDown("UNEQUIP")){
            ((GambitModel)GambitModel.instance).UnequipSelected();
        }
        else if(showingInventory && Sinput.GetButtonDown("DRINK")){
            ((GambitModel)GambitModel.instance).DrinkSelected();
        }
        else if(showingInventory && Sinput.GetButtonDown("THROW")){
            ((GambitModel)GambitModel.instance).ThrowSelected();
        }

        if(Sinput.GetButtonDownRepeating("Up")
            || Sinput.GetButtonDownRepeating("Down")
            || Sinput.GetButtonDownRepeating("Right")
            || Sinput.GetButtonDownRepeating("Left")){

            if(editMode){
                editMode = false;
            }
        }
    }

    public delegate void Callback(int x, int y);
    public Callback tileSelectionCallback;
    public void EnterTileSelection(Callback callback){
        tileSelectionMode = true;
        tileSelectionCallback = callback;
        tileSelectionSprite.transform.position = GameMap.instance.GetMapLocation(Model.instance.playerEntity);
    }
    public void ExitTileSelection(){
        tileSelectionMode = false;
        tileSelectionSprite.transform.position = new Vector3(-100,-100,0);
    }

    public bool showingPickScreen = false;
    public bool showingInventory = false;
    public bool tileSelectionMode = false;

    bool noInput = false;

    public void StopInput(){
        noInput = true;
    }

    public bool editMode = false;
}
