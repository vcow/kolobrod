using System;
using Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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

		public void OnLeftPointerDown(BaseEventData data)
		{
			if (!_character) return;
			_character.Walk(Vector2.left);
		}

		public void OnLeftPointerUp(BaseEventData data)
		{
			if (!_character) return;
			_character.Walk(Vector2.zero);
		}

		public void OnRightPointerDown(BaseEventData data)
		{
			if (!_character) return;
			_character.Walk(Vector2.right);
		}

		public void OnRightPointerUp(BaseEventData data)
		{
			if (!_character) return;
			_character.Walk(Vector2.zero);
		}

		public void OnMove(InputAction.CallbackContext context)
		{
			if (!_character) return;
			var value = context.phase == InputActionPhase.Canceled
				? Vector2.zero
				: context.ReadValue<Vector2>();
			_character.Walk(value);
		}

		public void OnPoint(InputAction.CallbackContext context)
		{
			if (!_character || EventSystem.current.IsPointerOverGameObject() ||
			    context.phase != InputActionPhase.Performed) return;
			var value = context.ReadValue<Vector2>();
			_character.Aim(value);
		}

		public void OnClick(InputAction.CallbackContext context)
		{
			if (!_character || EventSystem.current.IsPointerOverGameObject() ||
			    context.phase != InputActionPhase.Performed) return;
			_character.Fire();
		}
	}
}