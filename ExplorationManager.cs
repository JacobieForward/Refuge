using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ExplorationManager : MonoBehaviour {

	public static ExplorationManager instance;
	// Messagepanel Objects
	public GameObject messagepanel;
	public Text messagetext;
	public Transform messagespacer;
	// Characterpanel Objects
	public GameObject characterpanel;
	public Text leveltotal;
	public Text playername;
	public Text hardinesstotal;
	public Text healingtotal;
	public Text dexteritytotal;
	public Text strengthtotal;
	public Text statpointstotal;
	public Text maxhealthtotal;
	public Text currenthealthtotal;
	public Text moneyamounttext;
	//public static Image fadeoutimage;

	public GameObject WolfForCombat;

	// Equipment menu things
	public Button displayequipmentbutton;
	public GameObject equipmentpanel;
	public Transform EquipmentSpacer;
	public GameObject EquipmentButton;
	public Text EquipmentText;
	public Item SelectedItem;
	public Button equipbutton;
	public Button equipexitbutton;

	//static bool fading;

	// TODO: See if there's anything that can be done with seperate gamestates to make this obsolete
	// As it stands that would take too much time to be worth it on this development schedule

	void Awake() {
		instance = this;

		// Setting up all of these references is very performance intensive

		// Set up all references to UI objects
		messagepanel = GameObject.FindGameObjectWithTag ("MessagePanel");
		equipmentpanel = GameObject.FindGameObjectWithTag ("EquipmentPanel");
		displayequipmentbutton = GameObject.Find ("EquipmentButton").GetComponent<Button> ();
		messagetext = messagepanel.transform.FindChild ("MessageText").gameObject.GetComponent<Text> ();
		messagespacer = messagepanel.transform.FindChild("MessageSpacer");
		messagepanel.SetActive (false);
		characterpanel = GameObject.FindGameObjectWithTag("CharacterPanel");
		playername = GameObject.Find ("PlayerNameText").GetComponent<Text>();
		leveltotal = GameObject.Find ("LevelTotal").GetComponent<Text>();
		hardinesstotal = GameObject.Find ("HardinessTotal").GetComponent<Text>();
		dexteritytotal = GameObject.Find ("DexterityTotal").GetComponent<Text>();
		strengthtotal = GameObject.Find ("StrengthTotal").GetComponent<Text>();
		statpointstotal = GameObject.Find ("StatPointsTotal").GetComponent<Text>();
		healingtotal = GameObject.Find ("HealingTotal").GetComponent<Text>();
		maxhealthtotal = GameObject.Find ("MaximumHealthTotal").GetComponent<Text>();
		currenthealthtotal = GameObject.Find ("CurrentHealthTotal").GetComponent<Text>();
		//fadeoutimage = GameObject.Find ("FadeOutImage").GetComponent<Image>();
		//fadeoutimage.gameObject.SetActive (false);

		SelectedItem = GameManager.instance.weapon;

		characterpanel.SetActive (false);
		equipmentpanel.SetActive (false);

		//fading = false;

		// Temporary to test allies
		// GameManager.instance.alliesInCombat.Add(WolfForCombat);
	}

	void Start() {
		displayequipmentbutton.onClick.AddListener (() => DisplayEquipmentMenu());
		equipbutton.onClick.AddListener (() => Equip());
		equipexitbutton.onClick.AddListener (() => EquipExit());
	}

	// TODO: Finish this so we can fade into combat
	/*public static IEnumerator FadeOut() {
		if (fading) {
			yield break;
		}
		fading = true;
		fadeoutimage.gameObject.SetActive(true);
		for (float f = 0f; f <= 255; f += Time.deltaTime * (1f/5f)) {
			Color color = fadeoutimage.color;
			color.a = f;
			Debug.Log (color.a.ToString());
			fadeoutimage.color = color;
			// yield return new WaitForSeconds(Time.deltaTime * (1f/5f));
		}
		fading = false;
	}*/

	void EquipExit() {
		equipmentpanel.SetActive (false);
	}

	public void Equip() {
		if (SelectedItem.itemtype == Item.ItemTypes.WEAPON) {
			GameManager.instance.weapon = SelectedItem;
			GameManager.instance.abilitylist = SelectedItem.abilities;
		}
		if (SelectedItem.itemtype == Item.ItemTypes.ARMOR) {
			GameManager.instance.armor = SelectedItem;
		}
		DeleteEquipmentButtons ();
		CreateEquipmentButtons ();
	}

	void DisplayEquipmentMenu() {
		equipmentpanel.SetActive (true);
		DeleteEquipmentButtons ();
		CreateEquipmentButtons ();
	}

	void CreateEquipmentButtons() {
		foreach (Item equipment in GameManager.instance.EquipmentList) {
			// Creates a new button from the EquipmentButton
			GameObject newButton = Instantiate (EquipmentButton) as GameObject;

			EquipmentButton button = newButton.GetComponent<EquipmentButton> ();
			// EnemyState currentEnemy = enemy.GetComponent<EnemyState> ();


			Text ButtonText = newButton.transform.FindChild ("Text").gameObject.GetComponent<Text> ();
			ButtonText.text = equipment.name;
			if (GameManager.instance.weapon.Equals (equipment) || GameManager.instance.armor.Equals (equipment)) {
				ButtonText.text += " (Equipped)";
			}

			button.item = equipment;

			newButton.transform.SetParent (EquipmentSpacer);
		}
	}

	void DeleteEquipmentButtons() {
		List<GameObject> toDelete = new List<GameObject> ();
		toDelete.AddRange(GameObject.FindGameObjectsWithTag("EquipmentButton"));
		foreach (GameObject button in toDelete) {
			Object.Destroy (button);
		}
	}

	void Update() {
		// Constantly update character panel values
		playername.text = GameManager.instance.playername;
		leveltotal.text = GameManager.instance.level.ToString ();
		hardinesstotal.text = GameManager.instance.Hardiness.ToString();
		healingtotal.text = GameManager.instance.healingitems.ToString();
		dexteritytotal.text = GameManager.instance.Dexterity.ToString();
		strengthtotal.text = GameManager.instance.Strength.ToString();
		statpointstotal.text = GameManager.instance.StatPoints.ToString();
		maxhealthtotal.text = GameManager.instance.maxhealth.ToString();
		currenthealthtotal.text = GameManager.instance.currenthealth.ToString();
		moneyamounttext.text = GameManager.instance.money.ToString();
		if (Input.GetKeyDown ("return")) {
			// Display the character panel
			if (!characterpanel.activeSelf) {
				GameManager.instance.PlayerMovementEnabled = false;
				characterpanel.SetActive (true);
			} else {
				GameManager.instance.PlayerMovementEnabled = true;
				characterpanel.SetActive (false);
			}
		}
	}
}