#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// Provides methods and properties for capturing and processing voice data
    /// using the Steamworks Voice API functionality.
    /// </summary>
    public static class Voice
    {
        /// <summary>
        /// Provides methods for interacting with the Steamworks Voice API, allowing
        /// voice data management such as starting and stopping recording, retrieving
        /// available voice data, and decompressing recorded audio.
        /// </summary>
        public static class Client
        {
            /// <summary>
            /// Represents the optimal sample rate for processing Steam voice data, as determined
            /// by the Steamworks Voice API. Using this sample rate with voice data decompression
            /// can minimize CPU usage during audio processing.
            /// </summary>
            /// <remarks>
            /// This value reflects the native sample rate of the Steam voice decompressor. For best
            /// performance, it is recommended to use this rate when calling DecompressVoice; however,
            /// actual audio quality may vary depending on the capabilities of the device or audio output
            /// system being used. In some cases, better results may be achieved using the native sample
            /// rate of the audio output device (e.g., 48000 Hz or 44100 Hz).
            /// </remarks>
            public static uint OptimalSampleRate => SteamUser.GetVoiceOptimalSampleRate();

            /// <summary>
            /// Decodes the compressed voice data and retrieves the raw audio for playback or further processing.
            /// </summary>
            /// <param name="compressedData">The compressed voice data obtained from the GetVoice function.</param>
            /// <param name="resultBuffer">The buffer where the decompressed raw audio data will be stored.</param>
            /// <param name="resultsWrittenSize">The size of the decompressed audio written to the result buffer, or the required buffer size if the given buffer is too small.</param>
            /// <param name="desiredSampleRate">The sample rate of the decompressed audio, ranging from 11025 to 48000. Use GetVoiceOptimalSampleRate to determine the suggested sample rate.</param>
            /// <returns>An EVoiceResult indicating the status of the decompression operation.</returns>
            public static EVoiceResult DecompressVoice(byte[] compressedData, byte[] resultBuffer,
                out uint resultsWrittenSize, uint desiredSampleRate) => SteamUser.DecompressVoice(compressedData,
                (uint)compressedData.Length, resultBuffer, (uint)resultBuffer.Length, out resultsWrittenSize,
                desiredSampleRate);

            /// <summary>
            /// Checks if there is captured audio data available for retrieval and provides the size of the available compressed voice data.
            /// </summary>
            /// <param name="pcbCompressed">Outputs the size of the available compressed voice data in bytes.</param>
            /// <returns>An EVoiceResult indicating the result of the operation, such as success or error conditions.</returns>
            public static EVoiceResult GetAvailableVoice(out uint pcbCompressed) =>
                SteamUser.GetAvailableVoice(out pcbCompressed);

            /// <summary>
            /// Reads the captured audio data from the microphone buffer.
            /// </summary>
            /// <param name="pDestBuffer">The buffer where the audio data will be copied into.</param>
            /// <param name="nBytesWritten">The number of bytes written into the destination buffer. This value should match the size specified by GetAvailableVoice.</param>
            /// <returns>An EVoiceResult indicating the status of the voice data retrieval operation.</returns>
            public static EVoiceResult GetVoice(byte[] pDestBuffer, out uint nBytesWritten) =>
                SteamUser.GetVoice(true, pDestBuffer, (uint)pDestBuffer.Length, out nBytesWritten);

            /// <summary>
            /// Initiates voice recording using the Steamworks Voice API.
            /// Once started, recorded voice data can be retrieved by using the GetVoice method.
            /// </summary>
            public static void StartRecording() => SteamUser.StartVoiceRecording();

            /// <summary>
            /// Stops voice recording for the current session.
            /// Although the recording stops, the system may retain a short buffer period.
            /// It is recommended to continue calling GetVoice until it indicates that no more data is available.
            /// </summary>
            public static void StopRecording() => SteamUser.StopVoiceRecording();
        }
    }
}
#endif