using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
	// TODO: Clean up dead code and add better comments / Defensive code

	// Accessable outside of the class
	// Static means this will always refer to the class instead of the instance
	// Any script can now access one instance of gamemanger and modify it universally
	public static GameManager instance = null;
	public GameState currentstate = GameState.EXPLORE;

	// Prefab used for buttons in TextTriggers
	public GameObject messagebutton;

	public List<GameObject> enemiesInCombat = new List<GameObject> ();
	public List<GameObject> alliesInCombat = new List<GameObject> ();

	// Node currently used for the messagepanel
	public DialogueNode currentnode =  new DialogueNode ("starter", "ERROR", new List<string> ());

	public Dictionary<string, DialogueNode> dialogdictionary = new Dictionary<string, DialogueNode> ();

	// If this is false the player cannot move, the player can move if this is true
	public bool PlayerMovementEnabled = true;

	// Note: I deeply regret not putting player stats in a class and instantiating an instance of it here
	// TODO: If I have time, put all these stats into a single object (will have to find every call to player stats and change it up, could take a while)
	// Player Stats, they cannot be reduced below 3 by the player
	// Cannot be reduced below 1 by anything
	public string playername;
	public int level; // Each level gives 2 stat points
	public int Dexterity; // Dexterity increases the speed of the action bar in combat and increases a player's defense on a successful block
	// A character at five dexterity has a total action bar time of 5 seconds, and a character at the maximum, 20 dexterity
	// has half of the maximum bar time, 2.5 seconds
	public int Strength; // Strength increases damage and changes what types of equipment can be used (maybe makes heavier weapons slow the player down less)
	public int Hardiness; // Every 2 points of hardiness above 3 gives one universal damage resistence
	public int StatPoints; // Used to upgrade stats and skills
	public int maxhealth;
	public int currenthealth;
	public float stamina;
	public float AnimationAttackSpeed = 1f;
	public int money;
	public int healingitems;

	public int cashmoneyloot;
	public string lootstring;

	// The list of all the player's available abilities
	public List<Ability> abilitylist = new List<Ability>();

	// Player starting equipment
	public Item weapon;
	public Item armor;

	Item Club;
	Item SpikedClub;
	Item Knife;
	Item Cutlass;
	Item Cloak;
	Item Chainmail;

	public List<Item> EquipmentList = new List<Item> ();

	// These variables are for story elements (i.e. Did the player help the miser at the start of the first level?)
	public int helpedmiser = 0; // 0 means the miser encounter has not been activated, 1 means the player helped the miser, 2 means they did not
	public int spickygone = 0; // 0 means spicky is around, 1 means he's dead or chased off
	public int isaachouseguardhelped = 0; // 0 means the guard has not slipped the player any cash, 1 means they have
	public int nickiquest = 0; // 0 means the player has not gotten the quest, 1 means they have it, 2 means they have completed it, 3 means they were denied the quest
	public int isaacquest = 0; // 0 means the player has not gotten the quest, 1 means they have it, 2 means they have talked to jerry/alicia but not brought them back
	// 3 means they have forced jerry/alicia to come back 4 means they did so and talked to isaac about it
	// TODO: Remove talkedtojerry and use isaacquest for this
	public int talkedtojerry = 0; // 0 means the player has not forced jerry and alicia to come back and has not talked to them, 1 means they have and did not force them 
	// to go back
	public int deadrefugees = 0; // 0 if the player did not attack them, 1 if the player did
	public int happybeggar = 0; // 0 means the beggar is begging still, 1 means the player gave them enough money to cross the border, 2 means the player robbed him
	// 3 means the player knows to try and look poor to cross the border, 50 banknote discount on trying to cross (so 50 total)
	public int wagonpassed = 0; // 0 means the player has not searched the wagon, 1 means they have
	public int borderguard = 0; // 0 means nothing, 1 means the player paid the guards off, 2 means they killed the guards
	public int banditleader = 0; // 0 means the bandit leader is alive, 1 means she is dead
	public int secretwall = 0; // 0 means the wall is still secret, 1 means it has been opened and stays open

	int moneytocrossborder = 100; // Amount of money the player has to pay to cross the border

	// These gamestate enums form a simple state machine
	public enum GameState{
		EXPLORE,
		BATTLE,
		VICTORY
	}

	// This function is used for initializationd
	void Awake () {

		// This is purely for testing
		// TODO: Remove after implementing saves
		PlayerPrefs.DeleteAll ();

		// helpedmiser = 0; // Have to put this here or this is automatically set to 1

		// This if else sets the first game manager made to the static variable above (instance)
		// Otherwise it will destroy the newly created instance of gamemanager
		// This way there will only ever be one instance of game manager (making this a singleton)
		if (instance == null) {
			instance = this;
		} else  if (instance != this) {
			Destroy (gameObject);
		}



		// This function makes sure this object is not destroyed on level load
		DontDestroyOnLoad (gameObject);

		LoadDialogueNodes ();
		LoadItemInfo ();

		armor = Cloak;
		weapon = Club;
		EquipmentList.Add (Club);
		EquipmentList.Add (Cloak);

		// Setup default values for player stats
		playername = "Refugee";
		level = 1;
		StatPoints = 2;
		Hardiness = 5;
		Dexterity = 5;
		Strength = 5;
		maxhealth = 20;
		currenthealth = 20;
		money = 5;
		healingitems = 1;
		stamina = 10;
	}

	// This function is called every tick
	void Update(){
		// If the player presses the escape key the game will shut down
		if (Input.GetKeyDown("escape")) {
			Application.Quit ();
		}
		// The state machine is implemented here
		// It goes between different modes of play
		// (i.e. Player encounters an enemy so go to battle gamestate)
		switch (currentstate) {
		// TODO: Clean up all this changing to battle state stuff and rely solely on InitiateCombat
		case (GameState.EXPLORE):
			// Clear any enemy data passed before a previous battle
			// Go to explore mode
			if (SceneManager.GetActiveScene ().name != "Level1") {
				if (enemiesInCombat.Count > 0) {
					for (int i = 0; i < enemiesInCombat.Count - 1; i++) {
						enemiesInCombat [i] = null;
					}
				}
				PlayerMovementEnabled = true;
				SceneManager.LoadScene ("Level1");
			}
			break;
		case (GameState.BATTLE):
			Debug.Log ("started");
			// Go to battle mode
			if (SceneManager.GetActiveScene().name != "Battle") {
				SceneManager.LoadScene("Battle");
			}
			break;
		}
	}

	public void InitiateCombat (List<GameObject> Enemies, int loot, string loottext) {
		// Pass enemy array to the gamemanager
		// Take the array from the gamemanager to populate enemiesincombat in the BattleStateMachine
		Debug.Log("combat initiated");
		GameManager.instance.enemiesInCombat = Enemies;
		// Set the loot variables to those assigned to this encounter
		GameManager.instance.cashmoneyloot = loot;
		GameManager.instance.lootstring = loottext;

		// Fade out
		// StartCoroutine (ExplorationManager.FadeOut ());

		// Change the gamestate to battle to start the fight
		GameManager.instance.currentstate = GameManager.GameState.BATTLE;
	}

	public void PlayerWon () {
		// TODO: Victory music
		currentstate = GameState.VICTORY;
		SceneManager.LoadScene("Victory");
	}

	public void LevelUp() {
		level += 1;
		StatPoints += 2;
		maxhealth += 5;
	}

	// Always use this method to add to currenthealth to make sure it doesn't go above maxhealth
	// TODO: Add subtract method and make currenthealth private
	public void AddHealth(int toAdd) {
		currenthealth += toAdd;
		if (currenthealth > maxhealth) {
			currenthealth = maxhealth;
		}
	}

		
	// Contains all the dialogue nodes for textTrigger elements
	// Loads them into the dictionary for later use, called at startup
	public void LoadDialogueNodes() {
		// TODO: See the TODO in textTrigger
		// Explanation of how this works:
		// ButtonNode List additions hold the content of the button in the menu, followed by the node that clicking this button will go to
		// Each DialogueNode created contains the ID for the node, the text displayed in the node, and the list of buttons to be displayed there (buttonNodelist here)
		// Finally the DialogueNode is added to the dialoguedictionary, where it can be Accessed via its ID by TextTrigger
		// The first node accessed by a TextTrigger is decided by the public ID variable, which should match with the ID of the desired DialogueNode
		// in the dialoguedictionary
		// Make sure to clear the buttonNodeList after every node is added so it wont add old buttons to the next node

		// the node "exit" will always exit the dialogue window

		// NOTE: The dialog nodes will be reloaded every time the messagepanel is used, so the if statements will change the nodes depending on the player's circumstances
		List<string> buttonNodeList = new List<string> ();

		// First clear the previous nodes for reloading. This is not a very efficient way to do this, but this does not require a lot of efficiency
		dialogdictionary.Clear();

		buttonNodeList.Add ("Nowhere to go but forward./exit");
		DialogueNode CantGoBackText = new DialogueNode ("cantgoback", "You look back down the road you came from. Somewhere miles away there " +
			"are armies clashing, and you know that the soldiers of your homeland are losing. You've heard rumors of the invaders taking " +
			"slaves, and anything else they can find. You've seen whole villages destroyed and populations wiped off of the map. There is nothing " +
			"for you back there but a brutal, likely short, existence. It will be the same here, and your only hope of " +
			"safety is to get across the border. The smell of fire wafts from the south. So it goes in wartime.", buttonNodeList);
		dialogdictionary.Add ("cantgoback", CantGoBackText);
		buttonNodeList.Clear ();

		// This first set of nodes are all for the initial miser encounter
		buttonNodeList.Add ("Rush in to help!/helpmiser");
		buttonNodeList.Add ("Do Nothing./deadmiser");
		DialogueNode MiserUnderAttack = new DialogueNode ("miserunderattack", "The human figure is a tall, lanky man dressed in fine silk robes. A necklace inlaid with a large ruby glints " +
			"in the sunlight. He shakes visibly as he backs against the fence, waving a pitiful stick at the two slavering wolves " +
			"that growl and creep towards him, about to pounce.", buttonNodeList);
		dialogdictionary.Add("miserunderattack", MiserUnderAttack);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Defend Yourself./fight");
		DialogueNode HelpMiser = new DialogueNode ("helpmiser", "You rush in at once, imposing yourself between the wolves and their prey. " +
			"The animals do not seem to care. Their eyes are glazed over. Their ribs are showing through their matted fur. Even as you yell " +
			"and swing your weapon at the air to try and drive them off they jump at you.", buttonNodeList);
		dialogdictionary.Add ("helpmiser", HelpMiser);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Ask for a reward./alivemiserreward");
		buttonNodeList.Add ("Leave./leavemiser");
		DialogueNode AliveMiser = new DialogueNode ("alivemiser", "The man drops the stick he was holding and leans against the fence, still " +
			"trembling. 'T-Thank, you' he says, 'I thoug- I thought that was it.' He breaths deeply and seems to regain his composure before offering " +
			"a hand. 'It's so rare to see someone do something so genuinely selfless in these troubling times. You won't find any of that at the border " +
			"up ahead. I could hardly even get to the guard post, and when I got there they demanded I give them the clothes off of my back just to get " +
			"through!' For a moment the man's face is covered in a look of genuine misery. 'The very thought! I decided I would take my chances with the " +
			"invaders.' You doubt that his chances are very good with that necklace he's wearing.", buttonNodeList);
		dialogdictionary.Add ("alivemiser", AliveMiser);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Chase him./chasemiser");
		buttonNodeList.Add ("Watch him go./leavemiser");
		DialogueNode AliveMiserReward = new DialogueNode ("alivemiserreward", "The miser raises his hands and glances down the southern road. " +
			"'Look' he says, 'I think I really should be going before any more wolves show up. Thank you so much for your help. You're a very good " +
			"person. That's enough to get you through times like these.' Without waiting for a reaction, the miser walks down the road.", buttonNodeList);
		dialogdictionary.Add ("alivemiserreward", AliveMiserReward);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Take the necklace and go./robmiser");
		buttonNodeList.Add ("Kill him./killmiser");
		buttonNodeList.Add ("Leave him alone./runmiser");
		DialogueNode ChaseMiser = new DialogueNode ("chasemiser", "The miser sees you following him over his shoulder, and tries to run before tripping " +
			"on his robe and faceplanting on to the road. He tries to crawl away as you loom over him. He sputters, 'Please, I have a family back there. " +
			"I need to get back to them, get them across the border. You wouldn't hurt a man with a family would you? You wouldn't take food out of his " +
			"child's mouth?'", buttonNodeList);
		dialogdictionary.Add ("chasemiser", ChaseMiser);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Huh./exit");
		DialogueNode RunMiser = new DialogueNode ("runmiser", "The miser trips over a rock, stands up and gives you a horrified look before lifting " +
			"the hem of his robe and running away.", buttonNodeList);
		dialogdictionary.Add ("runmiser", RunMiser);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("No, no I'm not. Let him go./leavemiser");
		buttonNodeList.Add ("I'm not, but I do deserve that necklace./robmiser");
		buttonNodeList.Add ("No, but he deserves it./killedmiser");
		buttonNodeList.Add ("Maybe I Am./killedmiser");
		buttonNodeList.Add ("Yes./killedmiser");
		DialogueNode KillMiser = new DialogueNode ("killmiser", "You raise your weapon over your head. The miser cowers and shields his head with his " +
			"hands. You stop yourself for a moment. Do you really want to do this? Whatever you think of this man he is innocent and can't hurt you. " +
			"You're not a killer, are you?", buttonNodeList);
		dialogdictionary.Add ("killmiser", KillMiser);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Walk Away./exit");
		DialogueNode RobMiser = new DialogueNode ("robmiser", "You give the coward a couple of good kicks before you slip the necklace off of his neck and " +
			"hide it deep in your pack, it should be worth quite a bit 'No, no!' The miser says, 'That's all I have left.' He curls up and sobs into " +
			"the floor. 'You're just another bandit.' Some wolves, human or otherwise, will probably be along here shortly, time to leave.", buttonNodeList);
		dialogdictionary.Add ("robmiser", RobMiser);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Leave./exit");
		DialogueNode KilledMiser = new DialogueNode ("killedmiser", "It's over quickly. You glance up and down the road, seeing nobody, before you clean " +
			"your weapon on the miser's robe. Finally you take the necklace off of his neck. You make sure to hide it deep in your pack, that should be worth " +
			"quite a bit.", buttonNodeList);
		dialogdictionary.Add ("killedmiser", KilledMiser);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("To the border./exit");
		DialogueNode LeaveMiser = new DialogueNode ("leavemiser", "You leave the miser behind and walk back to the border, to the north.", buttonNodeList);
		dialogdictionary.Add ("leavemiser", LeaveMiser);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Slip Away./exit");
		buttonNodeList.Add ("Try to grab the necklace./lootmiser");
		DialogueNode DeadMiser = new DialogueNode ("deadmiser", "The wolves are quick, hungry and merciless. They rip through the man's robes in an " +
			"instant. It's a gruesome scene, but it doesn't even phase you. You have seen far worse, and recently. You could slip past the animals easily, but " +
			"the dead man's necklace might be useful.", buttonNodeList);
		dialogdictionary.Add ("deadmiser", DeadMiser);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Defend Yourself./fight");
		DialogueNode LootMiser = new DialogueNode ("lootmiser", "You creep around the wolves and grab the necklace from around the dead man's neck. " +
			"One of the animals turns and snarls at you. You hardly have time to raise your weapon before they attack.", buttonNodeList);
		dialogdictionary.Add ("lootmiser", LootMiser);
		buttonNodeList.Clear ();

		// These nodes are for the Spicky encounter at the edge of town
		// Spicky's currentnode starts here if the player helped the miser (i.e. helpedmiser == 1)
		buttonNodeList.Add ("What's going on under that cloak?/spickyhelpedmisercloak");
		buttonNodeList.Add ("Who are you?/spickyhelpedmiserwhoareyou");
		buttonNodeList.Add ("Ask about the border./spickyhelpedmiserborder");
		buttonNodeList.Add ("Ask about the town./spickyhelpedmisertown");
		buttonNodeList.Add ("Say Nothing./spickyhelpedmisernothing");
		DialogueNode SpickyConfrontHelpedMiser = new DialogueNode ("spickyconfronthelpedmiser", "A teenager stands in the road, " +
			"he wears a cloak that is certainly too warm for this season. You can clearly notice his hands fidgeting under it, probably " +
			"on a weapon. He says in a high pitched voice, 'Hold up there, haven't seen you around buddy. You probably don't know who's " +
			"town this is.'", buttonNodeList);
		dialogdictionary.Add ("spickyconfronthelpedmiser", SpickyConfrontHelpedMiser);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Draw your weapon./spickyhelpedmiserfight");
		buttonNodeList.Add ("'I'm not paying'./spickyhelpedmisertalk");
		buttonNodeList.Add ("Say Nothing./spickyhelpedmisernothingtwo");
		buttonNodeList.Add ("Laugh./spickyhelpedmiserlaugh");
		DialogueNode SpickyHelpedMiserNothing = new DialogueNode ("spickyhelpedmisernothing", "The teenager draws himself up to his full height, which " +
			"is still a foot shorter than yours. 'Tough guy, huh? You weren't so tough with that loaded guy down the street. Just up and let him go, no " +
			"matter how loaded he was. That's fine with Spicky, that's me, I got my cut out of that chump. Figured I'd be a little nice and leave him with " +
			"something. Maybe I shouldn't have, that's just going to go into someone else's pocket. Not yours though, tough guy. You better have fifty banknotes " +
			"in your mouth, or you can go back down the road with your rich buddy.'", buttonNodeList);
		dialogdictionary.Add ("spickyhelpedmisernothing", SpickyHelpedMiserNothing);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Draw your weapon./spickyhelpedmiserfight");
		buttonNodeList.Add ("'I'm not paying'./spickyhelpedmisertalk");
		buttonNodeList.Add ("Say Nothing./spickyhelpedmisernothingtwo");
		buttonNodeList.Add ("Laugh./spickyhelpedmiserlaugh");
		DialogueNode SpickyHelpedMiserBorder = new DialogueNode ("spickyhelpedmiserborder", "The teenager rolls his eyes and says, 'That's all anyone cares about, and " +
			"that's just fine by me. You want to give all your cash to those guards then go ahead, but you're going to have to give Spicky some cash first. Yea, " +
			"that's me. Fifty banknotes so you can go on your wild goose chase. Cough it up.'", buttonNodeList);
		dialogdictionary.Add ("spickyhelpedmiserborder", SpickyHelpedMiserBorder);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Draw your weapon./spickyhelpedmiserfight");
		buttonNodeList.Add ("'I'm not paying'./spickyhelpedmisertalk");
		buttonNodeList.Add ("Say Nothing./spickyhelpedmisernothingtwo");
		buttonNodeList.Add ("Laugh./spickyhelpedmiserlaugh");
		DialogueNode SpickyHelpedMiserWhoAreYou = new DialogueNode ("spickyhelpedmiserwhoareyou", "The teenager smirks and says, 'You should know, " +
			"this is my town. I'm Spicky, and I collect the tolls here. Fifty banknotes or you get the hell out of here.'", buttonNodeList);
		dialogdictionary.Add ("spickyhelpedmiserwhoareyou", SpickyHelpedMiserWhoAreYou);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Draw your weapon./spickyhelpedmiserfight");
		buttonNodeList.Add ("'I'm not paying'./spickyhelpedmisertalk");
		buttonNodeList.Add ("Say Nothing./spickyhelpedmisernothingtwo");
		buttonNodeList.Add ("Laugh./spickyhelpedmiserlaugh");
		DialogueNode SpickyHelpedMiserCloak = new DialogueNode ("spickyhelpedmisercloak", "The teenager stops fidgeting under his cloak and says, " +
			"'You let Spicky, that's me, mind his own business. Then you give me fifty banknotes or get the hell away from my town. Or I can always call " +
			"my buddies and we can introduce you to the thing under my cloak you're so curious about.'", buttonNodeList);
		dialogdictionary.Add ("spickyhelpedmisercloak", SpickyHelpedMiserCloak);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Draw your weapon./spickyhelpedmiserfight");
		buttonNodeList.Add ("'I'm not paying'./spickyhelpedmisertalk");
		buttonNodeList.Add ("Say Nothing./spickyhelpedmisernothingtwo");
		buttonNodeList.Add ("Laugh./spickyhelpedmiserlaugh");
		DialogueNode SpickyHelpedMiserTown = new DialogueNode ("spickyhelpedmisertown", "The teenager says, 'All you need to know is this town " +
			"belongs to Spicky, that's me.' He draws himself up to his full height, which still puts him at a foot shorter than you, and continues, " +
			"'There's a little toll to get in, if you can afford it. Fifty banknotes or you can walk. Got a problem with that and I call my boys and give " +
			"you a hard time.'", buttonNodeList);
		dialogdictionary.Add ("spickyhelpedmisertown", SpickyHelpedMiserTown);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Time for a fight./spickyhelpedmiserisaac");
		buttonNodeList.Add ("Run away./spickyhelpedmiserisaac");
		DialogueNode SpickyHelpedMiserFight = new DialogueNode ("spickyhelpedmiserfight", "You go for your weapon and Spicky's hand slips out of his " +
			"cloak, carrying a knife. He says, 'Wrong choice buddy, should have paid up.'", buttonNodeList);
		dialogdictionary.Add ("spickyhelpedmiserfight", SpickyHelpedMiserFight);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Explain that you don't have the money./spickyhelpedmiserisaac");
		buttonNodeList.Add ("Draw your weapon./spickyhelpedmiserisaac");
		buttonNodeList.Add ("Walk away./spickyhelpedmiserisaac");
		DialogueNode SpickyHelpedMiserTalk = new DialogueNode ("spickyhelpedmisertalk", "Spicky's hand emerges from his cloak, carrying a knife. 'I " +
			"told you,' he says. 'Fifty banknotes or you don't get in.' You try to explain that you don't have the money, but Spicky interrupts you, 'Spicky " +
			"doesn't care. If you don't have the money then you can't get in.' He scoffs. 'If you can't even afford to pay me, how do you think you're going " +
			"to get past those border guards? Those greedy bastards will want way more than I'm asking for.'", buttonNodeList);
		dialogdictionary.Add ("spickyhelpedmisertalk", SpickyHelpedMiserTalk);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Draw your weapon./spickyhelpedmiserisaac");
		buttonNodeList.Add ("Walk away./spickyhelpedmiserisaac");
		DialogueNode SpickyHelpedMiserNothingTwo = new DialogueNode ("spickyhelpedmisertalk", "'Oh, ' says Spicky. 'Still trying to be a tough guy, huh? " +
			"Well tough guys aren't welcome here, unless they're me, Spicky.' He draws a knife out of his cloak. 'Now get out of here.'", buttonNodeList);
		dialogdictionary.Add ("spickyhelpedmisernothingtwo", SpickyHelpedMiserNothingTwo);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Draw your weapon./spickyhelpedmiserisaac");
		buttonNodeList.Add ("Walk away./spickyhelpedmiserisaac");
		DialogueNode SpickyHelpedMiserLaugh = new DialogueNode ("spickyhelpedmiserlaugh", "You laugh, loudly and heartily in the teenager's face. Spicky turns " +
			"red, and whips a knife out of his cloak. 'Oh, you think this is funny?' He starts to yell, 'We'll see how funny I seem to you with this in your " +
			"belly.'", buttonNodeList);
		dialogdictionary.Add ("spickyhelpedmiserlaugh", SpickyHelpedMiserLaugh);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Walk away./exit");
		DialogueNode SpickyHelpedMiserIsaac = new DialogueNode ("spickyhelpedmiserisaac", "Suddenly a voice comes from your right. 'Alright,' it says, '" +
			"That's enough.' Spicky almost jumps out of his oversized boots, and you look over to see a heavily built, bearded and clearly aging man walking " +
			"over to the two of you. He taps the sword strapped on to his belt. 'What did I tell you about your little toll operation?' he says. Spicky looks " +
			"down at the ground and mutters, 'N-Not while y-you're alive, sir.' The big man nods and says, 'That's right, now get out of here and terrorise someone " +
			"else.' Spicky jams his hands inside his cloak and shuffles off. The big man turns to you and says, 'I'm Isaac. Come and talk to me in my house, it's " +
			"right by the border, can't miss it.' With that he speedwalks up the street.", buttonNodeList);
		dialogdictionary.Add ("spickyhelpedmiserisaac", SpickyHelpedMiserIsaac);
		buttonNodeList.Clear ();

		// Spicky's currentnode starts here if the player killed, robbed or let the miser die (i.e. helpedmiser == 2)
		buttonNodeList.Add ("Pay up./spickydeadmiserpaid");
		buttonNodeList.Add ("Say Nothing./spickydeadmisernothing");
		buttonNodeList.Add ("Ask about the town./spickydeadmisertown");
		buttonNodeList.Add ("Ask about the border./spickydeadmiserborder");
		DialogueNode SpickyConfrontDeadMiser = new DialogueNode ("spickyconfrontdeadmiser", "A teenager stands in the road, " +
			"he wears a cloak that is certainly too warm for this season. You can clearly notice his hands fidgeting under it, probably " +
			"on a weapon. He says in a high pitched voice, 'I'm Spicky and this is my turf. Don't think that just because you can rob some " +
			"helpless chump means you can get past the toll here. Twenty banknotes to get in, or you have to deal with me and my boys.' As far as you " +
			"can tell nobody but this teenager is actually threatening you.", buttonNodeList);
		dialogdictionary.Add ("spickyconfrontdeadmiser", SpickyConfrontDeadMiser);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Pay up./spickydeadmiserpaid");
		buttonNodeList.Add ("Draw your weapon./spickyhelpedmiserfight");
		buttonNodeList.Add ("Say Nothing./spickyhelpedmisernothingtwo");
		buttonNodeList.Add ("Laugh./spickyhelpedmiserlaugh");
		DialogueNode SpickyDeadMiserNothing = new DialogueNode ("spickydeadmisernothing", "The teenager takes a step closer and says, 'Don't try to play tough " +
			"guy with me. I'm Spicky and I run this town. Now you either pay up or you go back the way you came, got it?'", buttonNodeList);
		dialogdictionary.Add ("spickydeadmisernothing", SpickyDeadMiserNothing);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Walk into town./exit");
		DialogueNode SpickyDeadMiserPaid = new DialogueNode ("spickydeadmiserpaid", "You hand the money over, and Spicky swipes it out of your hand. " +
			"'Good,' he says. 'Now get moving and don't look at me funny.' He puffs his chest out and stares you down.", buttonNodeList);
		dialogdictionary.Add ("spickydeadmiserpaid", SpickyDeadMiserPaid);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Pay up./spickydeadmiserpaid");
		buttonNodeList.Add ("Draw your weapon./spickyhelpedmiserfight");
		buttonNodeList.Add ("'I'm not paying'./spickydeadmisertalk");
		buttonNodeList.Add ("Say Nothing./spickydeadmisernothingtwo");
		buttonNodeList.Add ("Laugh./spickyhelpedmiserlaugh");
		DialogueNode SpickyDeadMiserTown = new DialogueNode ("spickydeadmisertown", "The teenager says, 'All you need to know is this town " +
			"belongs to me, and no chump murderers like you are going to get their hands on it.' He draws himself up to his full height, which still puts him at a foot shorter than you, and continues, " +
			"'There's a little toll to get in, and I know you can afford it. Twenty banknotes or you can walk. Got a problem with that and I call my boys and give " +
			"you a hard time.'", buttonNodeList);
		dialogdictionary.Add ("spickydeadmisertown", SpickyDeadMiserTown);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Draw your weapon./spickyhelpedmiserisaac");
		buttonNodeList.Add ("Walk away./spickyhelpedmiserisaac");
		DialogueNode SpickyDeadMiserNothingTwo = new DialogueNode ("spickydeadmisertalk", "'Oh, ' says Spicky. 'Still trying to be a tough guy, huh? " +
			"Well tough guys aren't welcome here, unless they're me, Spicky.' He draws a knife out of his cloak. 'Now get out of here.'", buttonNodeList);
		dialogdictionary.Add ("spickydeadmisernothingtwo", SpickyDeadMiserNothingTwo);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Pay up./spickydeadmiserpaid");
		buttonNodeList.Add ("Draw your weapon./spickyhelpedmiserfight");
		buttonNodeList.Add ("'I'm not paying'./spickydeadmisertalk");
		buttonNodeList.Add ("Say Nothing./spickyhelpedmisernothingtwo");
		buttonNodeList.Add ("Laugh./spickyhelpedmiserlaugh");
		DialogueNode SpickyDeadMiserBorder = new DialogueNode ("spickydeadmiserborder", "The teenager rolls his eyes and says, 'That's all anyone cares about, and " +
			"that's just fine by me. You want to give all your cash to those guards then go ahead, but you're going to have to give Spicky some cash first. " +
			"Twenty banknotes so you can go on your wild goose chase. Cough it up. I know you got some off of that idiot down the road.'", buttonNodeList);
		dialogdictionary.Add ("spickydeadmiserborder", SpickyDeadMiserBorder);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Time for a fight/spickyhelpedmiserisaac");
		DialogueNode SpickyDeadMiserTalk = new DialogueNode ("spickydeadmisertalk", "'Won't pay? Alright, don't say I didn't warn you.' Spicky draws " +
			"a knife from under his cloak", buttonNodeList);
		dialogdictionary.Add ("spickydeadmisertalk", SpickyDeadMiserTalk);
		buttonNodeList.Clear ();

		// These nodes are for Isaac
		buttonNodeList.Add ("I certainly will./isaactalk2");
		buttonNodeList.Add ("We'll have to see./isaactalk2");
		buttonNodeList.Add ("What's up with that guy Spicky?/isaactalkspicky");
		buttonNodeList.Add ("Can you help me do that?/isaactalkhelpborder");
		buttonNodeList.Add ("Uhhh I have to go./exit");
		DialogueNode IsaacTalk = new DialogueNode ("isaactalk", "Isaac smiles as you walk in, though it is clearly forced. Despite his " +
			"imposing appearance there are bags under his eyes and his hands tremble slightly. 'Glad you could make it,' he says. 'I'm Isaac, " +
			"and you look like somebody who's only here to get across that border, and you might make it too.'", buttonNodeList);
		dialogdictionary.Add ("isaactalk", IsaacTalk);
		buttonNodeList.Clear ();

		if (isaacquest == 2) {
			buttonNodeList.Add ("I found them but they refused to leave./isaacquest2");
		}
		if (isaacquest == 3) {
			buttonNodeList.Add ("I convinced them to come back./isaacquest3");
		}
		buttonNodeList.Add ("Not yet./exit");
		DialogueNode IsaacPersistent = new DialogueNode ("isaacpersistent", "Isaac looks down at you and says, 'Did you find Jerry and Alicia?'", buttonNodeList);
		dialogdictionary.Add ("isaacpersistent", IsaacPersistent);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Goodbye./exit");
		DialogueNode IsaacPersistent2 = new DialogueNode ("isaacpersistent2", "'Hope you're doing well, good luck to you.'", buttonNodeList);
		dialogdictionary.Add ("isaacpersistent2", IsaacPersistent2);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("But they're helping people./isaacquest2disagree");
		buttonNodeList.Add ("I'll get right on it./exit");
		DialogueNode IsaacQuest2 = new DialogueNode ("isaacquest2", "Isaac grimmaces and says, 'Well I'm not paying you unless I see them here in town. Bring them " +
			"back or no deal. I don't care what you have to do, they're not safe in there.'", buttonNodeList);
		dialogdictionary.Add ("isaacquest2", IsaacQuest2);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("I'll get right on it./exit");
		DialogueNode IsaacQuest2Disagree = new DialogueNode ("isaacquest2disagree", "Isaac crosses his arms and says, 'They won't help anyone if they're dead. Now " +
			"go and bring them back. Force them if you have to.'", buttonNodeList);
		dialogdictionary.Add ("isaacquest2disagree", IsaacQuest2Disagree);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("I'll get right on it./exit");
		DialogueNode IsaacQuest3 = new DialogueNode ("isaacquest3", "Isaac beams and says, 'Good, good. I'll keep them out of the way when the invaders show up.' He " +
			"claps you on the back and says, 'Feel proud son, you've saved two lives today, and two good ones at that.' He rummages through his pockets and produces " +
			"a bag of coins. 'Here, for a job well done. Hopefully this will help you afford the bribe you'll need to get across.'", buttonNodeList);
		dialogdictionary.Add ("isaacquest3", IsaacQuest3);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Sure./isaactalkquest");
		buttonNodeList.Add ("Maybe for a little money.../isaactalkquest");
		buttonNodeList.Add ("Let me talk to you about that later./exit");
		DialogueNode IsaacTalk2 = new DialogueNode ("isaactalk2", "'Alright, alright.' Isaac nods. 'If you're going into the caves to get across " +
			"I was wondering if you could do me a favor on the way.' He slumps his shoulders a little. 'My sons and I have been trying to keep order " +
			"in this town, since the war started. It hasn't been easy, but we're more than well equipped enough to deal with the few trouble makers " +
			"that have come through here. No big gangs want to start anything so close to the border, at least they're not desperate enough to yet.' Still, " +
			"I don't have the manpower to keep things safe inside of the caves, and a couple of old friends of mine went in and never came back. Could you " +
			"find them?' A shadow crosses his face. 'Either way, you know. Get them to come back to town if you can, or just make sure they're safe. If uh, if " +
			"that's possible.'", buttonNodeList);
		dialogdictionary.Add ("isaactalk2", IsaacTalk2);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("I have another question./isaacloop");
		DialogueNode IsaacTalkQuest = new DialogueNode ("isaactalkquest", "Isaac smirks and says, 'You'll be paid, of course. Lord knows everyone around here " +
			"is clamoring for cash, especially if they want to get across the border. Those guards know a good grift when they see one, and they'll get every " +
			"penny they think they can get out of you, and otherwise you can just wait for the invaders to show up with the rest of us. Just come back when you've " +
			"found them. They're a middle aged couple, walked into the caves for no apparent reason yesterday. The man's named Jerry, keeps his head shaved. The woman's " +
			"named Alicia and she's not very talkative, has a scar across her cheek from a chicken that pecked her real badly.' Isaac pats you on the shoulder. 'Godspeed.'", buttonNodeList);
		dialogdictionary.Add ("isaactalkquest", IsaacTalkQuest);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Do you need any help?/isaactalk2");
		buttonNodeList.Add ("What's up with that guy Spicky?/isaactalkspicky");
		buttonNodeList.Add ("Can you help me get across the border?/isaactalkhelpborder");
		buttonNodeList.Add ("I have to go./exit");
		DialogueNode IsaacLoop = new DialogueNode ("isaacloop", "'Sure, shoot.'", buttonNodeList);
		dialogdictionary.Add ("isaacloop", IsaacLoop);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Let me ask you something else./isaacloop");
		buttonNodeList.Add ("I have to go./exit");
		DialogueNode IsaacTalkSpicky = new DialogueNode ("isaactalkspicky", "'Oh, he's just a local troublmaker.' Isaac shakes his head sadly. 'I try to help him " +
			"out, even offered to let him stay if he cleaned up his act. Hasn't done him any good. He just wants to hang around here and try to swindle as much " +
			"money from travelers as he can, he got the idea from the guards on the border doing the same thing, but they're soldiers. Spicky is just a single kid, " +
			"and if he keeps things up some desperate person is going to show him the error of his ways while I'm not around. As much as I try to keep him " +
			"from getting himself killed, there's only so much I can do.'", buttonNodeList);
		dialogdictionary.Add ("isaactalkspicky", IsaacTalkSpicky);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Let me ask you something else./isaacloop");
		buttonNodeList.Add ("I have to go./exit");
		DialogueNode IsaacTalkHelpBorder = new DialogueNode ("isaactalkhelpborder", "Isaac forces a chuckle and says, 'I wish I could, I really do. We can " +
			"barely help ourselves around here. Me and my sons are trying to keep the peace in town, and we can hardly do that with all the travelers coming through, " +
			"not all are friendly. We can't help anyone in the caves or to the border.'", buttonNodeList);
		dialogdictionary.Add ("isaactalkhelpborder", IsaacTalkHelpBorder);
		buttonNodeList.Clear ();

		// Town merchant nodes
		buttonNodeList.Add ("Do you have any medical supplies?/merchantmedicine");
		buttonNodeList.Add ("Do you have any weapons?/merchantweapon");
		buttonNodeList.Add ("Do you have any armor?/merchantarmor");
		buttonNodeList.Add ("Why haven't you crossed the border?/merchantborder");
		buttonNodeList.Add ("Maybe some other time./exit");
		DialogueNode Merchant = new DialogueNode ("merchant", "Despite the general dreariness of his surroundings, this person seems unnaturally cheery. He sports " +
			"a broad, forced grin and bows deeply to you. 'Hello my friend,' he says. 'It is good to make your acquaintance on this fine day. I am a merchant, stranded " +
			"here by misfortune, but my misfortune can be your fortune! While you are here why not take a look at what I have to offer? I'm sure you won't be " +
			"disappointed.'", buttonNodeList);
		dialogdictionary.Add ("merchant", Merchant);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Do you have any medical supplies?/merchantmedicine");
		buttonNodeList.Add ("Do you have any weapons?/merchantweapon");
		buttonNodeList.Add ("Do you have any armor?/merchantarmor");
		buttonNodeList.Add ("Maybe some other time./exit");
		DialogueNode MerchantBorder = new DialogueNode ("merchantborder", "The merchant's forcefully cheery face darkens for a moment. He says, 'Those guards " +
			"over there are extortionists by all accounts, and that's if you even make it to them. Nobody around here seems to be willing to make the trip " +
			"with so many bandits and creatures lurking in those tunnels. It seems there's no remorse for hard working people anymore.'" +
			"\n" +
			"\n" +
			"Then the fake smile appears on the merchant's face again. 'Well, no need for all of that dreariness, can I get you anything?'", buttonNodeList);
		dialogdictionary.Add ("merchantborder", MerchantBorder);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Do you have any medical supplies?/merchantmedicine");
		buttonNodeList.Add ("Do you have any weapons?/merchantweapon");
		buttonNodeList.Add ("Do you have any armor?/merchantarmor");
		buttonNodeList.Add ("Why haven't you crossed the border?/merchantborder");
		buttonNodeList.Add ("Maybe some other time./exit");
		DialogueNode MerchantLoop = new DialogueNode ("merchantloop", "'What can I help you with?'", buttonNodeList);
		dialogdictionary.Add ("merchantloop", MerchantLoop);
		buttonNodeList.Clear ();

		if (money >= 20) {
			buttonNodeList.Add ("Sure, I'll take one./merchantmedicinebuy");
		} else {
			buttonNodeList.Add ("Sure, I'll take one./merchantcantafford");
		}
		buttonNodeList.Add ("Let's discuss something else./merchantloop");
		buttonNodeList.Add ("Maybe some other time./exit");
		DialogueNode MerchantMedicine = new DialogueNode ("merchantmedicine", "The merchant says, 'I have some good bandages and salves, very potent ones. Very " +
			"potent and very useful in these troubling times. One packet, which is sufficient to assuage your wounds a great deal, goes for the low price of " +
			"twenty bank notes!' He flourishes one of his hands through the air for effect.", buttonNodeList);
		dialogdictionary.Add ("merchantmedicine", MerchantMedicine);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Let's discuss something else./merchantloop");
		buttonNodeList.Add ("Thanks, see you around./exit");
		DialogueNode MerchantMedicineBuy = new DialogueNode ("merchantmedicinebuy", "The merchant produces a wrapped package of bandages and strong-smelling " +
		                                  "cream. You've seen this stuff before and what this man is selling you is completely legitimate. The healing power of the stuff is close to " +
		                                  "supernatural, and works in seconds. 'Would you like another?' the merchant inquires, 'Or something else, perhaps?'", buttonNodeList);
		dialogdictionary.Add ("merchantmedicinebuy", MerchantMedicineBuy);
		buttonNodeList.Clear ();
		
			
		buttonNodeList.Add ("Let's discuss something else./merchantloop");
		buttonNodeList.Add ("I should go./exit");
		DialogueNode MerchantCantAfford = new DialogueNode ("merchantcantafford", "'Oh,' the merchant says, a pained look on his face, 'You don't have the funds " +
			"for that? Well maybe there's something cheaper you would like?'", buttonNodeList);
		dialogdictionary.Add ("merchantcantafford", MerchantCantAfford);
		buttonNodeList.Clear ();

		if (money >= 40) {
			buttonNodeList.Add ("Sure, let's do it./merchantweaponbuy");
		} else {
			buttonNodeList.Add ("Sure, let's do it./merchantcanotafford");
		}
		buttonNodeList.Add ("Let's discuss something else./merchantloop");
		buttonNodeList.Add ("Maybe some other time./exit");
		DialogueNode MerchantWeapon = new DialogueNode ("merchantweapon", "The merchant taps his chin. 'Well,' he says. 'I'm basically sold out of those, or to be " +
			"honest, thieved out. Great demand for weapons these days, especially by the... less scrupulous types. Tell you what. I see you have a crude, unreliable club there " +
			"a little time, some parts and honest labor and I can put a grip on it, and some spikes too. It will certainly be a nasty invitation to any aggressor! All " +
			"for a nominal fee of forty bank notes.'", buttonNodeList);
		dialogdictionary.Add ("merchantweapon", MerchantWeapon);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Let's discuss something else./merchantloop");
		buttonNodeList.Add ("Thanks, see you around./exit");
		DialogueNode MerchantWeaponBuy = new DialogueNode ("merchantweaponbuy", "The merchant accepts your wepaon, hops into his wagon, and several minutes of loud noises later he " +
			"emerges with a spiked club in hand. He hands it to you, gingerly. 'Can't help you any other way in the weapons department, I'm afraid, but you'll find " +
			"I also have many other fine items.'", buttonNodeList);
		dialogdictionary.Add ("merchantweaponbuy", MerchantWeaponBuy);
		buttonNodeList.Clear ();

		if (money >= 100) {
			buttonNodeList.Add ("Sure, let's do it./merchantarmorbuy");
		} else {
			buttonNodeList.Add ("Sure, let's do it./merchantcantafford");
		}
		buttonNodeList.Add ("Let's discuss something else./merchantloop");
		buttonNodeList.Add ("Maybe some other time./exit");
		DialogueNode MerchantArmor = new DialogueNode ("merchantarmor", "The merchant shrugs and says, 'Just like weapons, good protection is in high demand. " +
			"I have a single chainmail shirt, but with personal safety being at a premium, I could only part with it for a hundred bank notes, though it is well " +
			"worth it, I assure you.'", buttonNodeList);
		dialogdictionary.Add ("merchantarmor", MerchantArmor);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Let's discuss something else./merchantloop");
		buttonNodeList.Add ("Thanks, see you around./exit");
		DialogueNode MerchantArmorBuy = new DialogueNode ("merchantarmorbuy", "The merchant steps into his wagon, rummages around for a while, and emerges " +
			"with a suit of chainmail. To his credit it doesn't have a spot of rust on it. 'Here you are,' he says. 'I hope it serves you well, is there anything " +
			"else I can interest you in?'", buttonNodeList);
		dialogdictionary.Add ("merchantarmorbuy", MerchantArmorBuy);
		buttonNodeList.Clear ();

		// Here are a collection of nodes that do not connect into intricate dialogue encounters

		// The two isaac house guards
		buttonNodeList.Add ("Alright./exit");
		DialogueNode IsaacHouseGuard = new DialogueNode ("isaachouseguard", "A big, flat-nosed man carrying a long knife gives you a tired " +
			"look and says, 'Hey, don't rob anyone even if you have to.'", buttonNodeList);
		dialogdictionary.Add ("isaachouseguard", IsaacHouseGuard);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Thanks!/exit");
		DialogueNode IsaacHouseGuard2 = new DialogueNode ("isaachouseguard2", "You greet the guard, and he greets you back, saying, 'You seem like a good " +
			"sort, headed for the border? Look, it isn't much, but take this. We don't have a whole lot, but soon we'll likely have nothing. Those southern " +
			"bastards will likely take it all.'", buttonNodeList);
		dialogdictionary.Add ("isaachouseguard2", IsaacHouseGuard2);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Alright./exit");
		DialogueNode IsaacHouseGuard2Helped = new DialogueNode ("isaachouseguard2helped", "Isaac's guard nods respecfully and says, 'Take care of yourself " +
			"out there.'", buttonNodeList);
		dialogdictionary.Add ("isaachouseguard2helped", IsaacHouseGuard2Helped);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Alright./exit");
		DialogueNode IsaacHouseGuard2Bad = new DialogueNode ("isaachouseguard2bad", "The big man squints at you and says, 'Watch yourself.'", buttonNodeList);
		dialogdictionary.Add ("isaachouseguard2bad", IsaacHouseGuard2Bad);
		buttonNodeList.Clear ();

		// The corpse in the first room of the cave
		buttonNodeList.Add ("Take the Gem./cavecorpsegem");
		buttonNodeList.Add ("Best leave this alone./exit");
		DialogueNode CaveCorpse = new DialogueNode ("cavecorpse", "As you walk by you notice something sticking through this pile of whithered bones. It " +
			"must have been here for a while, and you're shocked that nobody else thought to look through them. If you're making it out right there is " +
			"a small gem sitting within the ribcage.", buttonNodeList);
		dialogdictionary.Add ("cavecorpse", CaveCorpse);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Crush the scorpion./cavecorpsegemcrush");
		buttonNodeList.Add ("Try to snatch the gem./cavecorpsegemsnatch");
		buttonNodeList.Add ("Best leave this alone./exit");
		DialogueNode CaveCorpseGem = new DialogueNode ("cavecorpsegem", "You reach your hand in for the gem, but you dart back away as something stings your " +
			"hand. Rubbing your wrist and looking back at the bones you notice a small scorpion sitting beside the gem, tail raised.", buttonNodeList);
		dialogdictionary.Add ("cavecorpsegem", CaveCorpseGem);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Ok./exit");
		DialogueNode CaveCorpseGemCrush = new DialogueNode ("cavecorpsegemcrush", "You easily smash the scorpion, but chip the gemstone with your weapon in the process. It's still " +
			"very nice, but won't be worth as much. You pocket it.", buttonNodeList);
		dialogdictionary.Add ("cavecorpsegemcrush", CaveCorpseGemCrush);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Ok./exit");
		DialogueNode CaveCorpseGemSnatch = new DialogueNode ("cavecorpsegemsnatch", "You move as quickly as you can and snatch the gem, but not before " +
			"the scorpion manages to sting you on the hand again. It smarts, but really it doesn't seem so bad with the weight of the stone in your pocket.", buttonNodeList);
		dialogdictionary.Add ("cavecorpsegemsnatch", CaveCorpseGemSnatch);
		buttonNodeList.Clear ();

		// These are for the wrecked house at the very start of the game
		buttonNodeList.Add ("Ok./exit");
		DialogueNode EnterWreckedHouse = new DialogueNode ("enterwreckedhouse", "This might have been some sort of office once, but now the counters and floors " +
			"are covered in dust and trash. A bed sits in one corner by some worn out chests. Someone must have " +
			"been living in here until recently, but all of the bedsheets are gone. Come to think of it, all the walls and " +
			"surfaces are completely bare.", buttonNodeList);
		dialogdictionary.Add ("enterwreckedhouse", EnterWreckedHouse);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Huh./exit");
		DialogueNode RuinedChests = new DialogueNode ("ruinedchests", "These chests have recently been emptied. " +
			"They are as bare as the rest of the building. If they were not bolted to the floor, you suspect that they " +
			"also would have been taken.", buttonNodeList);
		dialogdictionary.Add ("ruinedchests", RuinedChests);
		buttonNodeList.Clear ();

		// Nodes for spicky if he's still around after the first encounter
		buttonNodeList.Add ("Walk away./exit");
		buttonNodeList.Add ("Attack him./spickytalkingshitfight");
		DialogueNode SpickyTalkingShit = new DialogueNode ("spickytalkingshit", "Spicky looks you up and down and smirks, he strokes the knife " +
			"under his cloak as he says, 'You want to pay up again? Get a move on chump.'", buttonNodeList);
		dialogdictionary.Add ("spickytalkingshit", SpickyTalkingShit);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Attack him./fight");
		DialogueNode SpickyTalkingShitFight = new DialogueNode ("spickytalkingshitfight", "Spicky's knife is out just as quickly as your own weapon. " +
			"Spicky's eyes go wide as you advance. He says, 'Y-You'll r-r-regret this.' and raises his knife.", buttonNodeList);
		dialogdictionary.Add ("spickytalkingshitfight", SpickyTalkingShitFight);
		buttonNodeList.Clear ();

		// Hiding Refugee nodes

		buttonNodeList.Add ("Give me all your money./hidingrefugeerob");
		buttonNodeList.Add ("Could you help me get across the border?/hidingrefugeeborder");
		buttonNodeList.Add ("Do you need any help?/hidingrefugeehelp");
		buttonNodeList.Add ("What are you doing here?/hidingrefugeeask");
		DialogueNode HidingRefugee = new DialogueNode ("hidingrefugee", "You come across three miserable figures hidden in this alcove. They are thin and tired, worn down " +
			"by trecking through the warzone on the way to the border, or maybe something in this cave caused the bite marks and long gashes that they have shoddily " +
			"bandaged. The most hallow and well armed of them takes a step towards you, sword in hand, and says, 'Hi.' He doesn't sound very friendly. The other two " +
			"watch you, wide eyed with fear.", buttonNodeList);
		dialogdictionary.Add ("hidingrefugee", HidingRefugee);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Give me all your money./hidingrefugeerob");
		buttonNodeList.Add ("Could you help me get across the border?/hidingrefugeeborder");
		buttonNodeList.Add ("What are you doing here?/hidingrefugeeask");
		buttonNodeList.Add ("I should get going./exit");
		DialogueNode HidingRefugeeHelp = new DialogueNode ("hidingrefugeehelp", "'We would prefer it if you kept your distance.'", buttonNodeList);
		dialogdictionary.Add ("hidingrefugeehelp", HidingRefugeeHelp);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Give me all your money./hidingrefugeerob");
		buttonNodeList.Add ("Could you help me get across the border?/hidingrefugeeborder");
		buttonNodeList.Add ("Do you need any help?/hidingrefugeehelp");
		buttonNodeList.Add ("I should get going./exit");
		DialogueNode HidingRefugeeAsk = new DialogueNode ("hidingrefugeeask", "Their expressions become grimmer, if that was even possible. 'We were ambushed,' " +
			"says the armed one, 'We took the cave on the left at the fork ahead.' He gulps and his skin flushes. 'Suddenly bandits had us surrounded on both sides, " +
			"we three managed to get out, but well... There used to be more of us.'", buttonNodeList);
		dialogdictionary.Add ("hidingrefugeeask", HidingRefugeeAsk);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Give me all your money./hidingrefugeerob");
		buttonNodeList.Add ("What are you doing here?/hidingrefugeeask");
		buttonNodeList.Add ("Do you need any help?/hidingrefugeehelp");
		buttonNodeList.Add ("I should get going./exit");
		DialogueNode HidingRefugeeBorder = new DialogueNode ("hidingrefugeeborder", "The refugees give each other a look. The well armed one says, 'Not a chance " +
			"stranger. We aren't joining you. We've been fooled before.'", buttonNodeList);
		dialogdictionary.Add ("hidingrefugeeborder", HidingRefugeeBorder);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Cough it up./hidingrefugeerob2");
		buttonNodeList.Add ("I think I'll just fight you./hidingrefugeerob3");
		DialogueNode HidingRefugeeRob = new DialogueNode ("hidingrefugeerob", "'Should have known,' says the well armed refugee, and draws his sword. The others " +
			"look at you, and then at each other. One of them, a very old man, says, 'Couldn't we just give him something to get him away from us?' The armed " +
			"refugee looks back at him and says, 'No, not again. We barely have anything left.' The old man replies, 'We're supposed to fight him and then make our way " +
			"past those bandits? Your plan doesn't seem so foolproof anymore.' The armed one grits his teeth and says, 'I didn't think every living soul in this hellhole " +
			"would be trying to rob us.", buttonNodeList);
		dialogdictionary.Add ("hidingrefugeerob", HidingRefugeeRob);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Free Money!/exit");
		DialogueNode HidingRefugeeRob2 = new DialogueNode ("hidingrefugeerob2", "The old man throws a pouch at you, it jingles with coins. He pleads, 'Please leave.'", buttonNodeList);
		dialogdictionary.Add ("hidingrefugeerob2", HidingRefugeeRob2);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Fight./fight");
		DialogueNode HidingRefugeeRob3 = new DialogueNode ("hidingrefugeerob3", "The refugees draw their weapons as you advance. They probably have money saved up " +
			"for the border guards, and the cutlass that one of them wields looks very nice.", buttonNodeList);
		dialogdictionary.Add ("hidingrefugeerob3", HidingRefugeeRob3);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Ok./exit");
		DialogueNode HidingRefugeePersistent = new DialogueNode ("hidingrefugeepersistent", "'Leave us alone and keep your distance.' These are a very guarded " +
			"bunch.", buttonNodeList);
		dialogdictionary.Add ("hidingrefugeepersistent", HidingRefugeePersistent);
		buttonNodeList.Clear ();

		// Level up nodes
		buttonNodeList.Add ("Ok./caveentrancelevelup2");
		DialogueNode CaveEntranceLevelUp = new DialogueNode ("caveentrancelevelup", "You're here, at the entrance to the cave going to the border. From " +
			"everything you've heard, things will only get harder up ahead, and even if you make it through you'll have to deal with the border guards. Hopefully " +
			"you'll have the money to pay them off. If not, well maybe there will be another way. There has to be, you can't stay here. You glance back at the " +
			"town, standing in the sunlight just beyond the shadows you stand in. Soon, very soon, the invaders will be here. You can see the flames and destruction " +
			"in your minds eye, as you have with your real eyes so many times already. You collect yourself and walk into the darkness, determined not to be " +
			"around when that happens.", buttonNodeList);
		dialogdictionary.Add ("caveentrancelevelup", CaveEntranceLevelUp);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Ok./exit");
		DialogueNode CaveEntranceLevelUp2 = new DialogueNode ("caveentrancelevelup2", "(You've gained a level and two stat points! Go to the character menu by pressing " +
			"enter to add or remove stat points at will. No stat can be lower than 3. Dexterity allows you to act faster in combat and causes enemy attacks to miss " +
			"more frequently, and yours to hit more often. Hardiness makes you take less damage. Strength makes you deal more damage.)", buttonNodeList);
		dialogdictionary.Add ("caveentrancelevelup2", CaveEntranceLevelUp2);
		buttonNodeList.Clear ();

		// Tutorial nodes
		buttonNodeList.Add ("Ok./tutorial2");
		DialogueNode Tutorial = new DialogueNode ("tutorial", "There isn't much time left. It's been a long journey. After a decade of peace invaders from the south have swept through your " +
			"homeland. Now their armies advance on the last speck of your country they do not already control, a strip of land bordering the neighboring nation to " +
			"the north. You have traveled through warzones and ruins to make it across the border. Fleeing into the next country is your only hope at a peaceful " +
			"existence, but if your trek so far has been any indication it won't be easy to make it across. Still, you have to hurry. The invaders are almost here." +
			"\n" +
			"(Use WASD to move.)", buttonNodeList);
		dialogdictionary.Add ("tutorial", Tutorial);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Ok./exit");
		DialogueNode Tutorial2 = new DialogueNode ("tutorial2", "(Welcome to Refuge! Here are a couple of commands you should know. Enter opens up the character " +
			"menu, where you can allocate stat points and change your character as often as you like. There you can also view your level and statistics, as well " +
			"as use healing items when you are below maximum health. Stand next to an object or person and press space to interact with them.)" +
			"\n" +
			"(You already have some stat points, make sure to use them quickly.)", buttonNodeList);
		dialogdictionary.Add ("tutorial2", Tutorial2);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Ok./exit");
		DialogueNode Tutorial3 = new DialogueNode ("tutorial3", "A little further up the road you make out three figures, one is clearly a person, and the other two " +
			"are hunched, quadrapedal forms. They growl and circle the lone person.", buttonNodeList);
		dialogdictionary.Add ("tutorial3", Tutorial3);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Ok./exit");
		DialogueNode Tutorial4 = new DialogueNode ("tutorial4", "You can hear the howling. More wolves are likely waiting for you ahead. You glance at the knife " +
			"you found in the wagon. It might serve better than the club you're currently using. You wouldn't be able to block at attack with it, but you would " +
			"be able to strike much more quickly." +
			"\n" +
			"\n" +
			"(You have recieved a new weapon, the knife. It is a more offensively focused weapon than the club you already have equipped, though it is not necessarily better. To view your equipment " +
			"go to the character menu and select the 'View Equipment' button. There you can select and equip items you have found. After clicking on one you " +
			"can view a short description of its abilities and statistics.)", buttonNodeList);
		dialogdictionary.Add ("tutorial4", Tutorial4);
		buttonNodeList.Clear ();

		// Bandit parts of the cave nodes
		buttonNodeList.Add ("Ok./exit");
		DialogueNode BanditEntrance = new DialogueNode ("banditentrance", "There are clear signs of fighting here. Scratches line the walls and old bloodstains " +
			"are scattered on the floor. Strangely enough the wall to the south is completely clean.", buttonNodeList);
		dialogdictionary.Add ("banditentrance", BanditEntrance);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Press the panel./banditentrancewall2");
		buttonNodeList.Add ("Back Away./exit");
		DialogueNode BanditEntranceWall = new DialogueNode ("banditentrancewall", "Upon closer inspection you notice that there is a small panel in this wall. " +
			"Thinking back to better times, you remember that smugglers used to keep stashes and safehouses in these caves. This might be one of those, but judging " +
			"from the bloodstains in this hallway and the bandits you just fought, you doubt that you are the only one to notice this.", buttonNodeList);
		dialogdictionary.Add ("banditentrancewall", BanditEntranceWall);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Ok./exit");
		DialogueNode BanditEntranceWall2 = new DialogueNode ("banditentrancewall2", "When you press the panel the wall opens inward. You can hear the sounds " +
			"of whispers and feet shuffling down the hallway beyond.", buttonNodeList);
		dialogdictionary.Add ("banditentrancewall2", BanditEntranceWall2);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Ok./exit");
		DialogueNode BanditAltar = new DialogueNode ("banditaltar", "Strangely enough you find a plain wooden altar here. The scuff marks in front of it " +
			"would indicate that it is used often.", buttonNodeList);
		dialogdictionary.Add ("banditaltar", BanditAltar);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Great!/exit");
		DialogueNode BanditSpoils = new DialogueNode ("banditspoils", "This strongbox is full of odds and ends. There are clothes of all different sizes, " +
			"shoes, hairbrushes, cooking utensils, all sorts of things someone might carry while traveling. You can only assume that these were taken " +
			"from refugees. Pawing through the detritis, you find some bandages and a few loose coins.", buttonNodeList);
		dialogdictionary.Add ("banditspoils", BanditSpoils);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Your worst nightmare./banditleaderfight");
		buttonNodeList.Add ("Just a guy wondering what's going on in here./banditleaderexplanation");
		if (nickiquest == 1) {
			buttonNodeList.Add ("Someone who wants justice, I've been sent to kill you./banditleaderisaac");
		}
		buttonNodeList.Add ("Who are you?/banditleaderexplanation");
		buttonNodeList.Add ("I'm just trying to get to the border./banditleaderexplanation");
		DialogueNode BanditLeader = new DialogueNode ("banditleader", "Standing in the middle of this room is a very tall, muscular woman dressed in rusted " +
			"chainmail. She waves a cutlass in your direction and demands, 'Who are you?'", buttonNodeList);
		dialogdictionary.Add ("banditleader", BanditLeader);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Of course, for freedom!/banditleaderdonation");
		buttonNodeList.Add ("I don't give money to bandits./banditleaderfight");
		DialogueNode BanditLeaderExplanation = new DialogueNode ("banditleaderexplanation", "She stands at attention and says, 'I am Lieutenant Smolsk, leader " +
			"of the first resistance front. From here we will make the invader pay for every step they ever took on our native soil. We will be the force that " +
			"rescues our people and creates a free nation from the ashes!' She points her cutlass at you again. 'So if you are a true patriot you will give " +
			"a donation to our cause. The first resistance front needs donations to take this country back!'", buttonNodeList);
		dialogdictionary.Add ("banditleaderexplanation", BanditLeaderExplanation);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("I should be going./banditleaderexit");
		DialogueNode BanditLeaderDonation = new DialogueNode ("banditleaderdonation", "The Lieutenant takes your money with a smile. 'I see I've found a kindred " +
			"spirit in these tough times.'", buttonNodeList);
		dialogdictionary.Add ("banditleaderdonation", BanditLeaderDonation);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("You're nothing but a bandit./banditleaderfight");
		buttonNodeList.Add ("I'll say you're dead for some money./banditleaderbribe");
		buttonNodeList.Add ("I won't fight you if you will restore our country./banditleaderexit");
		DialogueNode BanditLeaderIsaac = new DialogueNode ("banditleaderisaac", "The woman rolls her eyes and says, 'I assume Isaac sent you to try and get rid of " +
			"me? Isaac is a smallminded fool. He probably doesn't even know who I am.' She sweeps her arm around the room, maybe she is gesturing at the many stolen " +
			"goods scattered around it. 'We are building an outpost for the liberation of our country.' She raises her cutlass to her forehead and salutes, 'I am " +
			"Lieutenant Smolsk, leader of the first resistance front! From here we will make the invader pay for every step they ever took on our native soil. We will be the force that " +
			"rescues our people and creates a free nation from the ashes!' She points her cutlass at you and says, 'So tell me, do you really want to kill your " +
			"only hope at turning things back to the way they were?'", buttonNodeList);
		dialogdictionary.Add ("banditleaderisaac", BanditLeaderIsaac);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Whoops./banditleaderfight");
		DialogueNode BanditLeaderBribe = new DialogueNode ("banditleaderbribe", "'You would ask that I take funds from the preservation of our country just to " +
			"get some bumpkin off of my back?' She looks absolutely horrified. 'You scoundrel, no real patriot can abide you being here. You're probably a spy " +
			"too!'", buttonNodeList);
		dialogdictionary.Add ("banditleaderbribe", BanditLeaderBribe);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Ok./exit");
		DialogueNode BanditLeaderExit = new DialogueNode ("banditleaderexit", "The Lieutenant grabs a military issue rucksack from behind a nearby barrel " +
			"'This location is no longer secure, so I will go. I wish you the best of luck, fight for our country wherever you go.' She rushes out of the room " +
			"with a wave, leaving you alone.", buttonNodeList);
		dialogdictionary.Add ("banditleaderexit", BanditLeaderExit);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("No, no wait./banditleaderfightwait");
		buttonNodeList.Add ("Let's do this./banditleaderfightaggressive");
		DialogueNode BanditLeaderFight = new DialogueNode ("banditleaderfight", "She regards you with a cold stare and advances, cutlass at the ready. 'So it's a fight " +
			"you want then? Well you'll get the best one you've ever had.'", buttonNodeList);
		dialogdictionary.Add ("banditleaderfight", BanditLeaderFight);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Draw your weapon./fight");
		DialogueNode BanditLeaderFightWait = new DialogueNode ("banditleaderfightwait", "Lieutenant Smolsk laughs and says, 'The time for talking with all enemies " +
			"of our free nation is over.' She charges.", buttonNodeList);
		dialogdictionary.Add ("banditleaderfightwait", BanditLeaderFightWait);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Draw your weapon./fight");
		DialogueNode BanditLeaderFightAggressive = new DialogueNode ("banditleaderfightaggressive", "Lieutnant Smolsk grins like a wolf and cries, 'For freedom!'" +
			" She charges.", buttonNodeList);
		dialogdictionary.Add ("banditleaderfightaggressive", BanditLeaderFightAggressive);
		buttonNodeList.Clear ();

		// Beggar right before the border post nodes
		buttonNodeList.Add ("Give him some change./beggargive");
		buttonNodeList.Add ("Keep Walking./beggarignore");
		buttonNodeList.Add ("Rob him./beggarrob");
		DialogueNode Beggar = new DialogueNode ("beggar", "You almost walk right past what you think is another pile of discarded rags, until a pitiful voice " +
			"emerges from the dirty clothing stacked against the wall of the cave. 'Some change, please? Spare a little change?' You stop and notice a dirt covered " +
			"face poking out from under the clothes, and then a hand pops out right under it. 'Please, just a little.'", buttonNodeList);
		dialogdictionary.Add ("beggar", Beggar);
		buttonNodeList.Clear ();

		if (money >= 5) {
			buttonNodeList.Add ("Stop and give him some money./beggargive");
		} else {
			buttonNodeList.Add ("Sorry, don't have any./exit");
		}
		buttonNodeList.Add ("Leave./exit");
		buttonNodeList.Add ("Rob him./beggarrob");
		DialogueNode BeggarIgnore = new DialogueNode ("beggarignore", "You keep walking, and the beggar pleads vainly, 'Just a little sir, I only need a little " +
			"to get across, have mercy.'", buttonNodeList);
		dialogdictionary.Add ("beggarignore", BeggarIgnore);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Wow./exit");
		DialogueNode BeggarRob = new DialogueNode ("beggarrob", "You pull out your weapon and point it at the man and say, 'All your money, right now.' The beggar's " +
			"jaw drops in disbelief. 'You... you'd rob me? Just a beggar? I have hardly anything.' A hand emerges from the rags, clutching some crumpled bills and " +
			"coins. The beggar clenches his jaw and says, 'Take it and choke on it.' Then his eyes start to water. 'I was so close,' the beggar says. 'So close " +
			"to living.' The tears are slow at first, and then right in front of you a fully grown man starts to wail and cry. 'I was going to get across,' he screams. '" +
			"I was going to live!' You slip away before it gets any worse.", buttonNodeList);
		dialogdictionary.Add ("beggarrob", BeggarRob);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("How much more do you need?/beggargive2");
		buttonNodeList.Add ("What do you know about the border outpost?/beggarborder");
		DialogueNode BeggarGive = new DialogueNode ("beggargive", "The dirty face smiles and says, 'Thank you so much. Just a little more and I can finally " +
			"get out of here. The outpost is right past me, but they won't let me through without a little more money. " +
			"I'm right here, sitting in this freezing cave, and so close. Can't tell you how frustrating that is.'", buttonNodeList);
		dialogdictionary.Add ("beggargive", BeggarGive);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Best of luck to you too./exit");
		DialogueNode BeggarBorder = new DialogueNode ("beggarborder", "'Ah,' the beggar says. 'I know all about the border, been living outside of it for days now. Let me give you a tip. The guards will ask " +
			"for some amount based off of what you look like. They didn't ask too much of me, but I saw some rich type pass through a day or two ago, and they took " +
			"everything he had off of him. He almost couldn't afford it. Take some of my rags and make yourself look poor before you actually walk up to the guards. " +
			"They'll ask for less, I guarantee it.' He hands you some smelly clothes, which you accept. 'You should probably get going now, best of luck.'", buttonNodeList);
		dialogdictionary.Add ("beggarborder", BeggarBorder);
		buttonNodeList.Clear ();

		if (money >= 20) {
			buttonNodeList.Add ("Sure, take it./beggargive3");
		} else {
			buttonNodeList.Add ("I'm afraid not./beggargivecant");
		}
		DialogueNode BeggarGive2 = new DialogueNode ("beggargive2", "A mixture of hope and disbelief glimmers in the man's watery eyes. 'Just twenty banknotes, twenty " +
			"and I'll be safe. You wouldn't... you wouldn't happen to have twenty banknotes, would you?'", buttonNodeList);
		dialogdictionary.Add ("beggargive2", BeggarGive2);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Thanks, best of luck./beggargive4");
		DialogueNode BeggarGive3 = new DialogueNode ("beggargive3", "The beggar takes the money with a shaking hand, which dissapears underneath the rags. 'I... " +
			"I can't believe it.' he says. 'Part of me never really believed I'd get out of this cave, but... it's finally happened. I'm finally free. They won't " +
			"get me like they got Aunty and Barbara. I made it.' Tears start to form in his rheumy eyes. 'I made it, I told them I would.' He stands up on shaky " +
			"legs. You can see just how emaciated and filthy he is. There are scabs all over him, which poke out through the holes in the raggedy shirt he wears. " +
			"The beggar takes your hands in his and shakes them vigorously. 'You are my savior, I can never repay you, but let me tell you what I know. If you want " +
			"to cross the border make yourself look poor. Take some of these clothes here. They don't ask for much from poor folk, because they know we can't afford " +
			"much.'", buttonNodeList);
		dialogdictionary.Add ("beggargive3", BeggarGive3);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Interesting./exit");
		DialogueNode BeggarGive4 = new DialogueNode ("beggargive4", "The beggar glances down the hallway behind him, and leans in conspiratorially. He smells " +
			"horrible. 'There's another way, an old smuggler's passage. I couldn't get through it because there's a bear sitting in it, but you might be able to. " +
			"I've been sitting in front of it this whole time, trying to work up the courage to sneak past. Found it totally by accident, wasn't a great backup " +
			"plan though.' He pats you on the shoulder and says, 'I'd recommend going through the outpost if you can, that bear is a nasty one. Thanks again, maybe " +
			"I'll see you on the other side.' He shoots down the hallway like a cannon, joints cracking.", buttonNodeList);
		dialogdictionary.Add ("beggargive4", BeggarGive4);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Thanks, best of luck./exit");
		DialogueNode BeggarGiveCant = new DialogueNode ("beggargivecant", "The glimmer dies, replaced once more by stubborn dissapointment. 'Well, I still appreciate " +
			"what you've done for me. The people that actually make it this far are almost always too caught up in making sure they can afford to get across that they " +
			"don't spare anything for me.' He sighs and shrinks into his pile of clothes. 'Well, before you go, let me give you a tip. The guards will ask " +
			"for some amount based off of what you look like. They didn't ask too much of me, but I saw some rich type pass through a day or two ago, and they took " +
			"everything he had off of him. He almost couldn't afford it. Take some of my rags and make yourself look poor before you actually walk up to the guards. " +
			"They'll ask for less, I guarantee it.' He hands you some smelly clothes, which you accept.", buttonNodeList);
		dialogdictionary.Add ("beggargivecant", BeggarGiveCant);
		buttonNodeList.Clear ();

		if (money >= 5) {
			buttonNodeList.Add ("Stop and give him some money./beggargive");
		} else {
			buttonNodeList.Add ("Sorry, don't have any./exit");
		}
		buttonNodeList.Add ("Ignore him./exit");
		buttonNodeList.Add ("Rob him./beggarrob");
		DialogueNode BeggarPersistent = new DialogueNode ("beggarpersistent", "The beggar's dirty visage pokes out from under the rags once more and says, 'Some " +
			"change?'", buttonNodeList);
		dialogdictionary.Add ("beggarpersistent", BeggarPersistent);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Should probably leave him alone./exit");
		DialogueNode BeggarPissed = new DialogueNode ("beggarpissed", "The beggar glares at you, tears not dry on his cheeks. You suspect he would attack if he " +
			"had the strength to.", buttonNodeList);
		dialogdictionary.Add ("beggarpissed", BeggarPissed);
		buttonNodeList.Clear ();

		// Secret passage node
		buttonNodeList.Add ("Okay./exit");
		DialogueNode SecretPassage = new DialogueNode ("secretpassage", "Just like the beggar said, there is a button hidden at the base of this wall, underneath " +
			"all of the rags. You press it and the wall slides open, revealing a narrow passageway.", buttonNodeList);
		dialogdictionary.Add ("secretpassage", SecretPassage);
		buttonNodeList.Clear ();

		//Nodes for wagon encounter
		buttonNodeList.Add ("Okay./exit");
		DialogueNode Wagon = new DialogueNode ("wagon", "You come across a common sight, an abandoned wagon in the road. As far as you see there's no sign " +
			"of the occupants except for a couple of crushed arrows and a good amount of broken junk laying around. The wagon itself is broken beyond repair. Not only " +
			"is it full of cracks and tears, but from the way its sitting you can tell that one of the axels has snapped. You don't hear anything, and nobody seems " +
			"to be waiting in ambush. It might be worth looking inside." +
			"\n" +
			"\n" +
			"(To interact with an object or person, such as this wagon, move next to it and press the spacebar.)", buttonNodeList);
		dialogdictionary.Add ("wagon", Wagon);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Woa./wagonsearch2");
		DialogueNode WagonSearch = new DialogueNode ("wagonsearch", "You creep towards the wagon, club drawn, and lift the flap. The scene inside is not as grisly " +
			"as you might have guessed it would be. There is more junk scattered about, most of it useless. However, you find five coins tucked in a discarded shoe. " +
			"After you pocket the coins you notice a trail of dried blood running into the back of the wagon. Before you have a chance to react, a wolf slinks out " +
			"from behind a box. It has a large knife stuck in its side. Despite it's grisly wound it snarls and leaps at you. The wolves around here are particuarily " +
			"tough and dangerous.", buttonNodeList);
		dialogdictionary.Add ("wagonsearch", WagonSearch);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Defend yourself./fight");
		DialogueNode WagonSearch2 = new DialogueNode ("wagonsearch2", "(Looks like you're in for a fight. To attack an enemy press the attack button and then click on their name on the red " +
			"panel that will be on the left. To use abilities click the ability button and then select whichever one you wish to use, then select an enemy. That is, " +
			"unless you are using a 'focus', an ability that is always available. Focus will increase your lowest stat by 2 for the duration of that combat, but you can " +
			"only use it once per fight.)" +
			"\n" +
			"\n" +
			"(Most importantly, to block press the space key right as the enemy attacks you, after the flashes. You might not get the timing right, but if you " +
			"do they will deal reduced damage to you. Remember that some enemy attacks are faster or slower, so keep an eye on them as they flash to attack.)", buttonNodeList);
		dialogdictionary.Add ("wagonsearch2", WagonSearch2);
		buttonNodeList.Clear ();

		// Nicki's nodes
		buttonNodeList.Add ("Hello?/nicki2");
		buttonNodeList.Add ("Uhhhh, bye./exit");
		DialogueNode Nicki = new DialogueNode ("nicki", "You approach a woman seated cross legged on the floor with her eyes closed. Her face is covered in dark " +
			"bandages, which ooze red in several places. Her hands shake in her lap.", buttonNodeList);
		dialogdictionary.Add ("nicki", Nicki);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Maybe I'll do for what?/nickiwhat");
		buttonNodeList.Add ("Who are you?/nickiwho");
		buttonNodeList.Add ("Uhhhh, bye./exit");
		DialogueNode Nicki2 = new DialogueNode ("nicki2", "Her eyes shoot open. Just looking at them makes you stop in your tracks. They blaze like metors, filled " +
			"with a singular hatred and determination like you have never seen before. Her brow furrows and her eyes narrow. Her voice is husky, gruff, like she " +
			"just inhaled a lungful of smoke. 'Maybe you'll do.'", buttonNodeList);
		dialogdictionary.Add ("nicki2", Nicki2);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Who?/nickiwho2");
		buttonNodeList.Add ("Woa, is that really necessary?/nickidenied");
		buttonNodeList.Add ("Uhhhh, bye./exit");
		DialogueNode NickiWho = new DialogueNode ("nickiwho", "She stares right into your eyes, never flinching or looking away. It's very unsettling. 'Someone with " +
			"a very lucrative job for you. Someone who is willing to give away every cent they own, and in doing so any chance of getting out of this miserable " +
			"country, to see someone dead and gone.'", buttonNodeList);
		dialogdictionary.Add ("nickiwho", NickiWho);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Who?/nickiwho2");
		buttonNodeList.Add ("Woa, is that really necessary?/nickidenied");
		buttonNodeList.Add ("Uhhhh, bye./exit");
		DialogueNode NickiWhat = new DialogueNode ("nickiwhat", "She stares right into your eyes, never flinching or looking away. It's very unsettling. 'I " +
			"have a very lucrative job for you. Kill someone, no questions asked, and I give you every banknote I own.'", buttonNodeList);
		dialogdictionary.Add ("nickiwhat", NickiWhat);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("What did she do to you?/nickidotoyou");
		buttonNodeList.Add ("I'll do it./nickidoit");
		buttonNodeList.Add ("I won't do it./nickidontdoit");
		DialogueNode NickiWho2 = new DialogueNode ("nickiwho2", "The hatred in her eyes subsides for only a moment, you've never seen somebody so angry. 'Good, " +
			"I can see I didn't pick badly. Now, in the caves that lead to the border you'll find some bandits. Do with them whatever you like, I only care " +
			"about their leader. She's a powerful woman and a strong fighter, but slow. She wields a cutlass. I would recommend getting some chainmail if you " +
			"can find some. Her name is Lieutenant Smolsk and she robs passerby while claiming to fight for our freedom. She's the worst hyppocrite.' The woman's " +
			"voice starts to rise. 'She kills those she claims to want to help, and worst of all doesn't even seem to think she is in the wrong at all. She will " +
			"attack anyone who has any money and isn't willing to part. She...' The woman trails off and for the first time since her eyes opened, looks away. 'Get rid " +
			"of her and return here. You can have every last cent I own. Just make sure it's done, it must be done.'", buttonNodeList);
		dialogdictionary.Add ("nickiwho2", NickiWho2);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("I'll do it./nickidoit");
		buttonNodeList.Add ("I won't do it./nickidontdoit");
		DialogueNode NickiDoToYou = new DialogueNode ("nickidotoyou", "She looks back at you and glares, 'Will you do it or not?'", buttonNodeList);
		dialogdictionary.Add ("nickidotoyou", NickiDoToYou);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Okay./exit");
		DialogueNode NickiDoIt = new DialogueNode ("nickidoit", "She closes her eyes and says, 'Good, return when it is done.' Before you walk away, you notice " +
			"that her hands have stopped shaking.", buttonNodeList);
		dialogdictionary.Add ("nickidoit", NickiDoIt);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Bye./exit");
		DialogueNode NickiDontDoIt = new DialogueNode ("nickidontdoit", "She closes her eyes and says, 'Then leave, and think about the injustices going on " +
			"in your path to the border, not to mention the money you would make by stopping them.'", buttonNodeList);
		dialogdictionary.Add ("nickidontdoit", NickiDontDoIt);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("No, wait I'll do it./nickidenied2");
		buttonNodeList.Add ("Well, bye then./exit");
		DialogueNode NickiDenied = new DialogueNode ("nickidenied", "She closes her eyes and says, 'I can see you're not the one to do this. You can go.'", buttonNodeList);
		dialogdictionary.Add ("nickidenied", NickiDenied);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Well, bye then./exit");
		DialogueNode NickiDenied2 = new DialogueNode ("nickidenied2", "She doesn't respond.", buttonNodeList);
		dialogdictionary.Add ("nickidenied2", NickiDenied2);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Damn./exit");
		DialogueNode NickiGone = new DialogueNode ("nickigone", "You're surprised to find that nobody is here. Instead of the woman who hired you there is a " +
			"small box on the table with a note attatched. It reads, 'As promised, in here you will find all the money I own.' You open it. It is empty.", buttonNodeList);
		dialogdictionary.Add ("nickigone", NickiGone);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Okay./exit");
		DialogueNode NickiPersistent = new DialogueNode ("nickipersistent", "She ignores you.", buttonNodeList);
		dialogdictionary.Add ("nickipersistent", NickiPersistent);
		buttonNodeList.Clear ();

		// Nodes for Jerry and Alicia
		buttonNodeList.Add ("What are you doing here?/jerryaliciawhat");
		buttonNodeList.Add ("What's the deal with the statue?/jerryaliciastatue");
		buttonNodeList.Add ("Could you help me get to the border?/jerryaliciaborder");
		buttonNodeList.Add ("Give me all your money./jerryaliciarob");
		if (isaacquest == 1) {
			buttonNodeList.Add ("Isaac wants you to come back to town./jerryaliciaisaac");
		}
		if (talkedtojerry == 1) {
			buttonNodeList.Add ("Isaac wants you to come back to town./jerryaliciaisaac");
		}
		buttonNodeList.Add ("I need to go./exit");
		DialogueNode JerryAlicia = new DialogueNode ("jerryalicia", "You're surprised to find a middle aged couple with pleasant smiles in this cave. They are standing " +
			"in front of a large statue depicting a warrior with a sword laid over his outstretched hands. The man, who is bald, approaches you with an outstretched hand " +
			"and says, 'Hello there, my name is Jerry. This is my wife Alicia.' Alicia looks at you with kind, curious eyes and nods. You notice that she has a " +
			"large scar across her cheek. You, still in a state close to shock, shake the man's hand and introduce yourself.", buttonNodeList);
		dialogdictionary.Add ("jerryalicia", JerryAlicia);
		buttonNodeList.Clear ();

		/*buttonNodeList.Add ("What are you doing here?/jerryaliciawhat");
		buttonNodeList.Add ("What's the deal with the statue?/jerryaliciastatue");
		buttonNodeList.Add ("Could you help me get to the border?/jerryaliciaborder");
		buttonNodeList.Add ("Give me all your money./jerryaliciarob");
		buttonNodeList.Add ("I need to go./exit");
		DialogueNode JerryAliciaLoop = new DialogueNode ("jerryalicialoop", "Jerry beams. 'Hope you're doing well!'", buttonNodeList);
		dialogdictionary.Add ("jerryalicialoop", JerryAliciaLoop);
		buttonNodeList.Clear ();*/

		if (isaacquest == 1) {
			buttonNodeList.Add ("Isaac wants you to come back to town./jerryaliciaisaac");
		}
		if (talkedtojerry == 1) {
			buttonNodeList.Add ("Isaac wants you to come back to town./jerryaliciaisaac");
		}
		buttonNodeList.Add ("What are you doing here?/jerryaliciawhat");
		buttonNodeList.Add ("What's the deal with the statue?/jerryaliciastatue");
		buttonNodeList.Add ("Could you help me get to the border?/jerryaliciaborder");
		buttonNodeList.Add ("Give me all your money./jerryaliciarob");
		buttonNodeList.Add ("I need to go./exit");
		DialogueNode JerryAliciaWhat = new DialogueNode ("jerryaliciawhat", "Alicia and Jerry exchange a glance. Alicia says, 'We are here to help.' Then Jerry chimes in. " +
			"'We think that it's important for people to realise that in these tough times it's better that we all come together. That's what this statue represents " +
			"for us, and that's why we help passerby as much as we can. We might put ourselves in danger, sure, but if we can't risk ourselves for our beliefs then " +
			"there's no point in having them.'", buttonNodeList);
		dialogdictionary.Add ("jerryaliciawhat", JerryAliciaWhat);
		buttonNodeList.Clear ();

		if (isaacquest == 1) {
			buttonNodeList.Add ("Isaac wants you to come back to town./jerryaliciaisaac");
		}
		if (talkedtojerry == 1) {
			buttonNodeList.Add ("Isaac wants you to come back to town./jerryaliciaisaac");
		}
		buttonNodeList.Add ("What are you doing here?/jerryaliciawhat");
		buttonNodeList.Add ("What's the deal with the statue?/jerryaliciastatue");
		buttonNodeList.Add ("Could you help me get to the border?/jerryaliciaborder");
		buttonNodeList.Add ("Give me all your money./jerryaliciarob");
		buttonNodeList.Add ("I need to go./exit");
		DialogueNode JerryAliciaStatue = new DialogueNode ("jerryaliciastatue", "Alicia pats the side of the statue, almost lovingly. Jerry says, 'Ah, this is an " +
			"old monument to another war, one nobody seems to remember much about. We passed by it on vacation a long time ago, and neither of us ever forgot it. People " +
			"suffered then, just like they do now, but things turned out all right. The wheel in the sky kept turning, and there was peace in the end. We know it will " +
			"be the same way again, we just want to save as many lives as possible, so more of us can see that wonderful day.'", buttonNodeList);
		dialogdictionary.Add ("jerryaliciastatue", JerryAliciaStatue);
		buttonNodeList.Clear ();

		if (isaacquest == 1) {
			buttonNodeList.Add ("Isaac wants you to come back to town./jerryaliciaisaac");
		}
		if (talkedtojerry == 1) {
			buttonNodeList.Add ("Isaac wants you to come back to town./jerryaliciaisaac");
		}
		buttonNodeList.Add ("What are you doing here?/jerryaliciawhat");
		buttonNodeList.Add ("What's the deal with the statue?/jerryaliciastatue");
		buttonNodeList.Add ("Could you help me get to the border?/jerryaliciaborder");
		buttonNodeList.Add ("Give me all your money./jerryaliciarob");
		buttonNodeList.Add ("I need to go./exit");
		DialogueNode JerryAliciaBorder = new DialogueNode ("jerryaliciaborder", "Jerry rubs his bald pate and says, 'I wish we could help you, but we already " +
			"gave away all the money that we had. All we can do is remind everyone that there will be a better tomorrow.' Alicia says, 'The outpost is very " +
			"near, you probably don't even need help anyway.'", buttonNodeList);
		dialogdictionary.Add ("jerryaliciaborder", JerryAliciaBorder);
		buttonNodeList.Clear ();

		if (isaacquest == 1) {
			buttonNodeList.Add ("Isaac wants you to come back to town./jerryaliciaisaac");
		}
		if (talkedtojerry == 1) {
			buttonNodeList.Add ("Isaac wants you to come back to town./jerryaliciaisaac");
		}
		buttonNodeList.Add ("What are you doing here?/jerryaliciawhat");
		buttonNodeList.Add ("What's the deal with the statue?/jerryaliciastatue");
		buttonNodeList.Add ("Could you help me get to the border?/jerryaliciaborder");
		buttonNodeList.Add ("Give me all your money./jerryaliciarob");
		buttonNodeList.Add ("I need to go./exit");
		DialogueNode JerryAliciaRob = new DialogueNode ("jerryaliciarob", "Jerry shrugs and says, 'We don't have anything to give you but good wishes.' Alicia says, 'You're not the first one to ask.' You weren't really asking, but " +
			"even searching them reveals they really do have nothing to take. Despite all of this, they still seem just as cheery as before.", buttonNodeList);
		dialogdictionary.Add ("jerryaliciarob", JerryAliciaRob);
		buttonNodeList.Clear ();

		/*if (isaacquest == 1) {
			buttonNodeList.Add ("Isaac wants you to come back to town./jerryaliciaisaac");
		}
		if (talkedtojerry == 1) {
			buttonNodeList.Add ("Isaac wants you to come back to town./jerryaliciaisaac");
		}
		buttonNodeList.Add ("What are you doing here?/jerryaliciawhat");
		buttonNodeList.Add ("What's the deal with the statue?/jerryaliciastatue");
		buttonNodeList.Add ("Could you help me get to the border?/jerryaliciaborder");
		buttonNodeList.Add ("Give me all your money./jerryaliciarob");
		buttonNodeList.Add ("I need to go./exit");
		DialogueNode JerryAliciaHeal = new DialogueNode ("jerryaliciaheal", "Alicia pulls some bandages and stitches out of her backpack, and with amazing " +
			"skill and speed binds all of your wounds. She finishes by rubbing some salve into them, and you're amazed at how much better you feel. Under the hand " +
			"of an experienced doctor you feel as good as new. Alicia scratches the scar on her face and says, 'I hope that helps.'", buttonNodeList);
		dialogdictionary.Add ("jerryaliciaheal", JerryAliciaHeal);
		buttonNodeList.Clear ();*/

		buttonNodeList.Add ("Force them to go back./jerryaliciaforce");
		buttonNodeList.Add ("Well, I guess I can't force you to go. I'll let Isaac know./exit");
		DialogueNode JerryAliciaIsaac = new DialogueNode ("jerryaliciaisaac", "Alicia and Jerry exchange a glance. 'Well' Jerry says, 'You can tell him " +
			"we aren't coming. Back there all we did was sit and wait for the inevitable. Here we're actually doing something. Isaac thinks he knows what's " +
			"best for everyone. This time he's wrong.' Alicia nods in agreement and crosses her arms. 'He'll have to force us if he thinks we're so incapable " +
			"of taking care of ourselves.'", buttonNodeList);
		dialogdictionary.Add ("jerryaliciaisaac", JerryAliciaIsaac);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Alright then./exit");
		DialogueNode JerryAliciaForce = new DialogueNode ("jerryaliciaforce", "You draw your weapon, and the couple's eyes shoot up in surprise. Jerry slowly puts his hands " +
			"out and says, 'Okay, okay. We'll go back. Just don't hurt anyone.' Alicia refuses to move for a moment, but with a little prodding from Jerry she follows " +
			"him to the south, staring daggers at you as she goes.", buttonNodeList);
		dialogdictionary.Add ("jerryaliciaforce", JerryAliciaForce);
		buttonNodeList.Clear ();

		// Pots to loot node
		buttonNodeList.Add ("Great!/exit");
		DialogueNode PotsToLoot = new DialogueNode ("potstoloot", "Glancing through the roadside debris, you find a few intact pots. One of them is full of healing salve." +
			"\n" +
			"\n" +
			"(You were probably injured on your way here. To use your healing items go to the character menu and click the appropriate button. This will regenerate " +
			"five health points.)", buttonNodeList);
		dialogdictionary.Add ("potstoloot", PotsToLoot);
		buttonNodeList.Clear ();

		// Border Outpost/Border Guards nodes
		buttonNodeList.Add ("Approach./exit");
		DialogueNode Border = new DialogueNode ("border", "With an exhausted sigh you lean against one of the cavern walls for a momentary rest. You almost " +
			"can't believe it. There, right in front of you, is the border outpost. Several bored guards laze behind a recently constructed palisade. They seem " +
			"quite miserable, like they don't know how good they have it. One in particular seems to perk up when he notices you through the gloom. Dressed in slightly " +
			"cleaner armor than the rest, he stands in the only gap in the palisade and motions for you to come forward.", buttonNodeList);
		dialogdictionary.Add ("border", Border);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Okay./exit");
		DialogueNode BorderGuardStatic = new DialogueNode ("borderguardstatic", "The soldier gives you a bored look and says, 'Keep it moving.'", buttonNodeList);
		dialogdictionary.Add ("borderguardstatic", BorderGuardStatic);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("I'd like to cross the border./borderguardborder");
		buttonNodeList.Add ("Aren't you worried about bandits or wolves?/borderguardask");
		buttonNodeList.Add ("Draw your weapon and attack./borderguardfight");
		buttonNodeList.Add ("I'll be right back./exit");
		DialogueNode BorderGuard = new DialogueNode ("borderguard", "The heavy armor and weapons this soldier carries would make him very imposing if it weren't " +
			"for his attitude. 'Halt.' he says in a slow monotone. 'State your business.' He eminates the smell of beer, roast meat, and apathy.", buttonNodeList);
		dialogdictionary.Add ("borderguard", BorderGuard);
		buttonNodeList.Clear ();

		if (happybeggar != 3 && happybeggar != 1) {
			buttonNodeList.Add ("I'd like to cross the border./borderguardborder");
		} else {
			buttonNodeList.Add ("I'd like to cross the border./borderguardborderpoor");
		}
		buttonNodeList.Add ("Draw your weapon and attack./borderguardfight");
		buttonNodeList.Add ("I'll be right back./exit");
		DialogueNode BorderGuardAsk = new DialogueNode ("borderguardask", "The guard spits on the ground and says, 'What a stupid question. Not even animals are dumb " +
			"enough to try and break through us. Now state your business.'", buttonNodeList);
		dialogdictionary.Add ("borderguardask", BorderGuardAsk);
		buttonNodeList.Clear ();

		if (money < moneytocrossborder) {
			buttonNodeList.Add ("But I don't have that much./borderguardcantafford");
		} else {
			buttonNodeList.Add ("Here you go./borderguardcanafford");
		}
		buttonNodeList.Add ("How dare you try and extort people in need./borderguardscold");
		buttonNodeList.Add ("I'll be right back./exit");
		DialogueNode BorderGuardBorderPoor = new DialogueNode ("borderguardborderpoor", "The guard wrinkles his nose and sniffs you. 'Huh, he says. 'Wars not been " +
			"kind to you, has it?' He looks you up and down, noting the disgusting, smelly rags that the beggar gave you. They seem to have the intended effect. 'There's " +
			"still a fee for crossing. Let's say fifty banknotes.'", buttonNodeList);
		dialogdictionary.Add ("borderguardborderpoor", BorderGuardBorderPoor);
		buttonNodeList.Clear ();

		if (money < moneytocrossborder) {
			buttonNodeList.Add ("But I don't have that much./borderguardcantafford");
		} else {
			buttonNodeList.Add ("Here you go./borderguardcanafford");
		}
		buttonNodeList.Add ("How dare you try and extort people in need./borderguardscold");
		buttonNodeList.Add ("Draw your weapon and attack./borderguardfight");
		buttonNodeList.Add ("I'll be right back./exit");
		DialogueNode BorderGuardBorder = new DialogueNode ("borderguardborder", "The guard looks you up and down and says, 'Well, there's a fee for that.' He grins " +
			"wolfishly. 'For you? I'd say a hundred banknotes. Yea, a hundred should do it.'", buttonNodeList);
		dialogdictionary.Add ("borderguardborder", BorderGuardBorder);
		buttonNodeList.Clear ();


		if (money < 200) {
			// moneytocross border won't update in time to be 200 here, but it can never be less or more than 200 in this situation
			buttonNodeList.Add ("But I don't have that much./borderguardcantafford");
		} else {
			buttonNodeList.Add ("Fine, take it./borderguardcanafford");
		}
		buttonNodeList.Add ("I'll be right back./exit");
		DialogueNode BorderGuardScold = new DialogueNode ("borderguardscold", "The guard straightens up and grins. 'Ah,' he says. 'Another one that thinks they're " +
			"so high and mighty. Another unlucky one at the bottom of the totem pole thinking he's the only one that has it so bad. Well I have a wife and kid. Things " +
			"aren't so rosy for us just because we're the ones on this side of the wall. You don't think you'd do the same? Well if you don't then you're wrong. People " +
			"in power are going to take what they need.' He pauses and looks at you. 'Just like you would.'" +
			"\n" +
			"\n" +
			"The two of you are silent for a moment. The guard looks at you with pity like a disappointed parent would at their child. 'The fee has just been updated " +
			"to 200 banknotes. To be honest, it's mostly on principle. We can't have hypocritical moralists in our country if they don't pay a price for their attitude.'", buttonNodeList);
		dialogdictionary.Add ("borderguardscold", BorderGuardScold);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("No no no, please let me in, oh god please./borderguardbeg");
		buttonNodeList.Add ("How dare you! Let me in damn it!/borderguardyell");
		buttonNodeList.Add ("You'll let me in even if I have to kill all of you! Draw your weapon and attack./borderguardfight");
		buttonNodeList.Add ("Leave./exit");
		DialogueNode BorderGuardCantAfford = new DialogueNode ("borderguardcantafford", "'Well, that's a shame.' The guard starts to ignore you, and looks over your " +
			"shoulder, as though there was something very interesting happening on the cave wall behind you. 'Move along then, no more immigrants today.'", buttonNodeList);
		dialogdictionary.Add ("borderguardcantafford", BorderGuardCantAfford);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Kill the bastard./borderguardfight");
		buttonNodeList.Add ("How dare you! Let me in damn it!/borderguardyell");
		buttonNodeList.Add ("Dust yourself off and leave./exit");
		DialogueNode BorderGuardBeg = new DialogueNode ("borderguardbeg", "You're on your knees, pleading with every last ounce of emotion left in you. You say, 'It can't " +
			"all have been for nothing.' You see every person that you've lost flash before your eyes. You see the things you've done, been forced to do. You " +
			"have to get across, need to. 'Please,' you say. 'You can't know the terrible things I've been through to get here. I'll give you every penny I have " +
			"just let me cross. Please, it's terrible over there. The invaders will be here any day. Please, for the love of god please.'" +
			"\n" +
			"\n" +
			"The border guard looks you up and down and kicks you in the chest, sending you sprawling across the floor.", buttonNodeList);
		dialogdictionary.Add ("borderguardbeg", BorderGuardBeg);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Kill the bastard./borderguardfight");
		buttonNodeList.Add ("Leave./exit");
		DialogueNode BorderGuardYell = new DialogueNode ("borderguardyell", "You scream and yell at the guard for a solid five minutes. He, probably very used to this, " +
			"stares at the wall and waits for you to leave.", buttonNodeList);
		dialogdictionary.Add ("borderguardyell", BorderGuardYell);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Okay./exit");
		DialogueNode BorderGuardCanAfford = new DialogueNode ("borderguardcanafford", "You hand the guard the money. He takes it mechanically and steps aside. 'Alright " +
			"go right through. Don't touch anything.' You step past the palisade. The guards keep an unenthusiastic eye on you as you walk north, over the border.", buttonNodeList);
		dialogdictionary.Add ("borderguardcanafford", BorderGuardCanAfford);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Anything to survive./fight");
		DialogueNode BorderGuardFight = new DialogueNode ("borderguardfight", "It never occured to the guard that you were talking to that someone would even try " +
			"to assault him. He's down before any of them can even react. Without thinking, you race forward and tackle another. When you're done beating him into " +
			"a pulp the other two have finally realised what is happening. They've never " +
			"seen someone desperate enough to try and take on four armed and trained soldiers before. Then again, they've never seen you before either." +
			"\n" +
			"\n" +
			"The last two soldiers draw their weapons and stand back, confused and afraid. You charge.", buttonNodeList);
		dialogdictionary.Add ("borderguardfight", BorderGuardFight);
		buttonNodeList.Clear ();

		// The nodes for the triggers the player encounters when crossing the border. Effectively the finale
		buttonNodeList.Add ("Keep walking./exit");
		DialogueNode BorderCrossed = new DialogueNode ("bordercrossed", "It starts to sink in. This is it. You've made it. You're home free. You suddenly " +
			"feel very focused. All of the exhaustion and even the pain of your old wounds seems to slip away, like they happened to someone else. It's over, you've " +
			"made it. With one step past a wooden wall you've put a terrible fate behind you.", buttonNodeList);
		dialogdictionary.Add ("bordercrossed", BorderCrossed);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("I did it!/victory");
		DialogueNode BorderCrossed2 = new DialogueNode ("bordercrossed2", "It's finally over. You're about to enter a place where you can walk down the street " +
			"without looking for robbers around the corners of abandoned buildings. The buildings won't even be abandoned." +
			"\n" +
			"\n" +
			"You're almost jubilant, almost worry free, but then another thought occurs to you. What will you do now? You don't know anybody in this country. You " +
			"don't know a trade other than fighting. You're covered in blood, scars, and weapons. At least in your homeland you knew which way was up, and what to " +
			"expect. Ahead, the future is unknown. It won't be violent, but it's not going to be easy." +
			"\n" +
			"\n" +
			"You squint in the sunlight and look around at the trees and grass. You can make out some buildings in the distace. You've overcome worse uncertainty than " +
			"this. A smile creeps up on your face for the first time in months, and you're not sure why. Oh well, you think to yourself, everyone has their cross to bear.", buttonNodeList);
		dialogdictionary.Add ("bordercrossed2", BorderCrossed2);
		buttonNodeList.Clear ();

		// Node for the bear cave
		buttonNodeList.Add ("Maybe this is a bad idea./exit");
		DialogueNode Bear = new DialogueNode ("bear", "The first thing you see in this bear's lair is a skeleton on the ground, surrounded by fragments of bone " +
			"and shattered furniture. The second thing you notice is the massive bear staring at you from across the room. It stands almost still, nostrils flaring. " +
			"It also happens to be standing right in your way. Unless you want to go to the border outpost, the only way across is through this animal.", buttonNodeList);
		dialogdictionary.Add ("bear", Bear);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("I made it!/victory");
		DialogueNode BearBorder = new DialogueNode ("bearborder", "You step over the corpse of your final opponent and stumble into the sunlight. The adrenaline " +
			"is still pumping through your body. Your head darts left and right as you look at the grass and trees with wide, wild eyes. You collapse to your knees " +
			"and throw your head back, drinking in the air and the light you might have never seen again. You made it. It starts to sink in. This is it. You've made it. " +
			"You're home free." +
			"\n" +
			"\n" +
			"You laugh harder than you ever have before.", buttonNodeList);
		dialogdictionary.Add ("bearborder", BearBorder);
		buttonNodeList.Clear ();

		// Nodes for random NPCs around town (not including isaacguard)
		buttonNodeList.Add ("Bye./exit");
		DialogueNode SadFarmer = new DialogueNode ("sadfarmer", "This thin man leaning on a pitchfork regards you with rheumy eyes. 'Don't go in the cave,' he says. '" +
			"I went in there with a whole group of others, none of them made it back out but me.'", buttonNodeList);
		dialogdictionary.Add ("sadfarmer", SadFarmer);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Pet the horse./horse2");
		buttonNodeList.Add ("Leave the animal alone./exit");
		DialogueNode Horse = new DialogueNode ("horse", "You come across a horse drinking greedily out of what seems to be a large birdbath. It lifts its head " +
			"and snorts as you approach.", buttonNodeList);
		dialogdictionary.Add ("horse", Horse);
		buttonNodeList.Clear ();

		buttonNodeList.Add ("Good girl./exit");
		DialogueNode Horse2 = new DialogueNode ("horse2", "The horse nibbles your hand as you reach to pet it, and then dips its head back into the water. " +
			"Everything is very peaceful for a few moments. You look around at the grass and flowers. Then your gaze settles on the entrance to the cave. You " +
			"give the horse one last pat on the neck and turn away.", buttonNodeList);
		dialogdictionary.Add ("horse2", Horse2);
		buttonNodeList.Clear ();

		// KEEP THESE, THEY ARE VITAL NODES
		// This is the exit node used for leaving the dialogue window
		DialogueNode exitNode = new DialogueNode ("exit", "EXIT ERROR", new List<string> ());
		dialogdictionary.Add ("exit", exitNode);

		// This node is used for initiating a combat encounter from the text
		DialogueNode fightNode = new DialogueNode ("fight", "FIGHT ERROR", new List<string> ());
		dialogdictionary.Add ("fight", fightNode);

		// This is the victory node! Means the player has won, transfers them to the win scene
		DialogueNode victoryNode = new DialogueNode ("victory", "VICTORY ERROR", new List<string> ());
		dialogdictionary.Add ("victory", victoryNode);
	}

	public void checkTextForBool(string destinationnode) {

		if (currentnode == dialogdictionary["deadmiser"]) {
			helpedmiser = 2;
		}
		if (currentnode == dialogdictionary["helpmiser"]) {
			helpedmiser = 1;
		}
		if (currentnode == dialogdictionary["robmiser"]) {
			money += 100;
			helpedmiser = 2;
		}
		if (currentnode == dialogdictionary["killedmiser"]) {
			money += 100;
			helpedmiser = 2;
		}
		if (currentnode == dialogdictionary["lootmiser"]) {
			money += 100;
		}
		if (currentnode == dialogdictionary["spickydeadmiserpaid"]) {
			money -= 20;
		}
		if (currentnode == dialogdictionary["spickyhelpedmiserisaac"]) {
			spickygone = 1;
		}
		if (currentnode == dialogdictionary["spickytalkingshitfight"]) {
			spickygone = 1;
		}
		if (currentnode == dialogdictionary ["cavecorpsegem"]) {
			GameManager.instance.currenthealth -= 2;
		}
		if (currentnode == dialogdictionary ["cavecorpsegemcrush"]) {
			currenthealth -= 2;
			money += 20;

		}
		if (currentnode == dialogdictionary ["cavecorpsegemsnatch"]) {
			currenthealth -= 1;
			instance.money += 30;
		}
		if (currentnode == dialogdictionary ["caveentrancelevelup"]) {
			LevelUp ();
			AddHealth (5);
		}
		if (currentnode == dialogdictionary ["isaachouseguard2"]) {
			money += 20;
			isaachouseguardhelped = 1;
		}
		if (currentnode == dialogdictionary ["merchantmedicinebuy"]) {
			money -= 20;
			healingitems += 1;
		}
		if (currentnode == dialogdictionary ["merchantweaponbuy"]) {
			money -= 40;
			// Replace club with spiked club
			EquipmentList.Remove(Club);
			EquipmentList.Add (SpikedClub);
			weapon = SpikedClub;
		}
		if (currentnode == dialogdictionary ["merchantarmorbuy"]) {
			money -= 100;
			// Replace armor with chainmail
			armor = Chainmail;
			EquipmentList.Add (Chainmail);
		}
		if (currentnode == dialogdictionary ["hidingrefugeerob2"]) {
			money += 30;
		}
		if (currentnode == dialogdictionary ["hidingrefugeerob3"]) {
			EquipmentList.Add (Cutlass);
			deadrefugees = 1;
		}
		if (currentnode == dialogdictionary ["isaactalkquest"]) {
			isaacquest = 1;
		}
		if (currentnode == dialogdictionary ["banditspoils"]) {
			money += 5;
			healingitems += 1;
		}
		if (currentnode == dialogdictionary ["banditleaderfight"]) {
			nickiquest = 2;
			GameObject NickiGoneTrigger = GameObject.Find ("NickiGoneText");
			NickiGoneTrigger.GetComponent<TextTrigger> ().enabled = true;
			NickiGoneTrigger.GetComponent<BoxCollider2D> ().enabled = true;

			EquipmentList.Add (Cutlass);
		}
		if (currentnode == dialogdictionary ["banditleaderdonation"]) {
			money -= 20;
			if (money < 0) {
				money = 0;
			}
		}
		if (currentnode == dialogdictionary ["beggargive"]) {
			money -= 5;
			moneytocrossborder = 50;
			happybeggar = 3;
		}
		if (currentnode == dialogdictionary ["borderguardscold"]) {
			moneytocrossborder = 200;
		}
		if (currentnode == dialogdictionary ["beggargive3"]) {
			money -= 20;
			happybeggar = 1;
		}
		if (currentnode == dialogdictionary ["beggarrob"]) {
			money += 30;
			happybeggar = 2;
		}
		if (currentnode == dialogdictionary ["wagonsearch"]) {
			money += 5;
			EquipmentList.Add (Knife);
		}
		if (currentnode == dialogdictionary ["nickidoit"]) {
			nickiquest = 1;
		}
		if (currentnode == dialogdictionary ["potstoloot"]) {
			healingitems += 1;
		}
		if (currentnode == dialogdictionary ["jerryaliciaisaac"]) {
			isaacquest = 2;
			talkedtojerry = 1;
		}
		if (currentnode == dialogdictionary ["jerryaliciaforce"]) {
			isaacquest = 3;
			talkedtojerry = 0;
		}
		if (currentnode == dialogdictionary ["isaacquest3"]) {
			money += 100;
			isaacquest = 4;
		}
		if (currentnode == dialogdictionary ["wagonsearch"]) {
			wagonpassed = 1;
		}
		if (currentnode == dialogdictionary ["borderguardcanafford"]) {
			borderguard = 1;
		}
		if (currentnode == dialogdictionary ["borderguardfight"]) {
			borderguard = 2;
		}
		if (currentnode == dialogdictionary ["nickidenied"]) {
			nickiquest = 3;
		}

		currentnode = dialogdictionary [destinationnode];
	}

	void LoadItemInfo() {
		Club = new Item("Club", "For hitting with.\nDoes between 1 and 4 blunt damage.\nAbilities: Headcrack - Does 1 extra damage and stuns the target " +
			"for one turn, reduced chance to hit.\nPassive Abilities: Sturdy - Reduce all incoming damage by one.", 1, 4, Item.DamageTypes.BLUNT);
		Club.abilities.Add (new Ability("Headcrack", -10, true, false));
		Club.abilities.Add (new Ability("Focus", 0, false, false));

		SpikedClub = new Item("Spiked Club", "For hitting with.\nDoes between 2 and 6 piercing damage.\nAbilities: Headcrack - Does 1 extra damage and stuns the target " +
			"for one turn, reduced chance to hit.\nPassive Abilities: Sturdy - Reduce all incoming damage by one.", 2, 6, Item.DamageTypes.PIERCE);
		SpikedClub.abilities.Add (new Ability("Headcrack", -10, true, false));
		SpikedClub.abilities.Add (new Ability("Focus", 0, false, false));

		Cutlass = new Item("Cutlass", "A wicked curved blade.\nDoes between 3 and 8 slashing damage.\nAbilities: None", 3, 8, Item.DamageTypes.SLASH);

		Cutlass.abilities.Add (new Ability("Focus", 0, false, false));

		Knife = new Item ("Knife", "For the very quick.\nDoes between 4 and 5 piercing damage.\nAbilities: Sneak Attack - Always hits, but does half damage.\nLacerate - " +
		"Makes an enemy bleed for one damage every turn for four turns, reduced chance to hit.\nPassive Abilities: Quick - Act faster in combat.\nSlight - Cannot block " +
		"with this weapon equipped.", 4, 5, Item.DamageTypes.PIERCE);
		Knife.abilities.Add (new Ability("Sneak Attack", 1000, false, false));
		Knife.abilities.Add (new Ability("Lacerate", -10, false, true));
		Knife.abilities.Add (new Ability("Focus", 0, false, false));

		Cloak = new Item ("Cloak", "Not great for absorbing blows, very warm though.\nReduces Blunt Damage by one.", 0, 1, 0);
		Chainmail = new Item ("Chainmail", "Not easy to get in these times. It's great protection, except against piercing attacks.\n" +
			"Reduces Blunt damage by 3, Slashing damage by 3 and piercing damage by 1.", 1, 3, 3);
	}
}