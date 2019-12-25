using Base.ScreenLocker;
using Common;
using UnityEngine.SceneManagement;
using Zenject;

namespace StartGameScene
{
	public class StartGameSceneInstaller : MonoInstaller<StartGameSceneInstaller>
	{
		private AvatarType _avatarType = AvatarType.Anna;

#pragma warning disable 649
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

		public void OnSelectAnna(bool select)
		{
			_avatarType = AvatarType.Anna;
		}

		public void OnSelectAntonio(bool select)
		{
			_avatarType = AvatarType.Antonio;
		}

		public void OnPlay()
		{
			_screenLockerManager.Lock(LockerType.SceneLoader,
				() =>
				{
					_sceneLoader.LoadSceneAsync("GameScene", LoadSceneMode.Single,
						container => container.Bind<AvatarType>().FromInstance(_avatarType).AsCached());
				});
		}
	}
}