using Microsoft.Xna.Framework.Input;
using Xunit;
using CoreEssentials.Inputs;

namespace CoreEssentials.Tests.Inputs;

public class KeyboardEventArgsTests
{
    [Fact]
    public void Constructor_SetsKeyAndDefaultModifiers()
    {
        var args = new KeyboardEventArgs(Keys.A);
        Assert.Equal(Keys.A, args.Key);
        Assert.Equal(KeyboardModifiers.None, args.Modifiers);
    }

    [Fact]
    public void Constructor_SetsExplicitModifiers()
    {
        var args = new KeyboardEventArgs(Keys.S, KeyboardModifiers.Control | KeyboardModifiers.Shift);
        Assert.True(args.IsControl);
        Assert.True(args.IsShift);
        Assert.False(args.IsAlt);
    }

    [Fact]
    public void ModifierFlags_AreIndependent()
    {
        var control = new KeyboardEventArgs(Keys.X, KeyboardModifiers.Control);
        Assert.True(control.IsControl);
        Assert.False(control.IsShift);
        Assert.False(control.IsAlt);

        var shift = new KeyboardEventArgs(Keys.X, KeyboardModifiers.Shift);
        Assert.True(shift.IsShift);
        Assert.False(shift.IsControl);

        var alt = new KeyboardEventArgs(Keys.X, KeyboardModifiers.Alt);
        Assert.True(alt.IsAlt);
        Assert.False(alt.IsControl);
    }

    [Theory]
    [InlineData(Keys.A, false, 'a')]
    [InlineData(Keys.B, true, 'B')]
    [InlineData(Keys.Z, false, 'z')]
    public void Character_Letters_RespectShift(Keys key, bool shift, char expected)
    {
        var mods = shift ? KeyboardModifiers.Shift : KeyboardModifiers.None;
        var args = new KeyboardEventArgs(key, mods);
        Assert.Equal(expected, args.Character);
    }

    [Theory]
    [InlineData(Keys.D0, false, '0')]
    [InlineData(Keys.D0, true, ')')]
    [InlineData(Keys.D1, true, '!')]
    [InlineData(Keys.D2, true, '@')]
    [InlineData(Keys.NumPad5, false, '5')]
    public void Character_Digits_RespectShift(Keys key, bool shift, char expected)
    {
        var mods = shift ? KeyboardModifiers.Shift : KeyboardModifiers.None;
        Assert.Equal(expected, new KeyboardEventArgs(key, mods).Character);
    }

    [Theory]
    [InlineData(Keys.Space, ' ')]
    [InlineData(Keys.Tab, '\t')]
    [InlineData(Keys.Enter, (char)13)]
    [InlineData(Keys.Back, (char)8)]
    [InlineData(Keys.Add, '+')]
    [InlineData(Keys.Divide, '/')]
    [InlineData(Keys.Subtract, '-')]
    public void Character_SpecialKeys_ReturnExpectedValue(Keys key, char expected)
    {
        Assert.Equal(expected, new KeyboardEventArgs(key).Character);
    }

    [Theory]
    [InlineData(Keys.OemComma, false, ',')]
    [InlineData(Keys.OemComma, true, '<')]
    [InlineData(Keys.OemOpenBrackets, true, '{')]
    [InlineData(Keys.OemCloseBrackets, false, ']')]
    [InlineData(Keys.OemPeriod, true, '>')]
    [InlineData(Keys.OemQuotes, true, '"')]
    [InlineData(Keys.OemSemicolon, false, ';')]
    [InlineData(Keys.OemTilde, true, '~')]
    public void Character_OemKeys_RespectShift(Keys key, bool shift, char expected)
    {
        var mods = shift ? KeyboardModifiers.Shift : KeyboardModifiers.None;
        Assert.Equal(expected, new KeyboardEventArgs(key, mods).Character);
    }

    [Fact]
    public void Character_NonPrintableKey_ReturnsNull()
    {
        Assert.Null(new KeyboardEventArgs(Keys.Left).Character);
        Assert.Null(new KeyboardEventArgs(Keys.F1).Character);
    }
}
