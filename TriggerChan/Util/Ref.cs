using System;
using System.Collections.Generic;
using System.Text;

namespace TriggersTools.DiscordBots.TriggerChan.Util {
	public class Ref<T> : BaseRef {

		public Ref(T value) {
			Value = value;
		}

		public T Value { get; }
	}

	public class BaseRef {

		public BaseRef() {
			RefCount = 0;
		}

		public void AddRef() {
			RefCount++;
		}

		public void RemoveRef() {
			if (IsUnused)
				throw new InvalidOperationException("Cannot remove ref with ref count of zero!");
			RefCount--;
		}

		public int RefCount { get; private set; }

		public bool IsUnused {
			get { return RefCount == 0; }
		}
	}
}
