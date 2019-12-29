using UnityEngine;

namespace AI
{
	public class BulletController : AmmoController
	{
		private readonly Collider2D[] _affected = new Collider2D[32];

#pragma warning disable 649
		[SerializeField] private float _power = 10f;
		[SerializeField] private float _radius = 5f;
#pragma warning restore 649

		private void Start()
		{
			Destroy(gameObject, 1f);
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			Destroy(gameObject);
		}

		private void OnCollisionEnter2D(Collision2D other)
		{
			var p = transform.position;
			var size = Physics2D.OverlapCircleNonAlloc(p, _radius, _affected);
			for (var i = 0; i < size; ++i)
			{
				var body = _affected[i].GetComponent<Rigidbody2D>();
				if (body == null) continue;
				body.AddForceAtPosition(new Vector2(_power, _power), p, ForceMode2D.Impulse);
			}

			Destroy(gameObject);
		}
	}
}