using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
namespace USI_MultipleMatch
{
    class Program
    {
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
			//A,Bから
        }
    }


}
