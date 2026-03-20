using UnityEngine;
using ProjectJS.Utils;
using ProjectJS.Structs;
using UnityEngine.SceneManagement;

namespace ProjectJS.Manager
{
	// HACK - Modification needed in Network env.
	public class SceneManagerEx
	{
		public SceneBase CurrentScene { get { return GameObject.FindFirstObjectByType<SceneBase>(); } }

		public void Init()
		{
			LoadScene(SceneEnum.Main);
		}
		public void ChangeScene(SceneEnum sceneEnum)
		{
			UnloadCurrentScene();
			LoadScene(sceneEnum);
		}

		private void LoadScene(SceneEnum sceneEnum)
		{
			CurrentScene?.Clear();
			SceneManager.LoadScene(sceneEnum.ToString() + "Scene", LoadSceneMode.Additive);
		}

		public async void UnloadCurrentScene()
		{
			if (CurrentScene.SceneEnum == SceneEnum.None) return;
			await SceneManager.UnloadSceneAsync(CurrentScene.SceneEnum.ToString() + "Scene");
		}

		
	}
}