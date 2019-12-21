using Base.ScreenLocker;
using Zenject;

namespace LoadGameScene
{
	public class LoadGameSceneInstaller : MonoInstaller<LoadGameSceneInstaller>
	{
#pragma warning disable 649
		[Inject] private readonly IScreenLockerManager _screenLockerManager;
#pragma warning restore 649

		public override void InstallBindings()
		{
		}

		public override void Start()
		{
			_screenLockerManager.Lock(LockerType.GameLoader, InitGame);
		}

		private void InitGame()
		{
			
		}
	}
}