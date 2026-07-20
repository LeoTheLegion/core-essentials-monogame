using System.Collections.Generic;
using Myra.Graphics2D.UI;
using CoreEssentials.GUI.Types;

namespace CoreEssentials.Engines.Myra.Widgets;

/// <summary>
/// Wrapper for a Myra Grid, implementing IGrid interface with static helper methods.
/// </summary>
public class GridWidget : ContainerWidget, IGrid
{
    /// <inheritdoc />
    public IList<float> RowProportions => _rowProxies;

    /// <inheritdoc />
    public IList<float> ColumnProportions => _colProxies;

    /// <inheritdoc />
    public float RowSpacing
    {
        get => Grid.RowSpacing;
        set => Grid.RowSpacing = value;
    }

    /// <inheritdoc />
    public float ColumnSpacing
    {
        get => Grid.ColumnSpacing;
        set => Grid.ColumnSpacing = value;
    }

    private readonly List<float> _rowProxies;
    private readonly List<float> _colProxies;

    protected new Grid Grid => (Grid)MyraWidget;

    /// <summary>
    /// Creates a default GridWidget. No parameters needed — uses Myra defaults.
    /// </summary>
    public GridWidget() : base(new Grid())
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
        var myra = Unwrap(widget);
        Grid.SetRow(myra, rowIndex);
    }

    /// <inheritdoc />
    public void SetColumn(IWidget widget, int columnIndex)
    {
        var myra = Unwrap(widget);
        Grid.SetColumn(myra, columnIndex);
    }

    /// <inheritdoc />
    public int GetRow(IWidget widget) => Grid.GetRow(Unwrap(widget));

    /// <inheritdoc />
    public int GetColumn(IWidget widget) => Grid.GetColumn(Unwrap(widget));

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
}
