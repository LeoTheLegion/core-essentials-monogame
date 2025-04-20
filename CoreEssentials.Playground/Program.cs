using CoreEssentials.Playground;
using CoreEssentials.SceneManagement;


using var game = new CoreEssentials.MainGame();

game.SceneManager.LoadScene(new PhysicsEntityScene());

game.Run();
