using UnityEngine;
using System.Collections;

public class Footsteps : MonoBehaviour
{
	[Header("Main")]
    public AudioSource soundsGO;

    private CharacterController controller;
    private PlayerController playerController;

	[System.Serializable]
	public class footsteps
	{
		public string groundTag;
		public AudioClip[] footstep;
	}

	[Header("Player Footsteps")]
	[Tooltip("Element 0 is always Untagged/Concrete and 1 is ladder")]
	public footsteps[] m_Footsteps;

    [Space(5)]
    [Header("Audio Volume")]
	public float audioVolumeCrouch = 0.1f;
	public float audioVolumeWalk = 0.2f;
	public float audioVolumeRun = 0.3f;

    [Header("Audio Step Lenght")]
    public float audioStepLengthCrouch = 0.75f;
	public float audioStepLengthWalk = 0.45f;
	public float audioStepLengthRun = 0.25f;

    private bool step = true;
    private string curMat;

    void OnEnable()
    {
        step = true;
        controller = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        float speed = controller.velocity.magnitude;
        curMat = hit.gameObject.tag;

		if (playerController.state == 2 || !step) return;

        if (controller.isGrounded && hit.normal.y > 0.3f)
        {
			for (int i = 0; i < m_Footsteps.Length; i++)
			{
				if (curMat == m_Footsteps[i].groundTag) 
				{
                    if(playerController.state == 0)
                    {
                        if (!playerController.run && speed >= playerController.walkSpeed) StartCoroutine(WalkOnGround());
                        if (playerController.run && speed <= playerController.runSpeed && speed > playerController.walkSpeed) StartCoroutine(RunOnGround());
                    }
                    else
                    {
                        if (speed >= playerController.crouchSpeed && speed < playerController.walkSpeed) StartCoroutine(CrouchOnGrouund());
                    }
				}
			}
        }
    }
	
	//Ladder footsteps
	public void PlayLadderSound()
	{
		soundsGO.PlayOneShot(m_Footsteps[1].footstep[Random.Range(0, m_Footsteps[1].footstep.Length)], audioVolumeWalk);
	}

    public IEnumerator JumpLand()
    {
        if (!soundsGO.enabled) yield break;

		for (int i = 0; i < m_Footsteps.Length; i++)
		{
			if (curMat == m_Footsteps[i].groundTag) 
			{
				soundsGO.PlayOneShot(m_Footsteps[i].footstep[Random.Range(0, m_Footsteps[i].footstep.Length)], 0.5f);
				yield return new WaitForSeconds(0.12f);
				soundsGO.PlayOneShot(m_Footsteps[i].footstep[Random.Range(0, m_Footsteps[i].footstep.Length)], 0.4f);
			}
		}
    }

	IEnumerator CrouchOnGrouund()
	{
		for (int i = 0; i < m_Footsteps.Length; i++)
		{
			if (curMat == m_Footsteps[i].groundTag)
			{
				step = false;
				soundsGO.PlayOneShot (m_Footsteps[i].footstep [Random.Range (0, m_Footsteps[i].footstep.Length)], audioVolumeCrouch);
				yield return new WaitForSeconds (audioStepLengthCrouch);
				step = true;
			}
		}
	}

	IEnumerator WalkOnGround()
	{
		for (int i = 0; i < m_Footsteps.Length; i++)
		{
			if (curMat == m_Footsteps[i].groundTag)
			{
				step = false;
				soundsGO.PlayOneShot(m_Footsteps[i].footstep[Random.Range(0, m_Footsteps[i].footstep.Length)], audioVolumeWalk);
				yield return new  WaitForSeconds (audioStepLengthWalk);
				step = true;
			}
		}
	}

	IEnumerator RunOnGround()
	{
		for (int i = 0; i < m_Footsteps.Length; i++)
		{
			if (curMat == m_Footsteps[i].groundTag)
			{
				step = false;
				soundsGO.PlayOneShot(m_Footsteps[i].footstep[Random.Range(0, m_Footsteps[i].footstep.Length)], audioVolumeRun);
				yield return new  WaitForSeconds (audioStepLengthRun);
				step = true;
			}
		}
	}
}