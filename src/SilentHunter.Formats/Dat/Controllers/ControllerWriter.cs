﻿using System;
using System.IO;
using System.Reflection;
using SilentHunter.Controllers;
using SilentHunter.Controllers.Decoration;
using SilentHunter.Dat.Controllers.Serialization;
using SilentHunter.Extensions;

namespace SilentHunter.Dat.Controllers
{
	public class ControllerWriter : IControllerWriter
	{
		private readonly IControllerFactory _controllerFactory;
		private readonly ControllerSerializerResolver _controllerSerializerResolver;

		public ControllerWriter(IControllerFactory controllerFactory)
		{
			_controllerFactory = controllerFactory ?? throw new ArgumentNullException(nameof(controllerFactory));
			_controllerSerializerResolver = new ControllerSerializerResolver();
		}

		/// <summary>
		/// Writes a controller to a stream.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="controller"></param>
		public void Write(Stream stream, object controller)
		{
			if (controller == null)
			{
				throw new ArgumentNullException(nameof(controller));
			}

			if (controller is byte[] byteBuffer)
			{
				// This is a raw byte controller. Simply write it.
				stream.Write(byteBuffer, 0, byteBuffer.Length);
				return;
			}

			using (var writer = new BinaryWriter(stream, FileEncoding.Default, true))
			{
				Type controllerType = controller.GetType();
				IControllerSerializer cs = _controllerSerializerResolver.GetSerializer(controllerType);

				if (controllerType.IsRawController())
				{
					ControllerAttribute controllerAttribute = controllerType.GetCustomAttribute<ControllerAttribute>() ?? new ControllerAttribute();
					if (controllerAttribute.SubType.HasValue)
					{
						writer.Write(controllerAttribute.SubType.Value);
					}

					if (typeof(AnimationController).IsAssignableFrom(controllerType) || !controllerAttribute.SubType.HasValue)
					{
						// Skip writing the field. The count value will be written by the RawList type.
					}
					else
					{
						writer.Write((ushort)0);
					}

					cs.Serialize(stream, (RawController)controller);

					return;
				}

				// Test if the type is from our controller assembly.
				if (!_controllerFactory.CanCreate(controllerType))
				{
					throw new NotSupportedException("Invalid controller.");
				}

				// We don't know the size yet, so just write 0 for now.
				writer.Write(0);

				long startPos = stream.Position;

				cs.Serialize(stream, (RawController)controller);

				// After the controller is written, determine and write the size.
				long currentPos = stream.Position;
				stream.Position = startPos - 4;
				writer.Write((int)(currentPos - startPos));

				// Restore position to the end of the controller.
				stream.Position = currentPos;
			}
		}
	}
}