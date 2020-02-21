using UnityEngine;

/*********************************************************************************
* The comments in this file are used to generate the API documentation. Please see
* Assets/RetroBlit/Docs for much easier reading!
*********************************************************************************/

/// <summary>
/// A definition of a TMX layer
/// </summary>
/// <remarks>
/// A definition of a TMX layer. These layers are stored in a loaded <see cref="TMXMap"/>.
/// <seedoc>Features:Tiled TMX Support</seedoc>
/// </remarks>
public class TMXLayer
{
    /// <summary>
    /// Size of layer in tiles
    /// </summary>
    protected Vector2i mSize;

    /// <summary>
    /// Pixel offset of layer
    /// </summary>
    protected Vector2i mOffset;

    /// <summary>
    /// Visibility flag of layer
    /// </summary>
    protected bool mVisible;

    /// <summary>
    /// Alpha transparency of layer
    /// </summary>
    protected byte mAlpha;

    /// <summary>
    /// Custom properties of layer
    /// </summary>
    protected TMXProperties mProperties = new TMXProperties();

    /// <summary>
    /// Size of layer in terms of tile count.
    /// </summary>
    /// <remarks>
    /// Size of layer in terms of tile count. If map is infinite this size is calculated from the minimum and maximum offsets between all chunks in the layer.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    public Vector2i size
    {
        get { return mSize; }
    }

    /// <summary>
    /// Offset of the layer in pixels.
    /// </summary>
    /// <remarks>
    /// Offset of the layer in pixels. If the layer is a child of Tiled Groups then the offset of those parents is merged into this offset.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    public Vector2i offset
    {
        get { return mOffset; }
    }

    /// <summary>
    /// Visibility flag.
    /// </summary>
    /// <remarks>
    /// Visibility flag of the layer. If any Tiled Group parent of this layer has visibility flag off then this visible flag will also be off.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    public bool visible
    {
        get { return mVisible; }
    }

    /// <summary>
    /// Alpha transparency of the layer
    /// </summary>
    /// <remarks>
    /// Alpha transparency of the layer. The alpha values of any parent Tiled Groups are multiplied into this alpha value.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    public byte alpha
    {
        get { return mAlpha; }
    }

    /// <summary>
    /// Custom properties of the layer
    /// </summary>
    /// <remarks>
    /// A collection of all the custom properties defined for this layer.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    /// <seealso cref="TMXProperties"/>
    public TMXProperties properties
    {
        get { return mProperties; }
    }
}
