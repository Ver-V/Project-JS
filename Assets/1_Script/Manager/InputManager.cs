namespace ProjectJS.Manager
{
	public class InputManager
	{
		private PlayerInput playerInput = null;
		public PlayerInput PlayerInput => playerInput;

		public void Init()
		{
			playerInput = new();
			playerInput.Enable();
		}
	}
}
