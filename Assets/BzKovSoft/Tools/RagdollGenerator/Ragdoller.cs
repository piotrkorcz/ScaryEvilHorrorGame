#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BzKovSoft.Tools.RagdollGenerator
{
	/// <summary>
	/// Class responsible for regdoll and unregdoll character
	/// </summary>
	sealed class Ragdoller
	{
		readonly bool _readyToGenerate;
		readonly Vector3 _playerDirection;
		readonly Transform _rootNode;

		readonly RagdollPartBox _pelvis;
		readonly RagdollPartCapsule _leftHip;
		readonly RagdollPartCapsule _leftKnee;
		readonly RagdollPartCapsule _rightHip;
		readonly RagdollPartCapsule _rightKnee;
		readonly RagdollPartCapsule _leftArm;
		readonly RagdollPartCapsule _leftElbow;
		readonly RagdollPartCapsule _rightArm;
		readonly RagdollPartCapsule _rightElbow;
		readonly RagdollPartBox _chest;
		readonly RagdollPartSphere _head;

		public Ragdoller(Transform player, Vector3 playerDirection)
		{
			_playerDirection = playerDirection;
			_readyToGenerate = false;

			// find Animator
			Animator animator = FindAnimator(player);
			if (animator == null)
				return;
			_rootNode = animator.transform;

			// specify all parts of ragdoll
			_pelvis = new RagdollPartBox(animator.GetBoneTransform(HumanBodyBones.Hips));
			_leftHip = new RagdollPartCapsule(animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg));
			_leftKnee = new RagdollPartCapsule(animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg));
			_rightHip = new RagdollPartCapsule(animator.GetBoneTransform(HumanBodyBones.RightUpperLeg));
			_rightKnee = new RagdollPartCapsule(animator.GetBoneTransform(HumanBodyBones.RightLowerLeg));
			_leftArm = new RagdollPartCapsule(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm));
			_leftElbow = new RagdollPartCapsule(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm));
			_rightArm = new RagdollPartCapsule(animator.GetBoneTransform(HumanBodyBones.RightUpperArm));
			_rightElbow = new RagdollPartCapsule(animator.GetBoneTransform(HumanBodyBones.RightLowerArm));
			_chest = new RagdollPartBox(animator.GetBoneTransform(HumanBodyBones.Chest));
			_head = new RagdollPartSphere(animator.GetBoneTransform(HumanBodyBones.Head));

			if (_chest.transform == null)
				_chest = new RagdollPartBox(animator.GetBoneTransform(HumanBodyBones.Spine));

			if (!CheckFields())
			{
				Debug.LogError("Not all nodes was found!");
				return;
			}

			_readyToGenerate = true;
		}
		/// <summary>
		/// Finds animator component in "player" and in parents till it find Animator component. Otherwise returns null
		/// </summary>
		static Animator FindAnimator(Transform player)
		{
			Animator animator;
			do
			{
				animator = player.GetComponent<Animator>();
				if (animator != null && animator.enabled)
					break;

				player = player.parent;
			}
			while (player != null);

			if (animator == null | player == null)
			{
				Debug.LogError("An Animator must be attached to find bones!");
				return null;
			}
			if (!animator.isHuman)
			{
				Debug.LogError("To auto detect bones, there are has to be humanoid Animator!");
				return null;
			}
			return animator;
		}
		/// <summary>
		/// Some checks before Applying ragdoll
		/// </summary>
		bool CheckFields()
		{
			if (_rootNode == null |
				_pelvis == null |
				_leftHip == null |
				_leftKnee == null |
				_rightHip == null |
				_rightKnee == null |
				_leftArm == null |
				_leftElbow == null |
				_rightArm == null |
				_rightElbow == null |
				_chest == null |
				_head == null)
				return false;

			return true;
		}
		/// <summary>
		/// Create all ragdoll's components and set their proterties
		/// </summary>
		public void ApplyRagdoll(float totalMass, bool asTrigger, float rigidDrag, CollisionDetectionMode cdMode)
		{
			if (!_readyToGenerate)
			{
				Debug.LogError("Initialization failed. Reinstance object!");
				return;
			}

			var weight = new WeightCalculator(totalMass);

			AddComponentesTo(_pelvis,     asTrigger, rigidDrag, cdMode, weight.Pelvis, false);
			AddComponentesTo(_leftHip,    asTrigger, rigidDrag, cdMode, weight.Hip,    true);
			AddComponentesTo(_leftKnee,   asTrigger, rigidDrag, cdMode, weight.Knee,   true);
			AddComponentesTo(_rightHip,   asTrigger, rigidDrag, cdMode, weight.Hip,    true);
			AddComponentesTo(_rightKnee,  asTrigger, rigidDrag, cdMode, weight.Knee,   true);
			AddComponentesTo(_leftArm,    asTrigger, rigidDrag, cdMode, weight.Arm,    true);
			AddComponentesTo(_leftElbow,  asTrigger, rigidDrag, cdMode, weight.Elbow,  true);
			AddComponentesTo(_rightArm,   asTrigger, rigidDrag, cdMode, weight.Arm,    true);
			AddComponentesTo(_rightElbow, asTrigger, rigidDrag, cdMode, weight.Elbow,  true);
			AddComponentesTo(_chest,      asTrigger, rigidDrag, cdMode, weight.Chest,  true);
			AddComponentesTo(_head,       asTrigger, rigidDrag, cdMode, weight.Head,   true);

			// Pelvis
			Vector3 pelvisSize = new Vector3(0.32f, 0.31f, 0.3f);
			Vector3 pelvisCenter = new Vector3(00f, 0.06f, -0.01f);
			_pelvis.collider.size = _pelvis.transform.InverseTransformVector(pelvisSize);
			_pelvis.collider.center = _pelvis.transform.InverseTransformVector(pelvisCenter);

			ApplySide(true);
			ApplySide(false);

			// Chest collider
			Vector3 chestSize = new Vector3(0.34f, 0.34f, 0.28f);

			float y = (pelvisSize.y + chestSize.y) / 2f + pelvisCenter.y;
			y -= _chest.transform.position.y - _pelvis.transform.position.y;
			_chest.collider.size = _chest.transform.InverseTransformVector(chestSize);
			_chest.collider.center = _chest.transform.InverseTransformVector(new Vector3(0f, y, -0.03f));

			// Chest joint
			var chestJoint = _chest.joint;
			ConfigureJointParams(_chest, _pelvis.rigidbody, _rootNode.right, _rootNode.forward);
			ConfigureJointLimits(chestJoint, -45f, 20f, 20f, 20f);

			// head
			_head.collider.radius = 0.1f;
			_head.collider.center = _head.transform.InverseTransformVector(new Vector3(0f, 0.09f, 0.03f));
			var headJoint = _head.joint;
			ConfigureJointParams(_head, _chest.rigidbody, _rootNode.right, _rootNode.forward);
			ConfigureJointLimits(headJoint, -45f, 20f, 20f, 20f);
		}

		static void ConfigureJointParams(RagdollPartBase part, Rigidbody anchor, Vector3 axis, Vector3 swingAxis)
		{
			part.joint.connectedBody = anchor;
			part.joint.axis = part.transform.InverseTransformDirection(axis);
			part.joint.swingAxis = part.transform.InverseTransformDirection(swingAxis);
		}

		static void ConfigureJointLimits(CharacterJoint joint, float lowTwist, float highTwist, float swing1, float swing2)
		{
			if (lowTwist > highTwist)
				throw new ArgumentException("wrong limitation: lowTwist > highTwist");

			var twistLimitSpring = joint.twistLimitSpring;
			joint.twistLimitSpring = twistLimitSpring;

			var swingLimitSpring = joint.swingLimitSpring;
			joint.swingLimitSpring = swingLimitSpring;

			// configure limits
			var lowTwistLimit = joint.lowTwistLimit;
			lowTwistLimit.limit = lowTwist;
			joint.lowTwistLimit = lowTwistLimit;
			var highTwistLimit = joint.highTwistLimit;
			highTwistLimit.limit = highTwist;
			joint.highTwistLimit = highTwistLimit;

			var swing1Limit = joint.swing1Limit;
			swing1Limit.limit = swing1;
			joint.swing1Limit = swing1Limit;
			var swing2Limit = joint.swing2Limit;
			swing2Limit.limit = swing2;
			joint.swing2Limit = swing2Limit;
		}

		/// <summary>
		/// Configure one hand and one leg
		/// </summary>
		/// <param name="leftSide">If true, configuration apply to left hand and left leg, otherwise right hand and right leg</param>
		void ApplySide(bool leftSide)
		{
			RagdollPartCapsule hip = (leftSide ? _leftHip : _rightHip);
			RagdollPartCapsule knee = (leftSide ? _leftKnee : _rightKnee);
			RagdollPartCapsule arm = (leftSide ? _leftArm : _rightArm);
			RagdollPartCapsule elbow = (leftSide ? _leftElbow : _rightElbow);

			ConfigureRagdollForLimb(hip, knee);
			ConfigureRagdollForLimb(arm, elbow);
			ConfigureHandJoints(arm, elbow, leftSide);
			ConfigureLegsJoints(hip, knee);
		}
		/// <summary>
		/// Configer one of 4 body parts: right leg, left leg, right hand or left hand
		/// </summary>
		static void ConfigureRagdollForLimb(RagdollPartCapsule limbUpper, RagdollPartCapsule limbLower)
		{
			// limbUpper
			CapsuleCollider upperCapsule = limbUpper.collider;
			upperCapsule.direction = GetXyzDirection(limbLower.transform.localPosition);
			upperCapsule.radius = 0.08f;
			Vector3 endLocalPosition = limbLower.transform.localPosition;
			upperCapsule.height = endLocalPosition.magnitude;
			upperCapsule.center = Vector3.Scale(endLocalPosition, new Vector3(0.5f, 0.5f, 0.5f));

			// limbLower
			var firstChildLocalPos = limbLower.transform.GetChild(0).localPosition;
			CapsuleCollider endCapsule = limbLower.collider;
			endCapsule.direction = GetXyzDirection(firstChildLocalPos);

			Vector3 handDirection = firstChildLocalPos.normalized;
			float endLength = FindLengthOfLimb(limbLower.transform);
			float halfEndLength = endLength / 2f;
			endCapsule.radius = 0.08f;
			endCapsule.height = endLength;
			endCapsule.center = Vector3.Scale(handDirection, new Vector3(halfEndLength, halfEndLength, halfEndLength));
		}
		/// <summary>
		/// Get the most appropriate direction in terms of PhysX (0,1,2 directions)
		/// </summary>
		static int GetXyzDirection(Vector3 node)
		{
			float x = Mathf.Abs(node.x);
			float y = Mathf.Abs(node.y);
			float z = Mathf.Abs(node.z);

			if (x > y & x > z)		// x is the bigest
				return 0;
			if (y > x & y > z)		// y is the bigest
				return 1;

			// z is the bigest
			return 2;
		}

		static float FindLengthOfLimb(Transform limb)
		{
			var relPostition = limb.transform.position - limb.transform.GetChild(0).position;
			var rotater = Quaternion.FromToRotation(relPostition.normalized, new Vector3(1f, 0f, 0f));
			float limbPosX = (rotater * limb.position).x;
			float maxDistance = 0f;

			// find the most far object that attached to 'limb'
			foreach (Transform t in limb.GetComponentsInChildren<Transform>())
			{
				Vector3 distance = rotater * t.position;
				float newDist = Mathf.Abs(limbPosX - distance.x);
				if (newDist > maxDistance)
					maxDistance = newDist;
			}
			return maxDistance;
		}

		void ConfigureHandJoints(RagdollPartCapsule arm, RagdollPartCapsule elbow, bool leftHand)
		{
			var armJoint = arm.joint;
			var elbowJoint = elbow.joint;

			var asideUpper = elbow.transform.position - arm.transform.position;
			var asideLower = elbow.transform.GetChild(0).position - elbow.transform.position;

			if (leftHand)
			{
				ConfigureJointLimits(armJoint, -100f, 30f, 100f, 45f);
				ConfigureJointLimits(elbowJoint, -120f, 0f, 10f, 90f);
				asideUpper = -asideUpper;
				asideLower = -asideLower;
			}
			else
			{
				ConfigureJointLimits(armJoint, -30f, 100f, 100f, 45f);
				ConfigureJointLimits(elbowJoint, 0f, 120f, 10f, 90f);
			}

			var upU = Vector3.Cross(_playerDirection, asideUpper);
			var upL = Vector3.Cross(_playerDirection, asideLower);
			ConfigureJointParams(arm, _chest.rigidbody, upU, _playerDirection);
			ConfigureJointParams(elbow, arm.rigidbody, upL, _playerDirection);
		}

		void ConfigureLegsJoints(RagdollPartCapsule hip, RagdollPartCapsule knee)
		{
			var hipJoint = hip.joint;
			var kneeJoint = knee.joint;

			ConfigureJointParams(hip, _pelvis.rigidbody, _rootNode.right, _rootNode.forward);
			ConfigureJointParams(knee, hip.rigidbody, _rootNode.right, _rootNode.forward);

			ConfigureJointLimits(hipJoint, -10f, 120f, 90f, 20f);
			ConfigureJointLimits(kneeJoint, -120f, 0f, 10f, 20f);
		}

		static void AddComponentesTo(RagdollPartBox part, bool asTrigger, float rigidDrag, CollisionDetectionMode cdMode, float mass, bool addJoint)
		{
			AddComponentesTo(part, rigidDrag, cdMode, mass, addJoint);
			GameObject go = part.transform.gameObject;

			part.collider = go.GetComponent<BoxCollider>();
			if (part.collider == null)
				part.collider = go.AddComponent<BoxCollider>();
			part.collider.isTrigger = asTrigger;
		}

		static void AddComponentesTo(RagdollPartCapsule part, bool asTrigger, float rigidDrag, CollisionDetectionMode cdMode, float mass, bool addJoint)
		{
			AddComponentesTo(part, rigidDrag, cdMode, mass, addJoint);
			GameObject go = part.transform.gameObject;

			part.collider = go.GetComponent<CapsuleCollider>();
			if (part.collider == null)
				part.collider = go.AddComponent<CapsuleCollider>();
			part.collider.isTrigger = asTrigger;
		}

		static void AddComponentesTo(RagdollPartSphere part, bool asTrigger, float rigidDrag, CollisionDetectionMode cdMode, float mass, bool addJoint)
		{
			AddComponentesTo(part, rigidDrag, cdMode, mass, addJoint);
			GameObject go = part.transform.gameObject;

			part.collider = go.GetComponent<SphereCollider>();
			if (part.collider == null)
				part.collider = go.AddComponent<SphereCollider>();
			part.collider.isTrigger = asTrigger;
		}

		static void AddComponentesTo(RagdollPartBase part, float rigidDrag, CollisionDetectionMode cdMode, float mass, bool addJoint)
		{
			GameObject go = part.transform.gameObject;

			part.rigidbody = go.GetComponent<Rigidbody>();
			if (part.rigidbody == null)
				part.rigidbody = go.AddComponent<Rigidbody>();
			part.rigidbody.mass = mass;
			part.rigidbody.linearDamping = rigidDrag;
			part.rigidbody.collisionDetectionMode = cdMode;

			if (addJoint)
			{
				part.joint = go.GetComponent<CharacterJoint>();
				if (part.joint == null)
					part.joint = go.AddComponent<CharacterJoint>();
			}
		}

		/// <summary>
		/// Remove all colliders, joints, and rigids
		/// </summary>
		public void ClearRagdoll()
		{
			foreach (var component in _pelvis.transform.GetComponentsInChildren<Collider>())
				GameObject.DestroyImmediate(component);
			foreach (var component in _pelvis.transform.GetComponentsInChildren<CharacterJoint>())
				GameObject.DestroyImmediate(component);
			foreach (var component in _pelvis.transform.GetComponentsInChildren<Rigidbody>())
				GameObject.DestroyImmediate(component);

			DeleteColliderNodes(_pelvis.transform);
		}
		/// <summary>
		/// Correct deleting collider with collider's separate nodes
		/// </summary>
		/// <param name="node"></param>
		private static void DeleteColliderNodes(Transform node)
		{
			const string colliderNodeSufix = "_ColliderRotator";

			for (int i = 0; i < node.childCount; ++i)
			{
				Transform child = node.GetChild(i);

				if (child.name.EndsWith(colliderNodeSufix))
					GameObject.DestroyImmediate(child.gameObject);
				else
					DeleteColliderNodes(child);
			}
		}
	}
}
#endif