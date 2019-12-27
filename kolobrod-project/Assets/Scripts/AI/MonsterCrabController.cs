using System.Collections;
using Anima2D;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace AI
{
	[DisallowMultipleComponent, RequireComponent(typeof(Animator))]
	public class MonsterCrabController : MonoBehaviour
	{
		private Rigidbody2D[] _rigidBodies;
		private IkLimb2D[] _limbs;

		private readonly CompositeDisposable _collisionHandlers = new CompositeDisposable();

#pragma warning disable 649
		[SerializeField] private Rigidbody2D _container;
#pragma warning restore 649

		private void Start()
		{
			_rigidBodies = GetComponentsInChildren<Rigidbody2D>();
			_limbs = GetComponentsInChildren<IkLimb2D>();
			foreach (var rigidBody in _rigidBodies)
			{
				rigidBody.bodyType = RigidbodyType2D.Kinematic;
				rigidBody.GetComponent<Collider2D>().isTrigger = true;
				_collisionHandlers.Add(rigidBody.OnTriggerEnter2DAsObservable().Subscribe(OnCollide));
			}

			_container.OnCollisionEnter2DAsObservable().First().Subscribe(collision =>
			{
//				_container.velocity = _container.velocity * Vector2.up;
			});
			StartCoroutine(Move());
		}

		private void OnDestroy()
		{
			_collisionHandlers.Dispose();
		}

		private void OnCollide(Collider2D collision)
		{
			Debug.Log("COLLIDE!");
		}

		private IEnumerator Move()
		{
			yield return new WaitForSeconds(3);
			Die();
/*
			_container.FixedUpdateAsObservable().Subscribe(unit =>
			{
				_container.velocity = Vector2.left * 5f;
			});
*/
		}

		public void OnAttack()
		{
		}

		public void Die()
		{
			transform.SetParent(null, true);
			Destroy(_container.gameObject);

			_collisionHandlers.Dispose();
			foreach (var rigidBody in _rigidBodies)
			{
				rigidBody.bodyType = RigidbodyType2D.Dynamic;
				rigidBody.GetComponent<Collider2D>().isTrigger = false;
			}

			foreach (var limb in _limbs)
			{
				limb.gameObject.SetActive(false);
			}
		}
	}
}