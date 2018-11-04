using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles {
	/// <summary>
	/// The interface for a media database user list.
	/// </summary>
	public interface IMediaList : IDatabaseUserList {
		/// <summary>
		/// Gets the time spent on media of this list type.
		/// </summary>
		TimeSpan? TimeSpent { get; }
		/// <summary>
		/// Gets the days spent on media of this list type.
		/// </summary>
		double? DaysSpent { get; }
		/// <summary>
		/// Gets the hours spent on media of this list type.
		/// </summary>
		double? HoursSpent { get; }
		/// <summary>
		/// Gets the minutes spent on media of this list type.
		/// </summary>
		double? MinutesSpent { get; }
		/// <summary>
		/// Gets the seconds spent on media of this list type.
		/// </summary>
		double? SecondsSpent { get; }

		/// <summary>
		/// Gets the user's mean score for titles out of 100.
		/// </summary>
		double? MeanScore100 { get; }
		/// <summary>
		/// Gets the user's mean score for titles out of 10.
		/// </summary>
		double? MeanScore10 { get; }
		/// <summary>
		/// Gets the user's mean score for titles out of 5.
		/// </summary>
		double? MeanScore5 { get; }
		/// <summary>
		/// Gets the user's mean score for titles out of 1.
		/// </summary>
		double? MeanScore { get; }
		/// <summary>
		/// Gets the user's standard deviation score for titles out of 100.
		/// </summary>
		double? StandardDeviation100 { get; }
		/// <summary>
		/// Gets the user's standard deviation score for titles out of 10.
		/// </summary>
		double? StandardDeviation10 { get; }
		/// <summary>
		/// Gets the user's standard deviation score for titles out of 5.
		/// </summary>
		double? StandardDeviation5 { get; }
		/// <summary>
		/// Gets the user's standard deviation score for titles out of 1.
		/// </summary>
		double? StandardDeviation { get; }

		/// <summary>
		/// Gets the number of titles currently being worked on.
		/// </summary>
		int? Current { get; }
		/// <summary>
		/// Gets the number of titles currently being repeated.
		/// </summary>
		int? Repeating { get; }
		/// <summary>
		/// Gets the number of completed titles.
		/// </summary>
		int? Completed { get; }
		/// <summary>
		/// Gets the number of paused/on-hold titles.
		/// </summary>
		int? OnHold { get; }
		/// <summary>
		/// Gets the number of dropped titles.
		/// </summary>
		int? Dropped { get; }
		/// <summary>
		/// Gets the number of planned titles.
		/// </summary>
		int? Planning { get; }
		/// <summary>
		/// Gets the total number of currently being worked on, including repeating.
		/// </summary>
		int? TotalCurrent { get; }
		/// <summary>
		/// Gets the total number of completed titles, including repeating.
		/// </summary>
		int? TotalCompleted { get; }
		/// <summary>
		/// Gets the total number of entries in the list.
		/// </summary>
		int? TotalEntries { get; }
	}
}
