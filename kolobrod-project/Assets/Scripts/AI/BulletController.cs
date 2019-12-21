using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;

namespace AI
{
	public class BulletController : AmmoController
	{
		private readonly Collider2D[] _affected = new Collider2D[32];
		private readonly List<ContactPoint2D> _contacts = new List<ContactPoint2D>();

#pragma warning disable 649
		[SerializeField] private float _power = 10f;
		[SerializeField] private float _radius = 5f;
		[SerializeField] private GameObject _hitSplashPrefab;
#pragma warning restore 649

		private void Start()
		{
			Destroy(gameObject, 1f);
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			if (_hitSplashPrefab != null &&
			    GetComponent<Collider2D>().bounds.Intersection(other.bounds, out var intersection))
			{
				var splash = Instantiate(_hitSplashPrefab, intersection.center, Quaternion.identity);
				Destroy(splash, 2f);
			}

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
				var vector = body.transform.lossyScale * _power;
				body.AddForceAtPosition(vector, p, ForceMode2D.Impulse);
			}

			other.GetContacts(_contacts);
			if (_hitSplashPrefab != null && _contacts.Count > 0)
			{
				var splash = Instantiate(_hitSplashPrefab, _contacts.First().point, Quaternion.identity);
				Destroy(splash, 2f);
			}

			Destroy(gameObject);
		}
	}
}