using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
namespace USI_MultipleMatch
{
	class Kifu
	{
		public static void KifulineToCSA() {
			Console.WriteLine("input kifu line by kifu.txt");
			Console.Write(">");
			string[] kifuline = Console.ReadLine().Split(':');
			if (kifuline.Length != 2) {
				Console.WriteLine("kifu line syntax error.");
				return;
			}
			Console.Write("csa output filename? > ");
			string filename = Console.ReadLine().Split(new char[] { ' ', '.' }, StringSplitOptions.RemoveEmptyEntries)[0] + ".csa";
			using (var fs = new StreamWriter(filename, false)) {
				fs.WriteLine("V2.2");

				//対局情報
				string[] info = kifuline[0].Split(' ');
				//対局者情報
				if (info[3].Split('(')[1][0] == 'A') {
					fs.WriteLine("N+PlayerA");
					fs.WriteLine("N-PlayerB");
				}
				else {
					fs.WriteLine("N+PlayerB");
					fs.WriteLine("N-PlayerA");
				}
				//棋戦名
				fs.WriteLine($@"$EVENT:{info[1]}");
				//持ち時間
				int byoyomi = int.Parse(Regex.Replace("5000ms", @"[^0-9]", "")) / 1000;
				fs.WriteLine(@"$TIME_LIMIT:00:00+" + byoyomi.ToString());
				//初期局面
				fs.WriteLine("PI");

				//指し手
				string[] moves = kifuline[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
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
	}
}
