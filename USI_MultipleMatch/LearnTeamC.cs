using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace USI_MultipleMatch {
	class LearnTeamC {
		public string teamname;

		public int batchnum;
		public int backup_span;
		public int ruiseki_count;
		public uint thinking_time;
		public int team_num { get => opponents.Count; }

		Player l_player;
		List<Player> opponents;

		public LearnTeamC(string teamname) {
			this.teamname = teamname;
			backup_span = 50;
			ruiseki_count = 0;
			thinking_time = 600;
			opponents = new List<Player>();
		}

		string getTeamfolder() {
			return $"./learnteamC/{teamname}";
		}

		public bool load() {
			string teamfolder = getTeamfolder();
			if (System.IO.File.Exists(teamfolder + "/setting.txt")) {
				int teamnum;
				using (StreamReader reader = new StreamReader(teamfolder + "/setting.txt")) {
					//チーム数、バッチ数
					teamnum = int.Parse(reader.ReadLine());
					batchnum = int.Parse(reader.ReadLine());
					backup_span = int.Parse(reader.ReadLine());
					ruiseki_count = int.Parse(reader.ReadLine());
					if (!reader.EndOfStream) thinking_time = uint.Parse(reader.ReadLine());
				}

				l_player = new Player($"{teamfolder}/L-Player.txt");
				for (int i = 1; i <= teamnum; i++) {
					opponents.Add(new Player($"{teamfolder}/Player{i}.txt"));
				}

				return true;
			}
			else {
				return false;
			}
		}

		public void save_settingfile() {
			string teamfolder = getTeamfolder();
			using (StreamWriter writer = new StreamWriter($"{teamfolder}/setting.txt")) {
				writer.WriteLine(opponents.Count);
				writer.WriteLine(batchnum);
				writer.WriteLine(backup_span);
				writer.WriteLine(ruiseki_count);
				writer.WriteLine(thinking_time);
			}
		}


		public void setting() {
			string teamfolder = getTeamfolder();
			if (!Directory.Exists(teamfolder)) Directory.CreateDirectory(teamfolder);
			if (System.IO.File.Exists(teamfolder + "/setting.txt")) {
				load();

				int opponentnum = opponents.Count;
				while (true) {
					Console.WriteLine("add Player? (y/n) > ");
					string ans = Console.ReadLine();
					if (ans != "y") break;

					opponentnum++;
					Console.Write($"Opponent Player {opponentnum} path? > ");
					string oPlayerpath = Console.ReadLine();
					var p = new Player(oPlayerpath, $"Player{opponentnum}");
					opponents.Add(p);
					p.settingsave($"{teamfolder}/Player{opponentnum}.txt");
				}

				save_settingfile();

			}
			else {
				Console.Write("Learn-Player path? > ");
				string lPlayerpath = Console.ReadLine();
				l_player = new Player(lPlayerpath, "L-Player");
				l_player.settingsave($"{teamfolder}/L-Player.txt");

				int opponentnum = 0;
				do {
					opponentnum++;
					Console.Write($"Opponent Player {opponentnum} path? > ");
					string oPlayerpath = Console.ReadLine();
					var p = new Player(oPlayerpath, $"Player{opponentnum}");
					opponents.Add(p);
					p.settingsave($"{teamfolder}/Player{opponentnum}.txt");

					Console.WriteLine("add Player? (y/n) > ");
					string ans = Console.ReadLine();
					if (ans != "y") break;
				} while (true);

				backup_span = opponentnum * 10;

				save_settingfile();
			}
		}

		public void versus(bool teban, int teamnum) {
			teamnum %= opponents.Count;
			Player b = teban ? l_player : opponents[teamnum];
			Player w = teban ? opponents[teamnum] : l_player;

			//対局
			string teamfolder = getTeamfolder();
			string start = "startpos";
			Match.match($"{teamname}-{ruiseki_count}", thinking_time, b, w, start, $"{teamfolder}/kifu.txt", 15000);

			ruiseki_count++;
			save_settingfile();
		}

		public void backup_param(string backupname) {
			string path = $"{getTeamfolder()}/evalbackup/{backupname}";
			Directory.CreateDirectory(path);
			l_player.save_eval(path);
		}
	}
}
