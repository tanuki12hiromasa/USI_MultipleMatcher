using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
namespace USI_MultipleMatch
{
	class Kifu
	{
		public static readonly string TimeFormat = "yyyy/MM/dd-HH:mm";
		public static void FoutKifu(DateTime starttime, string matchName, Player b, Player w, uint byoyomi, List<string> kifu, int startmoveindex, List<int> evals, Result result, string kifupath) {
			using (var kifuwriter = new StreamWriter(kifupath, true)) {
				kifuwriter.Write($"{starttime.ToString(TimeFormat)} {matchName} ({b.name} vs {w.name}) {byoyomi}ms: ");
				foreach (var move in kifu) kifuwriter.Write(move + " ");
				kifuwriter.WriteLine();
				switch (result) {
					case Result.SenteWin: kifuwriter.Write("SenteWin "); break;
					case Result.GoteWin: kifuwriter.Write("GoteWin "); break;
					case Result.Repetition: kifuwriter.Write("Sennichite "); break;
					case Result.Draw: kifuwriter.Write("Over400moves "); break;
				}
				kifuwriter.Write($"start={startmoveindex} ");
				kifuwriter.Write("eval: ");
				foreach (var eval in evals) kifuwriter.Write(eval + " ");
				kifuwriter.WriteLine();
			}
		}
		public static void KifulineToCSA() {
			Console.WriteLine("input kifu line by kifu.txt");
			Console.Write(">");
			string[] kifuline = Console.ReadLine().Split(':');
			if (kifuline.Length != 3) {
				Console.WriteLine("kifu line syntax error.");
				return;
			}
			Console.Write("csa output filename? > ");
			string filename = Console.ReadLine().Split(new char[] { ' ', '.' }, StringSplitOptions.RemoveEmptyEntries)[0] + ".csa";
			using (var fs = new StreamWriter(filename, false)) {
				fs.WriteLine("V2.2");
				//対局情報
				string[] info = kifuline[1].Split(' ');
				// <日:時> <title> (<sente> vs <gote>) <byoyomi>ms: <kifu>
				//対局者情報
				fs.WriteLine($"N+{info[2].Trim('(')}");
				fs.WriteLine($"N-{info[4].Trim(')')}");
				//棋戦名
				fs.WriteLine($@"$EVENT:{info[1]}");
				//持ち時間
				int byoyomi = int.Parse(Regex.Replace(info[5], @"[^0-9]", "")) / 1000;
				fs.WriteLine(@"$TIME_LIMIT:00:00+" + byoyomi.ToString());
				//初期局面
				fs.WriteLine("PI");

				//指し手
				string[] moves = kifuline[2].Split(' ', StringSplitOptions.RemoveEmptyEntries);
				Kyokumen kyokumen = new Kyokumen();
				bool kecchaku = false;
				for (int i = 0; i < moves.Length; i++) {
					string usimove = moves[i];
					if (usimove == "resign") {
						fs.WriteLine(@"%TORYO");
						kecchaku = true;
					}
					else if (usimove == "win") {
						fs.WriteLine(@"%KACHI");
						kecchaku = true;
					}
					else {
						kyokumen.proceed(usimove);
						fs.WriteLine(Kyokumen.usimove_to_csamove(usimove, kyokumen));
					}
					fs.WriteLine("T" + byoyomi.ToString());
				}
				if (!kecchaku) {
					if (moves.Length > Program.drawMoves) {
						fs.WriteLine(@"%HIKIWAKE");
					}
					else {
						fs.WriteLine(@"SENNICHITE");
					}
				}

				Console.WriteLine("kifu has been converted.");
			}
		}
		public static int CountStartPosLines(string filepath) {
			if (filepath == "none") return 0;
			int linecount = 0;
			try {
				foreach (var line in File.ReadLines(filepath)) {
					if (line.StartsWith("startpos")) {
						linecount++;
					}
					else {
						return linecount;
					}
				}
				return linecount;
			}
			catch(Exception e) {
				Console.WriteLine(e.Message);
				return 0;
			}
		}
		public static string GetRandomStartPos(string filepath,int maxline) {
			Random rnd = new Random();
			if (maxline > 0)
				return File.ReadLines(filepath).Skip(rnd.Next(maxline)).First();
			else
				return "startpos";
		}

		public static void KifutxtToSfen() {
			//使用するkifu.txtのpathを入力
			Console.Write("kifu.txt filepath? > ");
			string kifufilepath = Console.ReadLine();
			//kifu.txtから取得する行数を入力
			Console.Write("Where line start from? > ");
			int linenum = int.Parse(Console.ReadLine());
			//出力されるsfenファイルの名前を入力
			Console.Write("sfen output filename? > ");
			string sfenfilename = Console.ReadLine().Split(new char[] { ' ', '.' }, StringSplitOptions.RemoveEmptyEntries)[0] + ".sfen";
			using (var kifufs = new StreamReader(kifufilepath))
			using (var sfenfs = new StreamWriter(sfenfilename, true)) {
				if (linenum % 2 == 0) linenum++;
				for(int i = 1; i < linenum; i++) {
					kifufs.ReadLine();
				}
				var random = new Random();
				while (!kifufs.EndOfStream) {
					string kifuline = kifufs.ReadLine();
					string evalline = kifufs.ReadLine();
					var kifu = kifuline.Split(':')[2].Split(' ', StringSplitOptions.RemoveEmptyEntries);
					int.TryParse(evalline.Split(' ')[1].Split('=')[1], out int starttekazu);
					int usepos = random.Next(starttekazu, kifu.Length);
					if (usepos <= 0) continue;
					sfenfs.Write("startpos moves");
					for(int i = 0; i < usepos; i++) {
						sfenfs.Write(" " + kifu[i]);
					}
					sfenfs.Write('\n');
				}
			}
			Console.WriteLine("sfen out finished.");
		}
	}
}
