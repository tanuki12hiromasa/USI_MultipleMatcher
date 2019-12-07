using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace USI_MultipleMatch
{
	class Player
	{
		public string name;
		public string path;
		public List<string> options;
		public Player(string settingpath) {
			//1行目:name 2行目:ソフト名 3行目:path 4行目~:option
			using (StreamReader reader=new StreamReader(settingpath)) {
				name = reader.ReadLine();
				Console.WriteLine(reader.ReadLine());
				path = reader.ReadLine();
				options = new List<string>();
				while (!reader.EndOfStream) {
					options.Add(Program.setoptionusi(reader.ReadLine()));
				}
			}
		}
	}
}
