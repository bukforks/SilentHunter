﻿using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace SilentHunter
{
	/// <summary>
	/// Describes a vector in three-dimensional (3-D) space.
	/// </summary>
	/// <remarks>This type is only a simple stub for parsing DAT-files and thus is not ment to be used to manipulate vectors. Instead, implicit operators are implemented to cast to other Vector3 types.</remarks>
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct Vector3
	{
		public float X;
		public float Y;
		public float Z;

		public static readonly Vector3 Empty;

		/// <summary>
		/// Returns the fully qualified type name of this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String" /> containing a fully qualified type name.
		/// </returns>
		public override string ToString()
		{
			string sep = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
			return $"X: {X}{sep} Y: {Y}{sep} Z: {Z}";
		}

		public bool Equals(Vector3 other)
		{
			return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <returns>
		/// true if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, false.
		/// </returns>
		/// <param name="obj">The object to compare with the current instance. </param>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is Vector3 && Equals((Vector3)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = X.GetHashCode();
				hashCode = (hashCode * 397) ^ Y.GetHashCode();
				hashCode = (hashCode * 397) ^ Z.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(Vector3 left, Vector3 right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Vector3 left, Vector3 right)
		{
			return !left.Equals(right);
		}

		public static implicit operator SharpDX.Vector3(Vector3 vector)
		{
			return new SharpDX.Vector3
			{
				X = vector.X,
				Y = vector.Y,
				Z = vector.Z
			};
		}

		public static implicit operator Vector3(SharpDX.Vector3 vector)
		{
			return new Vector3
			{
				X = vector.X,
				Y = vector.Y,
				Z = vector.Z
			};
		}
	}
}