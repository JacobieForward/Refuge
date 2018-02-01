using UnityEngine;
using System.Collections;

public class Ability {

	public string abilityname;
	public int tohitmodifier;
	public bool stuns;
	public bool bleeds;

	// One massive constructor for all abilities
	public Ability (string ability, int tohitm, bool stun, bool bleed) {
		abilityname = ability;
		tohitmodifier = tohitm;
		stuns = stun;
		bleeds = bleed;
	}
}
