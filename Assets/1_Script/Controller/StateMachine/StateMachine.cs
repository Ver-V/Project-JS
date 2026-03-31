using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectJS.Controller
{
	public class StateMachine<T> where T : Enum
	{
		public class StateCoroutine
		{
			private Func<IEnumerator> onStart;
			private Func<IEnumerator> onEnd;
			public Func<IEnumerator> OnStart => onStart;
			public Func<IEnumerator> OnEnd => onEnd;

			public StateCoroutine(Func<IEnumerator> onStart, Func<IEnumerator> onEnd)
			{
				this.onStart = onStart;
				this.onEnd = onEnd;
			}
		}

		private Dictionary<T, StateCoroutine> stateDict = new();
		private StateCoroutine currentStateCoroutine = null;
		private MonoBehaviour runner = null;

		private Coroutine currentCoroutine = null;

		public StateMachine(MonoBehaviour runner)
		{
			this.runner = runner;
		}

		public void AddState(T state, Func<IEnumerator> onStart, Func<IEnumerator> onEnd = null)
		{
			if (stateDict.ContainsKey(state))
			{
				Debug.LogError("StateMachine - already has same state.");
				return;
			}

			stateDict.Add(state, new StateCoroutine(onStart, onEnd));
		}

		public void ChangeState(T state)
		{
			if (!stateDict.TryGetValue(state, out StateCoroutine stateCoroutine))
			{
				Debug.LogError($"StateMachine - State not found : {state.ToString()}");
				return;
			}

			if (currentCoroutine != null)
				runner.StopCoroutine(currentCoroutine);

			currentCoroutine = runner.StartCoroutine(ChangeStateCoroutine(stateCoroutine));

		}

		private IEnumerator ChangeStateCoroutine(StateCoroutine stateCoroutine)
		{
			if (currentStateCoroutine != null && currentStateCoroutine.OnEnd != null)
			{
				yield return currentStateCoroutine.OnEnd();
			}

			currentStateCoroutine = stateCoroutine;

			yield return currentStateCoroutine.OnStart();
		}
	}
}
