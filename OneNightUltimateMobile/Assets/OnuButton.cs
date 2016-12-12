using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OnuButton : MonoBehaviour {

	PlayerUi playerUi;
	int oguId;

	void Start() {
		GetComponent<Button>().onClick.AddListener(HandleClick);
	}

	public void Initialize(PlayerUi playerUi, int oguId) {
		this.playerUi = playerUi;
		this.oguId = oguId;
	}

	void HandleClick () {
		playerUi.HandleButtonClick(oguId);
	}
}
