﻿using System.IO;
using SilentHunter.Extensions;
using skwas.IO;

namespace SilentHunter.Dat.Controllers.Serialization
{
	public class StringValueSerializer : ControllerValueSerializer<string>
	{
		public override bool IsSupported(IControllerSerializationContext context)
		{
			return base.IsSupported(context) && !context.Member.HasAttribute<FixedStringAttribute>();
		}

		public override void Serialize(BinaryWriter writer, ControllerSerializationContext context)
		{
			if (context.Value != null)
			{
				// Write the variable string with one zero.
				writer.Write((string)context.Value, '\0');
			}
		}

		public override object Deserialize(BinaryReader reader, ControllerDeserializationContext context)
		{
			if (reader.BaseStream.Position == reader.BaseStream.Length)
			{
				return null;
			}

			return reader.ReadNullTerminatedString();
		}
	}
}