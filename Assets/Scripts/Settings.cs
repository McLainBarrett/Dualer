using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour {
	public List<settingMenu> sML= new List<settingMenu>();
	public Dictionary<string, settingMenu> settingsMenus = new Dictionary<string, settingMenu>();

	public GameObject settingMenuButton;
	public GameObject settingMenuObject;
	public GameObject settingField;

	protected void Start() {
		foreach (settingMenu sM in sML) {
			foreach (setting s in sM.sL) {
				sM.sets.Add(s.title, s);
			}
			settingsMenus.Add(sM.title, sM);
			sM.Load();
			sM.setIFVal();
		}


		GameObject SettingMenu = GameObject.Find("Settings Menu");
		int i = 0;
		foreach (KeyValuePair<string, settingMenu> setMenu in settingsMenus) {
			GameObject button = Instantiate(settingMenuButton, SettingMenu.transform);
			button.GetComponentInChildren<Text>().text = setMenu.Key;
			Rect buttonRT = button.GetComponent<RectTransform>().rect; Rect menuRT = SettingMenu.GetComponent<RectTransform>().rect;
			Vector2 buttonSize = new Vector2(buttonRT.width, buttonRT.height);
			int yBounds = Mathf.FloorToInt(menuRT.height/buttonSize.y);
			button.transform.localPosition = new Vector2(buttonSize.x * Mathf.FloorToInt(i / yBounds), -buttonSize.y * (i % yBounds)) + new Vector2(-menuRT.width/2 + buttonRT.width/2, menuRT.height/2 - buttonRT.height/2);

			int j = 0;
			foreach (KeyValuePair<string, setting> set in setMenu.Value.sets) {
				GameObject field = Instantiate(settingField, button.transform);
				button.GetComponentInChildren<Text>().text = setMenu.Key;
				Rect fieldRT = field.GetComponent<RectTransform>().rect;//RectTransform buttonRT = button.GetComponent<RectTransform>(); RectTransform menuRT = SettingMenu.GetComponent<RectTransform>();
				Vector2 fieldSize = new Vector2(fieldRT.width, fieldRT.height);
				yBounds = Mathf.FloorToInt(menuRT.height / fieldSize.y);
				button.transform.localPosition = new Vector2(buttonSize.x * Mathf.FloorToInt(i / yBounds), -buttonSize.y * (i % yBounds)) + new Vector2(-menuRT.width / 2 + fieldRT.width / 2, menuRT.height / 2 - fieldRT.height / 2);
				
				j++;
			}

			i++;
		}
	}

	public void Save() {
		foreach (var set in settingsMenus.Values) {
			set.Save();
		}
	}

	public void getIFVals() {
		foreach (var set in settingsMenus.Values) {
			set.getIFVal();
		}
	}
	public void setIFVals() {
		foreach (var set in settingsMenus.Values) {
			set.setIFVal();
		}
	}

	public void resetValues() {
		foreach (var set in settingsMenus.Values) {
			set.Reset();
		}
	}
}

[Serializable]
public class settingMenu {
	public string title;
	public List<setting> sL = new List<setting>();
	public Dictionary<string, setting> sets = new Dictionary<string, setting>();

	public void getIFVal() {
		foreach (setting set in sets.Values) {
			set.getIFVal();
		}
	}
	public void setIFVal() {
		foreach (setting set in sets.Values) {
			set.setIFVal();
		}
	}

	public void Save() {
		foreach (setting set in sets.Values) {
			set.Save(title);
		}
	}
	public void Load() {
		foreach (setting set in sets.Values) {
			set.Load(title);
		}
	}

	public void Reset() {
		foreach (setting set in sets.Values) {
			set.Reset();
		}
	}
}

[Serializable]
public class setting {
	public string title;
	public InputField inputField;
	[HideInInspector]
	public float value;
	public float defaultValue;

	public void getIFVal() {
		try {
			value = float.Parse(inputField.text);
		} catch (Exception e) { Debug.LogWarning(e); }
	}
	public void setIFVal() {
		inputField.text = value.ToString();
	}

	public void Save(string menuName) {
		PlayerPrefs.SetFloat(title + menuName, value);
	}
	public void Load(string menuName) {
		try {
			value = PlayerPrefs.GetFloat(title + menuName, -909);
			if (value == -909) { throw new Exception("Value not found"); }
		} catch(Exception e) { Debug.LogWarning(e); value = defaultValue; }
	}

	public void Reset() {
		value = defaultValue;
		setIFVal();
	}
}