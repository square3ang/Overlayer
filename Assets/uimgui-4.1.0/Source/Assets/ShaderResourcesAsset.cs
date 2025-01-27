using UnityEngine;

namespace UImGui.Assets
{
	[CreateAssetMenu(menuName = "Dear ImGui/Shader Resources")]
	public sealed class ShaderResourcesAsset : ScriptableObject
	{
		public Shader Mesh;
		public Shader Procedural;

		public string Texture;
		public string Vertices;
		public string BaseVertex;
	}
}
