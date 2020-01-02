using Base.ScreenLocker;
using GameScene.Signals;
using Zenject;

namespace GameScene
{
	public class GameSceneInstaller : MonoInstaller<GameSceneInstaller>
	{
#pragma warning disable 649
		[Inject] private readonly IScreenLockerManager _screenLockerManager;
#pragma warning restore 649

		public override void InstallBindings()
		{
			Container.DeclareSignal<MoveSignal>();
		}

		public override void Start()
		{
			_screenLockerManager.Unlock(null);
		}
	}
}