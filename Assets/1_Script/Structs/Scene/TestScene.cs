using ProjectJS.Utils;

namespace ProjectJS.Structs
{
    public class TestScene : SceneBase
    {
        protected override void Init()
        {
            base.Init();
            SceneEnum = SceneEnum.test;
        }

        public override void Clear()
        {
        }
    }
}