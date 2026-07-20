using System.Collections.Generic;

namespace CoreEssentials.GUI.Types;

/// <summary>
/// Grid layout container interface for organizing widgets in rows and columns.
/// </summary>
public interface IGrid : IContainer
{
    /// <summary>
    /// Gets the collection of row proportion values for this grid.
    /// Values represent relative sizing weights (e.g., 1, 2, 1 means middle row is twice as tall).
    /// </summary>
    IList<float> RowProportions { get; }

    /// <summary>
    /// Gets the collection of column proportion values for this grid.
    /// Values represent relative sizing weights (e.g., 1, 2 means second column is twice as wide).
    /// </summary>
    IList<float> ColumnProportions { get; }

    /// <summary>
    /// Gets or sets the spacing between rows in pixels.
    /// </summary>
    float RowSpacing { get; set; }

    /// <summary>
    /// Gets or sets the spacing between columns in pixels.
    /// </summary>
    float ColumnSpacing { get; set; }

    /// <summary>
    /// Sets the row index for a widget within this grid layout.
    /// </summary>
    void SetRow(IWidget widget, int rowIndex);

    /// <summary>
    /// Sets the column index for a widget within this grid layout.
    /// </summary>
    void SetColumn(IWidget widget, int columnIndex);

    /// <summary>
    /// Gets the row index of a widget within this grid layout.
    /// </summary>
    /// <param name="widget">The widget to query.</param>
    /// <returns>The row index, or -1 if not set.</returns>
    int GetRow(IWidget widget);

    /// <summary>
    /// Gets the column index of a widget within this grid layout.
    /// </summary>
    /// <param name="widget">The widget to query.</param>
    /// <returns>The column index, or -1 if not set.</returns>
    int GetColumn(IWidget widget);
}
