#nullable enable
using System;
using CoreEssentials.Inputs;
using Microsoft.Xna.Framework.Input;
using Xunit;

namespace CoreEssentials.Tests.Inputs.Keyboard;

/// <summary>
/// Device-free tests for <see cref="KeyboardEventArgs"/>: the pure modifier-bit flags and the
/// key→character mapping in the private <c>ToChar</c> helper (letters, digits, numpad, punctuation,
/// OEM keys under shift, control codes, and non-printable keys returning null). No input device is
/// involved — only the public <see cref="KeyboardEventArgs.Character"/> property is exercised.
/// </summary>
public class KeyboardEventArgsTests
{
    // ──────────────────────────── Constructor + modifier flags ────────────────────────────

    [Fact]
    public void Ctor_SetsKeyAndModifiers()
    {
        var e = new KeyboardEventArgs(Keys.A, KeyboardModifiers.Shift | KeyboardModifiers.Control);

        Assert.Equal(Keys.A, e.Key);
        Assert.True(e.IsShift);
        Assert.True(e.IsControl);
        Assert.False(e.IsAlt);
    }

    [Fact]
    public void Ctor_DefaultModifier_IsNone()
    {
        var e = new KeyboardEventArgs(Keys.B);

        Assert.Equal(KeyboardModifiers.None, e.Modifiers);
        Assert.False(e.IsControl);
        Assert.False(e.IsShift);
        Assert.False(e.IsAlt);
    }

    [Theory]
    [InlineData(KeyboardModifiers.Control, true, false, false)]
    [InlineData(KeyboardModifiers.Shift, false, true, false)]
    [InlineData(KeyboardModifiers.Alt, false, false, true)]
    [InlineData(KeyboardModifiers.None, false, false, false)]
    public void ModifierFlags_ReflectBits(KeyboardModifiers mods, bool expectCtrl, bool expectShift, bool expectAlt)
    {
        var e = new KeyboardEventArgs(Keys.X, mods);

        Assert.Equal(expectCtrl, e.IsControl);
        Assert.Equal(expectShift, e.IsShift);
        Assert.Equal(expectAlt, e.IsAlt);
    }

    // ──────────────────────────── Character mapping: letters ────────────────────────────

    [Fact]
    public void Character_Letters_AreLowercaseWithoutShift()
    {
        Assert.Equal('a', new KeyboardEventArgs(Keys.A).Character);
        Assert.Equal('m', new KeyboardEventArgs(Keys.M).Character);
        Assert.Equal('z', new KeyboardEventArgs(Keys.Z).Character);
    }

    [Fact]
    public void Character_Letters_AreUppercaseWithShift()
    {
        Assert.Equal('A', new KeyboardEventArgs(Keys.A, KeyboardModifiers.Shift).Character);
        Assert.Equal('M', new KeyboardEventArgs(Keys.M, KeyboardModifiers.Shift).Character);
        Assert.Equal('Z', new KeyboardEventArgs(Keys.Z, KeyboardModifiers.Shift).Character);
    }

    // ──────────────────────────── Character mapping: digits (top row + numpad) ────────────────────────────

    [Fact]
    public void Character_Digits_TopRow_WithShiftSymbols()
    {
        Assert.Equal('0', new KeyboardEventArgs(Keys.D0).Character);
        Assert.Equal(')', new KeyboardEventArgs(Keys.D0, KeyboardModifiers.Shift).Character);
        Assert.Equal('1', new KeyboardEventArgs(Keys.D1).Character);
        Assert.Equal('!', new KeyboardEventArgs(Keys.D1, KeyboardModifiers.Shift).Character);
        Assert.Equal('2', new KeyboardEventArgs(Keys.D2).Character);
        Assert.Equal('@', new KeyboardEventArgs(Keys.D2, KeyboardModifiers.Shift).Character);
        Assert.Equal('3', new KeyboardEventArgs(Keys.D3).Character);
        Assert.Equal('#', new KeyboardEventArgs(Keys.D3, KeyboardModifiers.Shift).Character);
        Assert.Equal('4', new KeyboardEventArgs(Keys.D4).Character);
        Assert.Equal('$', new KeyboardEventArgs(Keys.D4, KeyboardModifiers.Shift).Character);
        Assert.Equal('5', new KeyboardEventArgs(Keys.D5).Character);
        Assert.Equal('%', new KeyboardEventArgs(Keys.D5, KeyboardModifiers.Shift).Character);
        Assert.Equal('6', new KeyboardEventArgs(Keys.D6).Character);
        Assert.Equal('^', new KeyboardEventArgs(Keys.D6, KeyboardModifiers.Shift).Character);
        Assert.Equal('7', new KeyboardEventArgs(Keys.D7).Character);
        Assert.Equal('&', new KeyboardEventArgs(Keys.D7, KeyboardModifiers.Shift).Character);
        Assert.Equal('8', new KeyboardEventArgs(Keys.D8).Character);
        Assert.Equal('*', new KeyboardEventArgs(Keys.D8, KeyboardModifiers.Shift).Character);
        Assert.Equal('9', new KeyboardEventArgs(Keys.D9).Character);
        Assert.Equal('(', new KeyboardEventArgs(Keys.D9, KeyboardModifiers.Shift).Character);
    }

