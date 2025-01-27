using System;
using UnityEngine;

namespace UImGui
{
	[Serializable]
	public class ShaderData
	{
		public Shader Mesh;
		public Shader Procedural;

		public ShaderData Clone()
		{
			return (ShaderData)MemberwiseClone();
		}
	}
}
