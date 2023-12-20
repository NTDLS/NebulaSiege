﻿using NTDLS.Semaphore;
using NTDLS.PacketFraming.Payloads;
using NTDLS.PacketFraming.Payloads.Concrete;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static NTDLS.PacketFraming.Defaults;
using System.Net.Sockets;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net;

namespace NTDLS.PacketFraming
{
    /// <summary>
    /// Stream packets (especially TCP/IP) can be fragmented or combined. Framing rebuilds what was
    /// originally written to the stream while also providing compression, CRC checking and optional encryption.
    /// </summary>
    public static class Framing
    {
        /// <summary>
        /// The callback that is used to notify of the receipt of a notification frame.
        /// </summary>
        /// <param name="payload">The notification payload.</param>
        public delegate void ProcessFrameNotificationCallback(IFramePayloadNotification payload);

        private static readonly PessimisticSemaphore<Dictionary<string, MethodInfo>> _reflectioncache = new();


        #region Extension methods.

        /// <summary>
        /// Waits on bytes to become available on the stream, reads those bytes then parses the available frames (if any) and calls the appropriate callbacks.
        /// </summary>
        /// <param name="stream">The open stream that should be read from</param>
        /// <param name="frameBuffer">The frame buffer that will be used to receive bytes from the stream.</param>
        /// <param name="processNotificationCallback">Optional callback to call when a notification frame is received.</param>
        /// <returns>Returns true if the stream is healthy, returns false if disconnected.</returns>
        /// <exception cref="Exception"></exception>
        public static bool ReadAndProcessFrames(this UdpClient udpClient, ref IPEndPoint endPoint, FrameBuffer frameBuffer,
            ProcessFrameNotificationCallback? processNotificationCallback = null)
        {
            if (udpClient == null)
            {
                throw new Exception("ReadAndProcessFrames: client can not be null.");
            }

            var data = udpClient.Receive(ref endPoint);
            if (data.Length == 0)
            {
                return false;
            }

            if (frameBuffer.ReadStream(data))
            {
                ProcessFrameBuffer(frameBuffer, processNotificationCallback);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sends a one-time fire-and-forget notification to the stream.
        /// </summary>
        /// <param name="stream">The open stream that will be written to.</param>
        /// <param name="framePayload">The notification payload that will be written to the stream.</param>
        /// <exception cref="Exception"></exception>
        public static void WriteNotificationFrame(this UdpClient udpClient, string ipAddress, int port, IFramePayloadNotification framePayload)
        {
            var frameBody = new FrameBody(framePayload);

            var frameBytes = AssembleFrame(frameBody);

            udpClient.Send(frameBytes, frameBytes.Length, ipAddress, port);
        }

        /// <summary>
        /// Sends a one-time fire-and-forget byte array payload. These are and handled in processNotificationCallback().
        /// When a raw byte array is use, all json serilization is skipped and checks for this payload type are prioritized for performance.
        /// </summary>
        /// <param name="stream">The open stream that will be written to.</param>
        /// <param name="framePayload">The bytes will make up the body of the frame which is written to the stream.</param>
        /// <exception cref="Exception"></exception>
        public static void WriteBytesFrame(this UdpClient udpClient, string ipAddress, int port, byte[] framePayload)
        {
            if (udpClient == null)
            {
                throw new Exception("WriteBytesFrame: client can not be null.");
            }

            var frameBody = new FrameBody(framePayload);
            var frameBytes = AssembleFrame(frameBody);
            udpClient.Send(frameBytes, frameBytes.Length, ipAddress, port);
        }

        #endregion

        private static byte[] AssembleFrame(FrameBody frameBody)
        {
            var FrameBodyBytes = Utility.SerializeToByteArray(frameBody);
            var compressedFrameBodyBytes = Utility.Compress(FrameBodyBytes);

            var grossFrameSize = compressedFrameBodyBytes.Length + NtFrameDefaults.FRAME_HEADER_SIZE;
            var grossFrameBytes = new byte[grossFrameSize];
            var frameCrc = CRC16.ComputeChecksum(compressedFrameBodyBytes);

            Buffer.BlockCopy(BitConverter.GetBytes(NtFrameDefaults.FRAME_DELIMITER), 0, grossFrameBytes, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(grossFrameSize), 0, grossFrameBytes, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(frameCrc), 0, grossFrameBytes, 8, 2);
            Buffer.BlockCopy(compressedFrameBodyBytes, 0, grossFrameBytes, NtFrameDefaults.FRAME_HEADER_SIZE, compressedFrameBodyBytes.Length);

            return grossFrameBytes;
        }

        private static void SkipFrame(ref FrameBuffer frameBuffer)
        {
            var frameDelimiterBytes = new byte[4];

            for (int offset = 1; offset < frameBuffer.FrameBuilderLength - frameDelimiterBytes.Length; offset++)
            {
                Buffer.BlockCopy(frameBuffer.FrameBuilder, offset, frameDelimiterBytes, 0, frameDelimiterBytes.Length);

                var value = BitConverter.ToInt32(frameDelimiterBytes, 0);

                if (value == NtFrameDefaults.FRAME_DELIMITER)
                {
                    Buffer.BlockCopy(frameBuffer.FrameBuilder, offset, frameBuffer.FrameBuilder, 0, frameBuffer.FrameBuilderLength - offset);
                    frameBuffer.FrameBuilderLength -= offset;
                    return;
                }
            }
            Array.Clear(frameBuffer.FrameBuilder, 0, frameBuffer.FrameBuilder.Length);
            frameBuffer.FrameBuilderLength = 0;
        }

        private static void ProcessFrameBuffer(FrameBuffer frameBuffer,
            ProcessFrameNotificationCallback? processNotificationCallback)
        {
            if (frameBuffer.FrameBuilderLength + frameBuffer.ReceiveBufferUsed >= frameBuffer.FrameBuilder.Length)
            {
                Array.Resize(ref frameBuffer.FrameBuilder, frameBuffer.FrameBuilderLength + frameBuffer.ReceiveBufferUsed);
            }

            Buffer.BlockCopy(frameBuffer.ReceiveBuffer, 0, frameBuffer.FrameBuilder, frameBuffer.FrameBuilderLength, frameBuffer.ReceiveBufferUsed);

            frameBuffer.FrameBuilderLength = frameBuffer.FrameBuilderLength + frameBuffer.ReceiveBufferUsed;

            while (frameBuffer.FrameBuilderLength > NtFrameDefaults.FRAME_HEADER_SIZE) //[FrameSize] and [CRC16]
            {
                var frameDelimiterBytes = new byte[4];
                var frameSizeBytes = new byte[4];
                var expectedCRC16Bytes = new byte[2];

                Buffer.BlockCopy(frameBuffer.FrameBuilder, 0, frameDelimiterBytes, 0, frameDelimiterBytes.Length);
                Buffer.BlockCopy(frameBuffer.FrameBuilder, 4, frameSizeBytes, 0, frameSizeBytes.Length);
                Buffer.BlockCopy(frameBuffer.FrameBuilder, 8, expectedCRC16Bytes, 0, expectedCRC16Bytes.Length);

                var frameDelimiter = BitConverter.ToInt32(frameDelimiterBytes, 0);
                var grossFrameSize = BitConverter.ToInt32(frameSizeBytes, 0);
                var expectedCRC16 = BitConverter.ToUInt16(expectedCRC16Bytes, 0);

                if (frameDelimiter != NtFrameDefaults.FRAME_DELIMITER || grossFrameSize < 0)
                {
                    //Possible corrupt frame.
                    SkipFrame(ref frameBuffer);
                    continue;
                }

                if (frameBuffer.FrameBuilderLength < grossFrameSize)
                {
                    //We have data in the buffer, but it's not enough to make up
                    //  the entire message so we will break and wait on more data.
                    break;
                }

                if (CRC16.ComputeChecksum(frameBuffer.FrameBuilder, NtFrameDefaults.FRAME_HEADER_SIZE, grossFrameSize - NtFrameDefaults.FRAME_HEADER_SIZE) != expectedCRC16)
                {
                    //Corrupt frame.
                    SkipFrame(ref frameBuffer);
                    continue;
                }

                var netFrameSize = grossFrameSize - NtFrameDefaults.FRAME_HEADER_SIZE;
                var compressedFrameBodyBytes = new byte[netFrameSize];
                Buffer.BlockCopy(frameBuffer.FrameBuilder, NtFrameDefaults.FRAME_HEADER_SIZE, compressedFrameBodyBytes, 0, netFrameSize);

                var FrameBodyBytes = Utility.Decompress(compressedFrameBodyBytes);
                var frameBody = Utility.DeserializeToObject<FrameBody>(FrameBodyBytes);

                //Zero out the consumed portion of the frame buffer - more for fun than anything else.
                Array.Clear(frameBuffer.FrameBuilder, 0, grossFrameSize);

                Buffer.BlockCopy(frameBuffer.FrameBuilder, grossFrameSize, frameBuffer.FrameBuilder, 0, frameBuffer.FrameBuilderLength - grossFrameSize);
                frameBuffer.FrameBuilderLength -= grossFrameSize;

                var framePayload = ExtractFramePayload(frameBody);

                if (framePayload is FramePayloadBytes frameNotificationBytes)
                {
                    if (processNotificationCallback == null)
                    {
                        throw new Exception("ProcessFrameBuffer: A notification handler was not supplied.");
                    }
                    processNotificationCallback(frameNotificationBytes);
                }
                else if (framePayload is IFramePayloadNotification notification)
                {
                    if (processNotificationCallback == null)
                    {
                        throw new Exception("ProcessFrameBuffer: A notification handler was not supplied.");
                    }
                    processNotificationCallback(notification);
                }
                else
                {
                    throw new Exception("ProcessFrameBuffer: Encountered undefined frame payload type.");
                }
            }
        }

        /// <summary>
        /// Uses the "EnclosedPayloadType" to determine the type of the payload and then uses reflection
        /// to deserialize the json to that type. Deserialization is skipped when the type is byte[].
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static IFramePayload ExtractFramePayload(FrameBody frame)
        {
            if (frame.ObjectType == "byte[]")
            {
                return new FramePayloadBytes(frame.Bytes);
            }

            var genericToObjectMethod = _reflectioncache.Use((o) =>
            {
                if (o.TryGetValue(frame.ObjectType, out var method))
                {
                    return method;
                }
                return null;
            });

            string json = Encoding.UTF8.GetString(frame.Bytes);

            if (genericToObjectMethod != null)
            {
                return (IFramePayload?)genericToObjectMethod.Invoke(null, new object[] { json })
                    ?? throw new Exception($"ExtractFramePayload: Payload can not be null.");
            }

            var genericType = Type.GetType(frame.ObjectType)
                ?? throw new Exception($"ExtractFramePayload: Unknown payload type {frame.ObjectType}.");

            var toObjectMethod = typeof(Utility).GetMethod("JsonDeserializeToObject")
                ?? throw new Exception($"ExtractFramePayload: Could not find JsonDeserializeToObject().");

            genericToObjectMethod = toObjectMethod.MakeGenericMethod(genericType);

            _reflectioncache.Use((o) => o.TryAdd(frame.ObjectType, genericToObjectMethod));

            return (IFramePayload?)genericToObjectMethod.Invoke(null, new object[] { json })
                ?? throw new Exception($"ExtractFramePayload: Payload can not be null.");
        }
    }
}
