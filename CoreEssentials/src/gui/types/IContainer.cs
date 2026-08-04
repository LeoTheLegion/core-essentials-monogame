using System.Collections.Generic;

namespace CoreEssentials.GUI.Types;

/// <summary>
/// Abstraction for widget containers that can hold child widgets.
/// </summary>
public interface IContainer : IWidget
{
    /// <summary>
    /// Gets the collection of direct child widgets in this container.
    /// </summary>
    IList<IWidget> Children { get; }

    /// <summary>
    /// Gets all descendant widgets recursively, including those nested within child containers.
    /// </summary>
    IEnumerable<IWidget> Widgets { get; }

    /// <summary>
    /// Adds a widget as a direct child of this container.
    /// </summary>
    /// <param name="widget">The widget to add.</param>
    void AddChild(IWidget widget);

    /// <summary>
    /// Removes a direct child widget from this container.
    /// </summary>
    /// <param name="widget">The widget to remove.</param>
    void RemoveChild(IWidget widget);

    /// <summary>
    /// Removes all direct child widgets from this container.
    /// </summary>
    void ClearChildren();
}
