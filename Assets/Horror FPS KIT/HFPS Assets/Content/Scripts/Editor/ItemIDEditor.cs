using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemID)), CanEditMultipleObjects]
public class ItemIDEditor : Editor {

    public SerializedProperty
    ItemType_Prop,
    WeaponType_Prop,
    MessageType_Prop,
    Amount_Prop,
    WeaponID_Prop,
    InventoryID_Prop,
    BackpackExpand_Prop,
    Destroy_Prop,
    Disable_Prop,
    PickupSound_Prop,
    MessageText_prop;


    void OnEnable () {
		ItemType_Prop = serializedObject.FindProperty ("ItemType");
		WeaponType_Prop = serializedObject.FindProperty ("weaponType");
        MessageType_Prop = serializedObject.FindProperty("messageType");
        Amount_Prop = serializedObject.FindProperty("Amount");
		WeaponID_Prop = serializedObject.FindProperty ("WeaponID");
		InventoryID_Prop = serializedObject.FindProperty ("InventoryID");
		BackpackExpand_Prop = serializedObject.FindProperty ("BackpackExpand");    
		Destroy_Prop = serializedObject.FindProperty ("DestroyOnPickup");
        Disable_Prop = serializedObject.FindProperty("DisableOnPickup");
        PickupSound_Prop = serializedObject.FindProperty("PickupSound");
        MessageText_prop = serializedObject.FindProperty("message");
    }

	public override void OnInspectorGUI()
	{
		serializedObject.Update ();
        ItemID.Type type = (ItemID.Type)ItemType_Prop.enumValueIndex;
        ItemID.MessageType msg = (ItemID.MessageType)MessageType_Prop.enumValueIndex;

        EditorGUILayout.PropertyField(ItemType_Prop);
        EditorGUILayout.PropertyField(MessageType_Prop);

        switch (msg)
        {
            case ItemID.MessageType.Hint:
                EditorGUILayout.PropertyField(MessageText_prop, new GUIContent("Hint Message:"));
                break;
            case ItemID.MessageType.Message:
                EditorGUILayout.PropertyField(MessageText_prop, new GUIContent("Notification Message:"));
                break;
            case ItemID.MessageType.ItemName:
                EditorGUILayout.PropertyField(MessageText_prop, new GUIContent("Notification Item:"));
                break;
        }

        EditorGUILayout.PropertyField( Destroy_Prop, new GUIContent("Destroy On Pickup") );
        EditorGUILayout.PropertyField( Disable_Prop, new GUIContent("Disable On Pickup"));
        EditorGUILayout.PropertyField( PickupSound_Prop, new GUIContent("Pickup Sound:"));

        switch (type)
        {
            case ItemID.Type.NoInventoryItem:
                EditorGUILayout.PropertyField(WeaponID_Prop, new GUIContent("SwitcherID:"));
                break;

            case ItemID.Type.InventoryItem:
                EditorGUILayout.PropertyField(Amount_Prop, new GUIContent("Amount:"));
                EditorGUILayout.PropertyField(InventoryID_Prop, new GUIContent("InventoryID:"));
                break;

            case ItemID.Type.WeaponItem:
                EditorGUILayout.PropertyField(WeaponType_Prop);
                ItemID.WeaponType weapType = (ItemID.WeaponType)ItemType_Prop.enumValueIndex;

                EditorGUILayout.PropertyField(Amount_Prop, new GUIContent("Amount:"));
                EditorGUILayout.PropertyField(WeaponID_Prop, new GUIContent("WeaponID:"));
                EditorGUILayout.PropertyField(InventoryID_Prop, new GUIContent("InventoryID:"));
                break;

            case ItemID.Type.BackpackExpand:
                EditorGUILayout.PropertyField(BackpackExpand_Prop, new GUIContent("Expand Amount:"));
                break;
        }

		serializedObject.ApplyModifiedProperties ();
	}
}
