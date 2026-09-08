using System;
using System.Linq;
using CoreEssentials.Scenes;
using Xunit;

namespace CoreEssentials.Tests.SceneManagement;

/// <summary>
/// Tests for <see cref="SceneManifest"/> — the two-list scene manifest format
/// (<GameScenes> ordered list + optional <LoadingScenes> registry).
/// All tests use temp XML strings; no windows, no assets on disk.
/// </summary>
public class SceneManifestTests
{
    private const string FullXml = """
        <Scenes>
            <GameScenes>
                <Scene Name="HomeScene.xml" />
                <Scene Name="CharacterScene.xml" LoadingScreen="loading_main.xml" />
                <Scene Name="CameraScene.xml" />
            </GameScenes>
            <LoadingScenes>
                <LoadingScene Name="loading_main.xml" Default="true" />
                <LoadingScene Name="loading_physics.xml" />
            </LoadingScenes>
        </Scenes>
        """;

    // --- Happy path -------------------------------------------------------

    [Fact]
    public void Parse_ValidXml_PopulatesGameScenesInOrder()
    {
        var manifest = SceneManifest.Parse(FullXml);

        Assert.Equal(new[] { "HomeScene.xml", "CharacterScene.xml", "CameraScene.xml" },
            manifest.GameScenes.Select(e => e.Name));
    }

    [Fact]
    public void Parse_ValidXml_PopulatesLoadingScenes()
    {
        var manifest = SceneManifest.Parse(FullXml);

        Assert.Equal(new[] { "loading_main.xml", "loading_physics.xml" },
            manifest.LoadingScenes.Select(e => e.Name));
    }

    [Fact]
    public void Parse_ValidXml_RecordsPerSceneLoadingScreenAttribute()
    {
        var manifest = SceneManifest.Parse(FullXml);

        Assert.Equal("loading_main.xml", manifest.GameScenes[1].LoadingScreen);
        Assert.Null(manifest.GameScenes[0].LoadingScreen);
    }

    [Fact]
    public void Parse_ValidXml_DefaultLoadingSceneIsMarked()
    {
        var manifest = SceneManifest.Parse(FullXml);

        Assert.Equal("loading_main.xml", manifest.DefaultLoadingScene);
    }

    [Fact]
    public void Parse_NoLoadingScenesElement_IsAllowed()
    {
        var xml = """
            <Scenes>
                <GameScenes>
                    <Scene Name="HomeScene.xml" />
                </GameScenes>
            </Scenes>
            """;

        var manifest = SceneManifest.Parse(xml);

        Assert.Empty(manifest.LoadingScenes);
        Assert.Null(manifest.DefaultLoadingScene);
    }

    // --- Startup + navigation helpers --------------------------------------

    [Fact]
    public void StartupScene_IsFirstGameScene()
    {
        var manifest = SceneManifest.Parse(FullXml);

        Assert.Equal("HomeScene.xml", manifest.StartupScene);
    }

    [Fact]
    public void IndexOf_KnownName_ReturnsPosition()
    {
        var manifest = SceneManifest.Parse(FullXml);

        Assert.Equal(1, manifest.IndexOf("CharacterScene.xml"));
    }

    [Fact]
    public void IndexOf_UnknownName_ReturnsMinusOne()
    {
        var manifest = SceneManifest.Parse(FullXml);

        Assert.Equal(-1, manifest.IndexOf("NotListed.xml"));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    public void NextOf_InnerIndex_ReturnsNextPosition(int index, int expected)
    {
        var manifest = SceneManifest.Parse(FullXml);

        Assert.Equal(expected, manifest.NextOf(index));
    }

    [Fact]
    public void NextOf_LastIndex_ClampsAtEnd()
    {
        var manifest = SceneManifest.Parse(FullXml);

        Assert.Equal(2, manifest.NextOf(2));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 1)]
    public void PreviousOf_InnerIndex_ReturnsPreviousPosition(int index, int expected)
    {
        var manifest = SceneManifest.Parse(FullXml);

        Assert.Equal(expected, manifest.PreviousOf(index));
    }

    [Fact]
    public void PreviousOf_FirstIndex_ClampsAtStart()
    {
        var manifest = SceneManifest.Parse(FullXml);

        Assert.Equal(0, manifest.PreviousOf(0));
    }

    // --- Loading-screen resolution -----------------------------------------

    [Fact]
    public void LoadingScreenFor_SceneWithAttribute_ReturnsThatScreen()
    {
        var manifest = SceneManifest.Parse(FullXml);

        Assert.Equal("loading_main.xml", manifest.LoadingScreenFor("CharacterScene.xml"));
    }

    [Fact]
    public void LoadingScreenFor_SceneWithoutAttribute_FallsBackToDefault()
    {
        var manifest = SceneManifest.Parse(FullXml);

        Assert.Equal("loading_main.xml", manifest.LoadingScreenFor("HomeScene.xml"));
    }

