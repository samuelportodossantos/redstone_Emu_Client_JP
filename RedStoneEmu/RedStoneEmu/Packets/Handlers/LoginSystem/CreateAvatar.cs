using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RedStoneEmu.Database.PhpbbEF;
using RedStoneEmu.Database.RedStoneEF;
using RedStoneEmu.Provider;
using RedStoneLib.Model;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RedStoneLib.Model.Player;

namespace RedStoneEmu.Packets.Handlers.LoginSystem
{
    /// <summary>
    /// キャラ作成
    /// </summary>
    [PacketHandlerAttr(0x1004)]
    class CreateAvatar : PacketHandler
    {
        /// <summary>
        /// 新規作成
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="charName"></param>
        /// <param name="job"></param>
        public Player CreatePlayer(string userID, string charName, JOB job, gameContext dbForSaveItems)
        {
            Player player = new Player();
            player.UserID = userID;
            player.Name = charName;
            player.Job = job;
            player.Level = 1;
            player.MapSerial = 0;

            //装備・ステ
            switch (job)
            {
                case JOB.Swordsman://剣士
                case JOB.Warrior://戦士
                    player.EquipmentItems.WeaponLeft = new Item(152);
                    player.EquipmentItems.WeaponRight = new Item(172);
                    player.EquipmentItems.Shield = new Item(165);
                    player.BaseHP = 30;
                    player.BaseCP = 0;
                    player.BaseStatus = new ActorStatus()
                    {
                        Strength = 25,
                        Inteligence = 5,
                        Agility = 15,
                        Wisdom = 10,
                        Condition = 20,
                        Charisma = 15,
                        Luckey = 0
                    };
                    break;
                case JOB.Wizard://ウィザード
                case JOB.Wolfman://ウルフマン
                    player.EquipmentItems.WeaponLeft = new Item(180);
                    player.EquipmentItems.WeaponRight = new Item(185);
                    player.BaseHP = 15;
                    player.BaseCP = 15;
                    player.BaseStatus = new ActorStatus()
                    {
                        Strength = 5,
                        Inteligence = 25,
                        Agility = 5,
                        Wisdom = 20,
                        Condition = 10,
                        Charisma = 20,
                        Luckey = 5
                    };
                    break;
                case JOB.Bishop://ビショップ
                case JOB.Angel://追放天使
                    player.EquipmentItems.WeaponLeft = new Item(189);
                    player.EquipmentItems.WeaponRight = new Item(198);
                    player.BaseHP = 25;
                    player.BaseCP = 5;
                    player.BaseStatus = new ActorStatus()
                    {
                        Strength = 20,
                        Inteligence = 15,
                        Agility = 10,
                        Wisdom = 20,
                        Condition = 15,
                        Charisma = 10,
                        Luckey = 0
                    };
                    break;
                case JOB.Thief://シーフ
                case JOB.Monk://武闘家
                    player.EquipmentItems.WeaponLeft = new Item(203, 255);
                    player.EquipmentItems.Hand = new Item(71);
                    player.BaseHP = 20;
                    player.BaseCP = 0;
                    player.BaseStatus = new ActorStatus()
                    {
                        Strength = 15,
                        Inteligence = 10,
                        Agility = 25,
                        Wisdom = 5,
                        Condition = 10,
                        Charisma = 15,
                        Luckey = 10
                    };
                    break;
                case JOB.Lancer://ランサー
                case JOB.Archer://アーチャー
                    player.EquipmentItems.WeaponLeft = new Item(224);
                    player.EquipmentItems.WeaponRight = new Item(211);
                    player.EquipmentItems.Shield = new Item(216, 255);
                    player.BaseHP = 15;
                    player.BaseCP = 15;
                    player.BaseStatus = new ActorStatus()
                    {
                        Strength = 15,
                        Inteligence = 10,
                        Agility = 25,
                        Wisdom = 10,
                        Condition = 15,
                        Charisma = 10,
                        Luckey = 5
                    };
                    break;
                case JOB.Tamer://テイマー
                case JOB.Summoner://サマナー
                    player.EquipmentItems.WeaponLeft = new Item(237);
                    player.EquipmentItems.WeaponRight = new Item(237);
                    player.BaseHP = 30;
                    player.BaseCP = 0;
                    player.BaseStatus = new ActorStatus()
                    {
                        Strength = 25,
                        Inteligence = 5,
                        Agility = 15,
                        Wisdom = 10,
                        Condition = 20,
                        Charisma = 15,
                        Luckey = 0
                    };
                    break;
                case JOB.Princess://プリンセス
                case JOB.LittleWitch://リトルウィッチ
                    player.EquipmentItems.WeaponLeft = new Item(243);
                    player.EquipmentItems.WeaponRight = new Item(254);
                    player.BaseHP = 25;
                    player.BaseCP = 5;
                    player.BaseStatus = new ActorStatus()
                    {
                        Strength = 10,
                        Inteligence = 15,
                        Agility = 10,
                        Wisdom = 20,
                        Condition = 15,
                        Charisma = 15,
                        Luckey = 5
                    };
                    break;
                case JOB.Necro://ネクロ
                case JOB.Demon://悪魔
                    player.EquipmentItems.WeaponLeft = new Item(259);
                    player.EquipmentItems.WeaponRight = new Item(259);
                    player.BaseHP = 20;
                    player.BaseCP = 10;
                    player.BaseStatus = new ActorStatus()
                    {
                        Strength = 25,
                        Inteligence = 5,
                        Agility = 15,
                        Wisdom = 10,
                        Condition = 20,
                        Charisma = 15,
                        Luckey = 0
                    };
                    break;
                case JOB.NumerologyTeacher://霊術師
                case JOB.Fighter://闘士
                    player.EquipmentItems.WeaponLeft = new Item(4300);
                    player.EquipmentItems.WeaponRight = new Item(4553);
                    player.BaseHP = 15;
                    player.BaseCP = 15;
                    player.BaseStatus = new ActorStatus()
                    {
                        Strength = 20,
                        Inteligence = 15,
                        Agility = 15,
                        Wisdom = 10,
                        Condition = 15,
                        Charisma = 15,
                        Luckey = 0
                    };
                    break;
                case JOB.LightMaster://光奏師
                    player.EquipmentItems.WeaponLeft = new Item(4714);
                    player.BaseHP = 15;
                    player.BaseCP = 15;
                    player.BaseStatus = new ActorStatus()
                    {
                        Strength = 20,
                        Inteligence = 20,
                        Agility = 10,
                        Wisdom = 10,
                        Condition = 10,
                        Charisma = 10,
                        Luckey = 10
                    };
                    break;
                default:
                    break;
            }
            player.StateHPCPBonus = 100;
            player.LevelHPCPBobuns = 100;

            //Itemに追加
            dbForSaveItems?.Items.AddRange(player.EquipmentItems.Where(t => !t.IsEmpty));

            //スキル
            player.PlayerSkills = PlayerSkill.GetInitSkill(job);

            //ミニP
            player.MiniPet1 = 0x18;
            player.MiniPet2 = 0x18;

            //暫定
            //GMLevel = 7;
            player.Reflesh();
            // player.PosX = 74 * 64;
            // player.PosY = 67 * 32;
            player.PosX = 3000;
            player.PosY = 3000;
            player.NowHP = player.MaxHP;
            return player;
        }

        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            //職業
            var job = (Player.JOB)reader.ReadInt16();

