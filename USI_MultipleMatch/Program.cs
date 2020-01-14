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
				Console.Write("command?(r/s/tm/ts/ls/c/k/q) > ");
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
					case "consecutivegame":
					case "c":
						consecutive();
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
				playername = "PlayerB";
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
					randomposfilepath = Console.ReadLine();
					randomposlines = Kifu.CountStartPosLines(randomposfilepath);
					if (randomposlines == 0) {
						Console.WriteLine("failed to read startpos file.");
						randomposfilepath = null;
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
			Directory.CreateDirectory($"./playerlog/{matchname}");
			playera.settingsave($"./playerlog/{matchname}/PlayerA.txt");
			playerb.settingsave($"./playerlog/{matchname}/PlayerB.txt");
			//matchlistに沿ってA,Bの先後を入れ替えながら対局させる
			foreach (var m in matchlist) {
				uint[] results = new uint[4] { 0, 0, 0, 0 };
				string starttime = DateTime.Now.ToString(Kifu.TimeFormat);
				string startpos = "startpos";
				for (uint r = 1; r <= m.rounds; r++) {
					if (r % 2 != 0) {
						//a先手
						startpos = Kifu.GetRandomStartPos(randomposfilepath, randomposlines);
						var result = Match.match($"{matchname}-{r}", m.byoyomi, playera, playerb, startpos);
						switch (result) {
							case Result.SenteWin: results[0]++; Console.WriteLine($" {playera.name} win"); break;
							case Result.GoteWin: results[1]++; Console.WriteLine($" {playerb.name} win"); break;
							case Result.Repetition: results[2]++; Console.WriteLine(" Repetition Draw"); break;
							case Result.Draw: results[3]++; Console.WriteLine(" Draw"); break;
						}
					}
					else {
						//b先手
						var result = Match.match($"{matchname}-{r}", m.byoyomi, playerb, playera, startpos);
						switch (result) {
							case Result.SenteWin: results[1]++; Console.WriteLine($" {playerb.name} win"); break;
							case Result.GoteWin: results[0]++; Console.WriteLine($" {playera.name} win"); break;
							case Result.Repetition: results[2]++; Console.WriteLine(" Repetition Draw"); break;
							case Result.Draw: results[3]++; Console.WriteLine(" Draw"); break;
						}
					}
				}
				string matchResult = $"{starttime} {matchname} {m.byoyomi}ms: {results[0]}-{results[1]}-{results[2]}-{results[3]} ({playera.enginename} vs {playerb.enginename})";
				using (var resultwriter = new StreamWriter(@"./result.txt", true)) {
					resultwriter.WriteLine(matchResult);
				}
				Console.WriteLine(matchResult);
			}
			alive = false;
		}
		static void setuptournament() {
			Console.WriteLine("setup new tournament.");
			Console.Write("tournament name? > ");
			string tournamentname = Console.ReadLine();
			Directory.CreateDirectory($"tournament/{tournamentname}");
			Console.Write("byoyomi? > ");
			uint byoyomi = uint.Parse(Console.ReadLine());
			Console.Write("How many matches per 1 round? > ");
			uint matchnum = uint.Parse(Console.ReadLine());
			string randomposfilepath = null;
			while (randomposfilepath == null) {
				Console.Write("use random startpos?(y/n) > ");
				if (Console.ReadLine() == "y") {
					Console.Write("startpos file path? > ");
					randomposfilepath = Console.ReadLine();
					if (Kifu.CountStartPosLines(randomposfilepath) == 0) {
						Console.WriteLine("failed to read.");
						randomposfilepath = null;
					}
				}
				else {
					randomposfilepath = "none";
				}
			}
			Console.Write("number of players? > ");
			uint playernum = uint.Parse(Console.ReadLine());
			using (StreamWriter writer = new StreamWriter(@$"./tournament/{tournamentname}/tsetting.txt")) {
				writer.WriteLine($"byoyomi {byoyomi}");
				writer.WriteLine($"matchnum {matchnum}");
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
				player.settingsave($"./tournament/{tournamentname}/player{i}.txt");
			}
		}
		static void starttournament() {
			//tounamentを開く
			Console.WriteLine("start tournament.");
			Console.Write("tournament folder name? > ");
			string tournamentname = Console.ReadLine();
			//settingからbyoyomiと人数を読み込む
			uint byoyomi = 1000;
			uint matchnum = 5;
			uint playernum = 2;
			string startposfile;
			using (StreamReader reader = new StreamReader($"./tournament/{tournamentname}/tsetting.txt")) {
				byoyomi = uint.Parse(reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
				matchnum = uint.Parse(reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
				playernum = uint.Parse(reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
				startposfile = reader.ReadLine();
			}
			//playerリストを作る
			Player[] playerdata = new Player[playernum];
			List<int> players = new List<int>();
			for(int i = 0; i < playernum; i++) {
				playerdata[i] = new Player($"./tournament/{tournamentname}/player{i}.txt");
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
					var awin = tournamentVs(tournamentname, rank, byoyomi, matchnum, playerdata[a], playerdata[b], startposfile);
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
				using (StreamWriter writer = new StreamWriter($"./tournament/{tournamentname}/result.txt", true)) {
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
			using (StreamWriter writer = new StreamWriter($"./tournament/{tournamentname}/result.txt", true)) {
				writer.WriteLine($"Tournament Winner: {playerdata[players[0]].name}");
			}
			Console.WriteLine($"Tournament Winner: {playerdata[players[0]].name}");
			alive = false;
		}

		static bool tournamentVs(string tname,uint rank,uint byoyomi,uint matchnum,Player a,Player b,string startposfile) {
			uint win_a = 0, win_b = 0;
			int startposlines = Kifu.CountStartPosLines(startposfile);
			string startpos = "startpos";
			for (int game=1; ; game++) {
				if (game % 2 == 1) {
					startpos = Kifu.GetRandomStartPos(startposfile, startposlines);
					var result = Match.match($"{tname}-Round{rank}-{game}", byoyomi, a, b, startpos, $"./tournament/{tname}/kifu.txt");
					switch (result) {
						case Result.SenteWin: win_a++; Console.WriteLine($" {a.name} win"); break;
						case Result.GoteWin: win_b++; Console.WriteLine($" {b.name} win"); break;
						case Result.Repetition: Console.WriteLine(" Repetition Draw"); break;
						case Result.Draw: Console.WriteLine(" Draw"); break;
					}
				}
				else {
					var result = Match.match($"{tname}-Round{rank}-{game}", byoyomi, b, a, startpos, $"./tournament/{tname}/kifu.txt");
					switch (result) {
						case Result.SenteWin: win_b++; Console.WriteLine($" {b.name} win"); break;
						case Result.GoteWin: win_a++; Console.WriteLine($" {a.name} win"); break;
						case Result.Repetition: Console.WriteLine(" Repetition Draw"); break;
						case Result.Draw: Console.WriteLine(" Draw"); break;
					}
				}
				if(win_a > matchnum / 2) {
					return true;
				}
				if(win_b > matchnum / 2) {
					return false;
				}
			}
		}

		static void startleague() {
			Console.WriteLine("start league.");
			Console.Write("league folder name?(use tournament setup) > ");
			string leaguename = Console.ReadLine();
			//settingからbyoyomiと人数を読み込む
			uint byoyomi = 1000;
			uint matchnum = 2;
			uint playernum = 2;
			string startposfile;
			using (StreamReader reader = new StreamReader($"./tournament/{leaguename}/tsetting.txt")) {
				byoyomi = uint.Parse(reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
				matchnum = uint.Parse(reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
				playernum = uint.Parse(reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
				startposfile = reader.ReadLine();
			}
			int startposlines = Kifu.CountStartPosLines(startposfile);
			//playerリストを作る
			Player[] playerdata = new Player[playernum];
			for (int i = 0; i < playernum; i++) {
				playerdata[i] = new Player($"./tournament/{leaguename}/player{i}.txt");
			}
			uint[,,] results = new uint[playernum, playernum, 4];
			uint[,] points = new uint[playernum,4];
			for (int i = 0; i < playernum; i++) {
				for (int j = 0; j < 4; j++) {
					points[i, j] = 0;
					for(int h = 0; h < playernum; h++) {
						results[h, i, j] = 0;
					}
				}
			}
			//総当たり戦を行う
			for(int a = 0; a < playernum; a++) {
				for(int b = a + 1; b < playernum; b++) {
					string startpos = "startpos";
					for (int t = 1; t <= matchnum; t++) {
						if (t % 2 == 1) {
							startpos = Kifu.GetRandomStartPos(startposfile, startposlines);
							var result = Match.match($"{leaguename}-{t}", byoyomi, playerdata[a], playerdata[b], startpos, $"./tournament/{leaguename}/kifu.txt");
							switch (result) {
								case Result.SenteWin:
									points[a, 0]++; points[b, 1]++; results[a, b, 0]++; results[b, a, 1]++;
									Console.WriteLine($" {playerdata[a].name} win"); break;
								case Result.GoteWin:
									points[a, 1]++; points[b, 0]++; results[a, b, 1]++; results[b, a, 0]++;
									Console.WriteLine($" {playerdata[b].name} win"); break;
								case Result.Repetition:
									points[a, 2]++; points[b, 2]++; results[a, b, 2]++; results[b, a, 2]++;
									Console.WriteLine(" Repetition Draw"); break;
								case Result.Draw:
									points[a, 3]++; points[b, 3]++; results[a, b, 3]++; results[b, a, 3]++;
									Console.WriteLine(" Draw"); break;
							}
						}
						else {
							var result = Match.match($"{leaguename}-{t}", byoyomi, playerdata[b], playerdata[a], startpos, $"./tournament/{leaguename}/kifu.txt");
							switch (result) {
								case Result.SenteWin:
									points[b, 0]++; points[a, 1]++; results[b, a, 0]++; results[a, b, 1]++;
									Console.WriteLine($" {playerdata[b].name} win"); break;
								case Result.GoteWin:
									points[b, 1]++; points[a, 0]++; results[b, a, 1]++; results[a, b, 0]++;
									Console.WriteLine($" {playerdata[a].name} win"); break;
								case Result.Repetition:
									points[a, 2]++; points[b, 2]++; results[a, b, 2]++; results[b, a, 2]++;
									Console.WriteLine(" Repetition Draw"); break;
								case Result.Draw:
									points[a, 3]++; points[b, 3]++; results[a, b, 3]++; results[b, a, 3]++;
									Console.WriteLine(" Draw"); break;
							}
						}
					}
				}
			}
			//順位をまとめる
			int[] ranking = new int[playernum];
			for (int i = 0; i < playernum; i++) ranking[i] = i;
			ranking = ranking.OrderByDescending(i => ((int)points[i, 0] - points[i, 1])).ToArray();
			using (StreamWriter writer = new StreamWriter($"./tournament/{leaguename}/leagueresult.txt", true)) {
				writer.Write("       ");
				for(int a = 0; a < playernum; a++) {
					writer.Write(playerdata[a].name.PadRight(8));
				}
				writer.WriteLine();
				for (int a = 0; a < playernum; a++) {
					writer.Write(playerdata[a].name.PadRight(6));
					for (int b = 0; b < playernum; b++) {
						if (a != b) {
							writer.Write($" {results[a, b, 0]}-{results[a, b, 1]}-{results[a, b, 2]}-{results[a, b, 3]}");
						}
						else {
							writer.Write(" -------");
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

		static void consecutive() {
			Console.WriteLine("start consecutive game.");
			Console.Write("game folder name?(use tournament setup) > ");
			string leaguename = Console.ReadLine();
			//settingからbyoyomiと人数を読み込む
			uint byoyomi = 1000;
			uint matchnum = 2;
			uint playernum = 2;
			string startposfile;
			using (StreamReader reader = new StreamReader($"./tournament/{leaguename}/tsetting.txt")) {
				byoyomi = uint.Parse(reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
				matchnum = uint.Parse(reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
				playernum = uint.Parse(reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
				startposfile = reader.ReadLine();
			}
			int startposlines = Kifu.CountStartPosLines(startposfile);
			//playerリストを作る
			Player[] playerdata = new Player[playernum];
			for (int i = 0; i < playernum; i++) {
				playerdata[i] = new Player($"./tournament/{leaguename}/player{i}.txt");
			}
			//総当たり戦を行う
			for (int b = 1; b < playernum; b++) {
				string startpos = "startpos";
				uint[] results = new uint[4] { 0, 0, 0, 0 };
				string starttime = DateTime.Now.ToString(Kifu.TimeFormat);
				for (int t = 1; t <= matchnum; t++) {
					if (t % 2 == 1) {
						startpos = Kifu.GetRandomStartPos(startposfile, startposlines);
						var result = Match.match($"{leaguename}-{t}", byoyomi, playerdata[0], playerdata[b], startpos, $"./tournament/{leaguename}/kifu.txt");
						switch (result) {
							case Result.SenteWin:
								results[0]++; Console.WriteLine($" {playerdata[0].name} win"); break;
							case Result.GoteWin:
								results[1]++; Console.WriteLine($" {playerdata[b].name} win"); break;
							case Result.Repetition:
								results[2]++; Console.WriteLine(" Repetition Draw"); break;
							case Result.Draw:
								results[3]++; Console.WriteLine(" Draw"); break;
						}
					}
					else {
						var result = Match.match($"{leaguename}-{t}", byoyomi, playerdata[b], playerdata[0], startpos, $"./tournament/{leaguename}/kifu.txt");
						switch (result) {
							case Result.SenteWin:
								results[1]++;Console.WriteLine($" {playerdata[b].name} win"); break;
							case Result.GoteWin:
								results[0]++; Console.WriteLine($" {playerdata[0].name} win"); break;
							case Result.Repetition:
								results[2]++; Console.WriteLine(" Repetition Draw"); break;
							case Result.Draw:
								results[3]++; Console.WriteLine(" Draw"); break;
						}
					}
				}
				string matchResult = $"{starttime} {leaguename} {byoyomi}ms: {results[0]}-{results[1]}-{results[2]}-{results[3]} ({playerdata[0].name} vs {playerdata[b].name})";
				using (var resultwriter = new StreamWriter($"./tournament/{leaguename}/cresult.txt", true)) {
					resultwriter.WriteLine(matchResult);
				}
				Console.WriteLine(matchResult);
			}
			alive = false;
		}
	}
}
