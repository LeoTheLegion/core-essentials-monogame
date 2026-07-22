using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreEssentials.GUI.Types;
using CoreEssentials.GUI.Engines.Myra.Widgets;
using Desktop = global::Myra.Graphics2D.UI.Desktop;
using Panel = global::Myra.Graphics2D.UI.Panel;
using Container = global::Myra.Graphics2D.UI.Container;
using MyraEnv = global::Myra.MyraEnvironment;

namespace CoreEssentials.GUI.Engines.Myra;

/// <summary>
/// Myra-based implementation of IGuiManager. Wraps a single Desktop instance and delegates all operations to Myra.
/// </summary>
public class GuiManagerImpl : IGuiManager
{
    private Desktop? _desktop;
    private Panel? _rootPanel;

    /// <inheritdoc />
    public int Width => _rootPanel?.Width ?? 0;

    /// <inheritdoc />
    public int Height => _rootPanel?.Height ?? 0;

    /// <inheritdoc />
    public void Init(Game game, int width, int height)
    {
        MyraEnv.Game = game;

        _rootPanel = new Panel();
        _rootPanel.Width = width;
        _rootPanel.Height = height;

        _desktop = new Desktop();
        _desktop.Root = _rootPanel;
    }

    /// <inheritdoc />
    public void Shutdown()
    {
        if (_rootPanel != null)
        {
            _rootPanel.Widgets.Clear();
        }

        _desktop?.Dispose();
        _desktop = null;
        _rootPanel = null;
    }

    /// <inheritdoc />
    public void AddWidget(IWidget widget)
    {
        EnsureRootExists();
        var myra = WidgetWrapper.Unwrap(widget);
        _rootPanel!.Widgets.Add(myra);
    }

    /// <inheritdoc />
    public void RemoveWidget(IWidget widget)
    {
        if (_rootPanel == null) return;
        var myra = WidgetWrapper.Unwrap(widget);
        _rootPanel.Widgets.Remove(myra);
    }

    /// <inheritdoc />
    public void Draw(GameTime gameTime)
    {
        _desktop?.Render();
    }

    /// <inheritdoc />
    public bool IsAnyWidgetFocused()
    {
        if (_rootPanel == null) return false;

        for (int i = 0; i < _rootPanel.Widgets.Count; i++)
        {
            if (_IsWidgetFocused(_rootPanel.Widgets[i]))
                return true;
        }

        return false;
    }

    /// <inheritdoc />
    public bool IsWidgetFocused(IWidget? widget)
    {
        if (widget == null || _rootPanel == null) return false;
        var myra = WidgetWrapper.Unwrap(widget);
        return _IsWidgetFocused(myra);
    }

    /// <inheritdoc />
    public void SetDesktop(object desktop)
    {
        if (desktop is Desktop d)
            _desktop = d;
    }

    /// <inheritdoc />
    public IPanel? GetRootPanel()
    {
        if (_rootPanel == null) return null;
        return WidgetWrapper.TryGetFromMyra(_rootPanel) as IPanel;
    }

    private void EnsureRootExists()
    {
        if (_rootPanel == null)
            throw new InvalidOperationException("GuiManager has not been initialized. Call Init first.");
    }

    private bool _IsWidgetFocused(global::Myra.Graphics2D.UI.Widget widget)
    {
        if (widget == null) return false;

        if (widget is Container container)
        {
            for (int i = 0; i < container.Widgets.Count; i++)
            {
                var w = container.Widgets[i];
                if (_IsWidgetFocused(w)) return true;
            }
        }

        if (widget is global::Myra.Graphics2D.UI.ContentControl cc)
        {
            if (cc.IsMouseInside || cc.IsTouchInside || cc.IsKeyboardFocused)
                return true;

            var content = cc.Content as global::Myra.Graphics2D.UI.Widget;
            if (content != null && _IsWidgetFocused(content))
                return true;
        }

        if (widget is global::Myra.Graphics2D.UI.ComboView comboView)
        {
            return comboView.ListView.IsMouseInside || comboView.ListView.IsTouchInside;
        }

        return widget.IsMouseInside || widget.IsTouchInside || widget.IsKeyboardFocused;
    }
}
