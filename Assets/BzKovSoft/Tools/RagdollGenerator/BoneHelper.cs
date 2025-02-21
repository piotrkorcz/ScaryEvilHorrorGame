#if UNITY_EDITOR
using System.Globalization;
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BzKovSoft.Tools.RagdollGenerator
{
	/// <summary>
	/// Bone fixer. Main class that draws panel and do a lot of logic
	/// </summary>
	[CustomEditor(typeof(Animator))]
	public sealed class BoneFixer : Editor
	{
		/// <summary>
		/// If you rotate collider, the collider rotates via an additional
		/// node that have the same name + this text.
		/// </summary>
		const string ColliderNodeSufix = "_ColliderRotator";

		// current selected collider
		int _curPointIndex = -1;

		// variables that initialized in OnEnable()
		GameObject _go;
		Animator _animator;
		Transform _leftKnee;
		Transform _rightKnee;
		Transform _pelvis;
		Dictionary<string, Transform> _symmetricBones;
		
		// variables that initialized in FindColliders() function.
		// you can not do it in OnEnable() function, because,
		// when you rotate colliders, colliders might be
		// reattached to other objects
		Collider[] _colliders;
		Transform[] _transforms;

		int _ragdollTotalWeight = 60;			// weight of character (by default 60)
		CollisionDetectionMode _cdtCurrentItem; // selected collision detection mode

		SelectedMode _selectedMode = SelectedMode.NoAction;
		SelectedMode _lastSelectedMode;	// to detect, if mode changed from last frame
		readonly string[] _mode = {
			"No action",
			"Ragdoll",
			"Rotate Colliders",
			"Move Colliders",
			"Scale Colliders",
			"Joints",
		};
		enum SelectedMode
		{
			NoAction,
			Ragdoll,
			RotateColliders,
			MoveColliders,
			ScaleColliders,
			Joints,
			Hidden,
		}

		void OnEnable()
		{
			if (EditorUtility.IsPersistent(target))
				return; // if it is selected as asset, skip it

			_animator = (Animator)target;
			_go = _animator.gameObject;

			_symmetricBones = FindSymmetricBones(_animator);
			_leftKnee = _animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
			_rightKnee = _animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
			_pelvis = _animator.GetBoneTransform(HumanBodyBones.Hips);
		}

		void OnDisable()
		{
			UnityEditor.Tools.hidden = false;
		}
		/// <summary>
		/// Find symmetric bones. (e.g. for left arm, it finds right arm and for right leg it finds left leg)
		/// </summary>
		static Dictionary<string, Transform> FindSymmetricBones(Animator animator)
		{
			var symBones = new Dictionary<string, Transform>();

			// feet
			symBones.Add(animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).name, animator.GetBoneTransform(HumanBodyBones.RightLowerLeg));
			symBones.Add(animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).name, animator.GetBoneTransform(HumanBodyBones.RightUpperLeg));

			symBones.Add(animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).name, animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg));
			symBones.Add(animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).name, animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg));

			// hands
			symBones.Add(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).name, animator.GetBoneTransform(HumanBodyBones.RightLowerArm));
			symBones.Add(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).name, animator.GetBoneTransform(HumanBodyBones.RightUpperArm));

			symBones.Add(animator.GetBoneTransform(HumanBodyBones.RightLowerArm).name, animator.GetBoneTransform(HumanBodyBones.LeftLowerArm));
			symBones.Add(animator.GetBoneTransform(HumanBodyBones.RightUpperArm).name, animator.GetBoneTransform(HumanBodyBones.LeftUpperArm));

			return symBones;
		}
		/// <summary>
		/// Find all "CapsuleCollider", "BoxCollider" and "SphereCollider" colliders
		/// </summary>
		void FindColliders()
		{
			var cColliders = _go.GetComponentsInChildren<CapsuleCollider>();
			var bColliders = _go.GetComponentsInChildren<BoxCollider>();
			var sColliders = _go.GetComponentsInChildren<SphereCollider>();
			_colliders = new Collider[cColliders.Length + bColliders.Length + sColliders.Length];
			cColliders.CopyTo(_colliders, 0);
			bColliders.CopyTo(_colliders, cColliders.Length);
			sColliders.CopyTo(_colliders, cColliders.Length + bColliders.Length);

			_transforms = new Transform[_colliders.Length];
			for (int i = 0; i < _colliders.Length; ++i)
			{
				Transform transform = _colliders[i].transform;
				if (transform.name.EndsWith(ColliderNodeSufix, false, CultureInfo.InvariantCulture))
					transform = transform.parent;
				_transforms[i] = transform;
			}
		}

		void OnSceneGUI()
		{
			CheckStateBeforeDraw();

			UnityEditor.Tools.hidden = _selectedMode != SelectedMode.Hidden;

			if (_selectedMode == SelectedMode.Hidden)
				return;

			DrawPlayerDirection();

			if (_selectedMode == SelectedMode.RotateColliders |
				_selectedMode == SelectedMode.MoveColliders |
				_selectedMode == SelectedMode.ScaleColliders |
				_selectedMode == SelectedMode.Joints)
				DrawControls();

			DrawPanel();
		}
		/// <summary>
		/// Method intended to check before Draw GUI
		/// </summary>
		void CheckStateBeforeDraw()
		{
			// if selected item was changed, research colliders
			if (_selectedMode > 0 & _lastSelectedMode != _selectedMode)
				FindColliders();

			_lastSelectedMode = _selectedMode;
		}
		/// <summary>
		/// Draw arrow on the screen, forwarded to front of the character
		/// </summary>
		void DrawPlayerDirection()
		{
			float size = HandleUtility.GetHandleSize(_go.transform.position);
			var playerDirection = GetPlayerDirection();
			Color backupColor = Handles.color;
			Handles.color = Color.yellow;
			Handles.ArrowHandleCap(1, _go.transform.position, Quaternion.LookRotation(playerDirection, Vector3.up), size, EventType.Repaint);
			Handles.color = backupColor;
		}

		/// <summary>
		/// Draws controls of selected mode.
		/// </summary>
		void DrawControls()
		{
			for (int i = 0; i < _transforms.Length; i++)
			{
				Transform transform = _transforms[i];
				Vector3 pos = GetRotatorPosition(transform);
				float size = HandleUtility.GetHandleSize(pos);

				if (Handles.Button(pos, Quaternion.identity, size / 6f, size / 6f, Handles.SphereHandleCap))
					_curPointIndex = i;

				if (_curPointIndex != i)
					continue;

				// if current point controll was selected
				// draw other controls over it

				var rotatorRotation = GetRotatorRotarion(transform);
				switch (_selectedMode)
				{
					case SelectedMode.RotateColliders:
						ProcessRotation(rotatorRotation, transform, pos);
						break;
					case SelectedMode.MoveColliders:
						ProcessColliderResize(rotatorRotation, transform, pos);
						break;
					case SelectedMode.ScaleColliders:
						ProcessColliderMove(rotatorRotation, transform, pos);
						break;
					case SelectedMode.Joints:
						ProcessJoint(transform);
						break;
				}
			}
		}
		/// <summary>
		/// Rotate node's colider though controls
		/// </summary>
		static void ProcessRotation(Quaternion rotatorRotation, Transform transform, Vector3 pos)
		{
			Quaternion newRotation = Handles.RotationHandle(rotatorRotation, pos);
			Quaternion rotateBy = Quaternion.Inverse(rotatorRotation) * newRotation;

			if (rotateBy != Quaternion.identity)
			{
				transform = GetRotatorTransform(transform);
				RotateCollider(transform, rotateBy);
			}
		}
		/// <summary>
		/// Resize collider though controls
		/// </summary>
		/// <param name="transform">The node the collider is attached to</param>
		static void ProcessColliderResize(Quaternion rotatorRotation, Transform transform, Vector3 pos)
		{
			Vector3 newPosition = Handles.PositionHandle(pos, rotatorRotation);
			Vector3 translateBy = newPosition - pos;

			if (translateBy != Vector3.zero)
				MoveCollider(transform, translateBy);
		}
		/// <summary>
		/// Move collider though controls
		/// </summary>
		void ProcessColliderMove(Quaternion rotatorRotation, Transform transform, Vector3 pos)
		{
			float size = HandleUtility.GetHandleSize(pos);
			var collider = GetCollider(transform);

			// process each collider type in its own way
			CapsuleCollider cCollider = collider as CapsuleCollider;
			BoxCollider bCollider = collider as BoxCollider;
			SphereCollider sCollider = collider as SphereCollider;

			if (cCollider != null)
			{
				// for capsule collider draw circle and two dot controllers
				Vector3 direction;
				switch (cCollider.direction)
				{
					case 0:
						direction = Vector3.right;
						break;
					case 1:
						direction = Vector3.up;
						break;
					case 2:
						direction = Vector3.forward;
						break;
					default:
						throw new InvalidOperationException();
				}

				var t = Quaternion.LookRotation(cCollider.transform.TransformDirection(direction));

				// method "Handles.ScaleValueHandle" multiplies size on 0.15f
				// so to send exact size to "Handles.CircleCap",
				// I needed to multiply size on 1f/0.15f
				// Then to get a size a little bigger (to 130%) than
				// collider (for nice looking purpose), I multiply size by 1.3f
				const float magicNumber = 1f / 0.15f * 1.3f;

				// draw radius controll
				cCollider.radius = Handles.ScaleValueHandle(cCollider.radius, pos, t, cCollider.radius * magicNumber, Handles.CircleHandleCap, 0);
				Vector3 scaleHeightShift = cCollider.transform.TransformDirection(direction * cCollider.height / 2);

				// draw height controlls
				Vector3 scaleHeightPos1 = pos + scaleHeightShift;
				Vector3 scaleHeightPos2 = pos - scaleHeightShift;
				cCollider.height = Handles.ScaleValueHandle(cCollider.height, scaleHeightPos1, t, size * 0.5f, Handles.DotHandleCap, 0);
				cCollider.height = Handles.ScaleValueHandle(cCollider.height, scaleHeightPos2, t, size * 0.5f, Handles.DotHandleCap, 0);

				if (cCollider.height < 0.01f)
					cCollider.height = 0.01f;

				// resize symmetric colliders too
				Transform symBone;
				if (_symmetricBones.TryGetValue(transform.name, out symBone))
				{
					var symCapsule = GetCollider(symBone) as CapsuleCollider;
					if (symCapsule == null)
						return;

					symCapsule.radius = cCollider.radius;
					symCapsule.height = cCollider.height;
				}
			}
			else if (bCollider != null)
			{
				// resize Box collider
				bCollider.size = Handles.ScaleHandle(bCollider.size, pos, rotatorRotation, size);
			}
			else if (sCollider != null)
			{
				// resize Sphere collider
				sCollider.radius = Handles.RadiusHandle(rotatorRotation, pos, sCollider.radius, true);
			}
			else
				throw new InvalidOperationException("Unsupported Collider type: " + collider.GetType().FullName);
		}

		static void ProcessJoint(Transform transform)
		{
			CharacterJoint joint = transform.GetComponent<CharacterJoint>();
			if (joint == null)
				return;

			JointController.DrawControllers(joint);
		}
		/// <summary>
		/// Determines and return character's face direction
		/// </summary>
		Vector3 GetPlayerDirection()
		{
			Vector3 leftKnee = _leftKnee.transform.position - _pelvis.transform.position;
			Vector3 rightKnee = _rightKnee.transform.position - _pelvis.transform.position;

			return Vector3.Cross(leftKnee, rightKnee).normalized;
		}
		/// <summary>
		/// Draw application form panel
		/// </summary>
		void DrawPanel()
		{
			// set the form more transparent if "NoAction" is selected
			Color color = GUI.color;
			color.a = .7f;
			if (_selectedMode == SelectedMode.NoAction)
				color.a = .4f;
			GUI.color = color;
			Vector2 size;
			// set size of form for each selected mode
			switch (_selectedMode)
			{
				case SelectedMode.NoAction:
					size = new Vector2(150, 60);
					break;
				case SelectedMode.RotateColliders:
					size = new Vector2(150, 150);
					break;
				case SelectedMode.MoveColliders:
					size = new Vector2(150, 150);
					break;
				case SelectedMode.ScaleColliders:
					size = new Vector2(150, 150);
					break;
				case SelectedMode.Joints:
					size = new Vector2(150, 150);
					break;
				case SelectedMode.Ragdoll:
					size = new Vector2(150, 290);
					break;
				default:
					size = new Vector2(200, 200);
					break;
			}

			// draw panel
			Handles.BeginGUI();
			{
				// draw "Hide" botton
				Vector2 hideButtonSize = new Vector2(50f, 20f);
				if (GUI.Button(new Rect(10f + (size.x - hideButtonSize.x) / 2f, 50f, hideButtonSize.x, hideButtonSize.y), "Hide"))
					_selectedMode = SelectedMode.Hidden;

				GUI.BeginGroup(new Rect(10f, 50f + hideButtonSize.y, size.x, size.y));
				{
					// draw text
					GUI.Box(new Rect(new Vector2(), size), "Regdoll Generator");
					GUILayout.BeginArea(new Rect(10, 20, size.x - 20, size.y - 10));
					{
						// draw list of modes to select
						_selectedMode = (SelectedMode)GUILayout.SelectionGrid((int)(_selectedMode), _mode, 1, EditorStyles.radioButton);

						// for Ragdoll mode draw additional controlls
						if (_selectedMode == SelectedMode.Ragdoll)
						{
							DrawRagdollPanel();
						}

						GUILayout.EndArea();
					}
					GUI.EndGroup();
				}
				Handles.EndGUI();
			}
		}

		void DrawRagdollPanel()
		{
			GUILayout.BeginVertical("box");
			GUILayout.Label("Ragdoll:");
			if (GUILayout.Button("Create"))
				CreateRagdoll();
			if (GUILayout.Button("Remove"))
				RemoveRagdoll();

			EditorGUILayout.LabelField("Collision detection:");
			_cdtCurrentItem = (CollisionDetectionMode)EditorGUILayout.EnumPopup(_cdtCurrentItem);

			EditorGUILayout.LabelField("Total Weight:");
			_ragdollTotalWeight = EditorGUILayout.IntField(_ragdollTotalWeight);
			GUILayout.EndVertical();
		}
		/// <summary>
		/// Remove all colliders, joints, and rigids from "_go" object
		/// </summary>
		void RemoveRagdoll()
		{
			Ragdoller ragdoller = new Ragdoller(_go.transform, GetPlayerDirection());
			ragdoller.ClearRagdoll();
		}
		/// <summary>
		/// Create Ragdoll components on _go object
		/// </summary>
		void CreateRagdoll()
		{
			Ragdoller ragdoller = new Ragdoller(_go.transform, GetPlayerDirection());
			ragdoller.ApplyRagdoll(_ragdollTotalWeight, true, 0f, _cdtCurrentItem);
		}
		/// <summary>
		/// Get position of collider center
		/// </summary>
		static Vector3 GetRotatorPosition(Transform boneTransform)
		{
			Collider collider = GetCollider(boneTransform);
			CapsuleCollider cCollider = collider as CapsuleCollider;
			BoxCollider bCollider = collider as BoxCollider;
			SphereCollider sCollider = collider as SphereCollider;

			Vector3 colliderCenter;
			if (cCollider != null) colliderCenter = cCollider.center;
			else if (bCollider != null) colliderCenter = bCollider.center;
			else if (sCollider != null) colliderCenter = sCollider.center;
			else
				throw new InvalidOperationException("Unsupported Collider type: " + collider.GetType().FullName);

			var colliderTransform = collider.transform;
			return colliderTransform.TransformPoint(colliderCenter);
		}
		/// <summary>
		/// Get rotation of collider object
		/// </summary>
		static Quaternion GetRotatorRotarion(Transform boneTransform)
		{
			Collider collider = GetCollider(boneTransform);
			return collider.transform.rotation;
		}
		/// <summary>
		/// Move collider's center on "translateBy" in world space
		/// </summary>
		static void MoveCollider(Transform transform, Vector3 translateBy)
		{
			var collider = GetCollider(transform);

			CapsuleCollider cCollider = collider as CapsuleCollider;
			BoxCollider bCollider = collider as BoxCollider;
			SphereCollider sCollider = collider as SphereCollider;

			translateBy = collider.transform.InverseTransformDirection(translateBy);

			if (cCollider != null) cCollider.center += translateBy;
			else if (bCollider != null) bCollider.center += translateBy;
			else if (sCollider != null) sCollider.center += translateBy;
			else
				throw new InvalidOperationException("Unsupported Collider type: " + collider.GetType().FullName);
		}
		/// <summary>
		/// Get colliders' center in world space
		/// </summary>
		static Vector3 GetColliderPosition(Transform transform)
		{
			Collider collider = GetCollider(transform);
			CapsuleCollider cCollider = collider as CapsuleCollider;
			BoxCollider bCollider = collider as BoxCollider;
			SphereCollider sCollider = collider as SphereCollider;

			Vector3 center;
			if (cCollider != null) center = cCollider.center;
			else if (bCollider != null) center = bCollider.center;
			else if (sCollider != null) center = sCollider.center;
			else
				throw new InvalidOperationException("Unsupported Collider type: " + collider.GetType().FullName);

			return collider.transform.TransformPoint(center);
		}
		/// <summary>
		/// Set colliders' center in world space
		/// </summary>
		static void SetColliderPosition(Transform transform, Vector3 position)
		{
			Collider collider = GetCollider(transform);
			CapsuleCollider cCollider = collider as CapsuleCollider;
			BoxCollider bCollider = collider as BoxCollider;
			SphereCollider sCollider = collider as SphereCollider;

			Vector3 center = collider.transform.InverseTransformPoint(position);
			if (cCollider != null) cCollider.center = center;
			else if (bCollider != null) bCollider.center = center;
			else if (sCollider != null) sCollider.center = center;
			else
				throw new InvalidOperationException("Unsupported Collider type: " + collider.GetType().FullName);
		}
		/// <summary>
		/// Rotate collider without rotating "transform" object.
		/// </summary>
		static void RotateCollider(Transform transform, Quaternion rotateBy)
		{
			Vector3 prevPosition = GetColliderPosition(transform);

			transform.rotation *= rotateBy;

			SetColliderPosition(transform, prevPosition);
		}
		/// <summary>
		/// Gets object a collider attached to.
		/// Collider must have separate GameObject to allow a collider to rotate via it.
		/// So if that GameObject do not exists, creates it.
		/// </summary>
		static Transform GetRotatorTransform(Transform boneTransform)
		{
			var colliderRotatorName = boneTransform.name + ColliderNodeSufix;

			// find rotator node
			var rotatorTransform = boneTransform.Find(colliderRotatorName);
			if (rotatorTransform != null)
				return rotatorTransform;

			// if rotator node was not found, create it
			Collider collider = boneTransform.GetComponent<Collider>();
			if (collider == null)
				throw new ArgumentException("Bone '" + boneTransform.name + "' does not have collider attached to it or ColliderRotatorNode");

			GameObject colliderRotator = new GameObject(colliderRotatorName);
			rotatorTransform = colliderRotator.transform;

			ReattachCollider(boneTransform.gameObject, colliderRotator);
			rotatorTransform.parent = boneTransform;
			rotatorTransform.localPosition = Vector3.zero;
			rotatorTransform.localRotation = Quaternion.identity;
			rotatorTransform.localScale = Vector3.one;

			return colliderRotator.transform;
		}
		/// <summary>
		/// Duplicate collidr from "from" to "to" and delete it from "from"
		/// </summary>
		static void ReattachCollider(GameObject from, GameObject to)
		{
			var oldCollider = from.GetComponent<Collider>();
			CapsuleCollider cCollider = oldCollider as CapsuleCollider;
			BoxCollider bCollider = oldCollider as BoxCollider;
			Collider newCollider;
			if (cCollider != null)
			{
				CapsuleCollider newCapsuleCollider = to.AddComponent<CapsuleCollider>();
				newCollider = newCapsuleCollider;
				newCapsuleCollider.direction = cCollider.direction;
				newCapsuleCollider.radius = cCollider.radius;
				newCapsuleCollider.height = cCollider.height;
				newCapsuleCollider.center = cCollider.center;
			}
			else if (bCollider != null)
			{
				BoxCollider newBoxCollider = to.AddComponent<BoxCollider>();
				newCollider = newBoxCollider;
				newBoxCollider.size = bCollider.size;
				newBoxCollider.center = bCollider.center;
			}
			else
				throw new NotSupportedException("Collider type '" + oldCollider + "' does not supported to reattach.");

			newCollider.isTrigger = oldCollider.isTrigger;
			DestroyImmediate(oldCollider);
		}
		/// <summary>
		/// Get object a collider attached to. 
		/// </summary>
		static Collider GetCollider(Transform transform)
		{
			Collider collider = transform.GetComponent<Collider>();
			if (collider == null)
			{
				var rotatorName = transform.name + ColliderNodeSufix;
				var rotatorTransform = transform.Find(rotatorName);
				if (rotatorTransform != null)
					collider = rotatorTransform.GetComponent<Collider>();
			}

			if (collider == null)
				throw new ArgumentException("transform '" + transform.name + "' does not contain collider");

			return collider;
		}
	}
}
#endif
