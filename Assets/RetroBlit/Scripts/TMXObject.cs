using System.Collections.Generic;
using UnityEngine;

/*********************************************************************************
* The comments in this file are used to generate the API documentation. Please see
* Assets/RetroBlit/Docs for much easier reading!
*********************************************************************************/

/// <summary>
/// A definition of a TMX object
/// </summary>
/// <remarks>
/// A definition of a TMX object. TMX objects are geometric shapes and can be used for a variety of things. For example a rectangular
/// shape could be used to outline a trigger area of some event in the game.
/// </remarks>
public class TMXObject
{
    /// <summary>
    /// Name of the object
    /// </summary>
    protected string mName;

    /// <summary>
    /// Type of the object
    /// </summary>
    /// <remarks>This is the type property as defined by Tiled, it is not the same as the Shape of the object</remarks>
    protected string mType;

    /// <summary>
    /// Shape of the object
    /// </summary>
    protected Shape mShape = Shape.Rectangle;

    /// <summary>
    /// Rectangular area of the object
    /// </summary>
    protected Rect2i mRect = new Rect2i();

    /// <summary>
    /// Rotation of the object in clock-wise degrees
    /// </summary>
    protected float mRotation = 0;

    /// <summary>
    /// Visible flag of the object
    /// </summary>
    protected bool mVisible = true;

    /// <summary>
    /// Points in the object
    /// </summary>
    protected List<Vector2i> mPoints = new List<Vector2i>();

    /// <summary>
    /// Custom properties of the object
    /// </summary>
    protected TMXProperties mProperties = new TMXProperties();

    /// <summary>
    /// Shape of the TMX object
    /// </summary>
    /// <remarks>
    /// Shape of a TMX object.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    public enum Shape
    {
        /// <summary>
        /// Rectangle shape.
        /// </summary>
        /// <remarks>
        /// Rectangular shape defined by <see cref="TMXObject.rect"/>.
        /// <seedoc>Features:Tiled TMX Support</seedoc>
        /// </remarks>
        Rectangle,

        /// <summary>
        /// Ellipse shape.
        /// </summary>
        /// <remarks>
        /// Elliptical shape defined by <see cref="TMXObject.rect"/>
        /// <seedoc>Features:Tiled TMX Support</seedoc>
        /// </remarks>
        Ellipse,

        /// <summary>
        /// Point shape.
        /// </summary>
        /// <remarks>
        /// A point defined by <see cref="TMXObject.rect"/>.x, and <see cref="TMXObject.rect"/>.y.
        /// <seedoc>Features:Tiled TMX Support</seedoc>
        /// </remarks>
        Point,

        /// <summary>
        /// Polygon shape, made of connected points where first and last points are joined.
        /// </summary>
        /// <remarks>
        /// Polygon shape, made of connected points where first and last points are joined. The points are defined by <see cref="TMXObject.points"/>.
        /// <seedoc>Features:Tiled TMX Support</seedoc>
        /// </remarks>
        Polygon,

        /// <summary>
        /// Polyline shape made of connected points where first and last points are not joined.
        /// </summary>
        /// <remarks>
        /// Polyline shape made of connected points where first and last points are not joined. The points are defined by <see cref="TMXObject.points"/>.
        /// <seedoc>Features:Tiled TMX Support</seedoc>
        /// </remarks>
        Polyline,
    }

    /// <summary>
    /// Name of the object
    /// </summary>
    /// <remarks>
    /// Name of the object.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    public string name
    {
        get { return mName; }
    }

    /// <summary>
    /// Type of the object as defined in Tiled
    /// </summary>
    /// <remarks>
    /// Type of the object as defined in Tiled editor.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    public string type
    {
        get { return mType; }
    }

    /// <summary>
    /// Shape of the object.
    /// </summary>
    /// <remarks>
    /// The shape of the object, one of <see cref="TMXObject.Shape"/>
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    public Shape shape
    {
        get { return mShape; }
    }

    /// <summary>
    /// Rectangular bounding area of the shape.
    /// </summary>
    /// <remarks>
    /// Rectangular area defining the object. For <mref refid="TMXObject.Shape.Point">TMXObject.Shape.Point</mref> only the x and y coordinates are valid.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    public Rect2i rect
    {
        get { return mRect; }
    }

    /// <summary>
    /// Rotation of the shape in degrees, clockwise
    /// </summary>
    /// <remarks>
    /// Rotation of the shape in degrees, clockwise
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    public float rotation
    {
        get { return mRotation; }
    }

    /// <summary>
    /// Visible flag
    /// </summary>
    /// <remarks>
    /// The visibility flag of the object.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    public bool visible
    {
        get { return mVisible; }
    }

    /// <summary>
    /// List of points that make up a <see cref="TMXObject.Shape.Polygon"/> or <see cref="TMXObject.Shape.Polyline"/>.
    /// </summary>
    /// <remarks>
    /// List of points that make up a <see cref="TMXObject.Shape.Polygon"/> or <see cref="TMXObject.Shape.Polyline"/>.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    public List<Vector2i> points
    {
        get { return mPoints; }
    }

    /// <summary>
    /// Custom properties of the shape
    /// </summary>
    /// <remarks>
    /// Collection of custom properties defined for this object.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    /// <seealso cref="TMXProperties"/>
    public TMXProperties properties
    {
        get { return mProperties; }
    }
}