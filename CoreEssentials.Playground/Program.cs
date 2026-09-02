using CoreEssentials.Scenes;


using var game = new CoreEssentials.MainGame();

game.Graphics.PreferredBackBufferWidth = 1280;
game.Graphics.PreferredBackBufferHeight = 720;
game.Graphics.ApplyChanges();

// Boot purely from data files (Sprint 5a). The loading screen and the first scene are both
// strict-format XML assets staged into Content/ — no C# LoadingScene or scene subclass.
// Screen size is set once here rather than per-scene.
game.SceneManager.SetLoadingScene("loading.xml");
game.SceneManager.LoadScene("HomeScene.xml");

game.Run();
