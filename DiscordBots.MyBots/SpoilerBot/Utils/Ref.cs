using System;
using System.Collections.Generic;
using System.Text;

namespace TriggersTools.DiscordBots.SpoilerBot.Utils {
	/// <summary>
	/// A referenceable type value that counts the number of references to it manually.
	/// </summary>
	/// <typeparam name="T">The type of the referenced value.</typeparam>
	internal class Ref<T> : Ref, IRef<T> {
		#region Fields

		/// <summary>
		/// Gets the value contained by this ref.
		/// </summary>
		public T Value { get; }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructs the <see cref="Ref{T}"/> with the specified immutable value.
		/// </summary>
		/// <param name="value">The immutable value for the ref.</param>
		public Ref(T value) {
			Value = value;
		}

		#endregion
	}

	/// <summary>
	/// A referenceable type that counts the number of references to it manually.
	/// </summary>
	internal class Ref : IRef {
		#region Add/RemoveRef

		/// <summary>
		/// Adds a single reference to the counter.
		/// </summary>
		public void AddRef() {
			RefCount++;
		}
		/// <summary>
		/// Adds the specified amount of references to the counter.
		/// </summary>
		/// 
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="count"/> is less than zero.
		/// </exception>
		public void AddRefs(int count) {
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count));
			RefCount += count;
		}

		/// <summary>
		/// Removes a single reference from the counter.
		/// </summary>
		/// 
		/// <exception cref="InvalidOperationException">
		/// <see cref="RefCount"/> is zero.
		/// </exception>
		public void RemoveRef() {
			if (IsUnused)
				throw new InvalidOperationException("Cannot remove ref with ref count of zero!");
			RefCount--;
		}
		/// <summary>
		/// Removes the specified amount of references from the counter.
		/// </summary>
		/// 
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="count"/> is less than zero.
		/// </exception>
		/// 
		/// <exception cref="InvalidOperationException">
		/// <see cref="RefCount"/> - <paramref name="count"/> would be less than zero.<para/>
		/// <see cref="RefCount"/> is set to zero when this exception occurs.
		/// </exception>
		public void RemoveRefs(int count) {
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count));
			if (RefCount - count < 0) {
				RefCount = 0;
				throw new InvalidOperationException("Too many refs being removed from ref count!");
			}
			RefCount -= count;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the number of references in use.
		/// </summary>
		public int RefCount { get; private set; } = 0;
		/// <summary>
		/// Gets if the reference is unused (The <see cref="RefCount"/> is zero.)
		/// </summary>
		public bool IsUnused => RefCount == 0;

		#endregion
	}

	/// <summary>
	/// A referenceable interface that counts the number of references to it manually.
	/// </summary>
	internal interface IRef {
		#region Add/RemoveRef

		/// <summary>
		/// Adds a single reference to the counter.
		/// </summary>
		void AddRef();
		/// <summary>
		/// Adds the specified amount of references to the counter.
		/// </summary>
		/// 
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="count"/> is less than zero.
		/// </exception>
		void AddRefs(int count);

		/// <summary>
		/// Removes a single reference from the counter.
		/// </summary>
		/// 
		/// <exception cref="InvalidOperationException">
		/// <see cref="RefCount"/> is zero.
		/// </exception>
		void RemoveRef();
		/// <summary>
		/// Removes the specified amount of references from the counter.
		/// </summary>
		/// 
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="count"/> is less than zero.
		/// </exception>
		/// 
		/// <exception cref="InvalidOperationException">
		/// <see cref="RefCount"/> - <paramref name="count"/> would be less than zero.<para/>
		/// <see cref="RefCount"/> is set to zero when this exception occurs.
		/// </exception>
		void RemoveRefs(int count);

		#endregion

		#region Properties

		/// <summary>
		/// Gets the number of references in use.
		/// </summary>
		int RefCount { get; }
		/// <summary>
		/// Gets if the reference is unused (The <see cref="RefCount"/> is zero.)
		/// </summary>
		bool IsUnused { get; }
		
		#endregion
	}

	/// <summary>
	/// A referenceable interface value that counts the number of references to it manually.
	/// </summary>
	/// <typeparam name="T">The type of the referenced value.</typeparam>
	internal interface IRef<out T> : IRef {
		/// <summary>
		/// Gets the value contained by this ref.
		/// </summary>
		T Value { get; }
	}
}
