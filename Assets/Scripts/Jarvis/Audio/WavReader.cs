using System;
using UnityEngine;

namespace Chimeradroid.Jarvis.Audio
{
    public static class WavReader
    {
        public static AudioClip ToAudioClip(byte[] wavBytes, string name = "jarvis-tts")
        {
            if (wavBytes == null || wavBytes.Length < 44)
            {
                throw new ArgumentException("Invalid WAV: too short");
            }

            int offset = 0;
            if (ReadU32(wavBytes, offset) != 0x46464952) // "RIFF"
            {
                throw new ArgumentException("Invalid WAV: missing RIFF");
            }

            offset += 8; // RIFF + size
            if (ReadU32(wavBytes, offset) != 0x45564157) // "WAVE"
            {
                throw new ArgumentException("Invalid WAV: missing WAVE");
            }

            offset += 4;
            ushort audioFormat = 0;
            ushort channels = 0;
            uint sampleRate = 0;
            ushort bitsPerSample = 0;
            int dataOffset = -1;
            int dataSize = -1;

            while (offset + 8 <= wavBytes.Length)
            {
                uint chunkId = ReadU32(wavBytes, offset);
                int chunkSize = (int)ReadU32(wavBytes, offset + 4);
                offset += 8;

                if (chunkSize < 0 || offset + chunkSize > wavBytes.Length)
                {
                    break;
                }

                if (chunkId == 0x20746D66) // "fmt "
                {
                    audioFormat = ReadU16(wavBytes, offset);
                    channels = ReadU16(wavBytes, offset + 2);
                    sampleRate = ReadU32(wavBytes, offset + 4);
                    bitsPerSample = ReadU16(wavBytes, offset + 14);
                }
                else if (chunkId == 0x61746164) // "data"
                {
                    dataOffset = offset;
                    dataSize = chunkSize;
                }

                offset += chunkSize;
                if ((offset & 1) == 1)
                {
                    offset += 1; // pad byte
                }

                if (dataOffset >= 0 && audioFormat != 0 && sampleRate != 0)
                {
                    break;
                }
            }

            if (audioFormat != 1)
            {
                throw new ArgumentException($"Unsupported WAV format: {audioFormat} (expected PCM=1)");
            }

            if (dataOffset < 0 || dataSize <= 0)
            {
                throw new ArgumentException("Invalid WAV: missing data chunk");
            }

            if (channels == 0 || sampleRate == 0 || bitsPerSample == 0)
            {
                throw new ArgumentException("Invalid WAV: missing fmt values");
            }

            if (bitsPerSample != 16)
            {
                throw new ArgumentException($"Unsupported WAV bits: {bitsPerSample} (expected 16)");
            }

            int bytesPerSampleFrame = channels * (bitsPerSample / 8);
            int availableBytes = Math.Min(dataSize, wavBytes.Length - dataOffset);
            int sampleFrames = availableBytes / bytesPerSampleFrame;
            var samples = new float[sampleFrames * channels];

            int p = dataOffset;
            int s = 0;
            int end = dataOffset + sampleFrames * bytesPerSampleFrame;
            for (int i = 0; i < sampleFrames * channels && p + 1 < wavBytes.Length && p < end; i++)
            {
                short v = (short)(wavBytes[p] | (wavBytes[p + 1] << 8));
                samples[s++] = v / 32768f;
                p += 2;
            }

            var clip = AudioClip.Create(name, sampleFrames, channels, (int)sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static ushort ReadU16(byte[] b, int o)
        {
            return (ushort)(b[o] | (b[o + 1] << 8));
        }

        private static uint ReadU32(byte[] b, int o)
        {
            return (uint)(b[o] | (b[o + 1] << 8) | (b[o + 2] << 16) | (b[o + 3] << 24));
        }
    }
}