    [Fact]
    public void Character_Numpad_Digits_AreNumericIgnoringShift()
    {
        Assert.Equal('0', new KeyboardEventArgs(Keys.NumPad0).Character);
        Assert.Equal('0', new KeyboardEventArgs(Keys.NumPad0, KeyboardModifiers.Shift).Character);
        Assert.Equal('1', new KeyboardEventArgs(Keys.NumPad1).Character);
        Assert.Equal('2', new KeyboardEventArgs(Keys.NumPad2).Character);
        Assert.Equal('3', new KeyboardEventArgs(Keys.NumPad3).Character);
        Assert.Equal('4', new KeyboardEventArgs(Keys.NumPad4).Character);
        Assert.Equal('5', new KeyboardEventArgs(Keys.NumPad5).Character);
        Assert.Equal('6', new KeyboardEventArgs(Keys.NumPad6).Character);
        Assert.Equal('7', new KeyboardEventArgs(Keys.NumPad7).Character);
        Assert.Equal('8', new KeyboardEventArgs(Keys.NumPad8).Character);
        Assert.Equal('9', new KeyboardEventArgs(Keys.NumPad9).Character);
    }

    // ──────────────────────────── Character mapping: whitespace / control keys ────────────────────────────

    [Fact]
    public void Character_WhitespaceAndControlKeys()
    {
        Assert.Equal(' ', new KeyboardEventArgs(Keys.Space).Character);
        Assert.Equal('\t', new KeyboardEventArgs(Keys.Tab).Character);
        Assert.Equal((char)13, new KeyboardEventArgs(Keys.Enter).Character);
        Assert.Equal((char)8, new KeyboardEventArgs(Keys.Back).Character);
    }

    // ──────────────────────────── Character mapping: numpad operators ────────────────────────────

    [Fact]
    public void Character_NumpadOperators()
    {
        Assert.Equal('+', new KeyboardEventArgs(Keys.Add).Character);
        Assert.Equal('.', new KeyboardEventArgs(Keys.Decimal).Character);
        Assert.Equal('/', new KeyboardEventArgs(Keys.Divide).Character);
        Assert.Equal('*', new KeyboardEventArgs(Keys.Multiply).Character);
        Assert.Equal('-', new KeyboardEventArgs(Keys.Subtract).Character);
    }

    // ──────────────────────────── Character mapping: OEM keys (shift-sensitive) ────────────────────────────

    [Fact]
    public void Character_OemKeys_DefaultAndShift()
    {
        Assert.Equal('\\', new KeyboardEventArgs(Keys.OemBackslash).Character);

        Assert.Equal(',', new KeyboardEventArgs(Keys.OemComma).Character);
        Assert.Equal('<', new KeyboardEventArgs(Keys.OemComma, KeyboardModifiers.Shift).Character);

        Assert.Equal('[', new KeyboardEventArgs(Keys.OemOpenBrackets).Character);
        Assert.Equal('{', new KeyboardEventArgs(Keys.OemOpenBrackets, KeyboardModifiers.Shift).Character);

        Assert.Equal(']', new KeyboardEventArgs(Keys.OemCloseBrackets).Character);
        Assert.Equal('}', new KeyboardEventArgs(Keys.OemCloseBrackets, KeyboardModifiers.Shift).Character);

        Assert.Equal('.', new KeyboardEventArgs(Keys.OemPeriod).Character);
        Assert.Equal('>', new KeyboardEventArgs(Keys.OemPeriod, KeyboardModifiers.Shift).Character);

        Assert.Equal('\\', new KeyboardEventArgs(Keys.OemPipe).Character);
        Assert.Equal('|', new KeyboardEventArgs(Keys.OemPipe, KeyboardModifiers.Shift).Character);

        Assert.Equal('=', new KeyboardEventArgs(Keys.OemPlus).Character);
        Assert.Equal('+', new KeyboardEventArgs(Keys.OemPlus, KeyboardModifiers.Shift).Character);

        Assert.Equal('-', new KeyboardEventArgs(Keys.OemMinus).Character);
        Assert.Equal('_', new KeyboardEventArgs(Keys.OemMinus, KeyboardModifiers.Shift).Character);

        Assert.Equal('/', new KeyboardEventArgs(Keys.OemQuestion).Character);
        Assert.Equal('?', new KeyboardEventArgs(Keys.OemQuestion, KeyboardModifiers.Shift).Character);

        Assert.Equal('\'', new KeyboardEventArgs(Keys.OemQuotes).Character);
        Assert.Equal('"', new KeyboardEventArgs(Keys.OemQuotes, KeyboardModifiers.Shift).Character);

        Assert.Equal(';', new KeyboardEventArgs(Keys.OemSemicolon).Character);
        Assert.Equal(':', new KeyboardEventArgs(Keys.OemSemicolon, KeyboardModifiers.Shift).Character);

        Assert.Equal('`', new KeyboardEventArgs(Keys.OemTilde).Character);
        Assert.Equal('~', new KeyboardEventArgs(Keys.OemTilde, KeyboardModifiers.Shift).Character);
    }

    // ──────────────────────────── Character mapping: non-printable keys → null ────────────────────────────

    [Theory]
    [InlineData(Keys.Up)]
    [InlineData(Keys.Down)]
    [InlineData(Keys.Left)]
    [InlineData(Keys.Right)]
    [InlineData(Keys.F1)]
    [InlineData(Keys.F12)]
    [InlineData(Keys.Escape)]
    [InlineData(Keys.PageUp)]
    public void Character_NonPrintableKeys_ReturnNull(Keys key)
    {
        Assert.Null(new KeyboardEventArgs(key).Character);
    }
}
