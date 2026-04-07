using UnityEngine;

namespace ProjectJS.Manager
{
	public class Managers : MonoBehaviour
	{
		private static Managers instance;
		public static Managers Instance { get => instance; }

		void Awake()
		{
			Init();
		}

		private void Init()
		{
			if (null == instance)
			{
				instance = this;
				DontDestroyOnLoad(this.gameObject);
			}
			else
			{
				Destroy(this.gameObject);
			}

			_scene.Init();
			_input.Init();
			_pool.Init();
		}

		private static  ResourceManager _resource = new ResourceManager();
		private static  SceneManagerEx _scene = new SceneManagerEx();
		private static SpawnManager _spawn = new SpawnManager();
		private static InputManager _input = new InputManager();
		private static PoolingManager _pool = new PoolingManager();

		public static ResourceManager Resource { get => _resource; }
		public static SceneManagerEx Scene { get => _scene; }
		public static SpawnManager Spawn { get => _spawn; }
		public static InputManager Input { get => _input; }
		public static PlayerInput PlayerInput => _input.PlayerInput;
		public static PoolingManager Pool { get => _pool; }
	}
}