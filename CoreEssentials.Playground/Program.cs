using CoreEssentials.Playground;
using CoreEssentials.SceneManagement;


using var game = new CoreEssentials.MainGame();

SceneManager.LoadScene(new PhysicsEntityScene());

game.Run();
