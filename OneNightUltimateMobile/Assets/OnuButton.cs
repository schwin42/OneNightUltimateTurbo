using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OnuButton : MonoBehaviour {

	PlayerUi playerUi;
	int locationId;

	void Start() {
		GetComponent<Button>().onClick.AddListener(HandleClick);
	}

	public void Initialize(PlayerUi playerUi, int locationId) {
		this.playerUi = playerUi;
		this.locationId = locationId;
	}

	void HandleClick () {
		playerUi.HandleButtonClick(locationId);
	}
}
