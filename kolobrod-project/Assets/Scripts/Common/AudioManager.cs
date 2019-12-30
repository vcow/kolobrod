using Base.AudioManager;

namespace Common
{
	public class AudioManager : AudioManagerBase
	{
		protected override string AudioPersistKey => @"kolobrod_audio_key";
		protected override int SoundsLimit => 8;
	}
}