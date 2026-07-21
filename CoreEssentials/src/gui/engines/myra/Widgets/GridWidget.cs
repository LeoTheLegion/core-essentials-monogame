using System.Collections;
using System.Collections.Generic;
using Myra.Graphics2D.UI;
using CoreEssentials.GUI.Types;
using MyraGrid = Myra.Graphics2D.UI.Grid;

namespace CoreEssentials.GUI.Engines.Myra.Widgets;

/// <summary>
/// Wrapper for a Myra Grid, implementing IGrid interface with static helper methods.
/// </summary>
public class GridWidget : WidgetBase, IGrid
{
    /// <inheritdoc />
    public IList<float> RowProportions => _rowProxies;

    /// <inheritdoc />
    public IList<float> ColumnProportions => _colProxies;

    /// <inheritdoc />
    public float RowSpacing
    {
        get => (float)Grid.RowSpacing;
        set => Grid.RowSpacing = (int)value;
    }

    /// <inheritdoc />
    public float ColumnSpacing
    {
        get => (float)Grid.ColumnSpacing;
        set => Grid.ColumnSpacing = (int)value;
    }

    private readonly List<float> _rowProxies;
    private readonly List<float> _colProxies;

    protected new MyraGrid Grid => (MyraGrid)base.MyraWidget;

    /// <inheritdoc />
    public IList<IWidget> Children
    {
        get
        {
            var result = new List<IWidget>();
            foreach (var w in Grid.Widgets)
            {
                if (w is global::Myra.Graphics2D.UI.Widget myra && WidgetWrapper.TryGetFromMyra(myra) is IWidget wrapper)
                    result.Add(wrapper);
            }
            return result;
        }
    }

    /// <inheritdoc />
    public IEnumerable<IWidget> Widgets => GetDescendants(this);

    /// <inheritdoc />
    public void AddChild(IWidget widget)
    {
        var myra = WidgetWrapper.Unwrap(widget);
        Grid.Widgets.Add(myra);
    }

    /// <inheritdoc />
    public void RemoveChild(IWidget widget)
    {
        var myra = WidgetWrapper.Unwrap(widget);
        Grid.Widgets.Remove(myra);
    }

    /// <inheritdoc />
    public void ClearChildren() => Grid.Widgets.Clear();

    /// <summary>
    /// Creates a default GridWidget. No parameters needed — uses Myra defaults.
    /// </summary>
    public GridWidget() : base(new MyraGrid())
    {
        _rowProxies = new List<float>();
        _colProxies = new List<float>();

        // Sync with the underlying grid's proportions collection
        Grid.RowsProportions.CollectionChanged += (_, _) => SyncRows();
        Grid.ColumnsProportions.CollectionChanged += (_, _) => SyncColumns();

        SyncRows();
        SyncColumns();
    }

    /// <inheritdoc />
    public void SetRow(IWidget widget, int rowIndex)
    {
        var myra = WidgetWrapper.Unwrap(widget);
        Grid.SetRow(myra, rowIndex);
    }

    /// <inheritdoc />
    public void SetColumn(IWidget widget, int columnIndex)
    {
        var myra = WidgetWrapper.Unwrap(widget);
        Grid.SetColumn(myra, columnIndex);
    }

    /// <inheritdoc />
    public int GetRow(IWidget widget) => Grid.GetRow(WidgetWrapper.Unwrap(widget));

    /// <inheritdoc />
    public int GetColumn(IWidget widget) => Grid.GetColumn(WidgetWrapper.Unwrap(widget));

    private void SyncRows()
    {
        _rowProxies.Clear();
        foreach (var p in Grid.RowsProportions)
            _rowProxies.Add(p.Value);
    }

    private void SyncColumns()
    {
        _colProxies.Clear();
        foreach (var p in Grid.ColumnsProportions)
            _colProxies.Add(p.Value);
    }

    private static IEnumerable<IWidget> GetDescendants(IWidget widget)
    {
        if (widget is IContainer container)
        {
            foreach (var child in container.Children)
            {
                yield return child;
                foreach (var descendant in GetDescendants(child))
                    yield return descendant;
            }
        }
    }
}
