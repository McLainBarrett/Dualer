using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public static class Mods {//Add behavors for -, 0, +, .x
	public static class Player {
		public static class Max {
			public static Modifier Health = new Modifier(5);
			public static Modifier Energy = new Modifier(10);
		}
		public static class Starting {
			public static Modifier Health = new Modifier(5);
			public static Modifier Energy = new Modifier(0);
		}
		public static class Regen {
			public static Modifier Health = new Modifier(0.2f);
			public static Modifier Energy = new Modifier(1);
		}
		public static Modifier Velocity = new Modifier(5);
	}
	public static class Gun {
		public static Modifier RateOfFire = new Modifier(4);
		public static Modifier BulletVelocity = new Modifier(15);
		public static Modifier BulletLife = new Modifier(5);
		public static Modifier Knockback = new Modifier(30);
		public static Modifier Recoil = new Modifier(10);
		public static class Special {
			public static Modifier BulletCount = new Modifier(1);
			public static Modifier ExplosiveBullets = new Modifier(0);
			public static Modifier Airburst = new Modifier(0);
			public static Modifier BulletsLoop = new Modifier(0);
		}
	}
	public static class Arena {
		public static Modifier Punishment = new Modifier(1);
		public static Modifier Scale = new Modifier(10);
		public static Modifier Decay = new Modifier(0);
		public static Modifier DecayDelay = new Modifier(0);
	}
}

public static class ModsAid {
	public static MemberInfo[] GetAllMembers(Type classType) {
		MemberInfo[] mods = classType.GetMembers();
		List<MemberInfo> excludes = new List<MemberInfo>{ classType.GetMember("Equals")[0],
			classType.GetMember("GetHashCode")[0], classType.GetMember("GetType")[0], classType.GetMember("ToString")[0] };

		return mods.Except(excludes).ToArray();
	}

	public static void SaveLoad(MemberInfo[] MI, string path, bool Save) {
		foreach (MemberInfo Mi in MI) {
			if (Mi.MemberType == MemberTypes.Field) {
				Modifier mod = (Modifier)((FieldInfo)Mi).GetValue(Mi);
				if (Save) {
					mod.Save(path + Mi.Name);
				} else {
					mod.Load(path + Mi.Name);
					mod.path = path + Mi.Name;
				}

			} else if (Mi.MemberType == MemberTypes.NestedType) {
				SaveLoad(GetAllMembers((Type)Mi), path + Mi.Name + "/", Save);
			}
		}
	}

	public static string SavePresent(Modifier[] input, string name) {
		if (name == "") return null;
		string output = "";
		foreach (var inpu in input) {
			output += inpu.path + ":" + inpu.value + ";";
		}
		StreamWriter file = File.CreateText(Application.persistentDataPath + "/Resources/Presets/" + name + ".txt");
		file.Write(output);
		file.Close();
		return Application.persistentDataPath + "/Resources/Presets/" + name + ".txt";
	}

	public static Modifier[] LoadPresent(string filePath) {
		string file = File.ReadAllText(filePath);
		string[] modStrings = file.Split(';');
		List<Modifier> mods = new List<Modifier>();

		foreach (string modString in modStrings) {
			string[] modFrags = modString.Split(':');
			string[] pathElements = modFrags[0].Split('/');
			MemberInfo[] MI = GetAllMembers(typeof(Mods));
			MemberInfo Mi;
			Modifier mod = null;

			foreach (string elem in pathElements) {
				Mi = MI.Where(obj => obj.Name == elem).SingleOrDefault();
				if (Mi == null) continue;
				if (Mi.MemberType == MemberTypes.NestedType) {
					MI = GetAllMembers((Type)Mi);
				} else {
					mod = (Modifier)((FieldInfo)Mi).GetValue(Mi);
					break;
				}
			}

			if (mod != null) {
				mod.value = float.Parse(modFrags[1]);
				mods.Add(mod);
			}
		}
		return mods.ToArray();
	}
}

public class Modifier {
	private float defaultValue;
	public float value;
	public string path;
	public InputField InputField;

	public Modifier(float ModifierValue) {
		defaultValue = ModifierValue;
	}

	public void Save(string name) {
		PlayerPrefs.SetFloat(name, value);
		//Debug.Log("Saving @ Path, Name, Value: " + name + " -- " + value);
	}

	public void Load(string name) {
		path = name;
		float val = PlayerPrefs.GetFloat(name, -909);
		if (val == -909) {
			Debug.LogWarning("Value not found");
			value = defaultValue;
		} else {
			value = val;
		}
		//Debug.Log("Loading @ Name, Value: " + name + " -- " + value);
	}

	public void Reset() {
		value = defaultValue;
	}
}