using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AllyState : MonoBehaviour {

	// NOTE: It is a little unwieldly to have to have seperate scripts for players and allies
	// This is needed for several factors, such as triggers effecting only the player character's state
	// (i.e. deathtext) and the fact that player character stats have to be located in the GameManager
	// To be accessed in exploration as well as combat

	// NOTE: Player input and GUI are stored in the battle state machine as opposed to PlayerState

	// Despite being named BasePlayer this script tracks the stats for this ally
	// Player stats are held and tracked in the GameManager script which can be accessed with GameManager.instance
	public BasePlayer ally;

	private BattleStateMachine battlestatemachine; // Need a reference to the battlestate machine
	// public BasePlayer player; // A reference to the player object this state machine affects
	public Turnstate currentstate;
	private float currentcooldown = 1f; // Starts the battle at cooldown 0
	private float maximumcooldown = 5f; // The number that means the cooldown bar is full

	// IEnumerator things
	public GameObject EnemyTarget;
	private bool ActionStarted = false;
	private Vector3 startposition;

	// Reference the hitpoint counter
	public Text currenthptext;

	public enum Turnstate {
		START,
		BARWAITING, // The state for when the player's cooldown bar is filling up
		ADDTOLIST, // Adds this player to a list of players ready to act
		WAITING, // idle to wait for inputs
		ACTION,
		DEAD
	}


	// Use this for initialization
	void Start () {
		// Need access to the battle state machine script
		battlestatemachine = GameObject.Find ("BattleManager").GetComponent<BattleStateMachine>();

		// Reference to the player hp counter
		currenthptext = GameObject.Find("AllyCurrentHPText").GetComponent<Text>();

		// The current state begins at start
		currentstate = Turnstate.START;

		startposition = transform.position;	
	}

	// Update is called once per frame
	void Update () {
		// Have to call this every frame or the health might not update as it should
		UpdateAllyHealth();
		switch (currentstate) {
		case (Turnstate.START):
			// Initialize combat
			// Keeping this here for possible future initilization methods
			maximumcooldown = 5.25f - (((float)ally.Dexterity - 4) * .25f);
			currentstate = Turnstate.BARWAITING;
			break;
		case (Turnstate.BARWAITING):
			IncrementProgressBar ();
			break;
		case (Turnstate.ADDTOLIST):
			battlestatemachine.PlayersToManage.Add (this.gameObject);
			currentstate = Turnstate.WAITING;
			break;
		case (Turnstate.WAITING):
			// idle idle idle idle
			break;
		case (Turnstate.ACTION):
			StartCoroutine (TimeForAction());
			break;
		case (Turnstate.DEAD):

			break;
		}
	}

	void IncrementProgressBar() {
		// Note: Time.deltatime average is roughly about 0.0165 per tick, and Time.delta time averages to a second every second of real time
		// 5.25 - ((dex - 4)*.25)
		currentcooldown = currentcooldown + Time.deltaTime; // Increases the cooldown bar by the time that has passed
		float calculatedcooldown = currentcooldown / maximumcooldown;
		battlestatemachine.Ally1ProgressBar.transform.localScale = new Vector2(Mathf.Clamp(calculatedcooldown, 0, 1), battlestatemachine.Ally1ProgressBar.transform.localScale.y);
		// The line above updates the progress bar on screen
		// When the x scale on the cooldown bar is 0 it is empty when it is 1 it is full thus the clamp between 0 and 1
		if (currentcooldown >= maximumcooldown) {
			currentstate = Turnstate.ADDTOLIST;
		}
	}

	private IEnumerator TimeForAction() {
		if (ActionStarted) {
			yield break; // If action is started break out of the IEnumerator
		}

		ActionStarted = true;

		// Animate the enemy for the chosen attack (move it forward a little towards the target)
		Vector2 playerPosition = new Vector2(EnemyTarget.transform.position.x, EnemyTarget.transform.position.y - 1f);
		// Increase the y position so that the attacker does not get right on top of the player
		while (MoveToEnemy(playerPosition)){
			yield return null;
		}

		// Wait a little bit
		yield return new WaitForSeconds(ally.AnimationAttackSpeed);
		// Check for hit and do damage
		BaseEnemy enemy = EnemyTarget.GetComponent<EnemyState>().enemy;

		bool hit;
		hit = battlestatemachine.checkForHit (ally.Dexterity, enemy.Dexterity, 0);

		int damageresistance = 0;
		if (ally.weapon.damagetype == Item.DamageTypes.BLUNT) {
			damageresistance = enemy.armor.armorBonusBlunt;
		}
		if (ally.weapon.damagetype == Item.DamageTypes.SLASH) {
			damageresistance = enemy.armor.armorBonusSlash;
		}
		if (ally.weapon.damagetype == Item.DamageTypes.PIERCE) {
			damageresistance = enemy.armor.armorBonusPierce;
		}

		int damage =  battlestatemachine.calculateDamage (ally.weapon.minDamage, ally.weapon.maxDamage, ally.Strength, enemy.Hardiness, damageresistance);

		if (hit) {
			// A hit! Deduct damage from the enemy health
			// But first check for the type of attack
			/*if (battlestatemachine.CombatActionList[0].attacktype == "basic") {
				Debug.Log ("Player Used Basic Attack");
				enemy.CurrentHealth -= damage;
				battlestatemachine.CreatePopupText (damage.ToString (), EnemyTarget.transform);
			}*/

			Debug.Log ("Player Used Ability: " + battlestatemachine.CombatActionList [0].abilitytype.abilityname);
			enemy.CurrentHealth -= FindAbility (battlestatemachine.CombatActionList [0].abilitytype.abilityname, damage);
			battlestatemachine.CreatePopupText (FindAbility (battlestatemachine.CombatActionList [0].abilitytype.abilityname, damage).ToString (), EnemyTarget.transform);

		} else {
			// Not a hit
			battlestatemachine.CreatePopupText ("Miss!", EnemyTarget.transform);
		}
		// move the enemy back to the start position
		// Must use Vector3 for MoveTo functions
		// must create a new Vector3 here instead of using startposition or problems may occur
		Vector3 firstposition = startposition;
		while (MoveToStart (firstposition)) {
			yield return null;
		}
		// Remove this combataction from the list so we do not do the action twice
		battlestatemachine.CombatActionList.RemoveAt(0);
		// reset battle state machine to wait

		battlestatemachine.currentstate = BattleStateMachine.BattleState.WAIT;

		// The action is over so actionstarted is set back to false in preperation for the next action
		ActionStarted = false;

		// Reset the enemy state
		currentcooldown = 0f;
		currentstate = Turnstate.BARWAITING;
	}

	// TODO: Simplify to one move function using stored start positions for the enemy and the player

	// If moveTo is not done with Vector3 an error occurs with no workaround
	// AnimationAttackSpeed also determines the wait between attacks
	private bool MoveToEnemy(Vector3 target) {
		return target != (transform.position = Vector3.MoveTowards(transform.position, target, (ally.AnimationAttackSpeed * 5) * Time.deltaTime));
	}

	private bool MoveToStart(Vector3 target) {
		return target != (transform.position = Vector3.MoveTowards(transform.position, target, (ally.AnimationAttackSpeed * 5) * Time.deltaTime));
	}

	void UpdateAllyHealth() {
		currenthptext.text = ally.currenthealth.ToString();
		if (ally.currenthealth <= 0) {
			Time.timeScale = 0;
			Destroy (gameObject);
		}
	}

	int FindAbility(string ability, int damage) {
		if (ability == "Double Damage") {
			return damage * 2;
		}
		else {
			return damage;
		}
	}
}