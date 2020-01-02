using UnityEngine;

namespace GameScene.Signals
{
	public class MoveSignal
	{
		public Vector2 Direction { get; }

		public MoveSignal(Vector2 direction)
		{
			Direction = direction;
		}
	}
}