using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using TimeZoneConverter;
using TimeZoneNames;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	/// <summary>
	/// A struct containing both a <see cref="TimeZoneInfo"/> and its associated IANA <see cref="string"/>.
	/// </summary>
	public struct IanaTimeZone {

		#region Fields

		/// <summary>
		/// Gets the IANA name of the time zone.
		/// </summary>
		public string Iana { get; }
		/// <summary>
		/// Gets the info of the time zone.
		/// </summary>
		public TimeZoneInfo TimeZone { get; }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructs the <see cref="IanaTimeZone"/> and gets the <see cref="TimeZoneInfo"/> from the IANA
		/// Id.
		/// </summary>
		/// <param name="iana">The IANA Id to get the <see cref="TimeZoneInfo"/> from.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="iana"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="iana"/> is not an existing IANA.
		/// </exception>
		public IanaTimeZone(string iana) {
			Iana = iana ?? throw new ArgumentNullException(nameof(iana));
			if (TZConvert.TryGetTimeZoneInfo(iana, out TimeZoneInfo timeZoneInfo))
				TimeZone = timeZoneInfo;
			else
				throw new ArgumentException($"\"{iana}\" is not an existing IANA!");
		}
		/// <summary>
		/// Constructs the <see cref="IanaTimeZone"/> from the IANA Id and <see cref="TimeZoneInfo"/>.
		/// </summary>
		/// <param name="iana">The IANA Id.</param>
		/// <param name="timeZone">The time zone info.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="iana"/> or <paramref name="timeZone"/> is null.
		/// </exception>
		public IanaTimeZone(string iana, TimeZoneInfo timeZone) {
			Iana = iana ?? throw new ArgumentNullException(nameof(iana));
			TimeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
		}

		#endregion

		#region Object Overrides

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj"> The object to compare with the current instance.</param>
		/// <returns>
		/// true if obj and this instance are the same type and represent the same value; otherwise, false.
		/// </returns>
		public override bool Equals(object obj) {
			if (obj is IanaTimeZone ianaTimeZone)
				return this == ianaTimeZone;
			return base.Equals(obj);
		}
		/// <summary>
		/// Returns the hash code for the IANA.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode() => Iana.GetHashCode();
		/// <summary>
		/// Returns the string representation of this time zone, which is the IANA.
		/// </summary>
		/// <returns>The string representation of this time zone.</returns>
		public override string ToString() => Iana;

		#endregion

		#region Operators

		public static bool operator ==(IanaTimeZone a, IanaTimeZone b) {
			return string.Compare(a.Iana, b.Iana, true) == 0;
		}
		public static bool operator !=(IanaTimeZone a, IanaTimeZone b) {
			return string.Compare(a.Iana, b.Iana, true) != 0;
		}

		#endregion
	}
	/// <summary>
	/// A list of <see cref="IanaTimeZone"/>s that match a single abbreviation.
	/// </summary>
	public class TimeZoneAbbreviationMatches {
		
		#region Fields

		/// <summary>
		/// The abbreviation of these time zones.
		/// </summary>
		public string Abbreviation { get; }
		/// <summary>
		/// Gets the list of IANA time zones that match this abbreviation.
		/// </summary>
		public IReadOnlyList<IanaTimeZone> TimeZones { get; }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructs the Iana Time Zone abbreviation matches.
		/// </summary>
		/// <param name="abbreviation">The abbreviation for the for the matches.</param>
		/// <param name="ianaTimeZones">The time zones that match this abbreviation.</param>
		internal TimeZoneAbbreviationMatches(string abbreviation, IEnumerable<IanaTimeZone> ianaTimeZones) {
			Abbreviation = abbreviation;
			TimeZones = ianaTimeZones.ToImmutableArray();
		}
		/// <summary>
		/// Constructs the Iana Time Zone abbreviation matches.
		/// </summary>
		/// <param name="pair">The key is the abbreviation and the Value is the list of IANA time zones.</param>
		/// <param name="ianaTimeZones">The time zones that match this abbreviation.</param>
		internal TimeZoneAbbreviationMatches(KeyValuePair<string, HashSet<IanaTimeZone>> pair) {
			Abbreviation = pair.Key;
			TimeZones = pair.Value.ToImmutableArray();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets if there is more than one time zone result.
		/// </summary>
		public bool IsAmbiguous => TimeZones.Count > 1;
		/// <summary>
		/// Gets the first and only timezone. Throws an exception if <see cref="IsAmbiguous"/> is true.
		/// </summary>
		/// 
		/// <exception cref="InvalidOperationException">
		/// More than one time zone exists, thus <see cref="IsAmbiguous"/> is true.
		/// </exception>
		public IanaTimeZone TimeZone {
			get {
				if (IsAmbiguous)
					throw new InvalidOperationException($"Cannot get single TimeZone when {nameof(IsAmbiguous)} is true!");
				return TimeZones.First();
			}
		}
		/// <summary>
		/// Gets the number of time zone matches.
		/// </summary>
		public int Count => TimeZones.Count;

		#endregion
	}

	public class TimeZoneService {

		#region enum DaylightSavings

		private enum DaylightSavings {
			Generic,
			Standard,
			Daylight,
		}
		private static readonly DaylightSavings[] AllDaylightSavings = {
			DaylightSavings.Generic,
			DaylightSavings.Standard,
			DaylightSavings.Daylight,
		};

		#endregion

		#region Fields
		
		/// <summary>
		/// The collection of IanaTimeZone ambiguities for abbreviations.
		/// </summary>
		private readonly Dictionary<string, TimeZoneAbbreviationMatches> Abbreviations =
			new Dictionary<string, TimeZoneAbbreviationMatches>(StringComparer.OrdinalIgnoreCase);

		#endregion

		#region Constructors

		public TimeZoneService() {
			Dictionary<string, HashSet<IanaTimeZone>> abbreviations = new Dictionary<string, HashSet<IanaTimeZone>>();
			foreach (string countryCode in TZNames.GetCountryNames("en-US").Keys) {
				foreach (string iana in TZNames.GetTimeZonesForCountry(countryCode, "en-US").Keys) {
					AddAbbreviations(abbreviations, iana);
				}
			}
			// Convert the abbreviations to TimeZoneAbbreviationMatches
			foreach (var abbreviation in abbreviations) {
				Abbreviations.Add(abbreviation.Key, new TimeZoneAbbreviationMatches(abbreviation));
			}
		}

		#endregion

		#region Public Parsing

		/// <summary>
		/// Parses and returns the <see cref="TimeZoneInfo"/> from an abbreviation or IANA Id.
		/// </summary>
		/// <param name="input">The input string.</param>
		/// <param name="ambiguities">
		/// The output abbreviation ambiguities if an abbreviation match was found.
		/// </param>
		/// <returns>The matching <see cref="TimeZoneInfo"/> or null.</returns>
		public TimeZoneInfo ParseTimeZone(string input, out TimeZoneAbbreviationMatches ambiguities) {
			ambiguities = null;
			input = input.Trim();
			if (Abbreviations.TryGetValue(input, out ambiguities)) {
				if (!ambiguities.IsAmbiguous)
					return ambiguities.TimeZone.TimeZone;
			}
			else {
				// Try to get the Iana Id. Format the spaces into the proper underscores.
				input = string.Join("_", input.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries));
				if (TZConvert.TryGetTimeZoneInfo(input, out var timeZoneInfo))
					return timeZoneInfo;
			}
			return null;
		}
		/// <summary>
		/// Parses and returns the <see cref="TimeZoneInfo"/> from an abbreviation.
		/// </summary>
		/// <param name="abbreviation">The abbreviation string.</param>
		/// <param name="index">The index of the abbreviation in the list.</param>
		/// <returns>The matching <see cref="TimeZoneInfo"/> or null.</returns>
		public TimeZoneInfo ParseTimeZoneAbbreviation(string abbreviation, int index) {
			if (Abbreviations.TryGetValue(abbreviation.Trim(), out TimeZoneAbbreviationMatches ambiguities) &&
				index >= 1 && index <= ambiguities.Count)
			{
				return ambiguities.TimeZones[index - 1].TimeZone;
			}
			return null;
		}

		#endregion

		#region Private Building

		/// <summary>
		/// Gets the <see cref="TimeZoneValues"/> name from the <see cref="DaylightSavings"/> type.
		/// </summary>
		/// <param name="values">The time zone values to get the name for.</param>
		/// <param name="daylight">The daylight savings name to get.</param>
		/// <returns>The daylight savings name of the specified type.</returns>
		private string GetDaylight(TimeZoneValues values, DaylightSavings daylight) {
			switch (daylight) {
			case DaylightSavings.Generic: return values.Generic;
			case DaylightSavings.Standard: return values.Standard;
			case DaylightSavings.Daylight: return values.Daylight;
			default: throw new ArgumentException(nameof(daylight));
			}
		}
		/// <summary>
		/// Adds all abbreviations for a specific IANA Id.
		/// </summary>
		/// <param name="abbreviations">The abbreviation collection to add to.</param>
		/// <param name="iana">The IANA Id.</param>
		private void AddAbbreviations(Dictionary<string, HashSet<IanaTimeZone>> abbreviations, string iana) {
			var abbreviationDaylights = TZNames.GetAbbreviationsForTimeZone(iana, "en-US");
			var nameDaylights = TZNames.GetNamesForTimeZone(iana, "en-US");

			// Loop through each of the daylight savings types and get the abbreviation for each.
			foreach (var daylight in AllDaylightSavings) {
				//for (DaylightSavings daylight = AllDaylightSavings.Generic; daylight <= AllDaylightSavings.Daylight; daylight++) {
				string abbreviation = GetDaylight(abbreviationDaylights, daylight);
				string name = GetDaylight(nameDaylights, daylight);
				if (abbreviation == null)
					continue; // No daylight savings form for these abbreviation values.
							  // TODO: Check reason for name
				if (name == null)
					continue;
				// Fails with Antarctica/Troll
				if (!TZConvert.TryGetTimeZoneInfo(iana, out TimeZoneInfo timeZoneInfo))
					continue;
				if (!abbreviations.TryGetValue(abbreviation, out var ianaHashSet)) {
					ianaHashSet = new HashSet<IanaTimeZone>();
					abbreviations.Add(abbreviation, ianaHashSet);
				}
				ianaHashSet.Add(new IanaTimeZone(iana, timeZoneInfo));
			}
		}

		#endregion
	}
}
