﻿using System;
using System.Collections.Generic;
using System.IO;
using SilentHunter.Controllers;

namespace SilentHunter.Dat.Controllers.Serialization
{
	public class MeshAnimationControllerSerializer : IControllerSerializer
	{
		public void Deserialize(Stream stream, RawController controller)
		{
			MeshAnimationController mac = EnsureControllerType(controller);

			using (var reader = new BinaryReader(stream, FileEncoding.Default, true))
			{
				// Read n frames.
				ushort frameCount = reader.ReadUInt16();
				mac.Frames = new List<AnimationKeyFrame>(frameCount);
				for (int i = 0; i < frameCount; i++)
				{
					mac.Frames.Add(
						new AnimationKeyFrame
						{
							Time = reader.ReadSingle(),
							FrameNumber = reader.ReadUInt16()
						});
				}

				// Get the number of compressed frames and vertices.
				int compressedFrameCount = reader.ReadInt32();
				int vertexCount = reader.ReadInt32();

				mac.CompressedFrames = new List<CompressedVertices>(compressedFrameCount);
				for (int frameIndex = 0; frameIndex < compressedFrameCount; frameIndex++)
				{
					var cv = new CompressedVertices
					{
						Scale = reader.ReadSingle(),
						Translation = reader.ReadSingle(),
						Vertices = new List<short>(vertexCount)
					};

					for (int vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
					{
						cv.Vertices.Add(reader.ReadInt16());
					}

					mac.CompressedFrames.Add(cv);
				}

				if (stream.Position != stream.Length)
				{
					mac.Unknown0 = reader.ReadBytes((int)(stream.Length - stream.Position));
				}
			}
		}

		public void Serialize(Stream stream, RawController controller)
		{
			MeshAnimationController mac = EnsureControllerType(controller);

			if (mac.Frames.Count > ushort.MaxValue)
			{
				throw new InvalidOperationException($"Mesh animations can only support up to {ushort.MaxValue} frames.");
			}

			using (var writer = new BinaryWriter(stream, FileEncoding.Default, true))
			{
				// Write frames.
				writer.Write(unchecked((ushort)mac.Frames.Count));
				foreach (AnimationKeyFrame frame in mac.Frames)
				{
					writer.Write(frame.Time);
					writer.Write(frame.FrameNumber);
				}

				// Write mesh transforms.
				writer.Write(mac.CompressedFrames.Count);

				// Each frame should be same size, so if we have a frame, use vertex count of first.
				writer.Write(mac.CompressedFrames.Count > 0 ? mac.CompressedFrames[0].Vertices.Count : 0);
				foreach (CompressedVertices cv in mac.CompressedFrames)
				{
					writer.Write(cv.Scale);
					writer.Write(cv.Translation);

					foreach (short vertexIndex in cv.Vertices)
					{
						writer.Write(vertexIndex);
					}
				}

				if (mac.Unknown0?.Length > 0)
				{
					writer.Write(mac.Unknown0, 0, mac.Unknown0.Length);
				}
			}
		}

		private static MeshAnimationController EnsureControllerType(RawController controller)
		{
			if (controller is MeshAnimationController meshAnimationController)
			{
				return meshAnimationController;
			}

			throw new InvalidOperationException($"Expected controller of type {nameof(MeshAnimationController)}.");
		}
	}
}