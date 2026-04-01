using System.Collections;
using UnityEngine;
using Unity.Netcode;

namespace ProjectJS.Controller
{
	public partial class BossController : NetworkBehaviour
	{
		private enum State { Init, Roam, Detect, Pattern }
		private StateMachine<State> stateMachine;

		private void Awake()
		{
			stateMachine = new(this);
			stateMachine.AddState(State.Init, OnStartInit);
			stateMachine.AddState(State.Roam, OnStartRoam);
			stateMachine.AddState(State.Detect, OnStartDetect);
			stateMachine.AddState(State.Pattern, OnStartPattern, OnEndPattern);
			stateMachine.ChangeState(State.Init);
		}

		private IEnumerator OnStartInit()
		{
			Debug.Log("Init");
			yield return null;
			stateMachine.ChangeState(State.Roam);
		}
	
		private IEnumerator OnStartRoam()
		{
			Debug.Log("Roaming...");
			yield return new WaitForSeconds(1.0f);
			stateMachine.ChangeState(State.Detect);
		}
		
		private IEnumerator OnStartDetect()
		{
			Debug.Log("Detecting...");
			yield return new WaitForSeconds(1.0f);
			stateMachine.ChangeState(State.Pattern);
		}

		private IEnumerator OnStartPattern()
		{
			Debug.Log("StartPattern");
			yield return new WaitForSeconds(1.0f);
			stateMachine.ChangeState(State.Roam);
		}

		private IEnumerator OnEndPattern()
		{
			Debug.Log("EndPattern");
			yield return null;
		}
	}
}
