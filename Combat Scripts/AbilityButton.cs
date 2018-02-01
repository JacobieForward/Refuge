using UnityEngine;
using System.Collections;

public class AbilityButton : MonoBehaviour {

	public Ability ability;
	BattleStateMachine battlestatemachine;
	PlayerState player;

	void Awake() {
		battlestatemachine = GameObject.Find ("BattleManager").GetComponent<BattleStateMachine> ();
		player = GameObject.Find ("CombatPlayer").GetComponent<PlayerState> ();
	}

	public void SelectAbility() {
		// Needs a reference to the battle state machine, which saves input from the enemy prefab
		// GameObject.Find ("BattleManager").GetComponent<BattleStateMachine>().EnemySelection(EnemyPrefab);
		/*if (ability == "Headcrack") {

			if (GameManager.instance.stamina < 2) {
				Debug.Log ("Not Enough Stamina!");
				return;
			}
			GameManager.instance.stamina -= 2;

			battlestatemachine.AbilitySelection(ability);
		}
		if (ability == "Focus") {
			battlestatemachine.AbilitySelection(ability);
		}*/
		if (ability.abilityname == "Focus" && battlestatemachine.focused) {
			// TODO: Popup telling the player they can only focus once
		} else {

			battlestatemachine.AbilitySelection (ability);
		}
	}
}