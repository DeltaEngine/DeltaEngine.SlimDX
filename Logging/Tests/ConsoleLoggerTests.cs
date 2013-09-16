﻿using System;
using System.IO;
using DeltaEngine.Core;
using NUnit.Framework;

namespace DeltaEngine.Logging.Tests
{
	public class ConsoleLoggerTests
	{
		[SetUp]
		public void RedirectConsoleOutput()
		{
			defaultOut = Console.Out;
			console = new StringWriter();
			Console.SetOut(console);
		}

		private TextWriter defaultOut;
		private TextWriter console;

		[TearDown]
		public void RestoreConsoleOutput()
		{
			Console.SetOut(defaultOut);
			console.Dispose();
		}

		[Test]
		public void WriteInfo()
		{
			using (var logger = new ConsoleLogger())
			{
				Assert.AreEqual("", console.ToString());
				logger.Write(Logger.MessageType.Info, "Hello");
				Assert.IsTrue(console.ToString().Contains("Hello"), console.ToString());
			}
		}

		[Test]
		public void LogWarning()
		{
			using (var logger = new ConsoleLogger())
			{
				Assert.AreEqual("", console.ToString());
				logger.Write(Logger.MessageType.Warning, "Ohoh");
				Assert.IsTrue(console.ToString().Contains("Warning: Ohoh"), console.ToString());
				logger.Write(Logger.MessageType.Warning, new NullReferenceException().ToString());
				Assert.IsTrue(console.ToString().Contains("NullReferenceException"), console.ToString());
			}
		}

		[Test]
		public void LogError()
		{
			using (var logger = new ConsoleLogger())
			{
				Assert.AreEqual("", console.ToString());
				logger.Write(Logger.MessageType.Error, new NotSupportedException().ToString());
				Assert.IsTrue(console.ToString().Contains("NotSupportedException"), console.ToString());
			}
		}
	}
}