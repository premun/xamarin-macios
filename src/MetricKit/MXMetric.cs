#if IOS
using System;

using Foundation;
using ObjCRuntime;
using UIKit;

namespace MetricKit {

	public partial class MXMetric {

		public virtual NSDictionary DictionaryRepresentation {
			get {
				if (PlatformHelper.CheckSystemVersion (14,0))
					return _DictionaryRepresentation14;
				else
					return _DictionaryRepresentation13;
			}
		}
	}
}
#endif
