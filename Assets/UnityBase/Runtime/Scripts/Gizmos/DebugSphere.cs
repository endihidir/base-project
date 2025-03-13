using UnityEngine;

namespace UnityBase.Runtime.Utility
{
	[ExecuteInEditMode]
	public class DebugSphere : MonoBehaviour
	{
		public float radius = 0.5f;

		public Color color = Color.red;

		public bool onlyWhenSelected = false;

		private void OnDrawGizmos()
		{
			if (!onlyWhenSelected)
			{
				Gizmos.color = color;
				Gizmos.DrawSphere(transform.position, radius);
			}
		}

		private void OnDrawGizmosSelected()
		{
			if (onlyWhenSelected)
			{
				Gizmos.color = color;
				Gizmos.DrawSphere(transform.position, radius);
			}
		}
	}
}
