#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;

namespace BzKovSoft.Tools.RagdollGenerator
{
	/// <summary>
	/// Joint controller. Draws the controls for 'CharacterJoint' components in scene view.
	/// </summary>
	public static class JointController
	{
		/// <summary>
		/// Draws the controllers. Need to be invoked from 'OnSceneGUI()' method.
		/// </summary>
		/// <param name="joint">Joint.</param>
		public static void DrawControllers(CharacterJoint joint)
		{
			Color backupColor = Handles.color;
			Vector3 position = joint.transform.position + joint.anchor;
			float size = HandleUtility.GetHandleSize(position);										// red
			Vector3 swingAxisDir = joint.transform.TransformDirection(joint.swingAxis).normalized;	// green
			Vector3 axisDir = joint.transform.TransformDirection(joint.axis).normalized;			// yellow
			Vector3 direction = GetDirection(joint, swingAxisDir, axisDir);

			DrawTwist(joint, position, direction, axisDir, size);
			DrawSwing1(joint, position, direction, axisDir, swingAxisDir, size);
			DrawSwing2(joint, position, direction, swingAxisDir, size);

			var currRot = Quaternion.LookRotation(swingAxisDir, axisDir);
			Quaternion newRotation = Handles.RotationHandle(currRot, position);

			joint.swingAxis = joint.transform.InverseTransformDirection(newRotation * Vector3.forward);	// green
			joint.axis = joint.transform.InverseTransformDirection(newRotation * Vector3.up);	// yellow



			Handles.color = backupColor;
		}

		static void DrawTwist(CharacterJoint joint, Vector3 position, Vector3 direction, Vector3 axisDir, float size)
		{
			Handles.color = new Color(0.7f, 0.7f, 0.0f, 1f);
			Handles.ArrowHandleCap(0, position, Quaternion.LookRotation(axisDir), size * 1.1f, EventType.Repaint);
			
			Handles.color = new Color(0.7f, 0.7f, 0.0f, 1f);
			Vector3 twistNoraml = axisDir;
			var hightLimit = joint.highTwistLimit;
			var lowLimit = joint.lowTwistLimit;
			hightLimit.limit = -ProcessLimit(position, twistNoraml, direction, size, -hightLimit.limit);
			lowLimit.limit = -ProcessLimit(position, twistNoraml, direction, size, -lowLimit.limit);
			joint.highTwistLimit = hightLimit;
			joint.lowTwistLimit = lowLimit;
		}

		static void DrawSwing1(CharacterJoint joint, Vector3 position, Vector3 direction, Vector3 axisDir, Vector3 swingAxisDir, float size)
		{
			Handles.color = new Color(0.0f, 0.7f, 0.0f, 1f);
			Handles.ArrowHandleCap(0, position, Quaternion.LookRotation(swingAxisDir), size * 1.1f, EventType.Repaint);

			Handles.color = new Color(0.0f, 0.7f, 0.0f, 1f);
			Vector3 swing1Noraml = Vector3.Cross(axisDir, direction);
			var swing1Limit = joint.swing1Limit;
			swing1Limit.limit = ProcessLimit(position, swing1Noraml, direction, size, swing1Limit.limit);
			swing1Limit.limit = -ProcessLimit(position, swing1Noraml, direction, size, -swing1Limit.limit);
			if (swing1Limit.limit < 0f)
				swing1Limit.limit = 0f;
			joint.swing1Limit = swing1Limit;
		}

		static void DrawSwing2(CharacterJoint joint, Vector3 position, Vector3 direction, Vector3 swingAxisDir, float size)
		{
			Handles.color = new Color(1f, 0f, 0f, 1f);
			Handles.ArrowHandleCap(0, position, Quaternion.LookRotation(direction), size * 2f, EventType.Repaint);
			
			Handles.color = new Color(0.0f, 0.0f, 0.7f, 1f);
			Vector3 swing2Noraml = direction;
			var swing2Limit = joint.swing2Limit;
			swing2Limit.limit = ProcessLimit(position, swing2Noraml, swingAxisDir, size, swing2Limit.limit);
			swing2Limit.limit = -ProcessLimit(position, swing2Noraml, swingAxisDir, size, -swing2Limit.limit);
			if (swing2Limit.limit < 0f)
				swing2Limit.limit = 0f;
			joint.swing2Limit = swing2Limit;
		}

