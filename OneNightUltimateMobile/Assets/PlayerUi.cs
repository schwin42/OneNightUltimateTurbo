using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class PlayerUi : MonoBehaviour {

	private enum UiScreen {
		Night_InputControl = 0,
		Night_InputWait = 1,

	}

	private Player player;

	//Night input screen
	Text playerName;
	Text title;
	Text description;
	Transform buttonBox;

	void Start () { }

	void Update () { }

	public void Initialize(Player player) {
		title = transform.Find("Title").GetComponent<Text>();
		description = transform.Find("Description").GetComponent<Text>();
		playerName = transform.Find("PlayerName").GetComponent<Text>();
		buttonBox = transform.Find("InputPanel/Grid").transform;

		this.player = player;
		playerName.text = player.playerName;
	}

	public void WriteRoleToTitle() {
		title.text = "You are the " + player.dealtCard.role.ToString();
	}

	public void DisplayDescription() { //Show team alliegence, explain night action, and describe any special rules
		//Team allegiance- You are on the werewolf team.
		//Nature clarity if relevant- You are a villageperson.
		//Special win conditions- If there are no other werewolves, you win if an *other* player dies.
		//Cohort type- You can see other werewolves.
		//Cohort players- Allen is a werewolf.
		//Ogo selection- You may look at the card of another player or two cards from the center.
		//Selection controls- [Buttons for the three center cards]

		List<string> descriptionStrings = new List<string>();
		descriptionStrings.Add(player.dealtCard.team.description);
		descriptionStrings.Add(player.prompt.cohortString);
		description.text = string.Join(" ", descriptionStrings.ToArray());

		foreach(ButtonInfo info in player.prompt.buttons) {
			AddNightActionButton(info.label, info.ogoId);
		}
	}

	private void AddNightActionButton(string label, int oguId) {
		GameObject go = Instantiate(PrefabResource.instance.nightActionButton) as GameObject;
		go.transform.SetParent(buttonBox.transform);
		Text uiText = go.GetComponentInChildren<Text>();
		uiText.text = label;
		OnuButton onuButton = go.GetComponent<OnuButton>();
		onuButton.Initialize(this, oguId);
	}

	public void HandleButtonClick(int oguId) {
		switch(player.prompt.options) {
		case OptionsSet.None:
		case OptionsSet.May_CenterCard:
		case OptionsSet.Must_CenterCard:
			SubmitNightAction(new int[] { oguId });
			break;
		default:
			Debug.LogError("Unhandled options set: " + player.prompt.options);
			break;
		}
	}

	private void SubmitNightAction(int[] oguIds) {
		foreach(Transform button in buttonBox.transform) {
			Destroy(button.gameObject);
		}
		GameController.SubmitNightAction(player, oguIds);
	}

}
