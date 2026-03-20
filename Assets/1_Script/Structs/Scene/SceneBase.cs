using ProjectJS.Utils;
using UnityEngine;

namespace ProjectJS.Structs
{
    public abstract class SceneBase : MonoBehaviour
    {
        public SceneEnum SceneEnum { get; protected set; } = SceneEnum.None;

		private void Awake()
		{
			Init();
		}

		protected virtual void Init()
		{

		}

		public abstract void Clear();
	}
}