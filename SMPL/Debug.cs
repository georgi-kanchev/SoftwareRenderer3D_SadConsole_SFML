using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using SadConsole.Input;
using SadRogue.Primitives;

namespace SMPL.Profiling
{
	public static class Debug
	{
		public struct MethodInfo
		{
			public bool IncludePath { get; set; }
			public bool IncludeClass { get; set; }
			public bool IncludeReturnTypePath { get; set; }
			public bool IncludeReturnTypeClass { get; set; }
			public bool IncludeReturnTypeName { get; set; }
			public bool IncludeName { get; set; }
			public bool IncludeParamsTypePath { get; set; }
			public bool IncludeParamsTypeClass { get; set; }
			public bool IncludeParamsTypeName { get; set; }
		}

		const int width = 60, height = 10;

		internal static Window logWindow;
		internal static ListBox logList;
		internal static Button closeButton, clearButton;

		internal static List<Type> TypesSMPL;
		internal static List<Type> TypesUser;

		/// <summary>
		/// Wether the game is started from Visual Studio or its '.exe' file.
		/// </summary>
		public static bool IsActive { get { return Debugger.IsAttached; } }
		private static Keys hotkey;
		public static Keys ShowWindowHotkey
		{
			get => hotkey;
			set
			{
				hotkey = value;
				Log($"Press [{hotkey}] to hide or show the Debug Logs.");
			}
		}

		static Debug()
		{
			TypesSMPL = Assembly.GetCallingAssembly().GetTypes().ToList();
			TypesUser = Assembly.GetEntryAssembly().GetTypes().ToList();

			logWindow = new(width, height) { Title = "Debug Logs", TitleAlignment = HorizontalAlignment.Left };
			logList = new(width - 4, height - 2) { Position = new(2, 1) };
			closeButton = new(3) { Text = "X", Position = new(width - 3, 0) };
			clearButton = new(7) { Text = "Clear", Position = new(width - 11, 0) };
			closeButton.Click += CloseLogs;
			clearButton.Click += ClearLogs;
			Game.Instance.FrameUpdate += OnUpdate;
			logWindow.PositionChanged += OnDragged;
			logList.SelectedItemExecuted += OnLogClick;

			logWindow.Controls.Add(closeButton);
			logWindow.Controls.Add(clearButton);
			logWindow.Controls.Add(logList);

			ShowWindowHotkey = Keys.Tab;
			Log("Double-click [LMB] on a log to remove it.");
		}

		private static void OnLogClick(object sender, ListBox.SelectedItemEventArgs e) => logList.Items.Remove(e.Item);
		private static void OnDragged(object sender, ValueChangedEventArgs<Point> e) => logWindow.KeepInConsole(Simple.Console);
		private static void CloseLogs(object sender, EventArgs e) => logWindow.Hide();
		private static void ClearLogs(object sender, EventArgs e) => logList.Items.Clear();
		private static void OnUpdate(object sender, GameHost e)
		{
			if (Game.Instance.Keyboard.IsKeyPressed(ShowWindowHotkey))
				logWindow.IsVisible = logWindow.IsVisible == false;
		}

