using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnuToggle : MonoBehaviour {

	Toggle toggle;
	Image image;
	Color defaultColor;
	Color toggledColor = Color.green;

	PlayerUi ui;
	int selectionId;
	int noSelectionId;

	public void Initialize(PlayerUi ui, ToggleGroup toggleGroup, int selectionId, int noSelectionId) {
		this.ui = ui;
		this.selectionId = selectionId;
		this.noSelectionId = noSelectionId;

		toggle = GetComponent<Toggle> ();
		toggle.onValueChanged.AddListener (HandleValueChanged);
		image = GetComponent<Image> ();
		defaultColor = image.color;

		toggle.group = toggleGroup;
	}

	void HandleValueChanged(bool isOn) {
		if (isOn) {
			image.color = toggledColor;

			ui.HandleToggleChange (selectionId);
		} else {
			image.color = defaultColor;

			if (!toggle.group.AnyTogglesOn()) {
				ui.HandleToggleChange (noSelectionId);
			}
		}
	}
}
