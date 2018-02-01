using UnityEngine;
using System.Collections;


[System.Serializable]
public class BaseEnemy{
	// While it might be possible to have enemies use the BasePlayer.cs class
	// It is often better to seperate these things in case future changes require differences
	// Between the two.

	// TODO: list of enemy types

	// Enemy name THIS MUST BE THE SAME AS THE GAMEOBJECT NAME IN THE UNITY EDITOR OR THERE WILL BE ERRORS
	public string Name;
	public int level;

	public Item weapon;
	public Item armor;

	// Enemy Stats
	// These cannot ever go below 1
	public int Strength = 1;
	public int Hardiness = 1;
	public int Dexterity = 1;

	// Enemy health
	// Set to its appropriate value on character creation
	public float MaxHealth = 1;
	public float CurrentHealth = 1;

	// AnimationAttackSpeed is used for animations and has no effect on player statistics
	// Defaults at 1f
	public float AnimationAttackSpeed = 1f;
}
