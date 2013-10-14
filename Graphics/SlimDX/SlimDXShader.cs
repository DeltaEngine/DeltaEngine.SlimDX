using System.Text;
using DeltaEngine.Content;
using DeltaEngine.Datatypes;
using DeltaEngine.Graphics.Vertices;
using SlimDX.Direct3D9;
using SlimD3D9 = SlimDX.Direct3D9;
using VertexElementD3D9 = SlimDX.Direct3D9.VertexElement;

namespace DeltaEngine.Graphics.SlimDX
{
	/// <summary>
	/// SlimDX graphics shader.
	/// </summary>
	public class SlimDXShader : ShaderWithFormat
	{
		protected SlimDXShader(string contentName, SlimDXDevice device)
			: base(contentName)
		{
			this.device = device.NativeDevice;
		}

		private readonly SlimD3D9.Device device;

		public SlimDXShader(ShaderCreationData customShader, SlimDXDevice device)
			: base(customShader)
		{
			this.device = device.NativeDevice;
			Create();
		}

		protected override sealed void Create()
		{
			CreateVertexDeclaration();
			CreateVertexShader();
			CreatePixelShader();	
		}

		private void CreateVertexDeclaration()
		{
			int elementIndex = 0;
			var vertexElements = new VertexElementD3D9[Format.Elements.Length + 1];
			foreach (var vertexElement in Format.Elements)
				if (vertexElement.ElementType == VertexElementType.Position3D)
					vertexElements[elementIndex++] = GetVertexPosition3D(vertexElement.Offset);
				else if (vertexElement.ElementType == VertexElementType.Position2D)
					vertexElements[elementIndex++] = GetVertexPosition2D(vertexElement.Offset);
				else if (vertexElement.ElementType == VertexElementType.Normal)
					vertexElements[elementIndex++] = GetVertexNormal(vertexElement.Offset);
				else if (vertexElement.ElementType == VertexElementType.Color)
					vertexElements[elementIndex++] = GetVertexColor(vertexElement.Offset);
				else if (vertexElement.ElementType == VertexElementType.TextureUV)
					vertexElements[elementIndex++] = GetVertexTextureCoordinate(vertexElement.Offset);
				else if (vertexElement.ElementType == VertexElementType.LightMapUV)
					vertexElements[elementIndex++] = GetVertexLightMapUV(vertexElement.Offset);	
			vertexElements[elementIndex] = VertexElementD3D9.VertexDeclarationEnd;
			vertexDeclaration = new VertexDeclaration(device, vertexElements);
		}

		private VertexDeclaration vertexDeclaration;

		private static VertexElementD3D9 GetVertexPosition3D(int offset)
		{
			return new VertexElementD3D9(0, (short)offset, DeclarationType.Float3,
				DeclarationMethod.Default, DeclarationUsage.Position, 0);
		}

		private static VertexElementD3D9 GetVertexNormal(int offset)
		{
			return new VertexElementD3D9(0, (short)offset, DeclarationType.Float3,
				DeclarationMethod.Default, DeclarationUsage.Normal, 0);
		}

		private static VertexElementD3D9 GetVertexPosition2D(int offset)
		{
			return new VertexElementD3D9(0, (short)offset, DeclarationType.Float2,
				DeclarationMethod.Default, DeclarationUsage.Position, 0);
		}

		private static VertexElementD3D9 GetVertexColor(int offset)
		{
			return new VertexElementD3D9(0, (short)offset, DeclarationType.Color,
				DeclarationMethod.Default, DeclarationUsage.Color, 0);
		}

		private static VertexElementD3D9 GetVertexTextureCoordinate(int offset)
		{
			return new VertexElementD3D9(0, (short)offset, DeclarationType.Float2,
				DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0);
		}

		private static VertexElementD3D9 GetVertexLightMapUV(int offset)
		{
			return new VertexElementD3D9(0, (short)offset, DeclarationType.Float2,
				DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 1);
		}

		private void CreateVertexShader()
		{
			var bytecode = ShaderBytecode.Compile(Encoding.UTF8.GetBytes(DX9Code), "VS",
				"vs_2_0", ShaderFlags.None);
			vertexShader = new VertexShader(device, bytecode);
		}

		private VertexShader vertexShader;

		private void CreatePixelShader()
		{
			var bytecode = ShaderBytecode.Compile(Encoding.UTF8.GetBytes(DX9Code), "PS",
				"ps_2_0", ShaderFlags.None);
			pixelShader = new PixelShader(device, bytecode);
		}

		private PixelShader pixelShader;

		public override void SetModelViewProjectionMatrix(Matrix matrix)
		{
			device.SetVertexShaderConstant(0, Matrix.Transpose(matrix).GetValues);
		}

		public override void SetJointMatrices(Matrix[] jointMatrices)
		{
			// not supported yet
		}

		public override void SetDiffuseTexture(Image texture)
		{
			device.SetSamplerState(0, SamplerState.MipFilter, TextureFilter.None);
			device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Linear);
			device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Linear);
			device.SetTexture(0, (texture as SlimDXImage).NativeTexture);			
		}

		public override void SetLightmapTexture(Image texture)
		{
			device.SetSamplerState(1, SamplerState.MipFilter, TextureFilter.None);
			device.SetSamplerState(1, SamplerState.MinFilter, TextureFilter.Linear);
			device.SetSamplerState(1, SamplerState.MagFilter, TextureFilter.Linear);
			device.SetTexture(1, (texture as SlimDXImage).NativeTexture);			
		}

		public override void SetLightPosition(Vector3D vector)
		{
			// not implemented yet
		}

		public override void SetViewPosition(Vector3D vector)
		{
			// not implemented yet
		}

		public override void Bind()
		{
			device.SetTexture(0, null);
			device.VertexShader = vertexShader;
			device.PixelShader = pixelShader;
			device.VertexDeclaration = vertexDeclaration;
		}

		public override void BindVertexDeclaration() {}

		protected override void DisposeData()
		{
			vertexDeclaration.Dispose();
			pixelShader.Dispose();
			vertexShader.Dispose();
		}
	}
}