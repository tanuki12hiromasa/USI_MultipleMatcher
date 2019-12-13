using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Linq;
namespace USI_MultipleMatch
{
	class Program
	{
		static bool alive;
		public static uint drawMoves = 400;
		static void Main(string[] args) {
			alive = true;
			Console.WriteLine("連続対局プログラム");
			while (alive) {
				Console.Write("command?(r/s/tm/ts/ls/k/q) > ");
				switch (Console.ReadLine()) {
					case "register":
					case "r":
						register();
						break;
					case "start":
					case "s":
						start();
						break;
					case "tournamentmakematch":
					case "tm":
						setuptournament();
						break;
					case "tournamentstart":
					case "ts":
						starttournament();
						break;
					case "leaguestart":
					case "ls":
						startleague();
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
			string playername;
			string settingpath;
			if (p == "a" || p == "A") {
				playername = "PlayerA";
				settingpath = "./PlayerA.txt";
			}
			else {
				playername = "playerB";
				settingpath = "./PlayerB.txt";
			}
			while (true) {
				Console.Write("input resisterd usi engine's path > ");
				string path = Console.ReadLine();
				try {
					Player player = new Player(path, playername);
					player.settingsave(settingpath);
					return;
				}
				catch (Exception e) {
					Console.WriteLine(e.Message);
					throw e;
				}
			}
		}
		static void start() {
			List<(uint byoyomi, uint rounds)> matchlist = new List<(uint byoyomi, uint rounds)>();
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
				if (matchlist.Count > 0) {
					Console.Write("add more matches?(y/n) > ");
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
			//対局名を決める
			string matchname = null;
			while (matchname == null) {
				Console.Write("match name? > ");
				string[] sl = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
				if (sl.Length > 0){
					matchname = sl[0];
				}
			}
			int randomposlines = 0;
			string randomposfilepath = null;
			while (randomposfilepath == null) {
				Console.Write("use random startpos?(y/n) > ");
				if (Console.ReadLine() == "y") {
					Console.Write("startpos file path? > ");
					try {
						randomposfilepath = Console.ReadLine();
						randomposlines = Kifu.CountStartPosLines(randomposfilepath);
						Console.WriteLine($"lines: {randomposlines}");
					}
					catch(Exception e) {
						Console.WriteLine(e.Message);
					}
				}
				else {
					randomposfilepath = "none";
				}
			}
			//A,Bの設定を読み込む
			Player playera, playerb;
			try {
				playera = new Player(@"./PlayerA.txt");
			}
			catch (Exception e) {
				Console.WriteLine("error in load playerA setting");
				Console.WriteLine(e.Message);
				return;
			}
			try {
				playerb = new Player(@"./PlayerB.txt");
			}
			catch (Exception e) {
				Console.WriteLine("error in load playerB setting");
				Console.WriteLine(e.Message);
				return;
			}
			using (var resultwriter = new StreamWriter(@"./result.txt", true)) {
				//matchlistに沿ってA,Bの先後を入れ替えながら対局させる
				foreach (var m in matchlist) {
					uint[] results = new uint[4] { 0, 0, 0, 0 };
					string starttime = DateTime.Now.ToString(Kifu.TimeFormat);
					for (uint r = 1; r <= m.rounds; r++) {
						string startpos = Kifu.GetRandomStartPos(randomposfilepath, randomposlines);
						try
						{
							if (r % 2 != 0)
							{
								//a先手
								var result = Match.match(matchname, m.byoyomi, playera, playerb, startpos);
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
								var result = Match.match(matchname, m.byoyomi, playera, playerb, startpos);
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
					string matchResult = $"{starttime} {matchname} {m.byoyomi}ms: {results[0]}-{results[1]}-{results[2]}-{results[3]} ({playera.enginename} vs {playerb.enginename})";
					resultwriter.WriteLine(matchResult);
					Console.WriteLine(matchResult);
				}
			}
			alive = false;
		}
		static void setuptournament() {
			Console.WriteLine("setup new tournament.");
			Console.Write("tournament name? > ");
			string tournamentname = Console.ReadLine();
			Directory.CreateDirectory(tournamentname);
			Console.Write("byoyomi? > ");
			uint byoyomi = uint.Parse(Console.ReadLine());
			string randomposfilepath = null;
			while (randomposfilepath == null) {
				Console.Write("use random startpos?(y/n) > ");
				if (Console.ReadLine() == "y") {
					Console.Write("startpos file path? > ");
					try {
						randomposfilepath = Console.ReadLine();
						if(Kifu.CountStartPosLines(randomposfilepath) == 0) {
							Console.WriteLine("failed to read.");
							randomposfilepath = null;
						}
					}
					catch (Exception e) {
						Console.WriteLine(e.Message);
					}
				}
				else {
					randomposfilepath = "none";
				}
			}
			Console.Write("number of players? > ");
			uint playernum = uint.Parse(Console.ReadLine());
			using (StreamWriter writer = new StreamWriter(tournamentname + @"/setting.txt")) {
				writer.WriteLine($"byoyomi {byoyomi}");
				writer.WriteLine($"playernum {playernum}");
				writer.WriteLine(randomposfilepath);
			}
			Player player;
			while (true) {
				Console.Write("player's file path? > ");
				string path = Console.ReadLine();
				try {
					player = new Player(path, "tournamentPlayer");
					break;
				}
				catch (Exception e) {
					Console.WriteLine(e.Message);
				}
			}
			//playerを作成
			//1行目:name 2行目:ソフト名 3行目:path 4行目~:option
			for (uint i = 0; i < playernum; i++) {
				player.name = $"P{i}";
				player.settingsave($"./{tournamentname}/player{i}.txt");
			}
		}
		static void starttournament() {
			//tounamentを開く
			Console.WriteLine("start tournament.");
			Console.Write("tournament folder name? > ");
			string tournamentname = Console.ReadLine();
			//settingからbyoyomiと人数を読み込む
			uint byoyomi = 1000;
			uint playernum = 2;
			string startposfile;
			using (StreamReader reader = new StreamReader($"{tournamentname}/setting.txt")) {
				byoyomi = uint.Parse(reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
				playernum = uint.Parse(reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
				startposfile = reader.ReadLine();
			}
			//playerリストを作る
			Player[] playerdata = new Player[playernum];
			List<int> players = new List<int>();
			for(int i = 0; i < playernum; i++) {
				playerdata[i] = new Player($"{tournamentname}/player{i}.txt");
				players.Add(i);
			}
			//順番をシャッフル
			players = players.OrderBy(i => Guid.NewGuid()).ToList();
			//トーナメントを行う
			uint rank = 0;
			while (players.Count > 1) {
				rank++;
				List<int> survivor = new List<int>();
				List<int> loser = new List<int>();
				//前から順にマッチング
				for(int i = 1; i < players.Count; i += 2) {
					int a = players[i - 1];
					int b = players[i];
					var awin = tournamentVs(tournamentname, rank, byoyomi, playerdata[a], playerdata[b], startposfile);
					if (awin) {
						survivor.Add(a);
						loser.Add(b);
					}
					else {
						survivor.Add(b);
						loser.Add(a);
					}
				}
				//playersが奇数人だった場合,最後の一人はシードで上がる
				if (players.Count % 2 == 1) {
					survivor.Add(players[players.Count - 1]);
				}
				using (StreamWriter writer = new StreamWriter($"{tournamentname}/result.txt", true)) {
					writer.Write($"Round {rank} player: ");
					foreach (var i in players) { writer.Write($"{playerdata[i].name} "); }
					writer.WriteLine();
					writer.Write("loser: ");
					foreach(var i in loser) { writer.Write($"{playerdata[i].name} "); }
					writer.WriteLine();
				}
				players = survivor.OrderBy(i => Guid.NewGuid()).ToList();
			}
			//優勝playerを書き込む
			using (StreamWriter writer = new StreamWriter($"{tournamentname}/result.txt", true)) {
				writer.WriteLine($"Tournament Winner: {playerdata[players[0]].name}");
			}
			Console.WriteLine($"Tournament Winner: {playerdata[players[0]].name}");
			alive = false;
		}

		static bool tournamentVs(string tname,uint rank,uint byoyomi,Player a,Player b,string startposfile) {
			uint win_a = 0, win_b = 0;
			int startposlines = Kifu.CountStartPosLines(startposfile);
			for (int game=1; ; game++) {
				string startpos = Kifu.GetRandomStartPos(startposfile, startposlines);
				try {
					if (game % 2 == 1) {
						var result = Match.match($"{tname}-Round{rank}-{game}", byoyomi, a, b, startpos, $"./{tname}/kifu.txt");
						switch (result) {
							case Result.SenteWin: win_a++; Console.WriteLine($" {a.name} win"); break;
							case Result.GoteWin: win_b++; Console.WriteLine($" {b.name} win"); break;
							case Result.Repetition: Console.WriteLine(" Repetition Draw"); break;
							case Result.Draw: Console.WriteLine(" Draw"); break;
						}
					}
					else {
						var result = Match.match($"{tname}-Round{rank}-{game}", byoyomi, b, a, startpos, $"./{tname}/kifu.txt");
						switch (result) {
							case Result.SenteWin: win_b++; Console.WriteLine($" {b.name} win"); break;
							case Result.GoteWin: win_a++; Console.WriteLine($" {a.name} win"); break;
							case Result.Repetition: Console.WriteLine(" Repetition Draw"); break;
							case Result.Draw: Console.WriteLine(" Draw"); break;
						}
					}
				}
				catch(Exception e) {
					Console.WriteLine(e.Message);
					game--;
				}
				if(win_a >= 3 || (win_a == 2 && win_b == 0)) {
					return true;
				}
				if(win_b >= 3 || (win_b==2 && win_a == 0)) {
					return false;
				}
			}
		}

		static void startleague() {
			Console.WriteLine("start league.");
			Console.Write("league folder name?(use tournament setup) > ");
			string leaguentname = Console.ReadLine();
			//settingからbyoyomiと人数を読み込む
			uint byoyomi = 1000;
			uint playernum = 2;
			string startposfile;
			using (StreamReader reader = new StreamReader($"{leaguentname}/setting.txt")) {
				byoyomi = uint.Parse(reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
				playernum = uint.Parse(reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
				startposfile = reader.ReadLine();
			}
			int startposlines = Kifu.CountStartPosLines(startposfile);
			//playerリストを作る
			Player[] playerdata = new Player[playernum];
			for (int i = 0; i < playernum; i++) {
				playerdata[i] = new Player($"{leaguentname}/player{i}.txt");
			}
			uint[,] points = new uint[playernum,4];
			for (int i = 0; i < playernum; i++) {
				for (int j = 0; j < 4; j++) {
					points[i, j] = 0;
				}
			}
			Result[,] results = new Result[playernum, playernum];
			//総当たり戦を行う
			for(int b = 0; b < playernum; b++) {
				for(int w = 0; w < playernum; w++) {
					if (b != w) {
						string startpos = Kifu.GetRandomStartPos(startposfile, startposlines);
						var result = Match.match(leaguentname, byoyomi, playerdata[b], playerdata[w], startpos, $"{leaguentname}/kifu.txt");
						results[b, w] = result;
						switch (result) {
							case Result.SenteWin: points[b, 0]++; points[w, 1]++; Console.WriteLine($" {playerdata[b].name} win"); break;
							case Result.GoteWin: points[w, 0]++; points[b, 1]++; Console.WriteLine($" {playerdata[w].name} win"); break;
							case Result.Repetition: points[b, 2]++; points[w, 2]++; Console.WriteLine(" Repetition Draw"); break;
							case Result.Draw: points[b, 3]++; points[w, 3]++; Console.WriteLine(" Draw"); break;
						}
					}
					else {
						results[b, w] = Result.Draw;
					}
				}
			}
			//順位をまとめる
			int[] ranking = new int[playernum];
			for (int i = 0; i < playernum; i++) ranking[i] = i;
			ranking = ranking.OrderByDescending(i => (points[i, 0] - points[i, 1])).ToArray();
			using (StreamWriter writer = new StreamWriter($"{leaguentname}/result.txt", true)) {
				writer.Write("   ");
				for(int w = 0; w < playernum; w++) {
					writer.Write(playerdata[w].name.PadLeft(4));
				}
				writer.WriteLine();
				for (int b = 0; b < playernum; b++) {
					writer.Write(playerdata[b].name.PadRight(4));
					for (int w = 0; w < playernum; w++) {
						if (b != w) {
							writer.Write(results[b, w] switch {
								Result.SenteWin=>"  O ",
								Result.GoteWin=>"  X ",
								Result.Repetition=>"  R ",
								Result.Draw=>"  D ",
								_=>"  E "	});
						}
						else {
							writer.Write("  - ");
						}
					}
					writer.WriteLine();
				}
				writer.WriteLine("\n----------");
				writer.WriteLine("Ranking");
				for(int i = 0; i < playernum; i++) {
					int p = ranking[i];
					writer.WriteLine($"Rank{i+1}:{playerdata[p].name} {points[p, 0]}-{points[p, 1]}-{points[p, 2]}-{points[p, 3]}");
				}
			}
			alive = false;
		}
		
	}
}
