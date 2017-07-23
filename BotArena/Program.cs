using System;
using System.Runtime.Loader;
using System.Reflection;
using System.Linq;
using Luegen.Core;
using Luegen.BotArena.ConsoleInterface;
using System.Collections.Generic;
using System.IO;

namespace Luegen.BotArena
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length <= 1)
            {
                Console.WriteLine("Please provide at least two players as console arguments.");
                Console.WriteLine("Player can either be human or bot dll.");
                Console.WriteLine("Example with 4 Players: dotnet run -- BasicBot.dll human human BasicBot.dll");
                return;
            }

            var multiGameListener = new MultiGameListener();
            // multiGameListener.AddGameListener(new ConsoleGameOutput());
            var gameMaster = new GameMaster(multiGameListener);
            int playerNumber = 0;
            foreach (var arg in args)
            {
                ++playerNumber;
                Object player;
                if (arg.ToLower() == "human")
                {
                    player = new ConsolePlayerController();
                }
                else
                {
                    player = LoadBotFromAssembly(Path.GetFullPath(arg));
                }
                multiGameListener.AddGameListener((IGameListener)player);
                gameMaster.AddPlayer(new Player("Player", (IPlayerController)player));
                Console.WriteLine(player.GetType().Name + " is Player " + playerNumber);
            }
            int[] playerStats = new int[playerNumber + 1];
            const int numGames = 100000;
            for (var i = 0; i < numGames; ++i)
            {
                var loosingPlayer = gameMaster.StartGame();
                if (loosingPlayer >= 0)
                {
                    playerStats[loosingPlayer] += 1;
                }
                else
                {
                    playerStats[playerNumber] += 1;
                }
                var progress = (i / (double)numGames) * 100;
                if (progress % 10 == 0)
                {
                    Console.WriteLine(progress + "%");
                }
            }
            Console.WriteLine("Ties: {0}", (playerStats[playerNumber] / (double)numGames) * 100);
            for (int i = 0; i < playerStats.Length - 1; i++)
            {
                Console.WriteLine("Player {0} loose rate of {1}%", i + 1, (playerStats[i] / (double)numGames) * 100);
            }
        }

        static Object LoadBotFromAssembly(string assemblyPath)
        {
            try
            {
                var botAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
                foreach (var type in botAssembly.GetTypes())
                {
                    var interfaces = type.GetInterfaces();
                    if (interfaces.Contains(typeof(IGameListener)) && interfaces.Contains(typeof(IPlayerController)))
                    {
                        return Activator.CreateInstance(type);
                    }
                }
            }
            catch (ReflectionTypeLoadException e)
            {
                Console.WriteLine(e.Message);
                foreach (var loaderException in e.LoaderExceptions)
                {
                    Console.WriteLine(loaderException.Message);
                }
                throw e;
            }
            throw new Exception(string.Format(
                "Bot dll {0} does not contain a class that implements IGameListener and IPlayerController!", assemblyPath));
        }
    }
}
