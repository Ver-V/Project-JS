
namespace ProjectJS.Utils
{
	public static class Constants
	{
		public static readonly string KEY_LOBBYNAME = "LobbyName";
		public static readonly string KEY_GAMENAME = "GameName";
		public static readonly string KEY_PASSWORD = "Password";
		public static readonly string KEY_PASSWORDPROTECTED = "PasswordProtected";
		
		public static readonly string VALUE_GAMENAME = "ProjectJS";

		public static readonly string NAME_LOBBY = "lobby_name";
		public static readonly string NAME_SERVER = "Server";

		public static readonly string TAG_CHAT = "ChatMessage";
		public static readonly string TAG_PCARD = "PlayerCard";
		public static readonly string TAG_PLAYER = "Player";
		public static readonly string TAG_BOSS = "Boss";
		public static readonly string TAG_SCENE = "Scene";

		public static readonly int LAYER_INTERACTABLE = 1 << 10;
		public static readonly int LAYER_GROUND = 1 << 11;
		public static readonly int LAYER_PLAYER = 1 << 12;
		public static readonly int LAYER_BOSS = 1 << 13;

		public static readonly int MAX_PLAYERS = 4;
	}
}