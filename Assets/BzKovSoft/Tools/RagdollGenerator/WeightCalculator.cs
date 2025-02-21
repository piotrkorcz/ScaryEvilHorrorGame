#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BzKovSoft.Tools.RagdollGenerator
{
	/// <summary>
	/// Calculates weight of each character's bone
	/// </summary>
	struct WeightCalculator
	{
		public readonly float Pelvis;
		public readonly float Hip;
		public readonly float Knee;
		public readonly float Arm;
		public readonly float Elbow;
		public readonly float Chest;
		public readonly float Head;

		public WeightCalculator(float totalWeight)
		{
			Pelvis = totalWeight * 0.20f;
			Chest = totalWeight * 0.20f;
			Head = totalWeight * 0.05f;

			Hip = totalWeight * 0.20f / 2f;
			Knee = totalWeight * 0.20f / 2f;

			Arm = totalWeight * 0.08f / 2f;
			Elbow = totalWeight * 0.07f / 2f;

			float checkSum =
				Pelvis +
				Hip * 2f +
				Knee * 2f +
				Arm * 2f +
				Elbow * 2f +
				Chest +
				Head;
			if (Mathf.Abs(totalWeight - checkSum) > Mathf.Epsilon)
				Debug.LogError("totalWeight != checkSum (" + totalWeight.ToString() + ", " + checkSum.ToString() + ")");
		}
	}
}
#endif