using System;
using System.Collections.Generic;
namespace USI_MultipleMatch
{
    class Program
    {
        static bool alive;
        static void Main(string[] args)
        {
            alive = true;
            Console.WriteLine("Hello World!");
            while (alive)
            {
                Console.Write("command?()");
                switch (Console.ReadLine())
                {
                    case "resister":
                        resister();
                        break;
                    case "makematch":
                        makematch();
                        break;
                    case "start":
                        start();
                        break;
                    case "quit":
                        alive = false;
                        break;
                }
            }
        }

        static void resister()
        {

        }
        static List<(uint byoyomi, uint rounds)> matchlist;
        static void makematch()
        {
            while (true)
            {
                Console.WriteLine($"match {matchlist.Count+1}");
                Console.Write("byoyomi?(millisecond) > ");
                uint byo = uint.Parse(Console.ReadLine());
                Console.Write("rounds?(times) > ");
                uint times = uint.Parse(Console.ReadLine());
                matchlist.Add((byo, times));

                Console.Write("current matchlist is ");
                foreach(var match in matchlist)
                {
                    Console.Write($"[{match.byoyomi}ms,{match.rounds}rounds] ");
                }
                Console.Write("add match?(y/n) > ");
                if (Console.ReadLine()!="y")
                {
                    Console.WriteLine("resisterd.");
                    break;
                }
            }
        }
        static void start()
        {

        }
    }

    
}
