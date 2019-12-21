using Base.ScreenLocker;

namespace ScreenLocker
{
	public class SceneScreenLocker : CommonScreenLockerBase
	{
		public override LockerType LockerType => LockerType.SceneLoader;
	}
}