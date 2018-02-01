using UnityEngine;
using System.Collections;

public class StatButton : MonoBehaviour {

	// If increase is true clicking this button increases a stat
	// If increase is false clicking this button decreases a stat
	public bool increase;
	// This string indicates the stat changed by this button
	public string stat;


	// All buttons increase/decrease by one
	public void ChangeValue() {
		if (increase) {
			if (GameManager.instance.StatPoints > 0) {
				if (stat.Equals("Hardiness")) {
					GameManager.instance.Hardiness++;
					ExplorationManager.instance.hardinesstotal.text = GameManager.instance.Hardiness.ToString();
				}
				if (stat.Equals("Dexterity")) {
					GameManager.instance.Dexterity++;
					ExplorationManager.instance.dexteritytotal.text = GameManager.instance.Dexterity.ToString();
				}
				if (stat.Equals("Strength")) {
					GameManager.instance.Strength++;
					ExplorationManager.instance.strengthtotal.text = GameManager.instance.Strength.ToString();
				}

				GameManager.instance.StatPoints--;
				ExplorationManager.instance.statpointstotal.text = GameManager.instance.StatPoints.ToString();
			}
		} else {
			if (stat.Equals("Hardiness")) {
				if (GameManager.instance.Hardiness > 3) {
					GameManager.instance.Hardiness--;
					ExplorationManager.instance.hardinesstotal.text = GameManager.instance.Hardiness.ToString();
					GameManager.instance.StatPoints++;
					ExplorationManager.instance.statpointstotal.text = GameManager.instance.StatPoints.ToString();
				}
			}
			if (stat.Equals("Dexterity")) {
				if (GameManager.instance.Dexterity > 3) {
					GameManager.instance.Dexterity--;
					ExplorationManager.instance.dexteritytotal.text = GameManager.instance.Dexterity.ToString();
					GameManager.instance.StatPoints++;
					ExplorationManager.instance.statpointstotal.text = GameManager.instance.StatPoints.ToString();
				}
			}
			if (stat.Equals("Strength")) {
				if (GameManager.instance.Strength > 3) {
					GameManager.instance.Strength--;
					ExplorationManager.instance.strengthtotal.text = GameManager.instance.Strength.ToString();
					GameManager.instance.StatPoints++;
					ExplorationManager.instance.statpointstotal.text = GameManager.instance.StatPoints.ToString();
				}
			}
		}
	}

	public void UseHeal() {
		if (GameManager.instance.healingitems > 0 && GameManager.instance.currenthealth != GameManager.instance.maxhealth) {
			GameManager.instance.healingitems -= 1;
			ExplorationManager.instance.healingtotal.text = GameManager.instance.healingitems.ToString();
			ExplorationManager.instance.currenthealthtotal.text = GameManager.instance.currenthealth.ToString();
			GameManager.instance.AddHealth (5);
		}
	}

}