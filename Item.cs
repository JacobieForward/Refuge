using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Item {
	public ItemTypes itemtype;
	public DamageTypes damagetype;

	public int minDamage; // Damage is for weapons only
	public int maxDamage;

	public int armorBonusPierce;// Piercing weapons tend to be very fast with decent damage, but pierce armor is easy to find
	public int armorBonusBlunt; // Blunt weapons tend to do less damage, but a blunt armor bonus is hard to find
	public int armorBonusSlash; // Slash is the middle ground between blunt and pierce weapons

	public List<Ability> abilities = new List<Ability>();

	public string description;
	public string name;

	public enum ItemTypes {
		WEAPON,
		ARMOR,
		MISC
	}

	public enum DamageTypes {
		PIERCE,
		BLUNT,
		SLASH
	}

	// TODO: Store items in some kind of prefabs

	// Default Constructor for weapons
	public Item (string itemname, string descript, int mDmg, int mxDmg, DamageTypes dtype) {
		minDamage = mDmg;
		maxDamage = mxDmg;
		damagetype = dtype;
		itemtype = ItemTypes.WEAPON;
		description = descript;
		name = itemname;
		// abilities are added to a weapon after it is created
	}

	// Default Constructor for armor
	public Item (string itemname, string descript, int ABPierce, int ABBlunt, int ABSlash) {
		armorBonusPierce = ABPierce;
		armorBonusBlunt = ABBlunt;
		armorBonusSlash = ABSlash;
		itemtype = ItemTypes.ARMOR;
		description = descript;
		name = itemname;
	}

	// Default Constructor for Miscelanious Items
	public Item (string itemname, string descript) {
		name = itemname;
		description = descript;
		itemtype = ItemTypes.MISC;
	}

	// Default Constructor for all types
	public Item (string itemname, string descript, int mDmg, int mxDmg, int ABPierce, int ABBlunt, int ABSlash, ItemTypes itype, DamageTypes dtype) {
		minDamage = mDmg;
		maxDamage = mxDmg;
		armorBonusPierce = ABPierce;
		armorBonusBlunt = ABBlunt;
		armorBonusSlash = ABSlash;
		itemtype = itype;
		damagetype = dtype;
		description = descript;
		name = itemname;
	}
}