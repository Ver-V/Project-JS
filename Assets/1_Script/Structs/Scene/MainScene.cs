using ProjectJS.Utils;

namespace ProjectJS.Structs
{
	public class MainScene : SceneBase
	{
		protected override void Init()
		{
			base.Init();
			SceneEnum = SceneEnum.Main;
		}

		public override void Clear()
		{

		}
	}
}