    [Fact]
    public void LoadingScreenFor_NoDefaultDeclared_ReturnsNull()
    {
        var xml = """
            <Scenes>
                <GameScenes>
                    <Scene Name="HomeScene.xml" />
                </GameScenes>
                <LoadingScenes>
                    <LoadingScene Name="loading_a.xml" />
                </LoadingScenes>
            </Scenes>
            """;

        var manifest = SceneManifest.Parse(xml);

        Assert.Null(manifest.LoadingScreenFor("HomeScene.xml"));
    }

    [Fact]
    public void LoadingScreenFor_UnknownScene_ReturnsNull()
    {
        var manifest = SceneManifest.Parse(FullXml);

        Assert.Null(manifest.LoadingScreenFor("NotListed.xml"));
    }

    // --- Validation errors --------------------------------------------------

    [Fact]
    public void Parse_MalformedXml_Throws()
    {
        Assert.Throws<FormatException>(() => SceneManifest.Parse("<Scenes><GameScenes>"));
    }

    [Fact]
    public void Parse_WrongRootElement_Throws()
    {
        var xml = """
            <NotScenes>
                <GameScenes>
                    <Scene Name="HomeScene.xml" />
                </GameScenes>
            </NotScenes>
            """;

        Assert.Throws<FormatException>(() => SceneManifest.Parse(xml));
    }

    [Fact]
    public void Parse_MissingGameScenes_Throws()
    {
        var xml = """
            <Scenes>
                <LoadingScenes>
                    <LoadingScene Name="loading.xml" />
                </LoadingScenes>
            </Scenes>
            """;

        Assert.Throws<FormatException>(() => SceneManifest.Parse(xml));
    }

    [Fact]
    public void Parse_EmptyGameScenes_Throws()
    {
        var xml = """
            <Scenes>
                <GameScenes />
            </Scenes>
            """;

        Assert.Throws<FormatException>(() => SceneManifest.Parse(xml));
    }

    [Fact]
    public void Parse_DuplicateGameScene_Throws()
    {
        var xml = """
            <Scenes>
                <GameScenes>
                    <Scene Name="HomeScene.xml" />
                    <Scene Name="HomeScene.xml" />
                </GameScenes>
            </Scenes>
            """;

        var ex = Assert.Throws<FormatException>(() => SceneManifest.Parse(xml));
        Assert.Contains("HomeScene.xml", ex.Message);
    }

    [Fact]
    public void Parse_DuplicateLoadingScene_Throws()
    {
        var xml = """
            <Scenes>
                <GameScenes>
                    <Scene Name="HomeScene.xml" />
                </GameScenes>
                <LoadingScenes>
                    <LoadingScene Name="loading.xml" />
                    <LoadingScene Name="loading.xml" />
                </LoadingScenes>
            </Scenes>
            """;

        var ex = Assert.Throws<FormatException>(() => SceneManifest.Parse(xml));
        Assert.Contains("loading.xml", ex.Message);
    }

    [Fact]
    public void Parse_UnknownElement_Throws()
    {
        var xml = """
            <Scenes>
                <GameScenes>
                    <Scene Name="HomeScene.xml" />
                </GameScenes>
                <Bogus />
            </Scenes>
            """;

        Assert.Throws<FormatException>(() => SceneManifest.Parse(xml));
    }

    [Fact]
    public void Parse_SceneMissingName_Throws()
    {
        var xml = """
            <Scenes>
                <GameScenes>
                    <Scene />
                </GameScenes>
            </Scenes>
            """;

        Assert.Throws<FormatException>(() => SceneManifest.Parse(xml));
    }

    [Fact]
    public void Parse_LoadingScreenAttributeReferencingUndeclaredScreen_Throws()
    {
        var xml = """
            <Scenes>
                <GameScenes>
                    <Scene Name="HomeScene.xml" LoadingScreen="missing.xml" />
                </GameScenes>
                <LoadingScenes>
                    <LoadingScene Name="loading.xml" Default="true" />
                </LoadingScenes>
            </Scenes>
            """;

        var ex = Assert.Throws<FormatException>(() => SceneManifest.Parse(xml));
        Assert.Contains("missing.xml", ex.Message);
    }

    [Fact]
    public void Parse_MultipleDefaultLoadingScenes_Throws()
    {
        var xml = """
            <Scenes>
                <GameScenes>
                    <Scene Name="HomeScene.xml" />
                </GameScenes>
                <LoadingScenes>
                    <LoadingScene Name="loading_a.xml" Default="true" />
                    <LoadingScene Name="loading_b.xml" Default="true" />
                </LoadingScenes>
            </Scenes>
            """;

        Assert.Throws<FormatException>(() => SceneManifest.Parse(xml));
    }
}
