using UnityEngine;
using Zenject;

namespace AI
{
	[DisallowMultipleComponent]
	public class MonsterContainer : MonoBehaviour
	{
#pragma warning disable 649
		[SerializeField] private GameObject _monsterInstance;
#pragma warning restore 649

		[Inject]
		private void Construct(DiContainer container)
		{
			container.Inject(_monsterInstance);
		}
	}
}