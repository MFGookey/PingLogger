using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace PingLogger
{
	public class Program
	{
		private static bool _continueRunning;
		private readonly IPAddress _googleDNS;
		private readonly FileLogger _outageLogger;
		private int _sleepSeconds;
		private int _thresholdForOutage;

		public static void Main (string[] args)
		{
			Console.Clear ();
			Console.WriteLine ("Press q or Q to quit");
			ConsoleKeyInfo keyPress;

			_continueRunning = true;
			var program = new Program ();
			var runningProgram = new Thread (program.Run);
			runningProgram.Start ();
			do {
				keyPress = Console.ReadKey ();
			} while(keyPress.KeyChar.Equals ('q') == false && keyPress.KeyChar.Equals ('Q') == false);
				
			_continueRunning = false;
			Console.WriteLine ();
		}

		public Program(){
			_outageLogger = new FileLogger(@"c:\pinglogs", true);
			_googleDNS = IPAddress.Parse ("8.8.8.8");
			_thresholdForOutage = 10;
		}

		public void Run(){
			var pinger = new Ping ();
			var inOutage = false;
			int successes = 0;
			int failures = 0;
			int outages = 0;
			int failedPingCount = 0; // Count failures in a row, only report outage if above a threshold
			string lastStatus = "Nothing yet";
			RefreshUI ();
			while (_continueRunning) {
				UpdateStatus ("Pinging");
				try{
					var response = pinger.Send (_googleDNS);
					lastStatus = response.Status.ToString();
					if (response.Status != IPStatus.Success) {
						failures++;
						failedPingCount++;
						if (inOutage == false && failedPingCount >=_thresholdForOutage) {
							outages++;
							inOutage = true;
							_outageLogger.LogFormat ("!!! ACHTUNG STATUS={1}. {0}!!!", DateTime.Now, response.Status.ToString());
							_outageLogger.LogFormat ("Attempted destination: {0}", _googleDNS.ToString ());
						}
					} else {
						if (inOutage) {
							_outageLogger.LogFormat ("Recovered from outage at {0}.  Status: {1}", DateTime.Now, response.Status.ToString ());
							_outageLogger.Log("--------------------------------------------------");
							_outageLogger.Log(string.Empty);
							inOutage = false;
						}

						failedPingCount = 0;
						successes++;
					}
				}catch(Exception e){
					lastStatus = "Failure";
					failures++;
					if (inOutage == false && failedPingCount >= _thresholdForOutage) {
						outages++;
						inOutage = true;
					}
					failedPingCount++;
					_outageLogger.LogFormat ("!!! ACHTUNG EXCEPTION={1}. {0}!!!", DateTime.Now, e.Message);
					_outageLogger.LogFormat ("Attempted destination: {0}", _googleDNS.ToString ());
					_outageLogger.LogFormat ("Stack Trace:\n{0}", e.StackTrace);
				}
				UpdateUI (successes, failures, outages, lastStatus);
				UpdateStatus("Sleeping");
				if (inOutage || failedPingCount > 0) {
					_sleepSeconds = 1;
				} else {
					_sleepSeconds = 10;
				}

				for (int i = 0; i < _sleepSeconds && _continueRunning; i++) {
					Thread.Sleep (1000);
				}

				UpdateStatus("Awake");
			}
			_outageLogger.LogFormat ("Pinged {0} times, with {1} successes, and {2} failures over {3} outages.", (successes + failures), successes, failures, outages);
			_outageLogger.LogFormat ("Exiting at {0}", DateTime.Now);
			Console.WriteLine (); // For the q
			Console.WriteLine ("Press any key to exit...");
			Console.ReadKey ();
		}

		private void RefreshUI(){
			origRow = Console.CursorTop;
			origCol = Console.CursorLeft;
			Console.WriteLine ("Successes:");
			Console.WriteLine ();
			Console.WriteLine ("Failures:");
			Console.WriteLine ();
			Console.WriteLine ("Outages:");
			Console.WriteLine ();
			Console.WriteLine ("Total:");
			Console.WriteLine ();
			Console.WriteLine ("Last Result:");
			Console.WriteLine ();
			Console.WriteLine ("Currently:");
			Console.WriteLine ();
			UpdateUI (0, 0, 0, "Nothing yet");
		}
		int maxLastResultLength = 0;
		private void UpdateUI(int successes, int failures, int outages, string lastResult){
			WriteAt (successes.ToString (), 8, 1);
			WriteAt (failures.ToString (), 8, 3);
			WriteAt (outages.ToString (), 8, 5);
			WriteAt ((successes + failures).ToString (), 8, 7);
			if (lastResult.Length > maxLastResultLength) {
				maxLastResultLength = lastResult.Length;
			}

			if (lastResult.Length < maxLastResultLength) {
				for (int i = 0; i < (maxLastResultLength - lastResult.Length); i++) {
					WriteAt (" ", lastResult.Length + i + 8, 9);
				}
			}
			WriteAt (lastResult, 8, 9);
		}

		int maxStatusLength = 0;
		private void UpdateStatus(string status){
			if (status.Length > maxStatusLength) {
				maxStatusLength = status.Length;
			}
			WriteAt (status, 0, 11);
			if (status.Length < maxStatusLength) {
				for (int i = 0; i < (maxStatusLength - status.Length); i++) {
					WriteAt (" ", status.Length + i, 11);
				}
			}
		}

		protected static int origRow;
		protected static int origCol;

		protected static void WriteAt(string s, int x, int y)
		{
			try
			{
				Console.SetCursorPosition(origCol+x, origRow+y);
				Console.WriteLine(s);
			}
			catch (ArgumentOutOfRangeException e)
			{
				Console.Clear();
				Console.WriteLine(e.Message);
			}
		}
	}
}