﻿using System;
using System.Threading.Tasks;
using Microsoft.DotNet.XHarness.iOS.Shared.Execution;
using Microsoft.DotNet.XHarness.iOS.Shared.Logging;

namespace Microsoft.DotNet.XHarness.iOS.Shared.Tasks {

	public class BuildTool
	{
		public string TestName { get; set; }
		public IProcessManager ProcessManager { get; }
		public TestPlatform Platform { get; set; }
		public TestProject TestProject { get; set; }
		public ILog BuildLog { get; set; }

		public bool SpecifyPlatform { get; set; } = true;
		public bool SpecifyConfiguration { get; set; } = true;

		public BuildTool (IProcessManager processManager)
		{ 
			ProcessManager = processManager ?? throw new ArgumentNullException (nameof (processManager));
		}

		public BuildTool (IProcessManager processManager, TestPlatform platform) : this (processManager)
		{
			Platform = platform;
		}

		public virtual string Mode {
			get { return Platform.ToString (); }
			set { throw new NotSupportedException (); }
		}

		public virtual Task CleanAsync ()
		{
			Console.WriteLine ("Clean is not implemented for {0}", GetType ().Name);
			return Task.CompletedTask;
		}
	}
}
