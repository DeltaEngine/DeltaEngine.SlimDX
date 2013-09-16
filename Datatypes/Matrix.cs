﻿using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using DeltaEngine.Extensions;

namespace DeltaEngine.Datatypes
{
	/// <summary>
	/// 4x4 Matrix from 16 floats, access happens via indexer, optimizations done in BuildService.
	/// </summary>
	[DebuggerDisplay("Matrix(Right={Right},\nUp={Up},\nForward={Forward},\nTranslation={Translation})")]
	public struct Matrix : IEquatable<Matrix>
	{
		public Matrix(params float[] values)
			: this()
		{
			for (int i = 0; i < 16; i++)
				this[i] = values[i];
		}

		public float this[int index]
		{
			get
			{
				if (index >= 0 && index < 16)
					return GetValues[index];
				throw new IndexOutOfRangeException();
			}
			set
			{
				SetValue(index, value);
			}
		}

		public float[] GetValues
		{
			get
			{
				return new[]
				{ m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44 };
			}
		}

		private float m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44;

		private void SetValue(int index, float value)
		{
			if (index == 0)				m11 = value;
			else if (index == 1)	m12 = value;
			else if (index == 2)  m13 = value;
			else if (index == 3)	m14 = value; 
			else if (index == 4)	m21 = value;
			else if (index == 5)	m22 = value;
			else if (index == 6)	m23 = value;
			else if (index == 7)	m24 = value; 
			else if (index == 8)	m31 = value;
			else if (index == 9)	m32 = value;
			else if (index == 10)	m33 = value;
			else if (index == 11) m34 = value; 
			else if (index == 12) m41 = value;
			else if (index == 13) m42 = value;
			else if (index == 14) m43 = value;
			else if (index == 15) m44 = value;
		}

		public Vector Right
		{
			get { return new Vector(m11, m12, m13);}
			set
			{
				m11 = value.X;
				m12 = value.Y;
				m13 = value.Z;
			}
		}
		public Vector Up
		{
			get { return new Vector(m21, m22, m23); }
			set
			{
				m21 = value.X;
				m22 = value.Y;
				m23 = value.Z;
			}
		}
		public Vector Forward
		{
			get { return new Vector(m31, m32, m33); }
			set
			{
				m31 = value.X;
				m32 = value.Y;
				m33 = value.Z;
			}
		}
		public Vector Translation
		{
			get { return new Vector(m41, m42, m43); }
			set { m41 = value.X; m42 = value.Y; m43 = value.Z; }
		}

		public static readonly Matrix Identity = 
			new Matrix(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Matrix));

		public static Matrix CreateScale(float scaleX, float scaleY, float scaleZ)
		{
			return new Matrix(scaleX, 0, 0, 0, 0, scaleY, 0, 0, 0, 0, scaleZ, 0, 0, 0, 0, 1);
		}

		public static Matrix CreateRotationZyx(EulerAngles eulerAngles)
		{
			return CreateRotationZyx(eulerAngles.Pitch, eulerAngles.Yaw, eulerAngles.Roll);
		}

		public static Matrix CreateRotationZyx(float x, float y, float z)
		{
			return LinearMapExtensions.CreateRotationAboutZThenYThenX(x, y, z);
		}

		public static Matrix CreateTranslation(float x, float y, float z)
		{
			return new Matrix(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, x, y, z, 1);
		}

		public static Matrix Transpose(Matrix matrix)
		{
			return new Matrix(	matrix[0], matrix[4], matrix[8],  matrix[12],
													matrix[1], matrix[5], matrix[9],  matrix[13],
													matrix[2], matrix[6], matrix[10], matrix[14],
													matrix[3], matrix[7], matrix[11], matrix[15] );
		}

		public static Matrix Invert(Matrix matrix)
		{
			return LinearMapExtensions.InvertMatrix(matrix);
		}

		public static Matrix InverseTranspose(Matrix matrix)
		{
			return Transpose(Invert(matrix));
		}

		public static Matrix CreatePerspective(float fieldOfView, float aspectRatio,
			float nearPlaneDistance, float farPlaneDistance)
		{
			return LinearMapExtensions.CreatePerspectiveMatrix(fieldOfView, aspectRatio, 
				nearPlaneDistance, farPlaneDistance);
		}

		public static Matrix CreateOrthoProjection(Size viewportSize)
		{
			return LinearMapExtensions.CreateOrthoProjectionMatrix(viewportSize);
		}

		public static Matrix CreateOrthoProjection(Size viewportSize, float nearPlane, float farPlane)
		{
			return LinearMapExtensions.CreateOrthoProjectionMatrix(viewportSize, nearPlane, farPlane);
		}

		public static Matrix CreateLookAt(Vector cameraPosition, Vector cameraTarget, Vector cameraUp)
		{
			return LinearMapExtensions.CreateLookAtMatrix(cameraPosition, cameraTarget, cameraUp);
		}

		public Vector TransformNormal(Vector normal)
		{
			return LinearMapExtensions.TransformVector(normal, this);
		}

		public static Vector TransformHomogeneousCoordinate(Vector coord, Matrix matrix)
		{
			return LinearMapExtensions.TransformVectorWithHomogeneousCoordinate(coord, matrix);
		}

		public static Matrix CreateRotationX(float degrees)
		{
			return LinearMapExtensions.CreateMatrixRotatingAboutX(degrees);
		}

