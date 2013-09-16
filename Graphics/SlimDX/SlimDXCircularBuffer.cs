﻿using DeltaEngine.Core;
using DeltaEngine.Graphics.Vertices;
using SlimDX.Direct3D9;
using DeviceD3D9 = SlimDX.Direct3D9.Device;
using VertexBufferD3D9 = SlimDX.Direct3D9.VertexBuffer;
using VertexFormatD3D9 = SlimDX.Direct3D9.VertexFormat;
using IndexBufferD3D9 = SlimDX.Direct3D9.IndexBuffer;

namespace DeltaEngine.Graphics.SlimDX
{
	/// <summary>
	/// Basic functionality for all SlimDX based circular buffers to render small batches quickly.
	/// </summary>
	public class SlimDXCircularBuffer : CircularBuffer
	{
		public SlimDXCircularBuffer(Device device, ShaderWithFormat shader, BlendMode blendMode,
			VerticesMode drawMode = VerticesMode.Triangles)
			: base(device, shader, blendMode, drawMode)
		{
			dxDevice = (SlimDXDevice)device;
			dxDevice.DisposeNativeBuffers += DisposeNative;
		}

		private readonly SlimDXDevice dxDevice;

		protected override void CreateNative()
		{
			nativeDevice = (device as SlimDXDevice).NativeDevice;
			CreateNativeVertexBuffer();
			if (UsesIndexBuffer)
				CreateNativeIndexBuffer();
		}

		private DeviceD3D9 nativeDevice;

		private void CreateNativeVertexBuffer()
		{
			nativeVertexBuffer = new VertexBufferD3D9(nativeDevice, maxNumberOfVertices * vertexSize,
				Usage.Dynamic, VertexFormatD3D9.None, Pool.Default);
		}

		private VertexBufferD3D9 nativeVertexBuffer;

		private void CreateNativeIndexBuffer()
		{
			nativeIndexBuffer = new IndexBufferD3D9(nativeDevice, maxNumberOfIndices * indexSize,
				Usage.Dynamic, Pool.Default, true);
		}

		private IndexBufferD3D9 nativeIndexBuffer;

		protected override void AddDataNative<VertexType>(Chunk textureChunk, VertexType[] vertexData,
			short[] indices, int numberOfVertices, int numberOfIndices)
		{
			AddVertexDataNative(vertexData, numberOfVertices);
			if (!UsesIndexBuffer)
				return;
			if (indices == null)
				indices = ComputeIndices(textureChunk.NumberOfVertices, numberOfVertices);
			else if (totalIndicesCount > 0)
				indices = RemapIndices(indices, numberOfIndices);
			AddIndexDataNative(indices, numberOfIndices);
		}

		private void AddVertexDataNative<VertexType>(VertexType[] vertices, int numberOfVertices)
			where VertexType : struct
		{
			if (nativeVertexBuffer.Disposed)
				CreateNativeVertexBuffer();
			var stream = nativeVertexBuffer.Lock(totalVertexOffsetInBytes, numberOfVertices * vertexSize,
				LockFlags.None);
			stream.WriteRange(vertices, 0, numberOfVertices);
			nativeVertexBuffer.Unlock();
		}

		private void AddIndexDataNative(short[] indices, int numberOfIndices)
		{
			if (nativeIndexBuffer.Disposed)
				CreateNativeIndexBuffer();
			var stream = nativeIndexBuffer.Lock(totalIndexOffsetInBytes, numberOfIndices * indexSize,
				LockFlags.None);
			stream.WriteRange(indices, 0, numberOfIndices);
			nativeIndexBuffer.Unlock();
		}

		protected override void DisposeNative()
		{
			nativeVertexBuffer.Dispose();
			if (nativeIndexBuffer != null)
				nativeIndexBuffer.Dispose();
		}

		protected override void DrawChunk(Chunk chunk)
		{
			dxDevice.SetCounterClockwiseCullMode();
			if (UsesIndexBuffer)
				DrawChunkWithIndices(chunk);
			else
				DrawChunkWithoutIndices(chunk);
		}

		private void DrawChunkWithIndices(Chunk chunk)
		{
			if (chunk.Texture != null)
				shader.SetDiffuseTexture(chunk.Texture);
			nativeDevice.SetStreamSource(0, nativeVertexBuffer, 0, vertexSize);
			nativeDevice.Indices = nativeIndexBuffer;
			nativeDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, totalVerticesCount,
				chunk.FirstIndexOffsetInBytes / indexSize, chunk.NumberOfIndices / VerticesPerTriangle);
		}

		private const int VerticesPerTriangle = 3;

		private void DrawChunkWithoutIndices(Chunk chunk)
		{
			nativeDevice.SetStreamSource(0, nativeVertexBuffer, chunk.FirstVertexOffsetInBytes, vertexSize);
			nativeDevice.DrawPrimitives(PrimitiveType.LineList, 0, chunk.NumberOfVertices / VerticesPerLine);
		}

		private const int VerticesPerLine = 2;
	}
}