		public static uint CurrentLineNumber(int depth = 0)
		{
			if (IsCalledByUser(depth) == false)
				return default;
			var info = new StackFrame(depth + 1, true);
			return (uint)info.GetFileLineNumber();
		}
		public static string CurrentMethodName(MethodInfo methodInfo, int depth = 0)
		{
			if (IsCalledByUser(depth) == false)
				return default;
			var info = new StackFrame(depth + 1, true);
			var method = info?.GetMethod();
			if (method == null || method.DeclaringType == null)
				return null;

			var result = "";
			var temp = method?.ToString();

			var returnType = Get(temp.Split()[0]);
			var paramsSplit = temp.Replace(")", "").Split("(");
			var name = Get($"{method.DeclaringType}.{method.Name}");
			var includeParams = methodInfo.IncludeParamsTypePath || methodInfo.IncludeParamsTypeClass || methodInfo.IncludeParamsTypeName;
			var cl = method.DeclaringType.Name;

			returnType[0] = methodInfo.IncludeReturnTypePath ? returnType[0] : "";
			returnType[1] = methodInfo.IncludeReturnTypeClass ? returnType[1] : "";
			returnType[2] = methodInfo.IncludeReturnTypeName ? returnType[2] : "";


			name[0] = methodInfo.IncludePath ? name[0] : "";
			name[1] = methodInfo.IncludeClass ? name[1] : "";
			name[2] = methodInfo.IncludeName ? name[2] : "";

			result += $"{returnType[0]}{returnType[1]}{returnType[2]}";
			result += result == "" ? "" : " ";
			result += $"{name[0]}{name[1]}{name[2]}";
			for (int i = 1; i < paramsSplit.Length; i++)
			{
				var param = Get(paramsSplit[i]);
				param[0] = methodInfo.IncludeParamsTypePath ? param[0] : "";
				param[1] = methodInfo.IncludeParamsTypeClass ? param[1] : "";
				param[2] = methodInfo.IncludeParamsTypeName ? param[2] : "";

				result += includeParams ? "(" : "";
				result += $"{param[0]}{param[1]}{param[2]}";
				result += includeParams ? ")" : "";
			}

			return method == default ? default : Edit(result);

			string Edit(string name) => name.Replace(".ctor", $"{cl}.{cl}").Replace("ctor", $"new {cl}")
				.Replace("set_", "[SET]").Replace("get_", "[GET]".Replace('+', '.')).Replace("..", ".");
			string[] Get(string full)
			{
				var split = full.Contains(".") ? full.Split(".") : default;
				var name = split == default ? default : split[^1];
				var cl = split == default ? default : split[^2];
				var namesp = split == default ? default :
					full.Replace($".{name}", "").Replace($".{cl}", "");
				if (name == null)
					name = full;

				var result = new string[3];
				result[0] = namesp == null ? null : namesp + ".";
				result[1] = cl == null ? null : cl + ".";
				result[2] = name;
				return result;
			}
		}
		public static string CurrentFileName(int depth = 0)
		{
			if (IsCalledByUser(depth) == false)
				return default;
			var pathRaw = CurrentFilePath(depth + 1);
			if (pathRaw == null) return null;
			var path = pathRaw.Split('\\');
			var name = path[^1].Split('.');
			return name[0];
		}
		public static string CurrentFilePath(int depth = 0)
		{
			if (IsCalledByUser(depth) == false)
				return default;
			var info = new StackFrame(depth + 1, true);
			return info.GetFileName();
		}
		public static string CurrentFileDirectory(int depth = 0)
		{
			if (IsCalledByUser(depth) == false)
				return default;
			var fileName = new StackFrame(depth + 1, true).GetFileName();
			if (fileName == default)
				return default;
			var path = fileName.Split('\\');
			var dir = "";
			for (int i = 0; i < path.Length - 1; i++)
			{
				dir += path[i];
				if (i == path.Length - 2)
					continue;
				dir += "\\";
			}
			return dir;
		}

		public static void Log(object message)
		{
			if (IsActive == false)
				return;

			var str = message.ToString();
			var newLine = "";
			if (str.Length >= width - 4)
			{
				var noWordNewLine = true;
				for (int i = width - 4; i >= 0; i--)
					if (str[i] == ' ')
					{
						newLine = str[(i + 1)..];
						str = str.Substring(0, i);
						noWordNewLine = false;
						break;
					}
				if (noWordNewLine)
				{
					newLine = str[(width - 1)..];
					str = str.Substring(0, width - 1);
				}
			}

			logWindow.Show();
			logList.Items.Add(str);
			logList.SelectedIndex = logList.Items.Count - 1;
			logList.Update(TimeSpan.Zero);
			logList.ScrollToSelectedItem();
			logList.SelectedIndex = -1;

			if (newLine != "")
				Log(newLine);
		}
		public static void LogError(int depth, string description)
		{
			if (IsActive == false)
				return;

			var longestMethod = 0;
			var methods = new List<string>();
			var actions = new List<string>();

			if (depth >= 0)
				for (int i = 0; i < 100; i++)
					Add(depth + i + 1);

			Log($"[!] Error at");
			for (int i = methods.Count - 1; i >= 0; i--)
				Log($"[!] - {actions[i]}");
			Log($"[!] {description}");

			void Add(int depth)
			{
				if (depth < 0)
					return;
				var action = CurrentMethodName(new() { IncludeName = true, IncludeClass = true }, depth);
				if (string.IsNullOrEmpty(action))
					return;
				var methodName = $"{CurrentMethodName(new() { IncludeName = true, IncludeClass = true }, depth + 1)}()";

				longestMethod = methodName.Length > longestMethod ? methodName.Length : longestMethod;

				methods.Add(methodName);
				actions.Add($"{action}()");
			}
		}
		public static void Clear() => logWindow.Clear();

		private static bool IsCalledByUser(int depth)
		{
			var info = new StackFrame(depth + 2, true);
			var method = info.GetMethod();
			if (method == null || method.DeclaringType == null)
				return true;

			var parentInfo = new StackFrame(depth + 3, true);
			var parentMethod = parentInfo.GetMethod();
			return parentMethod == null || parentMethod.DeclaringType == null ||
				(parentMethod.DeclaringType == typeof(Event) && parentMethod.Name == "Trigger") ||
				TypesUser.Contains(parentMethod.DeclaringType);
		}
	}
}
