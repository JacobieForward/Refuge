using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class EnemyState : MonoBehaviour {

	private BattleStateMachine battlestatemachine;
	public BaseEnemy enemy;

	public Turnstate currentstate;
	private float currentcooldown = 0f; // Starts the battle at cooldown 0
	private float maximumcooldown = 3f; // The number that means the cooldown bar is full

	private Vector2 startposition; //This is for animations

	private bool ActionStarted = false; // Used in TimeForAction 
	public GameObject PlayerTarget; // Stores the player being targeted for attacks

	public List<Ability> abilities = new List<Ability> ();

	private bool blocked = false;
	private bool oneblockattempt = false;

	// Status Effects
	public bool stunned = false;
	public bool bleed = false;
	public int bleedtimer = 0;

	public Image ProgressBar; // The progressbar for the enemy
	// The bar being registered has to be placed into the component in the unity editor

	public enum Turnstate {
		START,
		PROCESSING,
		CHOOSEACTION,
		WAITING,
		ACTION,
		ATTACKING,
		FINISHATTACK
	}

	// Use this for initialization
	void Start () {
		battlestatemachine = GameObject.Find ("BattleManager").GetComponent<BattleStateMachine>();
		startposition = transform.position;
		currentstate = Turnstate.START;
	}

	// Update is called once per frame
	void Update () {
		// First check for enemy death since this can happen during any enemy state
		UpdateEnemyHealth();
		switch (currentstate) {
		case (Turnstate.START):
			// Initialize combat
			// Keeping this here for possible future initilization methods
			maximumcooldown = 5.25f - (((float)enemy.Dexterity - 4) * .25f);
			currentstate = Turnstate.PROCESSING;
			break;
		case (Turnstate.PROCESSING):
			
			IncrementProgress ();
			break;
		case (Turnstate.CHOOSEACTION):
			ChooseAction ();
			currentstate = Turnstate.WAITING;
			break;
		case (Turnstate.WAITING):
			
			break;
		case (Turnstate.ACTION):
			StartCoroutine (TimeForAction ());
			if (Input.GetKeyDown ("space")) {
				oneblockattempt = true;
			}
			break;
		case (Turnstate.ATTACKING):
			
			// The player has a chance to block during the enemy's attack animation
			// If the player has the knife equipped they can't block
			if (Input.GetKeyUp ("space") && !oneblockattempt && GameManager.instance.weapon.name != "Knife") {
				blocked = true;
			}
			break;
		case (Turnstate.FINISHATTACK):

		break;
		}
	}

	// Increments the amount of time until the enemy can act
	// The same as PlayerState.cs IncrementProgressBar but without the progress bar
	void IncrementProgress() {
		currentcooldown = currentcooldown + (Time.deltaTime * 1.5f); //((float)enemy.Dexterity * 0.0165f); // Increases the cooldown bar by the time that has passed
		// TODO: increment cooldown by an enemy stat either level or dexterity
		if (currentcooldown >= maximumcooldown) {
			currentstate = Turnstate.CHOOSEACTION;
		}
	}

	void ChooseAction() {
		CombatAction thisattack = new CombatAction();
		thisattack.Attacker = enemy.Name; // puts the name of this EnemyState entity into the Attacker variable
		thisattack.type = "Enemy"; // Set attacker type to enemy since an enemy is attacker
		thisattack.AttackerObject = this.gameObject; // puts this gameobject under the AttackerObject variable
		thisattack.AttackerTarget = battlestatemachine.PlayersInCombat[Random.Range(0, battlestatemachine.PlayersInCombat.Count)]; // There is only one player to target
		if (stunned) {
			// No actions this turn, marked in the CombatAction
			thisattack.stunnedaction = true;
		}
		if (bleedtimer == 0 && bleed) {
			bleed = false;
		}
		if (bleed) {
			// Take a damage
			enemy.CurrentHealth -= 1;
			bleedtimer -= 1;
			// Then let the world know
			battlestatemachine.CreatePopupText("Bleeding!" , gameObject.transform);
		}
		battlestatemachine.CollectActions(thisattack);
	}

	private IEnumerator TimeForAction() {
		if (ActionStarted) {
			yield break; // If action is started break out of the IEnumerator
		}

		ActionStarted = true;

		if (stunned) {
			stunned = false;
			battlestatemachine.CreatePopupText ("Stunned", gameObject.transform);

			currentstate = Turnstate.ATTACKING;
			currentstate = Turnstate.FINISHATTACK;
			// reset battle state machine to wait
			battlestatemachine.CombatActionList.RemoveAt(0);
			battlestatemachine.currentstate = BattleStateMachine.BattleState.WAIT;
			// The action is over so actionstarted is set back to false in preperation for the next action
			ActionStarted = false;

			// Reset the enemy state
			blocked = false;
			currentcooldown = 0f;
			currentstate = Turnstate.PROCESSING;
			// Finally break out of the coroutine with no action taken
			yield break;
		}

		// Animate the enemy for the chosen attack (move it forward a little towards the target)
		Vector2 playerPosition = new Vector2(PlayerTarget.transform.position.x, PlayerTarget.transform.position.y + 1f);
		// Increase the y position so that the attacker does not get right on top of the player
		while (MoveToEnemy(playerPosition)){
			yield return null;
		}

		StartCoroutine (battlestatemachine.SpriteBlink (2, gameObject));
		// During this state if the player hits the block button they block the attack
		currentstate = Turnstate.ATTACKING;

		// Wait a little bit
		yield return new WaitForSeconds(0.3f);
		// check for hit and then damage

		// Blocking is no longer available, so switch state
		currentstate = Turnstate.FINISHATTACK;
		bool hit;
		if (PlayerTarget.tag == "CombatPlayer") {
			hit = battlestatemachine.checkForHit(enemy.Dexterity, GameManager.instance.Dexterity, 0);
		} else {
			hit = battlestatemachine.checkForHit(enemy.Dexterity, PlayerTarget.GetComponent<AllyState>().ally.Dexterity, 0);
		}

		// Determine the correct damage resistance to be used in the damage calculation
		int damageresistance = 0;
		if (enemy.weapon.damagetype == Item.DamageTypes.BLUNT) {
			damageresistance = GameManager.instance.armor.armorBonusBlunt;
		}
		if (enemy.weapon.damagetype == Item.DamageTypes.SLASH) {
			damageresistance = GameManager.instance.armor.armorBonusSlash;
		}
		if (enemy.weapon.damagetype == Item.DamageTypes.PIERCE) {
			damageresistance = GameManager.instance.armor.armorBonusPierce;
		}
		// Add one damageresistance for items with the "Sturdy" passive ability
		if (GameManager.instance.weapon.name == "Club" || GameManager.instance.weapon.name == "Spiked Club") {
			damageresistance += 1;
		}
		// Determine the damage total depending on whether or not the player blocked the attack
		int damage = 0;


		if (PlayerTarget.tag == "CombatPlayer") {
			damage = battlestatemachine.calculateDamage (enemy.weapon.minDamage, enemy.weapon.maxDamage, enemy.Strength, GameManager.instance.Hardiness, damageresistance);
		} else {
			damage = battlestatemachine.calculateDamage (enemy.weapon.minDamage, enemy.weapon.maxDamage, enemy.Strength, PlayerTarget.GetComponent<AllyState>().ally.Hardiness, damageresistance);
		}
		
		if (hit) {
			// A hit! Deduct damage from the enemy health
			// First check if the player blocked to reduce damage
			if (blocked) {
				damage = (int)(damage / 2);
				// Blocked Animation
				battlestatemachine.PopAnimation(battlestatemachine.blocked, PlayerTarget.transform);
			}
			if (PlayerTarget.tag == "CombatPlayer") {
				GameManager.instance.currenthealth -= damage;
			} else {
				PlayerTarget.GetComponent<AllyState> ().ally.currenthealth -= damage;
			}
			battlestatemachine.CreatePopupText (damage.ToString(), PlayerTarget.transform);
			// shake the camera
			battlestatemachine.camerashaker.shakeDuration = 0.5f;
		} else {
			// Not a hit
			battlestatemachine.CreatePopupText ("Miss!", PlayerTarget.transform);
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
		blocked = false;
		currentcooldown = 0f;
		currentstate = Turnstate.PROCESSING;
	}

	// TODO: Simplify to one move function using stored start positions for the enemy and the player

	// If moveTo is not done with Vector3 an error occurs with no workaround
	// AnimationAttackSpeed also determines the wait between attacks
	private bool MoveToEnemy(Vector3 target) {
		return target != (transform.position = Vector3.MoveTowards(transform.position, target, (enemy.AnimationAttackSpeed * 5) * Time.deltaTime));
	}

	private bool MoveToStart(Vector3 target) {
		return target != (transform.position = Vector3.MoveTowards(transform.position, target, (enemy.AnimationAttackSpeed * 5) * Time.deltaTime));
	}

	void UpdateEnemyHealth() {
		if (enemy.CurrentHealth <= 0) {
			// TODO: Replace dead enemy with skeleton sprite?
			battlestatemachine.PopAnimation(battlestatemachine.death, transform);
			battlestatemachine.EnemiesInCombat.Remove(transform.root.gameObject);

			// gameObject.SetActive (false);
			Object.Destroy (transform.root.gameObject); // Can set a float as a second parameter to the time of the animation (or just leave the body there)
			// Just destroying the object throws an errors
		}
	}
}
