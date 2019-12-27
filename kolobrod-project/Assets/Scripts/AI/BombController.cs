using System.Collections;
using UnityEngine;

namespace AI
{
	[DisallowMultipleComponent]
	public class BombController : MonoBehaviour
	{
		private readonly Collider2D[] _affected = new Collider2D[32];

#pragma warning disable 649
		[SerializeField] private float _power = 10f;
		[SerializeField] private float _radius = 5f;
		[SerializeField] private float _delayTime = 3f;
#pragma warning restore 649

		private void Start()
		{
			StartCoroutine(Starter());
		}

		private IEnumerator Starter()
		{
			yield return new WaitForSeconds(_delayTime);
			Detonate();
		}

		public void Detonate()
		{
			var p = transform.position;
			var size = Physics2D.OverlapCircleNonAlloc(p, _radius, _affected);
			for (var i = 0; i < size; ++i)
			{
				var body = _affected[i].GetComponent<Rigidbody2D>();
				if (body == null) continue;
				body.AddForceAtPosition(new Vector2(_power, _power), p, ForceMode2D.Impulse);
			}
		}
	}
}