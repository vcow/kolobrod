using System;
using AI;
using Base.ScreenLocker;
using Common;
using Zenject;

namespace GameScene
{
	public class GameSceneInstaller : MonoInstaller<GameSceneInstaller>
	{
#pragma warning disable 649
		[Inject] private readonly AvatarType _avatarType;
		[Inject] private readonly IScreenLockerManager _screenLockerManager;
		[Inject] private readonly ZenjectSceneLoader _sceneLoader;
#pragma warning restore 649

		public override void InstallBindings()
		{
		}

		public override void Start()
		{
			switch (_avatarType)
			{
				case AvatarType.Anna:
					Container.InstantiatePrefabResourceForComponent<PlayerCharacterController>("Characters/Anna");
					break;
				case AvatarType.Antonio:
					Container.InstantiatePrefabResourceForComponent<PlayerCharacterController>("Characters/Antonio");
					break;
				default:
					throw new NotSupportedException();
			}

			_screenLockerManager.Unlock(null);
		}
	}
}