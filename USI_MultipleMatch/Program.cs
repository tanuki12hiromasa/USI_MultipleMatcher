﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;
namespace USI_MultipleMatch
{
	class Program
	{
		enum Result
		{
			SenteWin, GoteWin, Repetition, Draw
		}
		static bool alive;
		static List<(uint byoyomi, uint rounds)> matchlist;
		public static uint drawMoves = 400;
		static void Main(string[] args) {
			alive = true;
			matchlist = new List<(uint byoyomi, uint rounds)>();
			Console.WriteLine("連続対局プログラム");
			while (alive) {
				Console.Write("command?(r/m/s/k/q) > ");
				switch (Console.ReadLine()) {
					case "register":
					case "r":
						register();
						break;
					case "makematch":
					case "m":
						makematch();
						break;
					case "start":
					case "s":
						start();
						break;
					case "kifutocsa":
					case "k":
						Kifu.KifulineToCSA();
						break;
					case "quit":
					case "q":
						alive = false;
						break;
				}
			}
			Console.WriteLine("Program finished.");
		}

		static void register() {
			string p = " ";
			while (p != "a" && p != "A" && p != "b" && p != "B") {
				Console.Write("A or B? > ");
				p = Console.ReadLine();
			}
			StreamWriter writer;
			if (p == "a" || p == "A") {
				writer = new StreamWriter(@"./PlayerA.txt", false);
			}
			else {
				writer = new StreamWriter(@"./PlayerB.txt", false);
			}
			while (true) {
				Console.Write("input resisterd usi engine's path > ");
				string path = Console.ReadLine();
				try {
					using (Process engine = new Process()) {
						engine.StartInfo.UseShellExecute = false;
						engine.StartInfo.RedirectStandardOutput = true;
						engine.StartInfo.RedirectStandardInput = true;
						engine.StartInfo.FileName = path;
						engine.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(path);
						//pathからエンジンを起動してusiコマンドを入力
						engine.Start();
						engine.StandardInput.WriteLine("usi");
						while (true) {
							string usi = engine.StandardOutput.ReadLine();
							var tokens = usi.Split(' ');
							switch (tokens[0]) {
								case "id":
									if (tokens[1] == "name") {
										writer.WriteLine(tokens[2]);
										writer.WriteLine(path);
										writer.WriteLine("USI_Ponder check false");
										writer.WriteLine("USI_Hash spin 256");
									}
									break;
								case "option":
									writer.WriteLine($"{tokens[2]} {tokens[4]} {tokens[6]}");
									break;
								case "usiok":
									writer.Close();
									Console.WriteLine("register ok.");
									return;
							}
						}
					}
				}
				catch (Exception e) {
					Console.WriteLine(e.Message);
				}
			}
		}
		static void makematch() {
			if (matchlist.Count != 0) {
				Console.Write("matchlist ");
				foreach (var match in matchlist) {
					Console.Write($"[{match.byoyomi}ms,{match.rounds}回] ");
				}
				Console.WriteLine("");
				Console.WriteLine("is exist. Do you want to delete this matchlist sure?");
				Console.Write("(y/n) > ");
				if (Console.ReadLine() == "y") {
					matchlist.Clear();
					Console.WriteLine("matchlist deleted.");
				}
				else {
					Console.WriteLine("cancelled.");
					return;
				}
			}
			while (true) {
				Console.WriteLine($"match {matchlist.Count + 1}");
				Console.Write("1手の考慮時間?(ミリ秒) > ");
				uint byo = uint.Parse(Console.ReadLine());
				Console.Write("対戦回数? > ");
				uint times = uint.Parse(Console.ReadLine());
				Console.Write($"ok?(y/n) > ");
				if (Console.ReadLine() == "y")
					matchlist.Add((byo, times));
				Console.Write("current matchlist is ");
				if (matchlist.Count == 0) Console.Write("empty");
				foreach (var match in matchlist) {
					Console.Write($"[{match.byoyomi}ms,{match.rounds}回] ");
				}
				Console.WriteLine(".");
				Console.Write("continue?(y/n) > ");
				if (Console.ReadLine() != "y") {
					Console.Write("matchlist ");
					foreach (var match in matchlist) {
						Console.Write($"[{match.byoyomi}ms,{match.rounds}回] ");
					}
					Console.WriteLine("is registered.");
					break;
				}
			}
		}
		static void start() {
			if (matchlist.Count <= 0) {
				Console.WriteLine("error: matchlist not exist");
				return;
			}
			//A,Bの設定を読み込む
			string a_path, b_path;
			string a_name, b_name;
			var a_option = new List<string>();
			var b_option = new List<string>();
			try {
				using (StreamReader readerA = new StreamReader(@"./PlayerA.txt")) {
					a_name = readerA.ReadLine();
					Console.WriteLine($"playerA: {a_name}");
					a_path = readerA.ReadLine();
					Console.WriteLine(a_path);
					for (string sline = readerA.ReadLine(); sline != null && sline != ""; sline = readerA.ReadLine()) {
						a_option.Add(setoptionusi(sline));
						Console.WriteLine(a_option[a_option.Count - 1]);
					}
				}
			}
			catch (Exception e) {
				Console.WriteLine("error in load playerA setting");
				Console.WriteLine(e.Message);
				return;
			}
			try {
				using (StreamReader readerB = new StreamReader(@"./PlayerB.txt")) {
					b_name = readerB.ReadLine();
					Console.WriteLine($"playerB: {b_name}");
					b_path = readerB.ReadLine();
					Console.WriteLine(b_path);
					for (string sline = readerB.ReadLine(); sline != null && sline != ""; sline = readerB.ReadLine()) {
						b_option.Add(setoptionusi(sline));
						Console.WriteLine(b_option[b_option.Count - 1]);
					}
				}
			}
			catch (Exception e) {
				Console.WriteLine("error in load playerB setting");
				Console.WriteLine(e.Message);
				return;
			}
			//
			string exptitle = null;
			while (exptitle == null) {
				Console.Write("experiment name? > ");
				string[] sl = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
				if (sl.Length > 0){
					exptitle = sl[0];
				}
			}
			string expname = DateTime.Now.ToString("yyyyMMddHHmm") + ' ' + exptitle;
			using (var resultwriter = new StreamWriter(@"./result.txt", true)) {
				//matchlistに沿ってA,Bの先後を入れ替えながら対局させる
				foreach (var m in matchlist) {
					uint[] results = new uint[4] { 0, 0, 0, 0 };
					for (uint r = 1; r <= m.rounds; r++) {
						try
						{
							if (r % 2 != 0)
							{
								//a先手
								string matchname = $"{expname} {m.byoyomi}ms {r}(A): ";
								var result = match(matchname, m.byoyomi, a_path, a_option, b_path, b_option);
								switch (result)
								{
									case Result.SenteWin: results[0]++; Console.WriteLine(" PlayerA win"); break;
									case Result.GoteWin: results[1]++; Console.WriteLine(" PlayerB win"); break;
									case Result.Repetition: results[2]++; Console.WriteLine(" Repetition Draw"); break;
									case Result.Draw: results[3]++; Console.WriteLine(" Draw"); break;
								}
							}
							else
							{
								//b先手
								string matchname = $"{expname} {m.byoyomi}ms {r}(B): ";
								var result = match(matchname, m.byoyomi, b_path, b_option, a_path, a_option);
								switch (result)
								{
									case Result.SenteWin: results[1]++; Console.WriteLine(" PlayerB win"); break;
									case Result.GoteWin: results[0]++; Console.WriteLine(" PlayerA win"); break;
									case Result.Repetition: results[2]++; Console.WriteLine(" Repetition Draw"); break;
									case Result.Draw: results[3]++; Console.WriteLine(" Draw"); break;
								}
							}
						}
						catch(Exception e)
						{
							Console.WriteLine(e.Message);
							r--;
						}
					}
					string matchResult = $"{expname} {m.byoyomi}ms: {results[0]}-{results[1]}-{results[2]}-{results[3]} ({a_name} vs {b_name})";
					resultwriter.WriteLine(matchResult);
					Console.WriteLine(matchResult);
				}
			}
			alive = false;
		}
		static string setoptionusi(string settingline) {
			var token = settingline.Split(' ');
			return $"setoption name {token[0]} value {token[2]}";
		}
		static Result match(string matchName, uint byoyomi, string s_path, List<string> s_option, string g_path, List<string> g_option) {
			using (Process sente = new Process())
			using (Process gote = new Process()) {
				//先手起動
				sente.StartInfo.UseShellExecute = false;
				sente.StartInfo.RedirectStandardOutput = true;
				sente.StartInfo.RedirectStandardInput = true;
				sente.StartInfo.FileName = s_path;
				sente.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(s_path);
				sente.Start();
				sente.StandardInput.WriteLine("usi");
				while (true) { if (sente.StandardOutput.ReadLine() == "usiok") break; }
				foreach (string usi in s_option) sente.StandardInput.WriteLine(usi);
				sente.StandardInput.WriteLine("isready");
				while (true) { if (sente.StandardOutput.ReadLine() == "readyok") break; }
				//後手起動
				gote.StartInfo.UseShellExecute = false;
				gote.StartInfo.RedirectStandardOutput = true;
				gote.StartInfo.RedirectStandardInput = true;
				gote.StartInfo.FileName = g_path;
				gote.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(g_path);
				gote.Start();
				gote.StandardInput.WriteLine("usi");
				while (true) { if (gote.StandardOutput.ReadLine() == "usiok") break; }
				foreach (string usi in g_option) gote.StandardInput.WriteLine(usi);
				gote.StandardInput.WriteLine("isready");
				while (true) { if (gote.StandardOutput.ReadLine() == "readyok") break; }
				//usinewgame
				sente.StandardInput.WriteLine("usinewgame");
				gote.StandardInput.WriteLine("usinewgame");
				Console.Write(matchName);
				//初手はmovesが無いので特殊処理
				var position = new StringBuilder("position startpos");
				List<string> kifu = new List<string>();
				List<int> evals = new List<int>();
				string go = $"go btime 0 wtime 0 byoyomi {byoyomi}";
				List<Kyokumen> history = new List<Kyokumen> { new Kyokumen() };
				{
					var (move, eval) = GetMove(sente, position.ToString(), go);
					kifu.Add(move);
					evals.Add(eval);
					Console.Write($"b:{move}({eval})");
					history.Add(new Kyokumen(history[history.Count - 1], move));
					position.Append(" moves ").Append(move);
				}
				while (true) {
					{//後手
						var (move, eval) = GetMove(gote, position.ToString(), go);
						kifu.Add(move);
						evals.Add(-eval);
						Console.Write($" w:{move}({-eval})");
						if (move=="resign") {
							FoutKifu(matchName, kifu, evals, Result.SenteWin);
							gote.StandardInput.WriteLine("gameover lose");
							gote.StandardInput.WriteLine("quit");
							sente.StandardInput.WriteLine("gameover win");
							sente.StandardInput.WriteLine("quit");
							return Result.SenteWin;
						}
						else if(move == "win") {
							FoutKifu(matchName, kifu, evals, Result.GoteWin);
							gote.StandardInput.WriteLine("gameover win");
							gote.StandardInput.WriteLine("quit");
							sente.StandardInput.WriteLine("gameover lose");
							sente.StandardInput.WriteLine("quit");
							return Result.GoteWin;
						}
						var nextKyokumen = new Kyokumen(history[history.Count - 1], move);
						if (CheckRepetition(nextKyokumen, history)) {
							FoutKifu(matchName, kifu, evals, Result.Repetition);
							gote.StandardInput.WriteLine("gameover draw");
							gote.StandardInput.WriteLine("quit");
							sente.StandardInput.WriteLine("gameover draw");
							sente.StandardInput.WriteLine("quit");
							return Result.Repetition;
						}
						if (CheckEndless(history.Count)) {
							FoutKifu(matchName, kifu, evals, Result.Draw);
							gote.StandardInput.WriteLine("gameover draw");
							gote.StandardInput.WriteLine("quit");
							sente.StandardInput.WriteLine("gameover draw");
							sente.StandardInput.WriteLine("quit");
							return Result.Draw;
						}
						history.Add(nextKyokumen);
						position.Append(" ").Append(move);
					}
					{//先手
						var (move, eval) = GetMove(sente, position.ToString(), go);
						kifu.Add(move);
						evals.Add(eval);
						Console.Write($" b:{move}({eval})");
						if (move == "resign") {
							FoutKifu(matchName, kifu, evals, Result.GoteWin);
							sente.StandardInput.WriteLine("gameover lose");
							sente.StandardInput.WriteLine("quit");
							gote.StandardInput.WriteLine("gameover win");
							gote.StandardInput.WriteLine("quit");
							return Result.GoteWin;
						}
						else if (move == "win") {
							FoutKifu(matchName, kifu, evals, Result.SenteWin);
							sente.StandardInput.WriteLine("gameover win");
							sente.StandardInput.WriteLine("quit");
							gote.StandardInput.WriteLine("gameover lose");
							gote.StandardInput.WriteLine("quit");
							return Result.SenteWin;
						}
						var nextKyokumen = new Kyokumen(history[history.Count - 1], move);
						if (CheckRepetition(nextKyokumen, history)) {
							FoutKifu(matchName, kifu, evals, Result.Repetition);
							sente.StandardInput.WriteLine("gameover draw");
							sente.StandardInput.WriteLine("quit");
							gote.StandardInput.WriteLine("gameover draw");
							gote.StandardInput.WriteLine("quit");
							return Result.Repetition;
						}
						if (CheckEndless(history.Count)) {
							FoutKifu(matchName, kifu, evals, Result.Draw);
							sente.StandardInput.WriteLine("gameover draw");
							sente.StandardInput.WriteLine("quit");
							gote.StandardInput.WriteLine("gameover draw");
							gote.StandardInput.WriteLine("quit");
							return Result.Draw;
						}
						history.Add(nextKyokumen);
						position.Append(" ").Append(move);
					}
				}
			}
		}
		static (string move, int eval) GetMove(Process player, string position, string gobyoyomi) {
			int eval = 0;
			player.StandardInput.WriteLine(position);
			System.Threading.Tasks.Task.Delay(1).Wait();
			player.StandardInput.WriteLine(gobyoyomi);
			while (true) {
				if (player.HasExited) throw new System.IO.IOException("usi engine has crashed");
				string[] usi = player.StandardOutput.ReadLine()?.Split(' ');
				if (usi == null || usi.Length == 0) { continue; }
				else if (usi[0] == "info") {
					for(int i = 1; i < usi.Length - 1; i++) {
						if (usi[i] == "cp") {
							eval = int.Parse(usi[i + 1]);
							break;
						}
						else if (usi[i] == "mate") {
							if (usi[i + 1] == "+") eval = 90000;
							else if (usi[i + 1] == "-") eval = -90000;
							else {
								int mate = int.Parse(usi[i + 1]);
								if (mate >= 0) eval = 100000 - mate;
								else eval = -100000 - mate;
							}
							break;
						}
					}
				}
				else if (usi[0] == "bestmove") {
					return (usi[1], eval);
				}
			}
		}
		static bool CheckRepetition(Kyokumen kyokumen, List<Kyokumen> history) {
			int count = 0;
			foreach(Kyokumen his in history) {
				if (kyokumen == his) {
					count++;
				}
			}
			return count >= 3;
		}
		static bool CheckEndless(int moves) {
			return moves > drawMoves;
		}
		static void FoutKifu(string matchName, List<string> kifu, List<int> evals, Result result) {
			using (var kifuwriter = new StreamWriter(@"./kifu.txt", true)) {
				kifuwriter.Write(matchName);
				foreach (var move in kifu) kifuwriter.Write(move + " ");
				kifuwriter.WriteLine();
				switch (result) {
					case Result.SenteWin: kifuwriter.Write("SenteWin ");break;
					case Result.GoteWin: kifuwriter.Write("GoteWin ");break;
					case Result.Repetition: kifuwriter.Write("Sennichite ");break;
					case Result.Draw: kifuwriter.Write("Over400moves ");break;
				}
				kifuwriter.Write("eval: ");
				foreach (var eval in evals) kifuwriter.Write(eval + " ");
				kifuwriter.WriteLine();
			}
		}
	}
}
