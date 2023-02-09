using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

	#region Stats
	public bool playerID;

	public float MoveSpeed;
	public float FireSpeed;

	//Health/Energy
	public int Health;
	public int Energy;
	public int HealthCap;
	public int EnergyCap;

	private int overHealth;
	private int overEnergy;

	public float Hregen;
	public float Eregen;
	public float hgen;
	public float egen;

	public float rof;
	protected float reload = 0;
	#endregion
	#region Constant Refs
	public GameObject bul;
	public GameManager GM;
	public Image[] OverHealthBars;
	public Image[] OverEnergyBars;
	public Image[] HealthBars;
	public Image[] EnergyBars;
	public bool IhaveMybars = true;
	public Joystick Move;
	public Joystick Fire;

	public bool clone;
	public Player myClone;
	#endregion

	protected void Start() {
		StartCoroutine(ringCheck());
		statCheck();
	}
	protected void FixedUpdate() {
		if (!GM.paused) {
			transform.position += Time.fixedDeltaTime * MoveSpeed * (Vector3)Move.Output;
			Shoot(Fire.Output);

			reload -= Time.fixedDeltaTime;
			
			if (Health < HealthCap) {
				hgen += Hregen * Time.fixedDeltaTime;
				if (hgen >= 1) {
					Health++;
					hgen = 0;
				}
			}
			if (Energy < EnergyCap) {
				egen += Eregen * Time.fixedDeltaTime;
				if (egen >= 1) {
					Energy++;
					egen = 0;
				}
			}
		}
		#region Border
		var pos = transform.position;
		Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
		if (Mathf.Abs(pos.x) > screenBounds.x)
			transform.position *= new Vector2(-1, 1);
		if (Mathf.Abs(pos.y) > screenBounds.y)
			transform.position *= new Vector2(1, -1);
		#endregion
		updateUI();
		if (Health <= 0)
			GM.PlayerDeath(this);
	}

	public void ResetStats() {
		statCheck();
		#region HealthnEnergy
		if (Mods.Player.Starting.Health.value > 0)
			Health = Mathf.RoundToInt(Mods.Player.Starting.Health.value);
		else Health = HealthCap;
		Energy = Mathf.RoundToInt(Mods.Player.Starting.Energy.value);
		hgen = 0; egen = 0;
		#endregion
		#region Position
		var pos = GM.transform.localScale.x * 0.375f * (playerID ? -1 : 1);
		transform.position = new Vector2(pos, 0);
		GetComponent<Rigidbody2D>().velocity = Vector2.zero;
		#endregion
		#region overX
		overHealth = HealthCap - Health;
		overEnergy = EnergyCap - Energy;
		if (Energy < 0)
			overEnergy = Energy;
		#endregion
		#region Clone
		if (!clone) {
			if (myClone) Destroy(myClone.gameObject);
			if (Mods.Player.Max.Health.value < 0) {
				myClone = Instantiate(gameObject, transform.position, Quaternion.identity).GetComponent<Player>();
				myClone.clone = true;
				myClone.ResetStats();
			}
		} else {
			transform.position = Quaternion.Euler(0, 0, 30) * transform.position;//new Vector3(0, 2);
		}
		//foreach (var player in GameObject.FindGameObjectsWithTag("Player")) {
		//	Player pC = player.GetComponent<Player>();
		//	if (pC != this && pC.playerID == playerID)
		//		if (clone)	Destroy(pC.gameObject);
		//		else		clone = pC;
		//}
		//print(clone);
		//if (Mods.Player.Max.Health.value < 0 && !clone) {
		//	clone = Instantiate(gameObject, transform.position + new Vector3(0, 2), Quaternion.identity).GetComponent<Player>();
		//	//clone.ResetStats();
		//} else if (Mods.Player.Max.Health.value > 0 && clone) {
		//	Destroy(clone.gameObject);
		//}
		#endregion
		updateUI();
	}
	protected void statCheck() {
		HealthCap = Mods.Player.Max.Health.value != 0 ? Mathf.Abs(Mathf.RoundToInt(Mods.Player.Max.Health.value)) : 9999;
		EnergyCap = Mods.Player.Max.Energy.value != 0 ? Mathf.Abs(Mathf.RoundToInt(Mods.Player.Max.Energy.value)) : 9999;

		Hregen = Mods.Player.Regen.Health.value;
		Eregen = Mods.Player.Regen.Energy.value;

		MoveSpeed = Mods.Player.Velocity.value;
		FireSpeed = Mods.Gun.BulletVelocity.value;

		rof = Mods.Gun.RateOfFire.value;
	}

	protected void Shoot(Vector2 vec) {
		if (reload <= 0 && Energy > 0 && vec.magnitude > 0.75f) {
			float val = Mathf.Abs(Mods.Gun.Special.BulletCount.value);
			int cnt = Random.Range(0, 1f) > val - Mathf.FloorToInt(val) ? Mathf.FloorToInt(val) : Mathf.CeilToInt(val);
			bool reqEnergy = Mods.Gun.Special.BulletCount.value < 0;
			float degreeOffset = 5;
			float degree = (cnt % 2 == 1) ? cnt : (cnt - 1) / 2 * -degreeOffset;

			for (int i = 0; i < cnt; i++) {
				if (reqEnergy) {
					if (Energy > 0)
						Energy--;
					else
						break;
				}
				if (Mods.Gun.Special.BulletCount.value < 0)
					degree = Random.Range(-degreeOffset, degreeOffset);
				else
					degree += degreeOffset;
				Vector2 dir = Quaternion.Euler(0, 0, degree) * vec.normalized;
				var bullet = Instantiate(bul, transform.position + (Vector3)dir * 0.01f, Quaternion.identity);
				if (FireSpeed == 0) {
					var rb = bullet.GetComponent<Rigidbody2D>();
					rb.AddForce(dir * 300);
					rb.drag = 5;
				} else
					bullet.GetComponent<Rigidbody2D>().velocity = dir * FireSpeed;
				GetComponent<Rigidbody2D>().AddForce(-vec.normalized * Mods.Gun.Recoil.value * 10);
				bullet.GetComponent<Bullet>().player = this;
			}
			if (!reqEnergy) Energy--;
			if (rof > 0) reload = 1 / rof;
			else reload = Mathf.Abs(rof);
		}
	}

	protected void updateUI() {
		foreach (var bar in OverHealthBars) {
			if (Health > HealthCap)
				bar.fillAmount = (HealthCap - Health) / (float)overHealth;
			else
				bar.fillAmount = 0;
		}
		foreach (var bar in HealthBars) {
			bar.fillAmount = Health / (float)HealthCap;
		}

		foreach (var bar in OverEnergyBars) {
			if (Energy > EnergyCap) {
				bar.fillAmount = (EnergyCap - Energy) / (float)overEnergy;
				bar.color = Color.yellow;
			} else if (Energy < 0) {
				bar.fillAmount = (Energy) / (float)overEnergy;
				bar.color = new Color(0.2f, 0, 1);
			} else
				bar.fillAmount = 0;
		}
		foreach (var bar in EnergyBars) {
			bar.fillAmount = Energy / (float)EnergyCap;
		}
	}
	protected IEnumerator ringCheck() {
		var val = Mods.Arena.Punishment.value;
		if (val == 0)
			yield return null;
		else if (val > 0)
			yield return new WaitForSeconds(1/Mods.Arena.Punishment.value);
		else
			yield return new WaitForSeconds(1 / -Mods.Arena.Punishment.value);
		float pos = (transform.position - GM.gameObject.transform.position).magnitude;
		if (pos < Mathf.Abs(GM.Dot.localScale.x/2) || pos > GM.transform.localScale.x/2) {
			Health -= (Mods.Arena.Punishment.value > 0) ? 1 : -1;
		}
		StartCoroutine(ringCheck());
	}
}