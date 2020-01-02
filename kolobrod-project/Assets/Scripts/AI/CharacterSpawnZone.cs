using System;
using Common;
using UnityEngine;
using Zenject;

namespace AI
{
	[DisallowMultipleComponent]
	public class CharacterSpawnZone : MonoInstaller<CharacterSpawnZone>
	{
		private PlayerCharacterController _character;

#pragma warning disable 649
		[SerializeField] private Transform _spawnPosition;

		[Inject] private readonly AvatarType _avatarType;
#pragma warning restore 649

		public override void InstallBindings()
		{
			Container.Bind<CharacterSpawnZone>().FromInstance(this).AsCached();
		}

		public override void Start()
		{
			switch (_avatarType)
			{
				case AvatarType.Anna:
					_character = Container.InstantiatePrefabResourceForComponent<PlayerCharacterController>(
						"Characters/Anna");
					break;
				case AvatarType.Antonio:
					_character = Container.InstantiatePrefabResourceForComponent<PlayerCharacterController>(
						"Characters/Antonio");
					break;
				default:
					throw new NotSupportedException();
			}

			_character.transform.position = new Vector3(_spawnPosition.position.x, 0, 0);
		}
	}
}