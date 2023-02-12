FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine

WORKDIR /app

COPY . .

CMD dotnet run --project /app/RedStoneEmu/RedStoneEmu