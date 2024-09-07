namespace EmptyKeys.UserInterface.Media
{
    /// <summary>
    /// Implements Stride specific audio device
    /// </summary>
    public class StrideAudioDevice : AudioDevice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StrideAudioDevice"/> class.
        /// </summary>
        public StrideAudioDevice()
            : base()
        {
        }

        /// <summary>
        /// Creates the sound.
        /// </summary>
        /// <param name="nativeSound">The native sound.</param>
        /// <returns></returns>
        public override SoundBase CreateSound(object nativeSound)
        {
            return new StrideSound(nativeSound);
        }
    }
}
