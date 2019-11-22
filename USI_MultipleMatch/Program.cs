using System;
using System.Collections.Generic;
using System.IO;
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
                    case "resister":
                    case "r":
                        resister();
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

        static void resister() {
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
			while (true)
			{
				Console.Write("input resisterd usi engine's path > ");
				string path = Console.ReadLine();
				//pathからエンジンを起動してusiコマンドを受け付けるかチェック
				if (true)//usiコマンドを受け付けた
				{
					//optionを読み取ってテキストに出力
					return;
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
                    Console.WriteLine("resisterd.");
                    Console.Write("matchlist is ");
                    foreach (var match in matchlist) {
                        Console.Write($"[{match.byoyomi}ms,{match.rounds}回] ");
                    }
                    Console.WriteLine(".");
                    break;
                }
            }
        }
        static void start() {
			//A,Bから
        }
    }


}
