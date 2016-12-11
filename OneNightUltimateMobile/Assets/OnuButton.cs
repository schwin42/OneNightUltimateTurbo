using UnityEngine;
using System.Collections;

public class OnuButton : MonoBehaviour {

	PlayerUi playerUi;
	int oguId;

	public void Initialize(PlayerUi playerUi, int oguId) {
		this.playerUi = playerUi;
		this.oguId = oguId;
	}

	void onClick () {

	}
}
