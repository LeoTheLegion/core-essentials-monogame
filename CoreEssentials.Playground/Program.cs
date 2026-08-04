using CoreEssentials.Playground;
using CoreEssentials.Scenes;
using Microsoft.Xna.Framework;


using var game = new CoreEssentials.MainGame();

game.Graphics.PreferredBackBufferWidth = 1280;
game.Graphics.PreferredBackBufferHeight = 720;
game.Graphics.ApplyChanges();

// Create a loading screen with custom colors
LoadingScene loadingScene = new LoadingScene(
    "Loading Character Demo...", 
    Color.Black, 
    Color.LightBlue, 
    Color.White
);

// Set the loading scene for the SceneManager to use during transitions
game.SceneManager.SetLoadingScene(loadingScene);

// Use our new CharacterScene instead of the PhysicsEntityScene
game.SceneManager.LoadScene(new PhysicsEntityScene());

game.Run();
