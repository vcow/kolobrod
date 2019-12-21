using Base.ScreenLocker;

namespace ScreenLocker
{
	public class WaitScreenLocker : CommonScreenLockerBase
	{
		public override LockerType LockerType => LockerType.BusyWait;
	}
}