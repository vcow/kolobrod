using Base.ScreenLocker;

namespace ScreenLocker
{
	public class GameScreenLocker : CommonScreenLockerBase
	{
		public override LockerType LockerType => LockerType.GameLoader;

		public override void Activate(bool immediately = false)
		{
			base.Activate(true);
		}
	}
}