﻿using System;
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

        }
        static void start()
        {

        }
    }

    
}
