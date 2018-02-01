using UnityEngine;
using System.Collections;

[System.Serializable]
public class CombatAction {

	public string Attacker; // The name of the attacker
	public string type; // The type of attacker "Enemy" or "Player"
	public Ability abilitytype; // Denotes the type of attack 
	public GameObject AttackerObject; // Who attacks
	public GameObject AttackerTarget; // Who is being attacked

	public bool stunnedaction;
}
