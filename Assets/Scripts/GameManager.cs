using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	#region Scores
	public int player0Score;
	public int player1Score;

	public Text player0ScoreInd;
	public Text player1ScoreInd;
	#endregion

	#region Refs
	//public Settings settings;

	public Player player0;
	public Player player1;

	public GameObject pauseMenu;
	public GameObject WinInd;

	public GameObject prompt;
	public GameObject SettingMenu;

	public Transform Dot;
	//public float dotScale;
	#endregion

	public bool paused;
	public float arenaScale;
	float ASdecay; float ASdelay;
	float pauseStop = 0;

	protected void Start() {
		ModsAid.SaveLoad(ModsAid.GetAllMembers(typeof(Mods)), "/", false);
		NewGame(true);
		SettingMenu = pauseMenu.transform.Find("ModMenuGO").gameObject;
	}
	private void OnApplicationQuit() {
		ModsAid.SaveLoad(ModsAid.GetAllMembers(typeof(Mods)), "/", true);
	}

	protected void Update() {
		pauseStop -= Time.unscaledDeltaTime;
		if (paused) {
			Time.timeScale = 0;
			pauseMenu.SetActive(true);
			if (Input.touches.Length > 2 && pauseStop <= 0)
				paused = false;
		} else {
			Time.timeScale = 1;
			pauseMenu.SetActive(false);
			//if (ASdelay <= 0 && Mods.Arena.Scale.value >= ASdecay) ASdecay += Mods.Arena.Decay.value * Time.deltaTime;
			//else ASdelay -= Time.deltaTime;
			ASdelay -= Time.deltaTime;
			if (ASdelay <= 0) {
				if (Mods.Arena.DecayDelay.value >= 0) {
					if (arenaScale > 0) arenaScale -= Mods.Arena.Decay.value * Time.deltaTime;
					else arenaScale = 0;
					if (Mods.Arena.Scale.value < 0) Dot.localScale = transform.localScale / 2;
				} else  {
					float ls = Dot.localScale.x + Mods.Arena.Decay.value * Time.deltaTime;//Something is terribly wrong here
					if (ls < arenaScale+1)
						Dot.localScale = new Vector3(ls, ls);
				}
			}
		}
		transform.localScale = new Vector2(arenaScale, arenaScale);
		if (Mods.Arena.Decay.value < 0)
			Camera.main.orthographicSize = (arenaScale > 0 ? arenaScale : 10) / 10 * 6;

		player0ScoreInd.text = player0Score.ToString();
		player1ScoreInd.text = player1Score.ToString();
	}

	#region Reseting
	public void NewGame(bool resetScore) {
		if (resetScore)
			resetScores();
		ResetBoard();
	}

	public void PlayerDeath(Player player) {
		if (player.clone) {
			if ((player.playerID ? player1 : player0).gameObject.activeSelf) {
				Destroy(player.gameObject);
				return;
			}
			Destroy(player.gameObject);
		} else if (player.myClone) {
			player.gameObject.SetActive(false);
			return;
		}

		if (player.playerID) {
			player0Score++;
		} else {
			player1Score++;
		}
		ResetBoard();
		WinInd.SetActive(true);
		WinInd.GetComponent<Image>().color = player.playerID ? Color.red : Color.blue;
		pauseStop = 1;
	}

	protected void ResetBoard() {
		#region Destroying Objects
		List<GameObject> buls = new List<GameObject>();
		buls.AddRange(GameObject.FindGameObjectsWithTag("Bullet"));
		buls.AddRange(GameObject.FindGameObjectsWithTag("ParticleSystem"));
		int bc = buls.Count;
		for (int i = 0; i < bc; i++) {
			Destroy(buls[i]);
		}
		#endregion
		#region Scaling
		ASdelay = Mathf.Abs(Mods.Arena.DecayDelay.value);
		ASdecay = 0;
		arenaScale = Mathf.Abs(Mods.Arena.Scale.value);
		transform.localScale = new Vector2(arenaScale, arenaScale);
		Camera.main.orthographicSize = (arenaScale != 0 ? arenaScale : 10) / 10 * 6;
		if (Mods.Arena.Scale.value < 0) Dot.localScale = transform.localScale/2;
		else Dot.localScale = Vector2.zero;
		#endregion
		player0.gameObject.SetActive(true); player0.ResetStats();
		player1.gameObject.SetActive(true); player1.ResetStats();
		paused = true;
	}

	protected void resetScores() {
		player0Score = 0;
		player1Score = 0;
		WinInd.SetActive(false);
	}
	#endregion

	#region UI
	public void NG() {
		promptUser("New Game?", delegate { NewGame(true); });
	}

	public void SETS() {
		promptUser("Open Settings?", delegate { SettingMenu.SetActive(true); NewGame(false); });
	}

	protected void promptUser(string name, Action action) {
		prompt.SetActive(true);
		prompt.transform.Find("Title").GetComponent<Text>().text = name;
		var but = prompt.transform.Find("Confirm").GetComponent<Button>();
		but.onClick.RemoveAllListeners();
		but.onClick.AddListener(delegate { action(); });
		but.onClick.AddListener(delegate { prompt.SetActive(false); });
	}

	public void Pause(bool pause) {
		paused = pause;
	}
	#endregion
}