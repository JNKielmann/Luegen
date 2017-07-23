1. mkdir MyBot
2. cd MyBot
3. dotnet new classlib
4. dotnet restore
5. rm .\Class1.cs
6. cp -r ..\BasicBot\src .
7. mv .\src\BasicBot.cs .\src\JnkBot.cs
8. dotnet restore