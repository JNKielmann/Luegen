The .NET Core SDK is needed to run the code
run dotnet restore in folders Core BotArena and BasicBot

Creating a new bot:
1. mkdir MyBot
2. cd MyBot
3. dotnet new classlib
4. dotnet restore
5. dotnet add reference ../Core/Core.csproj
6. rm ./Class1.cs
7. cp -r ../BasicBot/src .
8. mv ./src/BasicBot.cs ./src/MyBot.cs

Now build your bot in MyBot.cs
To build your bot run
dotnet build
You can then let it play against itself by running the command 
dotnet run -p BotArena/BotArena.csproj -- ./LuegenmeisterBot/bin/Debug/netstandard1.4/LuegenmeisterBot.dll ./LuegenmeisterBot/bin/Debug/netstandard1.4/LuegenmeisterBot.dll
from the root directory. You can also edit example_bot_vs_bot.bat
