/*#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;

namespace BzKovSoft.Tools.RagdollGenerator
{
	[CustomEditor(typeof(CharacterJoint))]
	public class JointHelper : Editor
	{
		CharacterJoint _joint;

		// Use this for initialization
		void OnEnable()
		{
			if (Application.isPlaying || EditorUtility.IsPersistent(target))
				return;

			_joint = (CharacterJoint)target;

			UnityEditor.Tools.hidden = true;
		}

		private void OnDisable()
		{
			UnityEditor.Tools.hidden = false;
		}

		void OnSceneGUI()
		{
			if (Application.isPlaying)
				return;

			JointController.DrawControllers(_joint);
		}

		// Update is called once per frame
		void Update()
		{

		}
	}
}
#endif*/