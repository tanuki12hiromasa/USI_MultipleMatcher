using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
namespace USI_MultipleMatch
{
    class Program
    {
		enum Result
		{
			SenteWin,GoteWin,Draw
		}
        static bool alive;
        static List<(uint byoyomi, uint rounds)> matchlist;
        static void Main(string[] args) {
            alive = true;
            matchlist = new List<(uint byoyomi, uint rounds)>();
            Console.WriteLine("連続対局プログラム");
            while (alive) {
                Console.Write("command?(r/m/s/q) > ");
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
                    case "quit":
                    case "q":
                        alive = false;
                        break;
                }
            }
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
                Console.Write($"add [{byo}ms,{times}]?(y/n) > ");
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
			var a_option = new List<string>();
			var b_option = new List<string>();
			try {
				using (StreamReader readerA = new StreamReader(@"./ PlayerA.txt")) {
					Console.WriteLine($"playerA: {readerA.ReadLine()}");
					a_path = readerA.ReadLine();
					Console.WriteLine(a_path);
					for (string sline = readerA.ReadLine(); sline != null && sline != ""; sline = readerA.ReadLine()) {
						a_option.Add(setoptionusi(sline));
						Console.WriteLine(a_option.Count - 1);
					}
				}
			}
			catch (Exception e) {
				Console.WriteLine("error in load playerA setting");
				Console.WriteLine(e.Message);
				return;
			}
			try {
				using (StreamReader readerB = new StreamReader(@"./ PlayerB.txt")) {
					Console.WriteLine($"playerB: {readerB.ReadLine()}");
					b_path = readerB.ReadLine();
					Console.WriteLine(b_path);
					for (string sline = readerB.ReadLine(); sline != null && sline != ""; sline = readerB.ReadLine()) {
						a_option.Add(setoptionusi(sline));
						Console.WriteLine(b_option.Count - 1);
					}
				}
			}
			catch (Exception e) {
				Console.WriteLine("error in load playerB setting");
				Console.WriteLine(e.Message);
				return;
			}
			//
			Console.Write("experiment name? > ");
			string expname = DateTime.Now.ToString("yyMMddHHmm") + ' ' + Console.ReadLine();
			//matchlistに沿ってA,Bの先後を入れ替えながら対局させる
			foreach(var m in matchlist) {
				for(uint r = 1; r <= m.rounds; r++) {
					if (r % 2 == 0) {
						//a先手
						string matchname = $"{expname} {m.byoyomi} {r} a";
						match(matchname, m.byoyomi, a_path, a_option, b_path, b_option);
					}
					else {
						//b先手
						string matchname = $"{expname} {m.byoyomi} {r} b";
						match(matchname, m.byoyomi, b_path, b_option, a_path, a_option);
					}
				}
			}

		}
		static string setoptionusi(string settingline) {
			var token = settingline.Split(' ');
			return $"setoption name {token[0]} value {token[2]}";
		}
		static Result match(string matchName, uint byoyomi, string s_path, List<string> s_option, string g_path, List<string> g_option) {
			Console.Write($"{matchName} ");
			using (Process sente = new Process())
			using (Process gote = new Process()) {
				

			}
			return Result.Draw;
		}

    }


}
