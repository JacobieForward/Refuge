using UnityEngine;
using System.Collections;

public class EquipmentButton : MonoBehaviour {

	public Item item;
	public ExplorationManager explorationmanager;

	void Awake() {
		explorationmanager = GameObject.Find ("ExplorationManager").GetComponent<ExplorationManager>();
	}

	public void SelectEquipment() {
		// Needs a reference to the battle state machine, which saves input from the enemy prefab
		// GameObject.Find ("BattleManager").GetComponent<BattleStateMachine>().EnemySelection(EnemyPrefab);

		// Display equipment text
		explorationmanager.EquipmentText.text = item.description;
		explorationmanager.SelectedItem = item;
	}
}