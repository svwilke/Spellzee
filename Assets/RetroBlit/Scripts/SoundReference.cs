/*********************************************************************************
* The comments in this file are used to generate the API documentation. Please see
* Assets/RetroBlit/Docs for much easier reading!
*********************************************************************************/

/// <summary>
/// A reference to currently playing sound
/// </summary>
/// <remarks>
/// A reference to currently playing sound. This structure is returned by <see cref = "RB.SoundPlay"/> and can be used with
/// <see cref="RB.SoundStop"/>, <see cref="RB.SoundVolumeSet"/>, <see cref="RB.SoundVolumeGet"/>, <see cref="RB.SoundPitchSet"/>
/// and <see cref="RB.SoundPitchGet"/> to manage the sound as it plays.
/// </remarks>
public struct SoundReference
{
    private int mSoundChannel;
    private long mSequence;

    /// <summary>
    /// Sound reference constructor
    /// </summary>
    /// <remarks>
    /// There is no reason to construct a SoundReference, it's usually constructed by RetroBlit and used to refer to already playing sounds.
    /// <seedoc>Features:Sound</seedoc>
    /// </remarks>
    /// <param name="channel">Sound channel</param>
    /// <param name="seq">Sound sequence</param>
    public SoundReference(int channel, long seq)
    {
        mSoundChannel = channel;
        mSequence = seq;
    }

    /// <summary>
    /// Sound channel
    /// </summary>
    /// <remarks>
    /// Refers to the channel the sound is currently playing on. Used internally by RetroBlit.
    /// </remarks>
    public int SoundChannel
    {
        get
        {
            return mSoundChannel;
        }
    }

    /// <summary>
    /// Sound sequence.
    /// </summary>
    /// <remarks>
    /// Refers to the sequence of the sound used internally by RetroBlit.
    /// </remarks>
    public long Sequence
    {
        get
        {
            return mSequence;
        }
    }
}
