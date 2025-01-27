using System;

namespace UImGui
{
	[Serializable]
	public class ShaderProperties
	{
		public string Texture;
		public string Vertices;
		public string BaseVertex;

		public ShaderProperties Clone()
		{
			return (ShaderProperties)MemberwiseClone();
		}
	}
}
