using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace USI_MultipleMatch {
	class Learner {
		public string name;
		public string enginename;
		public List<string> options;
		public string learner_path;
		const double eval_learn_border = 200;
		public Learner(string settingpath) {
			//1行目:name 2行目:path 3行目~:option
			using (StreamReader reader = new StreamReader(settingpath)) {
				name = reader.ReadLine();
				enginename = reader.ReadLine();
				learner_path = reader.ReadLine();
				options = new List<string>();
				while (!reader.EndOfStream) {
					string option = reader.ReadLine();
					if (option != "") {
						options.Add(option);
					}
				}
			}
		}
		public Learner(string learner_path, string name) {
			this.name = name;
			this.learner_path = learner_path;
			options = new List<string>();
			using (Process engine = new Process()) {
				engine.StartInfo.UseShellExecute = false;
				engine.StartInfo.RedirectStandardOutput = true;
				engine.StartInfo.RedirectStandardInput = true;
				engine.StartInfo.FileName = learner_path;
				engine.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(learner_path);
				//pathからエンジンを起動してusiコマンドを入力
				engine.Start();
				engine.StandardInput.WriteLine("usi");
				while (true) {
					string usi = engine.StandardOutput.ReadLine();
					var tokens = usi.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					switch (tokens[0]) {
						case "id":
							if (tokens[1] == "name") {
								enginename = tokens[2];
							}
							break;
						case "option":
							options.Add($"{tokens[2]} {tokens[4]} {tokens[6]}");
							break;
						case "usiok":
							engine.StandardInput.WriteLine("quit");
							Console.WriteLine($"player {name}'s infomation have been aquired.");
							return;
					}
				}
			}
		}
		public void settingsave(string settingpath) {//player.txtにPlayerの情報を書き込む
			using (StreamWriter writer = new StreamWriter(settingpath, false)) {
				writer.WriteLine(name);
				writer.WriteLine(enginename);
				writer.WriteLine(learner_path);
				foreach (string option in options) {
					writer.WriteLine(option);
				}
			}
		}
		public void Start(Process process, bool redirectSO) {
			if (process == null) throw new IOException("Process is Null.");
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = redirectSO;
			process.StartInfo.RedirectStandardInput = true;
			process.StartInfo.RedirectStandardError = false;
			process.StartInfo.ErrorDialog = true;
			process.StartInfo.FileName = learner_path;
			process.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(learner_path);
			process.Start();
			foreach (string usi in options) process.StandardInput.WriteLine(setoptionusi(usi));
			process.StandardInput.WriteLine("init");
			while (redirectSO) { if (process.StandardOutput.ReadLine() == "readyok") break; }



		}

		public void Start(Process process) {
			Start(process, true);
		}



		public void Learn(Result result, bool player_teban, string startsfen, List<string> kifu, List<int> evals) {
			using Process engine = new Process();
			Start(engine, false);
			engine.StandardInput.WriteLine("learnbykifu");

			//棋譜入力
			engine.StandardInput.Write("position ");
			engine.StandardInput.Write(startsfen);
			engine.StandardInput.Write(" moves");
			foreach (var move in kifu) {
				engine.StandardInput.Write(" ");
				engine.StandardInput.Write(move);
			}
			engine.StandardInput.WriteLine();

			//手番(s/g)
			if (player_teban) {
				engine.StandardInput.WriteLine("s");
			}
			else {
				engine.StandardInput.WriteLine("g");
			}

			//勝敗(s/g/d)
			switch (result) {
				case Result.SenteWin: engine.StandardInput.WriteLine("s"); break;
				case Result.Repetition:
				case Result.GoteWin: engine.StandardInput.WriteLine("g"); break;
				case Result.Draw: engine.StandardInput.WriteLine("d"); break;
			}

			//学習開始手数 （初形は0とする）
			int startcount = 0;
			for (int n = 0; n < evals.Count; n++) {
				if (Math.Abs(evals[n]) >= eval_learn_border) {
					startcount = n;
					break;
				}
			}
			engine.StandardInput.WriteLine((startcount).ToString());
			Console.WriteLine($"learning start from {startcount} moves.");

			System.Threading.Tasks.Task.Delay(1).Wait();
			engine.StandardInput.WriteLine("n");
			engine.StandardInput.WriteLine("quit");

			engine.WaitForExit();
			/*
			while (true) {
				if (engine.HasExited) {
					Console.WriteLine("ERROR: ENGINE LOST");
					return;
				}
				string str = engine.StandardOutput.ReadLine();
				if(str != null && str!="") Console.WriteLine(str);
				if (str == "learning end.") break;
			}
			*/
			engine.Close();
		}

		static string setoptionusi(string settingline) {
			var token = settingline.Split(' ');
			return $"setoption name {token[0]} value {token[2]}";
		}

		public void save_eval(string folderpath) {
			using var proc = new Process();
			Start(proc);
			proc.StandardInput.WriteLine($"saveparam {Path.GetFullPath(folderpath)}");
			while (true) {
				string str = proc.StandardOutput.ReadLine();
				Console.WriteLine(str);
				if (str == "saveparam done.") break;
			}
			proc.StandardInput.WriteLine("quit");
			proc.Close();
		}
	}
}
