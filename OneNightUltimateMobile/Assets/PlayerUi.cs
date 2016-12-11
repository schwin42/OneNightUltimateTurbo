using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

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
		title.text = "You are the " + player.dealtCard.ToString();
	}

	public void DisplayDescription() { //Show team alliegence, explain night action, and describe any special rules
		Prompt prompt = GameController.GetPrompt(player);
		if(prompt == null) return;
		print("writing description: " + prompt.description);
		description.text = prompt.description;
		switch(prompt.options) {
		case OptionsSet.May_CenterCard:
			for(int i = 0; i < 3; i++) {
				AddNightActionButton("Card #" + (i + 1).ToString(), GameController.centerCards[0].runtimeId);
				AddNightActionButton("Card #" + (i + 1).ToString(), GameController.centerCards[1].runtimeId);
				AddNightActionButton("Card #" + (i + 1).ToString(), GameController.centerCards[2].runtimeId);
				AddNightActionButton("Pass", -1);
			}
			break;
		}
	}

	private void AddNightActionButton(string s, int oguId) {
		GameObject go = Instantiate(PrefabResource.instance.nightActionButton) as GameObject;
		go.transform.SetParent(buttonBox.transform);
		OnuButton button = go.GetComponent<OnuButton>();
		button.Initialize(this, oguId);
	}

	private void HandleButtonClick(int oguId) {
		switch(GameController.GetPrompt(player).options) {
		case OptionsSet.May_CenterCard:
			SubmitNightAction(new int[] { oguId });
			break;
		}
	}

	private void SubmitNightAction(int[] oguIds) {
		foreach(GameObject button in buttonBox.transform) {
			Destroy(button);
		}
		GameController.SubmitNightAction(player, oguIds);
	}

}
