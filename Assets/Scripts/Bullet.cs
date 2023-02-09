using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Bullet : MonoBehaviour {

	public Player player;
	protected Collider2D playerCol;
	public GameObject Explosion;
	protected bool live;
	public float proxyFuse;
	protected int armed;

	protected Player target = null;
	protected float range;

	public bool fragment;

	protected void Start() {
		if (player.playerID) {
			GetComponent<SpriteRenderer>().color = Color.blue;
		} else {
			GetComponent<SpriteRenderer>().color = Color.red;
		}
		playerCol = player.gameObject.GetComponent<Collider2D>();
		transform.GetChild(0).GetComponent<Collider2D>().enabled = false;
		proxyFuse = Mathf.Abs(Mods.Gun.Special.Airburst.value);
		StartCoroutine(kill());
	}

	private void Update() {
		if (!fragment) {
			if (!live && !Physics2D.OverlapCircleAll(transform.position, transform.localScale.x / 2).Contains(playerCol)) {
				transform.GetChild(0).GetComponent<Collider2D>().enabled = true;
				live = true;
			}
			if (proxyFuse > 0 && armed == 0 && !Physics2D.OverlapCircleAll(transform.position, proxyFuse).Contains(playerCol))
				armed = 1;
			if (armed == 1) {
				List<Player> players = new List<Player>();
				foreach (var item in Physics2D.OverlapCircleAll(transform.position, proxyFuse)) {
					var p = item.GetComponent<Player>();
					if (p) players.Add(p);
				}
				if (target) {
					print(target);
					var dist = (target.transform.position - transform.position).magnitude;
					if (dist > range) {
						print(dist + " -- " + range);
						if (Mods.Gun.Special.ExplosiveBullets.value != 0) Explode();
						else GetComponent<Rigidbody2D>().velocity = (target.transform.position - transform.position).normalized * Mods.Gun.BulletVelocity.value;
						armed = 2;
					}
					range = dist;
				} else {
					if (players.Where(e => e.playerID != player.playerID).ToArray().Length > 0 || (Mods.Gun.Special.Airburst.value < 0 && players.Count > 0)) {
						if (Mods.Gun.Special.Airburst.value < 0) target = players[0];
						else target = players.Where(e => e.playerID != player.playerID).ToArray()[0];
						range = (target.transform.position - transform.position).magnitude;
					}
				}
			}
		} else {
			live = true;
			transform.GetChild(0).GetComponent<Collider2D>().enabled = true;
			transform.localScale = new Vector2(0.25f, 0.25f);
		}

		if (Mods.Gun.Special.BulletsLoop.value != 0) {
			Vector2 pos = transform.position;
			Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
			Rigidbody2D rb = GetComponent<Rigidbody2D>();
			if (Mods.Gun.Special.BulletsLoop.value > 0) {
				if (Mathf.Abs(pos.x) > screenBounds.x) {
					transform.position *= new Vector2(-1, 1);
					rb.velocity *= Mods.Gun.Special.BulletsLoop.value;
				} else if (Mathf.Abs(pos.y) > screenBounds.y) {
					transform.position *= new Vector2(1, -1);
					rb.velocity *= Mods.Gun.Special.BulletsLoop.value;
				}
			} else {
				if (Mathf.Abs(pos.x) > screenBounds.x)
					rb.velocity *= new Vector2(-1, 1) * -Mods.Gun.Special.BulletsLoop.value;
				if (Mathf.Abs(pos.y) > screenBounds.y)
					rb.velocity *= new Vector2(1, -1) * -Mods.Gun.Special.BulletsLoop.value;
			}
			if (rb.velocity.magnitude > 150)
				rb.velocity /= rb.velocity.magnitude / 150;
		}

		if (transform.position.magnitude > 200) Destroy(gameObject);
	}

	protected void OnTriggerEnter2D(Collider2D col) {
		Player plaGO = col.gameObject.GetComponent<Player>();
		if (plaGO && live && (plaGO.playerID != player.playerID || Mods.Gun.BulletLife.value < 0)) {
			if (Mods.Gun.Special.ExplosiveBullets.value != 0 && !fragment) {
				Explode();
			} else {
				plaGO.GetComponent<Rigidbody2D>().AddForce((GetComponent<Rigidbody2D>().velocity).normalized * Mods.Gun.Knockback.value * 10);
				plaGO.Health -= 1;
				Burst();
			}
			return;
		}
	}

	protected IEnumerator kill() {
		if (Mods.Gun.BulletLife.value != 0) yield return null;
		yield return new WaitForSeconds(Mathf.Abs(Mods.Gun.BulletLife.value));
		if (Mods.Gun.Special.ExplosiveBullets.value > 0) Explode();
		else Burst();
	}

	protected void Burst() {
		ParticleSystem ps = transform.Find("ParticleSystem").GetComponent<ParticleSystem>();
		var mainPS = ps.main;
		mainPS.startColor = new ParticleSystem.MinMaxGradient(GetComponent<SpriteRenderer>().color, Color.black);
		
		ps.transform.parent = null;
		var rb = ps.gameObject.AddComponent<Rigidbody2D>();
		rb.gravityScale = 0;
		rb.velocity = GetComponent<Rigidbody2D>().velocity;

		ps.Play();
		Destroy(gameObject);
	}

	protected void Explode() {
		if (Mods.Gun.Special.ExplosiveBullets.value > 0) {
			var boom = Instantiate(Explosion, transform.position, Quaternion.identity);
			boom.transform.position = transform.position;
			boom.GetComponent<Explosion>().Boom();
			Destroy(gameObject);
		} else {
			float val = Mathf.Abs(Mods.Gun.Special.ExplosiveBullets.value);
			int cnt = Random.Range(0, 1f) > val - Mathf.FloorToInt(val) ? Mathf.FloorToInt(val) : Mathf.CeilToInt(val);

			for (int i = 0; i < cnt; i++) {
				float FireSpeed = Mods.Gun.BulletVelocity.value;
				Vector2 dir = Quaternion.Euler(0, 0, Random.Range(0, 359)) * Vector2.up;
				var bullet = Instantiate(this, transform.position + (Vector3)dir * 0.01f, Quaternion.identity);
				if (FireSpeed == 0) {
					var rb = bullet.GetComponent<Rigidbody2D>();
					rb.AddForce(dir * 300);
					rb.drag = 5;
				} else
					bullet.GetComponent<Rigidbody2D>().velocity = dir * FireSpeed;
				var bul = bullet.GetComponent<Bullet>();
				bul.player = player;
				bul.fragment = true;
			}
			Destroy(gameObject);
		}
	}
}