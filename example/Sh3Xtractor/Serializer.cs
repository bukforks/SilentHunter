using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SH3Textractor
{
	public static class Serializer
	{
		public class ShouldSerializeContractResolver : DefaultContractResolver
		{
			public static readonly ShouldSerializeContractResolver Instance =
				new ShouldSerializeContractResolver();

			protected override JsonProperty CreateProperty
			(
				MemberInfo member,
				MemberSerialization memberSerialization)
			{
				JsonProperty property = base.CreateProperty(member, memberSerialization);
				// example of skipping serialization of certain properties

				// if( property.DeclaringType == typeof(ModelChunk) &&
				//    property.PropertyName == "VertexIndices" ||
				//    property.PropertyName == "TextureIndices" ||
				//    property.PropertyName == "Vertices" ||
				//    property.PropertyName == "TextureCoordinates" ||
				//    property.PropertyName == "MaterialIndices" ||
				//    property.PropertyName == "Normals" )
				//    
				// {
				// 	property.ShouldSerialize = instance => false;
				// }

				return property;
			}
		}
	}
}

