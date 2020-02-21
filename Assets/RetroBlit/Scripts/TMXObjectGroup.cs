using System.Collections.Generic;
using UnityEngine;

/*********************************************************************************
* The comments in this file are used to generate the API documentation. Please see
* Assets/RetroBlit/Docs for much easier reading!
*********************************************************************************/

/// <summary>
/// A definition of a TMX object group
/// </summary>
/// <remarks>
/// A definition of a TMX object group. Object groups are loaded and stored in a <see cref="TMXMap"/>.
/// </remarks>
public class TMXObjectGroup
{
    /// <summary>
    /// Name of the object group
    /// </summary>
    protected string mName;

    /// <summary>
    /// Color of the objects in this group
    /// </summary>
    protected Color32 mColor;

    /// <summary>
    /// Alpha transparency of the group
    /// </summary>
    protected byte mAlpha = 255;

    /// <summary>
    /// Visible flag of the group
    /// </summary>
    protected bool mVisible = true;

    /// <summary>
    /// Pixel offset of the group
    /// </summary>
    protected Vector2i mOffset = Vector2i.zero;

    /// <summary>
    /// List of objects in the group
    /// </summary>
    protected List<TMXObject> mObjects = new List<TMXObject>();

    /// <summary>
    /// Custom properties of the group
    /// </summary>
    protected TMXProperties mProperties = new TMXProperties();

    /// <summary>
    /// Name of the object group
    /// </summary>
    /// <remarks>
    /// Name of the object group.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    /// <seealso cref="TMXObject"/>
    public string name
    {
        get { return mName; }
    }

    /// <summary>
    /// Color used for objects in this group
    /// </summary>
    /// <remarks>
    /// The color used for the objects in this group, as specified in Tiled editor.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    /// <seealso cref="TMXObject"/>
    public Color32 color
    {
        get { return mColor; }
    }

    /// <summary>
    /// Alpha transparency of the objects in this group.
    /// </summary>
    /// <remarks>
    /// The alpha transparency of the objects in this group. The alpha values of any parent Tiled Groups are multiplied into this alpha value.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    /// <seealso cref="TMXObject"/>
    public byte alpha
    {
        get { return mAlpha; }
    }

    /// <summary>
    /// Visibility flag.
    /// </summary>
    /// <remarks>
    /// The visibility flag of the objects in this group. If any Tiled Group parent of this object group has visibility flag off then this visible flag will also be off.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    /// <seealso cref="TMXObject"/>
    public bool visible
    {
        get { return mVisible; }
    }

    /// <summary>
    /// Offset of the object group in pixels.
    /// </summary>
    /// <remarks>
    /// Offset of the object group in pixels. If this object group is a child of Tiled Groups then the offset of those parents is merged into this offset.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    /// <seealso cref="TMXObject"/>
    public Vector2i offset
    {
        get { return mOffset; }
    }

    /// <summary>
    /// List of all objects in this group
    /// </summary>
    /// <remarks>
    /// List of all objects in this group.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    /// <seealso cref="TMXObject"/>
    public List<TMXObject> objects
    {
        get { return mObjects; }
    }

    /// <summary>
    /// Custom properties for this object group
    /// </summary>
    /// <remarks>
    /// A collection of all custom properties defined for this group.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    /// <seealso cref="TMXProperties"/>
    public TMXProperties properties
    {
        get { return mProperties; }
    }
}