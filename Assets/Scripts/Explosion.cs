using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {
	public SpriteRenderer blast;
	public void Boom() {
		float range = Mods.Gun.Special.ExplosiveBullets.value;
		transform.localScale = new Vector2(range*2, range*2);
		Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, range);
		foreach (var col in cols) {
			var player = col.gameObject.GetComponent<Player>();
			if (player) {
				player.Health -= 1;
				player.gameObject.GetComponent<Rigidbody2D>().AddForce((player.transform.position - transform.position).normalized * Mods.Gun.Knockback.value);
			}
		}
	}
	private void Update() {
		blast.color *= 0.9f;
	}
	
	IEnumerator Die() {
		yield return new WaitForSeconds(1);
		Destroy(gameObject);
	}
}