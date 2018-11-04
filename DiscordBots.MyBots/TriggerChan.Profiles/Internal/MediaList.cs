using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles {
	/// <summary>
	/// The internal implementation of <see cref="IMediaList"/>.
	/// </summary>
	internal abstract class MediaList : DatabaseUserList, IMediaList {
		/// <summary>
		/// Gets the time spent on media of this list type.
		/// </summary>
		public TimeSpan? TimeSpent { get; set; }
		/// <summary>
		/// Gets the days spent on media of this list type.
		/// </summary>
		public double? DaysSpent {
			get => TimeSpent?.TotalDays;
			set => TimeSpent = (value.HasValue ? TimeSpan.FromDays(value.Value) : (TimeSpan?) null);
		}
		/// <summary>
		/// Gets the hours spent on media of this list type.
		/// </summary>
		public double? HoursSpent {
			get => TimeSpent?.TotalHours;
			set => TimeSpent = (value.HasValue ? TimeSpan.FromHours(value.Value) : (TimeSpan?) null);
		}
		/// <summary>
		/// Gets the minutes spent on media of this list type.
		/// </summary>
		public double? MinutesSpent {
			get => TimeSpent?.TotalMinutes;
			set => TimeSpent = (value.HasValue ? TimeSpan.FromMinutes(value.Value) : (TimeSpan?) null);
		}
		/// <summary>
		/// Gets the seconds spent on media of this list type.
		/// </summary>
		public double? SecondsSpent {
			get => TimeSpent?.TotalSeconds;
			set => TimeSpent = (value.HasValue ? TimeSpan.FromSeconds(value.Value) : (TimeSpan?) null);
		}

		/// <summary>
		/// Gets the user's mean score for titles out of 100.
		/// </summary>
		public double? MeanScore100 {
			get => (MeanScore.HasValue ? (MeanScore * 100d) : null);
			set => MeanScore = (value.HasValue ? (value / 100d) : null);
		}
		/// <summary>
		/// Gets the user's mean score for titles out of 10.
		/// </summary>
		public double? MeanScore10 {
			get => (MeanScore.HasValue ? (MeanScore * 10d) : null);
			set => MeanScore = (value.HasValue ? (value / 10d) : null);
		}
		/// <summary>
		/// Gets the user's mean score for titles out of 5.
		/// </summary>
		public double? MeanScore5 {
			get => (MeanScore.HasValue ? (MeanScore * 5d) : null);
			set => MeanScore = (value.HasValue ? (value / 5d) : null);
		}
		/// <summary>
		/// Gets the user's mean score for titles out of 1.
		/// </summary>
		public double? MeanScore { get; set; }
		/// <summary>
		/// Gets the user's standard deviation score for titles out of 100.
		/// </summary>
		public double? StandardDeviation100 {
			get => (StandardDeviation.HasValue ? (StandardDeviation * 100d) : null);
			set => StandardDeviation = (value.HasValue ? (value / 100d) : null);
		}
		/// <summary>
		/// Gets the user's standard deviation score for titles out of 10.
		/// </summary>
		public double? StandardDeviation10 {
			get => (StandardDeviation.HasValue ? (StandardDeviation * 10d) : null);
			set => StandardDeviation = (value.HasValue ? (value / 10d) : null);
		}
		/// <summary>
		/// Gets the user's standard deviation score for titles out of 5.
		/// </summary>
		public double? StandardDeviation5 {
			get => (StandardDeviation.HasValue ? (StandardDeviation * 5d) : null);
			set => StandardDeviation = (value.HasValue ? (value / 5d) : null);
		}
		/// <summary>
		/// Gets the user's standard deviation score for titles out of 1.
		/// </summary>
		public double? StandardDeviation { get; set; }

		/// <summary>
		/// Gets the number of titles currently being worked on.
		/// </summary>
		public int? Current { get; set; }
		/// <summary>
		/// Gets the number of titles currently being repeated.
		/// </summary>
		public int? Repeating { get; set; }
		/// <summary>
		/// Gets the number of completed titles.
		/// </summary>
		public int? Completed { get; set; }
		/// <summary>
		/// Gets the number of paused/on-hold titles.
		/// </summary>
		public int? OnHold { get; set; }
		/// <summary>
		/// Gets the number of dropped titles.
		/// </summary>
		public int? Dropped { get; set; }
		/// <summary>
		/// Gets the number of planned titles.
		/// </summary>
		public int? Planning { get; set; }
		/// <summary>
		/// Gets the total number of currently being worked on, including repeating.
		/// </summary>
		public int? TotalCurrent {
			get => ((Current.HasValue || Repeating.HasValue) ? ((Current ?? 0) + (Repeating ?? 0)) : (int?) null);
		}
		/// <summary>
		/// Gets the total number of completed titles, including repeating.
		/// </summary>
		public int? TotalCompleted {
			get => ((Completed.HasValue || Repeating.HasValue) ? ((Completed ?? 0) + (Repeating ?? 0)) : (int?) null);
		}
		/// <summary>
		/// Gets the total number of entries in the list.
		/// </summary>
		public int? TotalEntries {
			get {
				if (Current.HasValue || Completed.HasValue || OnHold.HasValue ||
					Dropped.HasValue || Planning.HasValue || Repeating.HasValue)
					return	(Current ?? 0) + (Completed ?? 0) + (OnHold ?? 0) +
							(Dropped ?? 0) + (Planning ?? 0) + (Repeating ?? 0);
				return null;
			}
		}

		/// <summary>
		/// Gets the list of displayable fields in the list embed.
		/// </summary>
		/// <param name="infoLevel">The level of information to display.</param>
		/// <returns>The list of fields to display.</returns>
		public override IEnumerable<string> GetFields(InfoLevel infoLevel) {
			if (TimeSpent.HasValue) {
				if (MeanScore.HasValue)
					yield return $"Days: {TimeSpent.Value.TotalDays:N1} | Mean: {MeanScore10.Value:N2}";
				else
					yield return $"Days: {TimeSpent.Value.TotalDays:N1}";
			}
			else if (MeanScore.HasValue)
				yield return $"Mean: {MeanScore10.Value:N2}";

			var info = ListType.ToInfo();

			if (infoLevel == InfoLevel.All) {
				if (TotalCurrent.HasValue)
					yield return $"{info.StatusNames[ListStatus.Current]}: {TotalCurrent:N0}";
			}
			if (TotalCompleted.HasValue)
				yield return $"{info.StatusNames[ListStatus.Completed]}: {TotalCompleted:N0}";
			if (infoLevel == InfoLevel.All) {
				if (OnHold.HasValue)
					yield return $"{info.StatusNames[ListStatus.OnHold]}: {OnHold:N0}";
				if (Dropped.HasValue)
					yield return $"{info.StatusNames[ListStatus.Dropped]}: {Dropped:N0}";
				if (Planning.HasValue)
					yield return $"{info.StatusNames[ListStatus.Planning]}: {Planning:N0}";
			}
		}
	}
}
