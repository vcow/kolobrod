using Base.ScreenLocker;
using Zenject;

public class GameInstaller : MonoInstaller<GameInstaller>
{
	public override void InstallBindings()
	{
		SignalBusInstaller.Install(Container);
		Container.Bind<IScreenLockerManager>().FromComponentInNewPrefabResource("ScreenLockerManager").AsSingle();
	}
}