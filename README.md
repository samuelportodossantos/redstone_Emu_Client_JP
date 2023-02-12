### Introduction

This is a incomplete work, so there has a lot os unplemented packets, please let work to finish it.

This server will run inside a docker container. To setup your development build please fallow this steps:

1 - There has a Makefile file, if want to use this shortcut file, you must install Makefile. (Please open Makefile and take a look on commands)

```bash
#install make 
sudo apt-get install makefile
```

2 - If you won't use makefile, please don't forget to make a copy of .env.example as .env before run containers.

3 - Run make commands to up, down or recreate containers
```bash
#create a copy from .env.example and setup containers
make init 

#turn containers down
make down

#turn containers up
make up

#restart containers
make restart

#show server logs
make logs

#show database logs
make logs-db
```
### Important ###
Don't worry about database, it will create automatically.

### Namespace overview ###

| 名前空間 \| Namespace | 概要 \| Description |
|:-----------|:------------|
| RedStoneEmu.Database | Entity Data Model & DBContext |
| RedStoneEmu.Model | In-game abstractions |
| RedStoneEmu.NetWork| TCP/IP communication |
| RedStoneEmu.Packet| Packet handlers and outgoing packets |