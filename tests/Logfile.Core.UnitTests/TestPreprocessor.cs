using EventRouter.Core;
using System;
using System.Collections.Generic;

namespace Logfile.Core.UnitTests
{
	class TestPreprocessor : IRoutablePreprocessor<LogEvent<StandardLoglevel>>, IDisposable
	{
		public bool OnEnqueueing => true;

		public IEnumerable<LogEvent<StandardLoglevel>> Process(LogEvent<StandardLoglevel> routable)
		{
			return null;
		}

		public void Dispose()
		{
			this.DisposeCallback?.Invoke();
		}

		public Action DisposeCallback { get; set; }
	}
}
