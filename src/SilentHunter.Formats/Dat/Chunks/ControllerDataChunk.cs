﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SilentHunter.Dat.Controllers;
using skwas.IO;

namespace SilentHunter.Dat.Chunks
{
	public sealed class ControllerDataChunk : DatChunk
	{
		private readonly ControllerAssembly _controllerAssembly;
		private readonly IControllerFactory _controllerFactory;

		public ControllerDataChunk(ControllerAssembly controllerAssembly, IControllerFactory controllerFactory)
			: base(DatFile.Magics.ControllerData)
		{
			_controllerAssembly = controllerAssembly ?? throw new ArgumentNullException(nameof(controllerAssembly));
			_controllerFactory = controllerFactory ?? throw new ArgumentNullException(nameof(controllerFactory));
		}

		private long _origin, _localOrigin;

		private byte[] _rawControllerData;
		private object _controllerData;

		public dynamic ControllerData
		{
			get
			{
				if (_controllerData == null && _rawControllerData != null)
				{
					string controllerName = GetControllerName();

					using (var ms = new MemoryStream(_rawControllerData))
					{
						// Attempt to deserialize.
						var reader = new ControllerReader(_controllerAssembly, _controllerFactory);
						_controllerData = reader.Read(ms, controllerName);

						// If controller data is a byte array, the controller was not deserialized. Either it's not implemented, or the data or the implementation contains a bug.
						// To keep file integrity, we just store the data as unknown data.
						if (_controllerData is byte[])
						{
							UnknownData.Add(new UnknownChunkData(_origin,
								_localOrigin,
								_controllerData,
								"Failed to read controller data. Either the data contains a bug, or S3D's controller definition is missing or incorrect."));
						}
					}

					// We no longer need raw data.
					_rawControllerData = null;
				}

				return _controllerData;
			}
			set => _controllerData = value;
		}

		public string ControllerName => _controllerData?.GetType().Name ?? string.Empty;

		public override bool SupportsParentId => true;

		protected override void Serialize(Stream stream)
		{
			//			var regionStream = new RegionStream(stream, -1, false);
			using (var writer = new BinaryWriter(stream, Encoding.ParseEncoding, true))
			{
				writer.Write(ParentId);

				writer.Write((ulong)0); // Always zero.

				var controllerWriter = new ControllerWriter(_controllerFactory);
				controllerWriter.Write(stream, ControllerData);
			}
		}

		protected override void Deserialize(Stream stream)
		{
			var regionStream = stream as RegionStream;

			using (var reader = new BinaryReader(stream, Encoding.ParseEncoding, true))
			{
				// Read parent id.
				ParentId = reader.ReadUInt64();

				ulong alwaysZero = reader.ReadUInt64();
				Debug.Assert(alwaysZero == 0, "Expected 0.");

				// Cache position.
				_localOrigin = stream.Position;
				_origin = regionStream?.BaseStream.Position ?? _localOrigin;

				// Read raw controller data.
				_rawControllerData = reader.ReadBytes((int)(stream.Length - _localOrigin));
			}
		}

		/// <summary>
		/// Get the controller name from previous chunk, which should be a Controller chunk. If the previous chunk is not a Controller chunk, null is returned.
		/// </summary>
		/// <returns></returns>
		private string GetControllerName()
		{
			ControllerChunk prevControllerChunk = null;
			if (ParentFile.Chunks.Count > 0)
			{
				if (ParentFile is DatFile f)
				{
					// Find it by searching up.
					prevControllerChunk = (ControllerChunk)f.Chunks
						.Reverse()
						.FirstOrDefault(c =>
							c.Magic == DatFile.Magics.Controller && c.Id == ParentId
						);
				}
			}

			string controllerName = prevControllerChunk?.Name;

			// Code needed to detect StateMachineClass.
			if (string.IsNullOrEmpty(controllerName) && SubType == -1)
			{
				controllerName = "StateMachineClass";
			}

			return controllerName;
		}
	}
}