		public static Matrix CreateRotationY(float degrees)
		{
			return LinearMapExtensions.CreateMatrixRotatingAboutY(degrees);
		}

		public static Matrix CreateRotationZ(float degrees)
		{
			return LinearMapExtensions.CreateMatrixRotatingAboutZ(degrees);
		}

		/// <summary>
		/// Further details on how to compute matrix from quaternion:
		/// http://renderfeather.googlecode.com/hg-history/034a1900d6e8b6c92440382658d2b01fc732c5de/Doc/optimized%2520Matrix%2520quaternion%2520conversion.pdf
		/// </summary>
		public static Matrix FromQuaternion(Quaternion quaternion)
		{
			float qxx = quaternion.X * quaternion.X;
			float qyy = quaternion.Y * quaternion.Y;
			float qzz = quaternion.Z * quaternion.Z;
			float qxy = quaternion.X * quaternion.Y;
			float qzw = quaternion.Z * quaternion.W;
			float qxz = quaternion.X * quaternion.Z;
			float qyw = quaternion.Y * quaternion.W;
			float qyz = quaternion.Y * quaternion.Z;
			float qxw = quaternion.X * quaternion.W;
			return new Matrix(
				1.0f - 2.0f * (qyy + qzz), 2.0f * (qxy + qzw), 2.0f * (qxz - qyw), 0.0f,
				2.0f * (qxy - qzw), 1.0f - 2.0f * (qxx + qzz), 2.0f * (qyz + qxw), 0.0f,
				2.0f * (qxz + qyw), 2.0f * (qyz - qxw), 1.0f - 2.0f * (qxx + qyy), 0.0f,
				0.0f, 0.0f, 0.0f, 1.0f);
		}

		/// <summary>
		/// More details how to calculate Matrix Determinants: http://en.wikipedia.org/wiki/Determinant
		/// </summary>
		[Pure]
		public float GetDeterminant()
		{
			float det33X44 = this[15] * this[10] - this[14] * this[11];
			float det32X44 = this[9] * this[15] - this[13] * this[11];
			float det32X43 = this[9] * this[14] - this[13] * this[10];
			float det31X44 = this[8] * this[15] - this[12] * this[11];
			float det31X43 = this[8] * this[14] - this[12] * this[10];
			float det31X42 = this[8] * this[13] - this[12] * this[9];
			float det11 = this[0] * (this[5] * det33X44 - this[6] * det32X44 + this[7] * det32X43);
			float det12 = this[1] * (this[4] * det33X44 - this[6] * det31X44 + this[7] * det31X43);
			float det13 = this[2] * (this[4] * det32X44 - this[5] * det31X44 + this[7] * det31X42);
			float det14 = this[3] * (this[4] * det32X43 - this[5] * det31X43 + this[6] * det31X42);
			return det11 - det12 + det13 - det14;
		}

		public static bool operator ==(Matrix matrix1, Matrix matrix2)
		{
			return matrix1.Equals(matrix2);
		}

		public bool Equals(Matrix other)
		{
			for (int i = 0; i < 16; i++)
				if (!this[i].IsNearlyEqual(other[i]))
					return false;
			return true;
		}

		public static bool operator !=(Matrix matrix1, Matrix matrix2)
		{
			return !(matrix1 == matrix2);
		}

		public bool IsNearlyEqual(Matrix matrix)
		{
			for (int i = 0; i < 16; i++)
				if (!this[i].IsNearlyEqual(matrix[i]))
					return false;

			return true;
		}

		public static Matrix operator /(Matrix matrix, float scalar)
		{
			var value = 1.0f / scalar;
			return new Matrix(matrix.m11 * value, matrix.m12 * value, matrix.m13 * value, matrix.m14 * value,
												matrix.m21 * value, matrix.m22 * value, matrix.m23 * value, matrix.m24 * value,
												matrix.m31 * value, matrix.m32 * value, matrix.m33 * value, matrix.m34 * value,
												matrix.m41 * value, matrix.m42 * value, matrix.m43 * value, matrix.m44 * value);
		}

		public static Vector operator *(Matrix matrix, Vector vector)
		{
			return new Vector(vector.X * matrix.m11 + vector.Y * matrix.m21 + vector.Z * matrix.m31 + matrix.m41,
												vector.X * matrix.m12 + vector.Y * matrix.m22 + vector.Z * matrix.m32 + matrix.m42,
												vector.X * matrix.m13 + vector.Y * matrix.m23 + vector.Z * matrix.m33 + matrix.m43);
		}

		public static Matrix operator *(Matrix matrix1, Matrix matrix2)
		{
			var result = new float[16];
			for (int i = 0; i < 4; i++)
				for (int j = 0; j < 4; j++)
					for (int k = 0; k < 4; k++)
						result[i * 4 + j] += matrix1[i * 4 + k] * matrix2[k * 4 + j];

			return new Matrix(result);
		}

		public override int GetHashCode()
		{
			return (int)GetValues.Aggregate((a, b) => a.GetHashCode() ^ b.GetHashCode());
		}

		public override bool Equals(object obj)
		{
			return obj is Matrix && Equals((Matrix)obj);
		}

		public override string ToString()
		{
			return string.Join(", ", GetValues);
		}
	}
}