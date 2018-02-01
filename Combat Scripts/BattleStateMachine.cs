using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class BattleStateMachine : MonoBehaviour {
	// NOTE: Player input and GUI are stored in the battle state machine as opposed to PlayerState

	// TODO: Maybe have some strong moves take longer to perform (time is a resource)
	public enum BattleState {
		WAIT,
		TAKEACTION,
		PERFORMACTION // An idle state that waits for the animation to play
	}

	public BattleState currentstate;

	// Not necessary as the gameplan stands, but if multiple heros need to be added this is required
	public List<GameObject> PlayersInCombat = new List<GameObject>();
	// A list for enemies and a list for players in this combat encounter
	public List<GameObject> EnemiesInCombat = new List<GameObject> ();

	public List<GameObject> PlayersToManage = new List<GameObject>();

	public Image PlayerProgressBar;

	public Image Ally1ProgressBar;

	private CombatAction PlayerChoice;

	public GameObject EnemyButton; // Needed for adding enemies to the combat GUI
	public GameObject AbilityButton; // Ability button prefab for adding abilities to the combat GUI
	public Button EnemyExitButton;
	public Transform Spacer; // The spacer in the combat GUI
	public Transform AbilitySpacer; // The spacer for the ability selection menu

	public List<CombatAction> CombatActionList = new List<CombatAction> ();

	// This stores all the states that the player's GUI might be in
	public PlayerGUI PlayerInput;

	// Camera shaking controller
	public CameraShake camerashaker;

	// Reference to popup text prefab
	public DamageText popuptext;
	public PopupAnimation blocked;
	public PopupAnimation death;
	private static GameObject battlecanvas;

	// public GameObject AllyBar;

	public string loweststatname = "";
	public bool focused = false;

	public enum PlayerGUI {
		ACTIVATE,
		WAITING,
		ATTACKCHOICEINPUT, // Choose which kind of attack to use
		ENEMYCHOICEINPUT, // select enemy to attack
		DONE
	}

	public GameObject AttackPanel;
	public GameObject EnemySelectPanel;
	public GameObject AbilityPanel;
	public GameObject LootPanel;
	public Button AbilityExitButton;

	public Text LootText;
	public Button LootButton;
	bool looting = true;



	// Use this for initialization
	void Start () {
		// First create all the actors for the battle, enemies and then the player and their allies
		CreateEnemies ();
		CreateAllies ();

		// These two AddRange calls add all game objects with the appropriate tags to the appropriate lists (enemies, players)
		PlayersInCombat.AddRange(GameObject.FindGameObjectsWithTag ("CombatPlayer"));
		PlayersInCombat.AddRange(GameObject.FindGameObjectsWithTag ("CombatAlly"));
		EnemiesInCombat.AddRange (GameObject.FindGameObjectsWithTag ("CombatEnemy"));

		CreateEnemyButtons ();
		CreateAbilityButtons ();

		camerashaker = GameObject.Find ("Main Camera").GetComponent<CameraShake> ();

		battlecanvas = GameObject.Find ("BattleCanvas");

		// The panel to select an attack or an enemy to attack begins inactive
		AttackPanel.SetActive (false);
		EnemySelectPanel.SetActive (false);
		AbilityPanel.SetActive (false);
		LootPanel.SetActive (false);

		LootButton.onClick.AddListener (() => LootButtonClick());
		AbilityExitButton.onClick.AddListener (() => AbilityExit());
		EnemyExitButton.onClick.AddListener (() => EnemyExit());
		//AllyBar.SetActive (false);


		// Set game states to their beginning states
		PlayerInput = PlayerGUI.ACTIVATE;
		currentstate = BattleState.WAIT;
	}
	
	// Update is called once per frame
	void Update () {
		if (EnemiesInCombat.Count == 0) {
			// All enemies are gone so the player has won!
			// Loot screen
			// Clear any enemy data passed before a previous battle
			if (GameManager.instance.enemiesInCombat.Count > 0) {
				for (int i = 0; i < GameManager.instance.enemiesInCombat.Count - 1; i++) {
					GameManager.instance.enemiesInCombat [i] = null;
				}
			}
			if (looting) {
				LootPanel.SetActive (true);
				LootText.text = GameManager.instance.lootstring;
				looting = false;
			}

			if (!LootPanel.activeSelf) {
				LootPanel.SetActive (false);
				// Add loot
				GameManager.instance.money += GameManager.instance.cashmoneyloot;
				GameManager.instance.cashmoneyloot = 0;


				// Once the loot panel is disabled...
				// Go back to exploration mode
				// GameManager.instance.stamina = 10;
				// Remove focused stat (if not focused loweststatname will not equal any of these)
				// Have to check for focused or update might run twice and remove 2 from a stat
				if (focused) {
					if (loweststatname == "Hardiness") {
						GameManager.instance.Hardiness -= 2;
					}
					if (loweststatname == "Strength") {
						GameManager.instance.Strength -= 2;
					}
					if (loweststatname == "Dexterity") {
						GameManager.instance.Dexterity -= 2;
					}
				}
				focused = false;
				// Go back to exploration mode
				GameManager.instance.currentstate = GameManager.GameState.EXPLORE;
			}
		}
		switch (currentstate) {
		case (BattleState.WAIT):
			if (CombatActionList.Count > 0) {
				currentstate = BattleState.TAKEACTION;
			}
			break;
		case (BattleState.TAKEACTION):
			GameObject actionPerformer = CombatActionList [0].AttackerObject;
			if (CombatActionList [0].type == "Enemy") {
				if (actionPerformer != null) {
					EnemyState enemystate = actionPerformer.GetComponent<EnemyState> ();
					enemystate.PlayerTarget = CombatActionList [0].AttackerTarget;
					enemystate.currentstate = EnemyState.Turnstate.ACTION;
					currentstate = BattleState.PERFORMACTION;
				} else if (actionPerformer == null) {
					CombatActionList.RemoveAt (0);
					currentstate = BattleState.WAIT;
				}
			}

			if (CombatActionList [0].type == "Player") {
				if (actionPerformer.tag.Equals("CombatAlly")) {
					AllyState allystate = actionPerformer.GetComponent<AllyState> ();
					allystate.EnemyTarget = CombatActionList [0].AttackerTarget;
					allystate.currentstate = AllyState.Turnstate.ACTION;
					currentstate = BattleState.PERFORMACTION;
				} else {
					PlayerState playerstate = actionPerformer.GetComponent<PlayerState> ();
					playerstate.EnemyTarget = CombatActionList [0].AttackerTarget;
					playerstate.currentstate = PlayerState.Turnstate.ACTION;
					currentstate = BattleState.PERFORMACTION;
				}
			}
			break;
		case (BattleState.PERFORMACTION):

			break;
		}

		switch (PlayerInput){
		case (PlayerGUI.ACTIVATE):
			// If at least one player
			if (PlayersToManage.Count > 0) {
				// Sets the player that is going to attack to active
				// Since there are no allies I've removed the selector, it's pretty pointless with just one entity the player can control
				// PlayersToManage[0].transform.FindChild("Selector").gameObject.SetActive(true);
				PlayerChoice = new CombatAction ();

				AttackPanel.SetActive (true);
				PlayerInput = PlayerGUI.ATTACKCHOICEINPUT;
			}
			break;
		case (PlayerGUI.WAITING):
			
			// idle idle idle idle
			// TODO: Decide if waiting PlayerGUI state is necessary or not
			break;
		case (PlayerGUI.ATTACKCHOICEINPUT):

			break;
		case (PlayerGUI.ENEMYCHOICEINPUT):

			break;
		case (PlayerGUI.DONE):
			PlayerInputDone ();
			break;
		}
	}

	void AbilityExit() {
		AbilityPanel.SetActive (false);
	}

	void EnemyExit() {
		EnemySelectPanel.SetActive (false);
		AttackPanel.SetActive (true);

		PlayerInput = PlayerGUI.ATTACKCHOICEINPUT;
	}

	// Gathers all actions from player/enemy state machines
	public void CollectActions(CombatAction action) {
		// Populate CombatActionList with CombatActions
		CombatActionList.Add(action);
	}

	// TODO: Remove old stamina, just clean up this mess in general, go through TODOS


	// Populates the EnemyPanel with buttons for each enemy
	void CreateEnemyButtons() {
		foreach (GameObject enemy in EnemiesInCombat) {
			// Creates a new button from the EnemyButton prefab
			GameObject newButton = Instantiate (EnemyButton) as GameObject;
			// Gets information from the button and enemy information from the current enemy in EnemiesInCombat
			EnemyButton button = newButton.GetComponent<EnemyButton> ();
			EnemyState currentEnemy = enemy.GetComponent<EnemyState> ();

			// Finds the text component in the new Button and sets it to enemy.Name from the enemy being accessed in the foreach statement
			Text ButtonText = newButton.transform.FindChild ("Text").gameObject.GetComponent<Text> ();
			ButtonText.text = currentEnemy.enemy.Name;

			button.EnemyPrefab = enemy;

			newButton.transform.SetParent (Spacer);
		}
	}

	// Populates the EnemyPanel with buttons for each ability
	void CreateAbilityButtons() {
		foreach (Ability ability in GameManager.instance.weapon.abilities) {
			// Creates a new button from the AbilityButton prefab
			GameObject newButton = Instantiate (AbilityButton) as GameObject;
			// Gets information from the button and enemy information from the current enemy in EnemiesInCombat
			AbilityButton button = newButton.GetComponent<AbilityButton> ();

			// Finds the text component in the new Button and sets it to enemy.Name from the enemy being accessed in the foreach statement
			Text ButtonText = newButton.transform.FindChild ("Text").gameObject.GetComponent<Text> ();
			ButtonText.text = ability.abilityname;
			button.ability = ability;

			newButton.transform.SetParent (AbilitySpacer);
		}
	}

	// Called when the attack button is pressed
	public void AttackInput() {
		PlayerChoice.Attacker = PlayersToManage[0].name;
		PlayerChoice.AttackerObject = PlayersToManage [0];
		if (PlayerChoice.abilitytype == null) {
			PlayerChoice.abilitytype = new Ability ("", 0, false, false); // A blank abilitytype that denotes a basic attack
		}
		PlayerChoice.type = "Player";

		// Since attack has been chosen the attack panel is no longer needed and the player can then
		// Select an enemy via the enemyselectpanel
		AttackPanel.SetActive (false);
		EnemySelectPanel.SetActive (true);

		PlayerInput = PlayerGUI.ENEMYCHOICEINPUT;

		// TODO: Allow the player to go back to the attackpanel
	}

	public void NonAttackInput() {
		PlayerChoice.Attacker = PlayersToManage[0].name;
		PlayerChoice.AttackerObject = PlayersToManage [0];
		PlayerChoice.type = "Player";

		int loweststat = GameManager.instance.Hardiness;
		loweststatname = "Hardiness";
		if (GameManager.instance.Strength < loweststat) {
			loweststat = GameManager.instance.Strength;
			loweststatname = "Strength";
		}
		if (GameManager.instance.Dexterity < loweststat) {
			loweststat = GameManager.instance.Dexterity;
			loweststatname = "Dexterity";
		}
		if (loweststatname == "Hardiness") {
			GameManager.instance.Hardiness += 2;
		}
		if (loweststatname == "Strength") {
			GameManager.instance.Strength += 2;
		}
		if (loweststatname == "Dexterity") {
			GameManager.instance.Dexterity += 2;
		}
		focused = true;
		// Since attack has been chosen the attack panel is no longer needed and the player can then
		// Select an enemy via the enemyselectpanel
		AttackPanel.SetActive (false);
		EnemySelectPanel.SetActive (true);



		PlayerInput = PlayerGUI.DONE;
	}

	// Called when the use ability button is pressed
	public void AbilityInput() {
		// Set the ability panel active
		AbilityPanel.SetActive (true);

		// TODO: Back buttons for the EnemyPanel
	}

	// Selects which enemy will be attacked
	public void EnemySelection(GameObject enemyChoice) {
		PlayerChoice.AttackerTarget = enemyChoice;
		PlayerInput = PlayerGUI.DONE;
	}

	public void AbilitySelection (Ability abilityChoice) {
		PlayerChoice.abilitytype = abilityChoice;
		AbilityPanel.SetActive (false);

		// If statements here for every ability that does not select an enemy
		if (abilityChoice.abilityname == "Focus") {
			NonAttackInput ();
		} else {
			// This is an ability that requires selecting an enemy
			AttackInput ();
		}
	}

	void PlayerInputDone() {
		CombatActionList.Add (PlayerChoice);

		// Once the enemy is selected the panel goes away until the next enemy selection
		EnemySelectPanel.SetActive (false);
		PlayersToManage[0].transform.FindChild("Selector").gameObject.SetActive(false);
		PlayersToManage.RemoveAt (0);
		PlayerInput = PlayerGUI.ACTIVATE;
	}

	// Spawns enemies from EnemiesInCombat to certain positions
	// Maximum of five enemies per combat
	void CreateEnemies() {
		// As it stands here the spawn order is center, top left, top right, far right, far left
		int[,] coordinates = new int[,] { {0,3}, {-3,3}, {3,3}, {5,2}, {-5,2}};
		int z = -1;
		int i = 0;
		foreach (GameObject enemy in GameManager.instance.enemiesInCombat) {
			Instantiate (enemy, new Vector3 (coordinates[i,0], coordinates[i,1], z), Quaternion.identity);
			i++;
		}
	}

	// Spawns the allies from PlayersToManage at certain positions
	// Maximum of three allies including the player
	void CreateAllies() {
		// As it stands here the spawn order is left of player
		int[,] coordinates = new int[,] { {-3,0}};
		int z = -1;
		int i = 0;
		foreach (GameObject ally in GameManager.instance.alliesInCombat) {
			Instantiate (ally, new Vector3 (coordinates[i,0], coordinates[i,1], z), Quaternion.identity);
			i++;
		}
	}

	// Creates popuptext that will float in the air for a moment and then dissapears
	// Text - The text to be displayed in the popup
	// location - where the text appears in the canvas
	// For now this only works on the battlecanvas
	public void CreatePopupText(string text, Transform location) {
		Vector3 screenPosition = Camera.main.WorldToScreenPoint(location.position);
		DamageText textinstance = (DamageText)Instantiate (popuptext, screenPosition, Quaternion.identity, battlecanvas.transform);
		textinstance.setText (text);
	}

	// Creates a shield that pops up and fades beside a character to indicated that
	// They have blocked an attack
	// location - The transform of the blocking character, the shield will pop up 1 to the right of it
	public void PopAnimation(PopupAnimation prefabtoinstantiate, Transform location) {
		Vector3 screenPosition = Camera.main.WorldToScreenPoint (location.position);
		screenPosition.x += 1;
		// This will throw a warning since I have to declare a PopupAnimation, but I don't need to do anything with it
		PopupAnimation animationinstance = (PopupAnimation)Instantiate (prefabtoinstantiate, screenPosition, Quaternion.identity, battlecanvas.transform);
	}

	// Used in PlayerState, AllyState and EnemyState to check for hits
	public bool checkForHit(int attackerDexterity, int defenderDexterity, int abilitymodifier) {
		int basehit = 75 + abilitymodifier;
		int dexmodifier = attackerDexterity - defenderDexterity;
		basehit = basehit + (dexmodifier * 5);
		if (basehit > 95) {
			basehit = 95;
		}
		if (Random.Range (1, 100) < basehit) {
			return true;
		}
		return false;
	}

	// Used in PlayerState, AllyState and EnemyState to calculate base damage
	public int calculateDamage(int minDamage, int maxDamage, int Strength, int targethardiness, int resistance) {
		int damage = Random.Range (minDamage, maxDamage) + (Strength - (int)((targethardiness - 3)/2)) - resistance;
		if (damage < 0) {
			damage = 0;
		}
		return damage;
	}

	public IEnumerator SpriteBlink(int numberoftimes, GameObject objecttoblink) {
		SpriteRenderer spritecolor = objecttoblink.GetComponent<SpriteRenderer>();
		for (int i = 0; i < numberoftimes; i++) {
			spritecolor.color = Color.red;
			yield return new WaitForSeconds(.1f);
			spritecolor.color = Color.white;
			yield return new WaitForSeconds (.1f);
		}
	}

	void LootButtonClick () {
		LootPanel.SetActive (false);
	}
}