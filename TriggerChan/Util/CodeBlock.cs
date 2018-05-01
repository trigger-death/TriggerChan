using System;
using System.Collections.Generic;
using System.Text;

namespace TriggersTools.DiscordBots.TriggerChan.Util {
	public enum CodeBlockType {
		None,
		Quote,
		Full,
	}

	public struct CodeBlock {
		public static readonly CodeBlock None = new CodeBlock() {
			Start = -1,
			Length = 0,
		};

		public CodeBlockType Type { get; set; }
		public int Start { get; set; }
		public int Length { get; set; }
		public int End {
			get { return Start + Length; }
			set { Length = value - Start; }
		}

		public bool Contains(int index) {
			if (Type != CodeBlockType.None)
				return (index >= Start && index < End);
			return false;
		}

		public override bool Equals(object obj) {
			if (obj is CodeBlock) {
				return this == ((CodeBlock) obj);
			}
			return base.Equals(obj);
		}

		public override int GetHashCode() {
			return (int) Type ^
				BitRotating.RotateLeft(Start, 2) ^
				BitRotating.RotateLeft(Length, 17);
		}

		public static implicit operator CodeBlockType(CodeBlock block) {
			return block.Type;
		}

		public static bool operator ==(CodeBlock a, CodeBlock b) {
			return a.Type == b.Type &&
				(a.Type == CodeBlockType.None ||
				(a.Start == b.Start &&
				a.Length == b.Length));
		}

		public static bool operator !=(CodeBlock a, CodeBlock b) {
			return a.Type != b.Type ||
				(a.Type != CodeBlockType.None &&
				(a.Start != b.Start ||
				a.Length != b.Length));
		}
	}
}
