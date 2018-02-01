using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerState : MonoBehaviour {

	// NOTE: It is a little unwieldly to have to have seperate scripts for players and allies
	// This is needed for several factors, such as triggers effecting only the player character's state
	// (i.e. deathtext) and the fact that player character stats have to be located in the GameManager
	// To be accessed in exploration as well as combat

	// NOTE: Player input and GUI are stored in the battle state machine as opposed to PlayerState

	private BattleStateMachine battlestatemachine; // Need a reference to the battlestate machine
	// public BasePlayer player; // A reference to the player object this state machine affects
	public Turnstate currentstate;
	public float currentcooldown = 2.5f; // Starts the battle at half cooldown
	private float maximumcooldown = 3f; // The number that means the cooldown bar is full

	// public Image ProgressBar; // The progressbar for the player
	// The bar being registered has to be placed into the component in the unity editor

	// IEnumerator things
	public GameObject EnemyTarget;
	private bool ActionStarted = false;
	private Vector3 startposition;

	// Reference the player hitpoint counter
	public Text currenthptext;

	// Text that is Displayed on player death
	public GameObject DeathText;

	// Status effects
	public bool stunned = false;
	bool bleeding;


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
		// TODO: Assign all of these in the editor instead of this script
		// Need access to the battle state machine script
		battlestatemachine = GameObject.Find ("BattleManager").GetComponent<BattleStateMachine>();

		// Reference to the player hp counter
		currenthptext = GameObject.Find("CurrentHPText").GetComponent<Text>();

		// The current state begins at start
		currentstate = Turnstate.START;

		DeathText = GameObject.Find ("DeathText");

		DeathText.SetActive (false);

		startposition = transform.position;	

		// If the knife is equipped the cooldown is reduced to 2/3 its original length. Currentcooldown is also changed to reflect this
		if (GameManager.instance.weapon.name == "Knife") {
			maximumcooldown = maximumcooldown * (2f / 3f);
			currentcooldown = currentcooldown * (2f / 3f);
		}
	}

	// Update is called once per frame
	void Update () {
		// Have to call this every frame or the health might not update as it should
		UpdatePlayerHealth();
		switch (currentstate) {
		case (Turnstate.START):
			// Initialize combat
			// Keeping this here for possible future initilization methods
			maximumcooldown = 5.25f - (((float)GameManager.instance.Dexterity - 4) * .25f);
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
			
			if (battlestatemachine.CombatActionList [0].abilitytype.abilityname == "Focus") {
				battlestatemachine.CombatActionList.RemoveAt (0);
				currentcooldown = 0;
				battlestatemachine.currentstate = BattleStateMachine.BattleState.WAIT;
				currentstate = Turnstate.BARWAITING;
			} else {
				StartCoroutine (TimeForAction ());
			}
			break;
		case (Turnstate.DEAD):

			break;
		}
	}

	void IncrementProgressBar() {
		// Note: Time.deltatime average is roughly about 0.0165 per tick, and Time.delta time averages to a second every second of real time
		// 5.25 - ((dex - 4)*.25)
		currentcooldown = currentcooldown + (Time.deltaTime * 1.5f); // Increases the cooldown bar by the time that has passed
		float calculatedcooldown = currentcooldown / maximumcooldown;
		battlestatemachine.PlayerProgressBar.transform.localScale = new Vector2(Mathf.Clamp(calculatedcooldown, 0, 1), battlestatemachine.PlayerProgressBar.transform.localScale.y);
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
		yield return new WaitForSeconds(GameManager.instance.AnimationAttackSpeed);
		// Check for hit and then do damage
		BaseEnemy enemy = EnemyTarget.GetComponent<EnemyState>().enemy;

		bool hit;
		hit = battlestatemachine.checkForHit (GameManager.instance.Dexterity, enemy.Dexterity, battlestatemachine.CombatActionList [0].abilitytype.tohitmodifier);

		int damageresistance = 0;
		if (GameManager.instance.weapon.damagetype == Item.DamageTypes.BLUNT) {
			damageresistance = enemy.armor.armorBonusBlunt;
		}
		if (GameManager.instance.weapon.damagetype == Item.DamageTypes.SLASH) {
			damageresistance = enemy.armor.armorBonusSlash;
		}
		if (GameManager.instance.weapon.damagetype == Item.DamageTypes.PIERCE) {
			damageresistance = enemy.armor.armorBonusPierce;
		}

		int damage =  battlestatemachine.calculateDamage (GameManager.instance.weapon.minDamage, GameManager.instance.weapon.maxDamage, 
			GameManager.instance.Strength, enemy.Hardiness, damageresistance);

		if (hit) {
			// A hit! Deduct damage from the enemy health
			// But first check for the type of attack
			/*if (battlestatemachine.CombatActionList[0].attacktype == "basic") {
				Debug.Log ("Player Used Basic Attack");
				enemy.CurrentHealth -= damage;
				battlestatemachine.CreatePopupText (damage.ToString (), EnemyTarget.transform);
			}*/
			int abilitydamage = FindAbility (battlestatemachine.CombatActionList [0].abilitytype, damage);
			enemy.CurrentHealth -= abilitydamage;
			battlestatemachine.CreatePopupText (abilitydamage.ToString (), EnemyTarget.transform);

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
		return target != (transform.position = Vector3.MoveTowards(transform.position, target, (GameManager.instance.AnimationAttackSpeed * 5) * Time.deltaTime));
	}

	private bool MoveToStart(Vector3 target) {
		return target != (transform.position = Vector3.MoveTowards(transform.position, target, (GameManager.instance.AnimationAttackSpeed * 5) * Time.deltaTime));
	}

	void UpdatePlayerHealth() {
		currenthptext.text = GameManager.instance.currenthealth.ToString();
		if (GameManager.instance.currenthealth <= 0) {
			Time.timeScale = 0;
			DeathText.SetActive (true);
		}

		// TODO: Once saves are implemented, click to go back to last save or load menu can go here
		// Using DEAD state
	}

	int FindAbility(Ability ability, int damage) {
		if (ability.abilityname == "Headcrack") {
			// Stun target and increase damge by 1
			EnemyTarget.GetComponent<EnemyState>().stunned = true;
			return damage + 1;
		}
		if (ability.abilityname == "Lacerate") {
			// Bleed for three turns, regular damage
			EnemyTarget.GetComponent<EnemyState>().bleed = true;
			EnemyTarget.GetComponent<EnemyState>().bleedtimer = 3;
			return damage;
		}
		if (ability.abilityname == "Sneak Attack") {
			return damage / 2;
		}
		if (ability.abilityname == "Focus") {
			return 0;
		}
		else {
			return damage;
		}
	}
}