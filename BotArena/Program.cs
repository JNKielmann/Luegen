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
            multiGameListener.AddGameListener(new ConsoleGameOutput());
            var gameMaster = new GameMaster(multiGameListener);

            foreach (var arg in args)
            {
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
                gameMaster.AddPlayer(new Player("Player2", (IPlayerController)player));
            }

            gameMaster.StartGame();
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