		static Vector3 GetDirection(CharacterJoint joint, Vector3 swingAxisDir, Vector3 axisDir)
		{
			Vector3 direction = Vector3.Cross(swingAxisDir, axisDir);
			Vector3 direction2 = GetDirection(joint);

			//Handles.color = new Color(1f, 0f, 0f, 1f);
			//Handles.DrawLine(joint.transform.position, joint.transform.position + direction * 100);
			//Handles.color = new Color(0f, 1f, 0f, 1f);
			//Handles.DrawLine(joint.transform.position, joint.transform.position + direction2 * 100);
			float r = Vector3.Dot(direction, direction2);

			return direction *Mathf.Sign(r);
		}

		static Vector3 GetDirection(CharacterJoint joint)
		{
			var transform = joint.transform;
			if (transform.childCount == 0)
			{
				// in now children. Return direction related to parent
				return (joint.transform.position - joint.connectedBody.transform.position).normalized;
			}
			Vector3 direction = Vector3.zero;

			for (int ch = 0; ch < transform.childCount; ++ch)
			{
				// take to account colliders that attached to children
				var colliders = transform.GetChild(ch).GetComponents<Collider>();
				for (int i = 0; i < colliders.Length; ++i)
				{
					Collider collider = colliders[i];
					CapsuleCollider cCollider = collider as CapsuleCollider;
					BoxCollider bCollider = collider as BoxCollider;
					SphereCollider sCollider = collider as SphereCollider;
					if (cCollider != null)
						direction += collider.transform.TransformDirection(cCollider.center);
					if (bCollider != null)
						direction += collider.transform.TransformDirection(bCollider.center);
					if (sCollider != null)
						direction += collider.transform.TransformDirection(sCollider.center);
				}
			}

			// if colliders was found, return average direction to colliders.
			if (direction != Vector3.zero)
				return direction.normalized;

			// otherwise, take direction to first child
			for (int i = 0; i < transform.childCount; ++i)
				direction += transform.GetChild(i).localPosition;
			return transform.TransformDirection(direction).normalized;
		}

		/// <summary>
		/// Draws arc with controls
		/// </summary>
		/// <returns>New limit.</returns>
		/// <param name="position">Position of center of arc</param>
		/// <param name="planeNormal">Plane normal in which arc are to be drawn</param>
		/// <param name="startDir">Start direction of arc</param>
		/// <param name="size">Radius of arc</param>
		/// <param name="limit">Current limit</param>
		static float ProcessLimit(Vector3 position, Vector3 planeNormal, Vector3 startDir, float size, float limit)
		{
			Vector3 cross = Vector3.Cross(planeNormal, startDir);
			startDir = Vector3.Cross(cross, planeNormal);

			Vector3 controllerDir = (Quaternion.AngleAxis(limit, planeNormal) * startDir);
			Vector3 controllerPos = position + (controllerDir * size * 1.2f);

			Color backupColor = Handles.color;
			Color newColor = backupColor * 2;
			newColor.a = 1f;
			Handles.color = newColor;
			Handles.DrawLine(position, controllerPos);

			newColor.a = 0.2f;
			Handles.color = newColor;

			Handles.DrawSolidArc(
				position,
				planeNormal,
				startDir,
				limit, size);

			newColor.a = 1f;
			Handles.color = newColor;

			var fmh_174_65_638313502167844880 = Quaternion.identity; bool positionChanged = Handles.FreeMoveHandle(controllerPos, size * 0.1f, Vector3.zero, Handles.SphereHandleCap) != controllerPos;
			if (positionChanged)
			{
				var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
				float rayDistance;

				Plane plane = new Plane(planeNormal, position);
				if (plane.Raycast(ray, out rayDistance))
					controllerPos = ray.GetPoint(rayDistance);
				controllerPos = position + (controllerPos - position).normalized * size * 1.2f;

				// Get the angle in degrees between 0 and 180
				limit = Vector3.Angle(startDir, controllerPos - position);
				// Determine if the degree value should be negative.  Here, a positive value
				// from the dot product means that our vector is on the right of the reference vector   
				// whereas a negative value means we're on the left.
				float sign = Mathf.Sign(Vector3.Dot(cross, controllerPos - position));
				limit *= sign;

				limit = Mathf.Round(limit / 5f) * 5f;	// i need this to snap rotation
			}

			Handles.color = backupColor;
			return limit;
		}
	}
}
#endif