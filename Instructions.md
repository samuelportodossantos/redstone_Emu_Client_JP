## Database config
(postgresql)

Port: 5432
Host: localhost
Username: root
Password: admin

These configurations can be altered at `Program.cs::ConfigureServices`

### Create migrations:
Run inside RedStoneEmu/RedStoneEmu
```
dotnet ef migrations add <name> --context <context>
```
name: name of the migration
context: dbContext to run
* `RedStoneEmu.Database.RedStoneEF.loginContext`
* `RedStoneEmu.Database.RedStoneEF.gameContext`
* `RedStoneEmu.Database.PhpbbEF.PhpBBContext`


### Update Database with latest migrations
Run inside RedStoneEmu/RedStoneEmu
```
dotnet ef database update --context <context>
```

Same contexts as above