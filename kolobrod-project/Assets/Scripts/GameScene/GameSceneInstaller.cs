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
			_screenLockerManager.Unlock(null);
		}
	}
}