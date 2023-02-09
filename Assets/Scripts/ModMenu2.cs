using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class ModMenu2 : MonoBehaviour {

	public bool origin = false;

	[HideInInspector]
	public GameObject MyButton;
	public GameObject ModField;
	public GameObject MenuButton;
	private GameObject ModMenuGO;

	public MemberInfo[] Me;
	//public Dictionary<string, object> Me;
	public List<GameObject> Contents = new List<GameObject>();

	private void Start() {
		ModMenuGO = Resources.Load<GameObject>("Prefabs/ModMenuGO");
		if (origin)
			Spawn(ModsAid.GetAllMembers(typeof(Mods)));
	}

	private void Spawn(MemberInfo[] MI) {
		Me = MI;
		Rect myRect = gameObject.GetComponent<RectTransform>().rect;
		int i = 0;
		foreach (MemberInfo Mi in MI) {
			GameObject thing = null;

			string name = Mi.Name;
			name = System.Text.RegularExpressions.Regex.Replace(name, "[A-Z]", " $0");
			name.TrimStart(' ');
			Modifier mod = null;
			Type MiClass = null;

			if (Mi.MemberType == MemberTypes.Field) {
				thing = ModField;
				mod = (Modifier)((FieldInfo)Mi).GetValue(Mi);
			} else if (Mi.MemberType == MemberTypes.NestedType) {
				thing = MenuButton;
				MiClass = (Type)Mi;
			}

			GameObject button = Instantiate(thing, transform);
			button.GetComponentInChildren<Text>().text = name;
			Rect buttonRT = button.GetComponent<RectTransform>().rect;
			Vector2 buttonSize = new Vector2(buttonRT.width, buttonRT.height * 1.1f);
			int yBounds = Mathf.FloorToInt(myRect.height / buttonSize.y);
			button.transform.localPosition = new Vector2(buttonSize.x * (Mathf.FloorToInt(i / yBounds) + 1), -buttonSize.y * (i % yBounds)) 
				+ new Vector2(-myRect.width / 2 + buttonRT.width / 2, myRect.height / 2 - buttonRT.height / 2);

			if (mod != null) {
				InputField IF = button.GetComponentInChildren<InputField>();
				mod.InputField = IF;
				IF.onEndEdit.AddListener(delegate { IFSetMod(mod, button); });
				var ifb = IF.transform.parent.GetComponentInChildren<Button>();
				ifb.onClick.AddListener(delegate { PresetMenu.ToggleSelection(mod, ifb.gameObject); });
				IF.text = mod.value.ToString();
				ifb.transform.Find("Title").GetComponent<Text>().text = name;
				button.transform.SetAsFirstSibling();
				Contents.Add(IF.transform.parent.gameObject);
			} else {
				if (!ModMenuGO) ModMenuGO = Resources.Load<GameObject>("Prefabs/ModMenuGO");
				GameObject MenuGO = Instantiate(ModMenuGO, transform);
				Rect RTM = MenuGO.GetComponent<RectTransform>().rect;
				RTM.center = new Vector2(0, 0);
				MenuGO.GetComponentInChildren<ModMenu2>().MyButton = button;
				MenuGO.GetComponentInChildren<ModMenu2>().Spawn(ModsAid.GetAllMembers(MiClass));
				button.GetComponentInChildren<Button>().onClick.AddListener(delegate { MenuGO.SetActive(true); });
				MenuGO.SetActive(false);
				button.transform.SetAsFirstSibling();
				MenuGO.transform.SetAsLastSibling();
				Contents.Add(MenuGO.gameObject);
			}

			i++;
		}
	}

	public void IFSetMod(Modifier mod, GameObject go) {
		mod.value = float.Parse(go.GetComponentInChildren<InputField>().text);
	}

	public void ResetMe() {
		ResetFields(Me);
	}
	private void ResetFields(MemberInfo[] MI) {
		foreach (MemberInfo Mi in MI) {
			if (Mi.MemberType == MemberTypes.Field) {
				Modifier mod = (Modifier)((FieldInfo)Mi).GetValue(Mi);
				mod.Reset();
				mod.InputField.text = mod.value.ToString();
			} else if (Mi.MemberType == MemberTypes.NestedType) {
				ResetFields(ModsAid.GetAllMembers((Type)Mi));
			}
		}
	}

	public void RefreshMe() {
		RefreshFields(Me);
	}
	private void RefreshFields(MemberInfo[] MI) {
		foreach (MemberInfo Mi in MI) {
			if (Mi.MemberType == MemberTypes.Field) {
				Modifier mod = (Modifier)((FieldInfo)Mi).GetValue(Mi);
				mod.InputField.text = mod.value.ToString();
			} else if (Mi.MemberType == MemberTypes.NestedType) {
				RefreshFields(ModsAid.GetAllMembers((Type)Mi));
			}
		}
	}
}