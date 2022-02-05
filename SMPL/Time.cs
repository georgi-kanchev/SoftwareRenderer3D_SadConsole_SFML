using SFML.System;
using System;

namespace SMPL.Profiling
{
	public static class Time
	{
		public struct UnitDisplay
		{
			public bool AreSkipped { get; set; }
			public string Display { get; set; }

			public UnitDisplay(bool areSkipped = false, string display = "")
			{
				Display = display;
				AreSkipped = areSkipped;
			}
		}
		public struct Format
		{
			public string Separator { get; set; }
			public UnitDisplay Hours { get; set; }
			public UnitDisplay Minutes { get; set; }
			public UnitDisplay Seconds { get; set; }
			public UnitDisplay Milliseconds { get; set; }

			public Format(UnitDisplay hours = new(), UnitDisplay minutes = new(), UnitDisplay seconds = new(),
				UnitDisplay milliseconds = new(), string separator = ":")
			{
				Hours = hours;
				Minutes = minutes;
				Seconds = seconds;
				Milliseconds = milliseconds;
				Separator = separator;
			}
		}
		public enum ChoiceConvertion
		{
			MillisecondsToSeconds, MillisecondsToMinutes,
			SecondsToMilliseconds, SecondsToMinutes, SecondsToHours,
			MinutesToMilliseconds, MinutesToSeconds, MinutesToHours, MinutesToDays,
			HoursToSeconds, HoursToMinutes, HoursToDays, HoursToWeeks,
			DaysToMinutes, DaysToHours, DaysToWeeks,
			WeeksToHours, WeeksToDays
		}
		public enum ChoiceUnit { Frame, Second }

		internal static Clock time = new(), delta = new(), updateFPS = new();
		public static float Clock { get; internal set; }
		public static float Delta { get; internal set; }
		public static float FPS { get; internal set; }
		public static float FPSAverage { get; internal set; }
		public static float GameClock { get; internal set; }
		public static uint FrameCount { get; internal set; }

		internal static void Update()
		{
			GameClock = time.ElapsedTime.AsSeconds();
			Delta = delta.ElapsedTime.AsSeconds();
			delta.Restart();
			Clock = (float)DateTime.Now.TimeOfDay.TotalSeconds;
			if (updateFPS.ElapsedTime.AsSeconds() > 0.1f)
			{
				updateFPS.Restart();
				FPS = 1f / Delta;
				FPSAverage = FrameCount / GameClock;
			}
			FrameCount++;
		}

		public static string ToTimeText(this float seconds, Format format = new())
		{
			seconds = seconds.Sign(false);
			var secondsStr = $"{seconds:F0}";
			var ms = 0;
			if (secondsStr.Contains('.'))
			{
				var spl = secondsStr.Split('.');
				ms = int.Parse(spl[1]) * 100;
				seconds = seconds.Round(toward: Extensions.ChoiceRoundWay.Down);
			}
			var sec = seconds % 60;
			var min = ToTime(seconds, Time.ChoiceConvertion.SecondsToMinutes) % 60;
			var hr = ToTime(seconds, Time.ChoiceConvertion.SecondsToHours);
			var msShow = !format.Milliseconds.AreSkipped;
			var secShow = !format.Seconds.AreSkipped;
			var minShow = !format.Minutes.AreSkipped;
			var hrShow = !format.Hours.AreSkipped;

			var sep = format.Separator == null || format.Separator == "" ? ":" : format.Separator;
			var msStr = msShow ? $"{ms:D2}" : "";
			var secStr = secShow ? $"{(int)sec:D2}" : "";
			var minStr = minShow ? $"{(int)min:D2}" : "";
			var hrStr = hrShow ? $"{(int)hr:D2}" : "";
			var msF = msShow ? $"{format.Milliseconds.Display}" : "";
			var secF = secShow ? $"{format.Seconds.Display}" : "";
			var minF = minShow ? $"{format.Minutes.Display}" : "";
			var hrF = hrShow ? $"{format.Hours.Display}" : "";
			var secMsSep = msShow && (secShow || minShow || hrShow) ? $"{sep}" : "";
			var minSecSep = secShow && (minShow || hrShow) ? $"{sep}" : "";
			var hrMinSep = minShow && hrShow ? $"{sep}" : "";

			return $"{hrStr}{hrF}{hrMinSep}{minStr}{minF}{minSecSep}{secStr}{secF}{secMsSep}{msStr}{msF}";
		}
		public static float ToTime(this float number, ChoiceConvertion convertType)
		{
			return convertType switch
			{
				Time.ChoiceConvertion.MillisecondsToSeconds => number / 1000,
				Time.ChoiceConvertion.MillisecondsToMinutes => number / 1000 / 60,
				Time.ChoiceConvertion.SecondsToMilliseconds => number * 1000,
				Time.ChoiceConvertion.SecondsToMinutes => number / 60,
				Time.ChoiceConvertion.SecondsToHours => number / 3600,
				Time.ChoiceConvertion.MinutesToMilliseconds => number * 60000,
				Time.ChoiceConvertion.MinutesToSeconds => number * 60,
				Time.ChoiceConvertion.MinutesToHours => number / 60,
				Time.ChoiceConvertion.MinutesToDays => number / 1440,
				Time.ChoiceConvertion.HoursToSeconds => number * 3600,
				Time.ChoiceConvertion.HoursToMinutes => number * 60,
				Time.ChoiceConvertion.HoursToDays => number / 24,
				Time.ChoiceConvertion.HoursToWeeks => number / 168,
				Time.ChoiceConvertion.DaysToMinutes => number * 1440,
				Time.ChoiceConvertion.DaysToHours => number * 24,
				Time.ChoiceConvertion.DaysToWeeks => number / 7,
				Time.ChoiceConvertion.WeeksToHours => number * 168,
				Time.ChoiceConvertion.WeeksToDays => number * 7,
				_ => 0,
			};
		}
	}
}
