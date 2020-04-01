using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace USI_MultipleMatch
{
	class MakeData
	{
		public static void makedata() {
			Console.WriteLine("makedata mode.");
			Console.Write("command?(er/q) > ");
			switch (Console.ReadLine()) {
				case "evalranking":
				case "er":
					evalranking();
					break;
				case "q":
				case "quit":
					return;
			}
		}

		static void evalranking() {
			//sfenのpath,出力先のpath,探索時間を決める
			Console.Write("sfen filepath? > ");
			string sfenpath = Console.ReadLine();
			Console.Write("output filepath? > ");
			string outputpath = Console.ReadLine();
			Console.Write("search time?(second) > ");
			uint byo = uint.Parse(Console.ReadLine());
			using var sfenfs = new StreamReader(sfenpath);
			//playerAを読み込む
			var player = new Player(@"./PlayerA.txt");
			Console.WriteLine("start making evalranking");
			while (!sfenfs.EndOfStream) {
				//sfenの局面
				string position = "position " + sfenfs.ReadLine();
				//ファイル名を決定
				string filename = DateTime.Now.ToString("yyyyMMdd_HHmmss");
				using var process = new Process();
				try {
					//searchstatistics evalrankingを実行
					player.Start(process);
					process.StandardInput.WriteLine(position);
					process.StandardInput.WriteLine("searchstatistics evalranking " + byo.ToString() + " " + filename + " " + outputpath);
					//finisedが来るまで待機
					while (process.StandardOutput.ReadLine() != "finished.");
					Console.WriteLine("finish: " + position);
					process.StandardInput.WriteLine("quit");
				}
				catch(Exception e) {
					Console.WriteLine("error at: " + position);
					Console.WriteLine(e.Message);
				}
				finally {
					process.Kill();
				}
			}
			Console.WriteLine("finished.");
		}
	}
}
