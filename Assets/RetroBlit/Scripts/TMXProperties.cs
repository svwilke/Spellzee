using System.Collections.Generic;
using UnityEngine;

/*********************************************************************************
* The comments in this file are used to generate the API documentation. Please see
* Assets/RetroBlit/Docs for much easier reading!
*********************************************************************************/

/// <summary>
/// Contains TMX Properties and methods to access them
/// </summary>
/// <remarks>
/// Contains TMX Properties and methods to access them.
/// </remarks>
public class TMXProperties
{
    /// <summary>
    /// Dictionary containing all string values
    /// </summary>
    protected Dictionary<string, string> mStrings = null;

    /// <summary>
    /// Dictionary containing all boolean values
    /// </summary>
    protected Dictionary<string, bool> mBooleans = null;

    /// <summary>
    /// Dictionary containing all integer values
    /// </summary>
    protected Dictionary<string, int> mIntegers = null;

    /// <summary>
    /// Dictionary containing all floats values
    /// </summary>
    protected Dictionary<string, float> mFloats = null;

    /// <summary>
    /// Dictionary containing all color values
    /// </summary>
    protected Dictionary<string, Color32> mColors = null;

    /// <summary>
    /// Add a new property
    /// </summary>
    /// <remarks>
    /// Add a new property to this property collection.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    public void Add(string key, string value)
    {
        if (mStrings == null)
        {
            mStrings = new Dictionary<string, string>();
        }

        mStrings[key] = value;
    }

    /// <summary>
    /// Add a new boolean property
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    public void Add(string key, bool value)
    {
        if (mBooleans == null)
        {
            mBooleans = new Dictionary<string, bool>();
        }

        mBooleans[key] = value;
    }

    /// <summary>
    /// Add a new integer property
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    public void Add(string key, int value)
    {
        if (mIntegers == null)
        {
            mIntegers = new Dictionary<string, int>();
        }

        mIntegers[key] = value;
    }

    /// <summary>
    /// Add a new float property
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    public void Add(string key, float value)
    {
        if (mFloats == null)
        {
            mFloats = new Dictionary<string, float>();
        }

        mFloats[key] = value;
    }

    /// <summary>
    /// Add a new color property
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    public void Add(string key, Color32 value)
    {
        if (mColors == null)
        {
            mColors = new Dictionary<string, Color32>();
        }

        mColors[key] = value;
    }

    /// <summary>
    /// Get a string property by its key
    /// </summary>
    /// <remarks>
    /// Get a string property by its key.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    /// <param name="key">Key</param>
    /// <returns>String value, or null if not found</returns>
    public string GetString(string key)
    {
        if (mStrings == null)
        {
            return null;
        }

        return mStrings[key];
    }

    /// <summary>
    /// Get a boolean property by its key
    /// </summary>
    /// <remarks>
    /// Get a boolean property by its key.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    /// <param name="key">Key</param>
    /// <returns>Boolean value, or false if not found</returns>
    public bool GetBool(string key)
    {
        if (mBooleans == null)
        {
            return false;
        }

        return mBooleans[key];
    }

    /// <summary>
    /// Get an integer property by its key
    /// </summary>
    /// <remarks>
    /// Get an integer property by its key.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    /// <param name="key">Key</param>
    /// <returns>Integer value, or 0 if not found</returns>
    public int GetInt(string key)
    {
        if (mIntegers == null)
        {
            return 0;
        }

        return mIntegers[key];
    }

    /// <summary>
    /// Get a float property by its key
    /// </summary>
    /// <remarks>
    /// Get a float property by its key.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    /// <param name="key">Key</param>
    /// <returns>Float value, or 0 if not found</returns>
    public float GetFloat(string key)
    {
        if (mFloats == null)
        {
            return 0;
        }

        return mFloats[key];
    }

    /// <summary>
    /// Get a color property by its key
    /// </summary>
    /// <remarks>
    /// Get a color property by its key.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    /// <param name="key">Key</param>
    /// <returns>Color value, or <see cref="Color32.black"/> if not found</returns>
    public Color32 GetColor(string key)
    {
        if (mColors == null)
        {
            return Color.black;
        }

        return mColors[key];
    }
}
