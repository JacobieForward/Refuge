using UnityEngine;
using System.Collections;


[System.Serializable]
public class BasePlayer{
	// Making this class serializeable allows ediiting of the following variables
	// In the unity scene editor, which makes creating prefabs much more easy

	// Player name
	public string Name = "Player";
	public int level;

	public Item weapon;
	public Item armor;

	// Player Stats
	// These cannot ever go below 1
	public int Strength = 1;
	public int Awareness = 1;
	public int Knowledge = 1;
	public int Hardiness = 1;
	public int Dexterity = 1;

	// Player health
	// Set to its appropriate value on character creation
	public float maxhealth = 1;
	public float currenthealth = 1;
	public float stamina = 10;

	// Speed for attack animations
	public float AnimationAttackSpeed = 1f;
}
