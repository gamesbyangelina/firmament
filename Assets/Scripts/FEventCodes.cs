using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FEventCodes 
{

    public static string GET_ALL_PICKABLES = "GET_ALL_PICKABLES";
    public static string GET_WIELDED_LIST = "GET_WIELDED_LIST";
    public static string GET_VISIBLE_LIST = "GET_VISIBLE_LIST";
    public static string GET_CARRIED_LIST = "GET_CARRIED_LIST";
    //equipper receives this
    public static string EQUIP_ITEM = "EQUIP_ITEM";
    public static string UNEQUIP_ITEM = "UNEQUIP_ITEM";
    public static string LOSE_ITEM = "LOSE_ITEM";
    public static string DROP_ITEM_HERE = "DROP_ITEM_HERE";
    public static string CONSUME_ITEM = "CONSUME_ITEM";
    //item receives this
    public static string WAS_EQUIPPED = "WAS_EQUIPPED";
    public static string WAS_UNEQUIPPED = "WAS_UNEQUIPPED";

    public static string DISARM_WEAPON = "DISARM_WEAPON";
    public static string ENTITY_SPAWN = "ENTITY_SPAWN";
    public static string POSITION_CHANGED = "POSITION_CHANGED";
    public static string ENTITY_MELEE_ATTACK = "ENTITY_MELEE_ATTACK";
    public static string LOSE_HP = "LOSE_HP";
    public static string ENTITY_RECEIVE_MELEE_ATTACK = "ENTITY_RECEIVE_MELEE_ATTACK";
    public static string ENTITY_RECEIVE_THROWN_ATTACK = "ENTITY_RECEIVE_THROWN_ATTACK";
    public static string ENTITY_RECEIVE_MAGICAL_ATTACK = "ENTITY_RECEIVE_MAGICAL_ATTACK";
    public static string ENTITY_TRY_MOVE_INTO = "ENTITY_TRY_MOVE_INTO";
    public static string ENTITY_LEAVE_TILE = "ENTITY_LEAVE_TILE";
    public static string ENTITY_MOVE = "ENTITY_MOVE";
    public static string ENTITY_ENTER_TILE = "ENTITY_ENTER_TILE";
    public static string ENTITY_WOULD_DIE = "ENTITY_WOULD_DIE";
    public static string ENTITY_DIED = "ENTITY_DIED";
    public static string GET_ENTITY_NAME = "GET_ENTITY_NAME";
    public static string GET_NAME_MODIFIERS = "GET_NAME_MODIFIERS";
    public static string TAKE_TURN = "TAKE_TURN";
    public static string END_TURN = "END_TURN";
    public static string CHANGE_SPRITE = "CHANGE_SPRITE";
    public static string CHANGE_SPRITE_LAYER = "CHANGE_SPRITE_LAYER";
    public static string RECEIVE_ITEM = "RECEIVE_ITEM";
    public static string PICKED_UP = "PICKED_UP";
    public static string TRY_PICK_UP_OBJECT = "TRY_PICK_UP_OBJECT";
    public static string GET_ENTITY_AFFORDANCES = "GET_ENTITY_AFFORDANCES";
    public static string DRINK = "DRINK";
    public static string HEAL_HP = "HEAL_HP";
    public static string CONTAINER_BREAK = "CONTAINER_BREAK";
    public static string GET_LOCATION = "GET_LOCATION";
    public static string GAME_START = "GAME_START";
    public static string INFLICT_BLEEDING = "INFLICT_BLEEDING";
    public static string THROWN = "THROWN";

    public static string SET_FIRE_TO = "SET_FIRE_TO";
    public static string GET_ENVIRONMENT_STATUS = "GET_ENV_STATUS";
}
