using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.Packaging
{
	public enum StepResultKind
	{
		Successful, Warning, Error
	}
	public class StepResult
	{
		internal static readonly StepResult Default = new StepResult() { Kind = StepResultKind.Successful };

		public StepResultKind Kind { get; set; }
		internal bool NeedRestart { get; set; }
		internal bool NeedSetReferencePhase { get; set; }

		internal void Merge(StepResult other)
		{
			if (other.Kind > Kind)
				Kind = other.Kind;
			NeedRestart |= other.NeedRestart;
			NeedSetReferencePhase |= other.NeedSetReferencePhase;
		}
	}
}
