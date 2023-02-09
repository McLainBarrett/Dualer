using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Linq;

public class PresetMenu : MonoBehaviour {

	//Refs
	public ModMenu2 ModMenu;

	//Prefabs
	public GameObject buttonPrefab;
	public GameObject content;

	//Lists
	public List<Modifier> modsSelected = new List<Modifier>();
	public List<GameObject> fieldsSelected = new List<GameObject>();
	[HideInInspector]
	public List<string> presetPaths = new List<string>();

	//UI
	private InputField PresetNameInputField;
	public GameObject selectedButton;
	public string selectedPresetPath;

	private void Start() {
		RefreshList();
		PresetNameInputField = gameObject.GetComponentInChildren<InputField>();
	}

	private void ConstructList() {
		foreach (Transform child in content.transform) {
			Destroy(child.gameObject);
		}
		float listYval = 0;
		foreach (var presetPath in presetPaths) {
			var button = Instantiate(buttonPrefab, content.transform);
			string[] pathFrags= presetPath.Split('/');
			button.GetComponentInChildren<Text>().text = pathFrags[pathFrags.Length-1].Replace(".txt", "");
			button.GetComponent<Button>().onClick.AddListener(delegate { Select(button, presetPath); });

			Rect RT = button.GetComponent<RectTransform>().rect;
			if (listYval == 0)
				listYval -= RT.height/2;
			else
				listYval -= RT.height * 1.1f;
			var rt = content.GetComponent<RectTransform>();
			if (-listYval > rt.rect.height)
				rt.sizeDelta += new Vector2(0, RT.height * 1.1f);
			button.transform.localPosition = new Vector2(RT.width/2, listYval);
		}
	}
	private void RefreshList() {
		presetPaths = new List<string>();
		if (!new DirectoryInfo(Application.persistentDataPath + "/Resources/Presets").Exists)
			Directory.CreateDirectory(Application.persistentDataPath + "/Resources/Presets");
		foreach (FileInfo file in new DirectoryInfo(Application.persistentDataPath + "/Resources/Presets").GetFiles("*.txt")) {
			if (!presetPaths.Contains(Application.persistentDataPath + "/Resources/Presets/" + file.Name))
				presetPaths.Add(Application.persistentDataPath + "/Resources/Presets/" + file.Name);
		}
		ConstructList();
	}

	public static void ToggleSelection(Modifier mod, GameObject button) {
		PresetMenu PM = GameObject.FindGameObjectWithTag("PresetMenu").GetComponent<PresetMenu>();

		if (!PM.modsSelected.Contains(mod)) {
			PM.modsSelected.Add(mod);
			PM.fieldsSelected.Add(button);
			button.GetComponent<Image>().color = Color.green;
			ModMenu2 MM = button.transform.parent.parent.GetComponent<ModMenu2>();
			if (MM.MyButton) {
				MM.MyButton.GetComponent<Image>().color = Color.green;
			}
		} else {
			PM.modsSelected.Remove(mod);
			PM.fieldsSelected.Remove(button);
			button.GetComponent<Image>().color = Color.white;
			ModMenu2 MM = button.transform.parent.parent.GetComponent<ModMenu2>();
			if (MM.MyButton && MM.Contents.Where(x => !PM.fieldsSelected.Contains(x)).ToArray().Length > 0) {
				MM.MyButton.GetComponent<Image>().color = Color.black;
			}
		}
	}

	public void Select(GameObject button, string path) {
		selectedPresetPath = path;
		if (selectedButton)
			selectedButton.GetComponent<Image>().color = Color.white;
		button.GetComponent<Image>().color = Color.green / 2;
		selectedButton = button;
	}
	public void Save() {
		if (PresetNameInputField.text != "") {
			string path = ModsAid.SavePresent(modsSelected.ToArray(), PresetNameInputField.text);
			RefreshList();
		}
	}
	public void Load() {
		while (modsSelected.Count > 0) {
			ToggleSelection(modsSelected[0], fieldsSelected[0].transform.parent.Find("Button").gameObject);
		}

		if (selectedPresetPath != "") {
			var mods = ModsAid.LoadPresent(selectedPresetPath);
			foreach (var mod in mods) {
				if (mod.InputField)
					ToggleSelection(mod, mod.InputField.transform.parent.Find("Button").gameObject);

				//mod.InputField.transform.parent.GetComponentInChildren<Button>().GetComponent<Image>().color = 
			}
			var pthfgs = selectedPresetPath.Split('/');
			PresetNameInputField.text = pthfgs[pthfgs.Length-1];
		}
			
		ModMenu.RefreshMe();
	}
	public void Delete() {
		if (selectedPresetPath != "") {
			File.Delete(selectedPresetPath);
			RefreshList();
		}
	}
	public void ClearSelection() {
		while (modsSelected.Count > 0) {
			ToggleSelection(modsSelected[0], fieldsSelected[0].transform.parent.Find("Button").gameObject);
		}
		if (selectedButton)
			selectedButton.GetComponent<Image>().color = Color.white;
		selectedButton = null;
		selectedPresetPath = "";
		PresetNameInputField.text = "";
	}
}