using Concentus.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.VoiceNext.Codec
{
    //Based on https://github.com/lostromb/concentus/blob/master/CSharp/ConcentusDemo/ConcentusCodec.cs and DSharpPlus's original OpusCodec file
    internal class OpusTestCodec
    {
        private readonly OpusEncoder _encoder;
        private readonly OpusDecoder _decoder;
        private readonly int _sample_rate;
        
        internal OpusTestCodec(int sample_rate, int channels, VoiceApplication application)
        {
            _encoder = new OpusEncoder(sample_rate, channels, (Concentus.Enums.OpusApplication)(int)application);
            _decoder = new OpusDecoder(sample_rate, channels);
            
            _sample_rate = sample_rate;
        }

        public byte[] Encode(byte[] buffer, int offset, int count, int bitrate = 16)
        {
            short[] data = BytesToShorts(buffer);
            int frame_size = FrameCount(count, bitrate);
            byte[] output = new byte[10000];
            int encoded_size = _encoder.Encode(data, offset, frame_size, output, 0, output.Length);
            byte[] finalOutput = new byte[encoded_size];
            Array.Copy(output, 0, finalOutput, 0, finalOutput.Length);
            return finalOutput;
        }

        public byte[] Decode(byte[] buffer, int offset, int count, int bitrate = 16)
        {
            int frame_size = FrameCount(count, bitrate);
            short[] outputBuffer = new short[frame_size];
            _decoder.Decode(buffer, offset, count, outputBuffer, 0, frame_size, false);
            
            return ShortsToBytes(outputBuffer);
        }

        private int FrameCount(int length, int bitRate)
        {
            var bps = (bitRate >> 2) & ~1; // (bitrate / 8) * 2;
            return length / bps;
        }

        /// <summary>
        /// Converts interleaved byte samples (such as what you get from a capture device)
        /// into linear short samples (that are much easier to work with)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static short[] BytesToShorts(byte[] input)
        {
            return BytesToShorts(input, 0, input.Length);
        }

        /// <summary>
        /// Converts interleaved byte samples (such as what you get from a capture device)
        /// into linear short samples (that are much easier to work with)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static short[] BytesToShorts(byte[] input, int offset, int length)
        {
            short[] processedValues = new short[length / 2];
            for (int c = 0; c < processedValues.Length; c++)
            {
                processedValues[c] = (short)(((int)input[(c * 2) + offset]) << 0);
                processedValues[c] += (short)(((int)input[(c * 2) + 1 + offset]) << 8);
            }

            return processedValues;
        }

        /// <summary>
        /// Converts linear short samples into interleaved byte samples, for writing to a file, waveout device, etc.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static byte[] ShortsToBytes(short[] input)
        {
            return ShortsToBytes(input, 0, input.Length);
        }

        /// <summary>
        /// Converts linear short samples into interleaved byte samples, for writing to a file, waveout device, etc.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static byte[] ShortsToBytes(short[] input, int offset, int length)
        {
            byte[] processedValues = new byte[length * 2];
            for (int c = 0; c < length; c++)
            {
                processedValues[c * 2] = (byte)(input[c + offset] & 0xFF);
                processedValues[c * 2 + 1] = (byte)((input[c + offset] >> 8) & 0xFF);
            }

            return processedValues;
        }
    }
}