            //セキュリティコード
            var packet_security_code = reader.ReadUInt16();

            //不明
            reader.ReadUInt32();

            //名前
            string charName = reader.ReadSjis(0x12);

            //セキュリティコード違う
            if (context.PacketSecurityCode != packet_security_code)
            {
                Logger.WriteError("セキュリティコードが違います．>>[{0}]", charName);
                context.Disconnect();
                return;
            }

            //パケットセキュリティコード変更
            context.PacketSecurityCode = (ushort)(new Random().Next());


            using (var db = new gameContext())
            {
                //すでに存在する名前
                var task = db.Players.SingleOrDefaultAsync(t => t.Name == charName);
                task.Wait();
                if (task.Result != null)
                {
                    context.SendPacket(new ResultCreateAvatarPacket(ResultCreateAvatarPacket.CreateAvatarResult.NameExist, context.PacketSecurityCode));
                    return;
                }

                try
                {
                    //作成
                    var NewPlayer = CreatePlayer(context.User.UserID, charName, job, db);

                    //GMチェック
                    var phpbb = new PhpBBContext();
                    var account = phpbb.phpbb_users.SingleOrDefaultAsync(t => t.username == context.User.UserID).Result;
                    if (account?.user_rank == 1)
                    {
                        NewPlayer.GMLevel = 7;
                    }

                    db.Players.Add(NewPlayer);
                    db.SaveChangesAsync().Wait();

                    //ローカルで追加
                    ((LoginClient)context).Avatars.Add(NewPlayer);

                    //成功パケ
                    int avatarIndex = ((LoginClient)context).Avatars.Count - 1;
                    context.SendPacket(new ResultCreateAvatarPacket(NewPlayer,
                        (ushort)avatarIndex,
                        ServerList.GameServerInfos.SingleOrDefault(t => t.ServerId == context.ServerID)?.Host,
                        context.PacketSecurityCode)
                        );
                }
                catch (Exception ex)
                {
                    //失敗
                    context.SendPacket(new ResultCreateAvatarPacket(ResultCreateAvatarPacket.CreateAvatarResult.UnknownFailed1, context.PacketSecurityCode));
                    Logger.WriteException("[RemoveAvatar]", ex);
                }
            }
        }
    }
}
