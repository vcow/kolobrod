#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

namespace Input
{
#if UNITY_EDITOR
	[InitializeOnLoad]
#endif
	public class PointerInputComposite : InputBindingComposite<Vector2>
	{
		[InputControl(layout = "Button")] public int Button;
		[InputControl(layout = "Vector2")] public int Position;

		public override Vector2 ReadValue(ref InputBindingCompositeContext context)
		{
			return context.ReadValue<Vector2, Vector2MagnitudeComparer>(Position);
		}

		public override float EvaluateMagnitude(ref InputBindingCompositeContext context)
		{
			return context.ReadValue<float>(Button);
		}

#if UNITY_EDITOR
		static PointerInputComposite()
		{
			Register();
		}
#endif

		[RuntimeInitializeOnLoadMethod]
		private static void Register()
		{
			InputSystem.RegisterBindingComposite<PointerInputComposite>();
		}
	}
}