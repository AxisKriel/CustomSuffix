/*
Current To-do List
[X] Put WIP source code on Git
[X] Make chat custom suffix shows up (OnChat) - Now plugin is usable but is without stability check and database save
[ ] SQLite and SQL Database to keep suffixes
[ ] OnJoin/OnLeave suffix information check
[ ] Timer to save database each period of time
[ ] Command to clear whole database
[ ] Server executed command check
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Timers;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace CustomSuffix
{
    [ApiVersion (1,15)]
    public class CustomSuffix : TerrariaPlugin
    {
        public override string Name { get { return "CustomSuffix"; } }
        public override string Description { get { return "Uses custom suffix for each user instead of group suffix"; } }
        public override string Author { get { return "AquaBlitz11"; } }
        public override Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }
        PlySuffix[] Psuffix = new PlySuffix[256];

        public CustomSuffix(Main game) : base(game)
        {

        }

        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.ServerJoin.Register(this, OnJoin);
            ServerApi.Hooks.ServerChat.Register(this, OnChat);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
        }

        protected override void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                ServerApi.Hooks.ServerJoin.Deregister(this, OnJoin);
                ServerApi.Hooks.ServerChat.Deregister(this, OnChat);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
            }
            base.Dispose(Disposing);
        }

        public void OnInitialize(EventArgs args)
        {
            Commands.ChatCommands.Add(new Command(CMDSetSuffix, "suffix")
            {
                AllowServer = false,
                HelpText = "Allow to use custom suffix."
            });
        }

        public void OnJoin(JoinEventArgs args)
        {
            Psuffix[args.Who] = new PlySuffix();
            //Database Clearance
        }

        public void OnChat(ServerChatEventArgs e)
        {
            if (e.Handled)
                return;
            TSPlayer tPly = TShock.Players[e.Who];
            PlySuffix sPly = Psuffix[e.Who];
            if (tPly == null || sPly == null)
                return;
            if (sPly.Suffix != null && !e.Text.StartsWith("/") && !tPly.mute)
            {
                e.Handled = true;
                TSPlayer.All.SendMessage(String.Format(TShock.Config.ChatFormat, tPly.Group.Name, tPly.Group.Prefix,
                    tPly.Name, sPly.Suffix, e.Text), tPly.Group.R, tPly.Group.G, tPly.Group.B);
            }
        }

        public void OnLeave(LeaveEventArgs args)
        {
            Psuffix[args.Who] = null;
            //Database Clearance
        }

        void CMDSetSuffix(CommandArgs e)
        { //Help Command
            if (e.Parameters.Count == 0 || e.Parameters[0].ToLower() == "help" || e.Parameters[0].ToLower() == "info"
                || e.Parameters[0].ToLower() == "command" || e.Parameters[0].ToLower() == "commands")
            {
                string PlginVer = String.Format("v{0}.{1}.{2}.{3}", Version.Major.ToString(), Version.Minor.ToString(), Version.Build.ToString(), Version.Revision.ToString());
                e.Player.SendMessage("Custom Suffix " + PlginVer +". Authored by AquaBlitz11", Color.Goldenrod);
                if (e.Player.Group.HasPermission(Permissions.sufset))
                {
                    e.Player.SendMessage("List of available commands:", Color.Gold);
                    e.Player.SendMessage("/suffix check - Check current custom suffix", Color.Gold);
                    e.Player.SendMessage("/suffix set - Set your own custom suffix", Color.Gold);
                    e.Player.SendMessage("/suffix toggle - Toggle usage of custom suffix", Color.Gold);
                    e.Player.SendMessage("You may append \"help\" right after the command to see explanation.", Color.Gold);
                }
                else
                    e.Player.SendMessage("You don't have any command available.", Color.Gold);
                
            } //Check Command
            else if (e.Parameters[0].ToLower() == "check")
            {
                if (e.Player.Group.HasPermission(Permissions.sufset))
                {
                    if (e.Parameters.Count == 1)
                    {
                        e.Player.SendMessage("Your current suffix information", Color.Goldenrod);
                        string CurrentSuffix = Psuffix[e.Player.Index].Suffix != null ? Psuffix[e.Player.Index].Suffix : "None";
                        string SuffixStatus = Psuffix[e.Player.Index].Status ? "On" : "Off";
                        e.Player.SendMessage("Current Suffix : " + CurrentSuffix, Color.Gold);
                        e.Player.SendMessage("Usage Status : " + SuffixStatus, Color.Gold);
                    }
                    else if (e.Parameters[1].ToLower() == "help")
                    {
                        e.Player.SendMessage("Help for custom suffix command : /suffix check", Color.Goldenrod);
                        if (e.Player.Group.HasPermission(Permissions.others))
                        {
                            e.Player.SendMessage("Available arguments : [player]", Color.Gold);
                            e.Player.SendMessage("[player] - Check another player's suffix information", Color.Gold);
                        }
                        else
                            e.Player.SendMessage("Available arguments : None", Color.Gold);
                        e.Player.SendMessage("Use to check current custom suffix text and status.", Color.Gold);
                    }
                    else
                    {
                        if (e.Player.Group.HasPermission(Permissions.others))
                        {
                            List<string> Name = new List<string>();
                            foreach (string NameFound in e.Parameters)
                            {
                                if (NameFound != e.Parameters[0])
                                    Name.Add(NameFound);
                            }
                            string PlayerName = String.Join(" ", Name.ToArray());
                            List<TSPlayer> Players = TShock.Utils.FindPlayer(PlayerName);
                            if (Players.Count == 0)
                                e.Player.SendErrorMessage("Invalid Player!");
                            else if (Players.Count > 1)
                                e.Player.SendErrorMessage("More than one player matched!");
                            else
                            {
                                TSPlayer Ply = Players[0];
                                e.Player.SendMessage(Ply.Name + "'s current suffix information", Color.Goldenrod);
                                string CurrentSuffix = Psuffix[Ply.Index].Suffix != null ? Psuffix[e.Player.Index].Suffix : "None";
                                string SuffixStatus = Psuffix[Ply.Index].Status ? "On" : "Off";
                                e.Player.SendMessage("Current Suffix : " + CurrentSuffix, Color.Gold);
                                e.Player.SendMessage("Usage Status : " + SuffixStatus, Color.Gold);
                            }
                        }
                        else
                        {
                            e.Player.SendMessage("Your current suffix information", Color.Goldenrod);
                            string CurrentSuffix = Psuffix[e.Player.Index].Suffix != null ? Psuffix[e.Player.Index].Suffix : "None";
                            string SuffixStatus = Psuffix[e.Player.Index].Status ? "On" : "Off";
                            e.Player.SendMessage("Current Suffix : " + CurrentSuffix, Color.Gold);
                            e.Player.SendMessage("Usage Status : " + SuffixStatus, Color.Gold);
                        }
                    }
                }
                else
                    e.Player.SendErrorMessage("You do not have access to this command.");
            }
            else if (e.Parameters[0].ToLower() == "set")
            {
                if (e.Player.Group.HasPermission(Permissions.sufset))
                {
                    if (e.Parameters.Count == 1 || e.Parameters[1].ToLower() == "help")
                    {
                        e.Player.SendMessage("Help for custom suffix command : /suffix set", Color.Goldenrod);
                        if (e.Player.Group.HasPermission(Permissions.others))
                        {
                            e.Player.SendMessage("Available arguments : [player] <suffix>", Color.Gold);
                            e.Player.SendMessage("[player] - Set another player's suffix", Color.Gold);
                            e.Player.SendMessage("<suffix> - New suffix to set", Color.Gold);
                        }
                        else
                        {
                            e.Player.SendMessage("Available arguments : <suffix>", Color.Gold);
                            e.Player.SendMessage("<suffix> - New suffix to set", Color.Gold);
                        }
                        e.Player.SendMessage("Use to set custom suffix to show on chat.", Color.Gold);
                    }
                    else
                    {
                        if (!e.Player.Group.HasPermission(Permissions.others))
                        {
                            List<string> Name = new List<string>();
                            foreach (string NameFound in e.Parameters)
                            {
                                if (NameFound != e.Parameters[0])
                                    Name.Add(NameFound);
                            }
                            string SuffixToSet = String.Join(" ", Name.ToArray());
                            Psuffix[e.Player.Index].Suffix = SuffixToSet;
                            Psuffix[e.Player.Index].Status = true;
                            e.Player.SendSuccessMessage("Your suffix has been set to {0}!", SuffixToSet);
                        }
                        else
                        {
                            if (e.Parameters.Count == 2)
                            {
                                Psuffix[e.Player.Index].Suffix = e.Parameters[1];
                                Psuffix[e.Player.Index].Status = true;
                                e.Player.SendSuccessMessage("Your suffix has been set to {0}!", e.Parameters[1]);
                            }
                            else
                            {
                                List<TSPlayer> PlayerFound = TShock.Utils.FindPlayer(e.Parameters[1]);
                                if (PlayerFound.Count == 0)
                                    e.Player.SendErrorMessage("Invalid Player!");
                                else if (PlayerFound.Count > 1)
                                    e.Player.SendErrorMessage("More than one player matched!");
                                else if (!PlayerFound[0].Group.HasPermission(Permissions.sufset))
                                {
                                    e.Player.SendErrorMessage("{0} doesn't have permission to use custom suffix.", PlayerFound[0].Name);
                                }
                                else
                                {
                                    List<string> Name = new List<string>();
                                    foreach (string NameFound in e.Parameters)
                                    {
                                        if (NameFound != e.Parameters[0] && NameFound != e.Parameters[1])
                                            Name.Add(NameFound);
                                    }
                                    string SuffixToSet = String.Join(" ", Name.ToArray());
                                    Psuffix[PlayerFound[0].Index].Suffix = SuffixToSet;
                                    Psuffix[PlayerFound[0].Index].Status = true;
                                    e.Player.SendSuccessMessage("{0}'s suffix has been set to {1}!", SuffixToSet, e.Parameters[1]);
                                    PlayerFound[0].SendInfoMessage("{0} changed your custom suffix to {1}!", e.Player.Name, SuffixToSet);
                                }
                            }
                        }
                    }
                }
                else
                    e.Player.SendErrorMessage("You do not have access to this command.");
            } //Toggle Command
            else if (e.Parameters[0].ToLower() == "toggle")
            {
                if (e.Player.Group.HasPermission(Permissions.sufset))
                {
                    if (e.Parameters.Count == 1 || e.Parameters[1].ToLower() == "help")
                    {
                        e.Player.SendMessage("Help for custom suffix command : /suffix toggle", Color.Goldenrod);
                        if (e.Player.Group.HasPermission(Permissions.others))
                        {
                            e.Player.SendMessage("Available arguments : [player] <status>", Color.Gold);
                            e.Player.SendMessage("[player] - Set another player's suffix status", Color.Gold);
                            e.Player.SendMessage("<status> - On, Off for suffix status or Del to delete suffix", Color.Gold);
                        }
                        else
                        {
                            e.Player.SendMessage("Available arguments : <suffix>", Color.Gold);
                            e.Player.SendMessage("<status> - On, Off for suffix status or Del to delete suffix", Color.Gold);
                        }
                        e.Player.SendMessage("Use to set status of custom suffix usage.", Color.Gold);
                    }
                    else
                    {
                        if (!e.Player.Group.HasPermission(Permissions.others))
                        {
                            if (e.Parameters[1].ToLower() == "on")
                            {
                                if (Psuffix[e.Player.Index].Suffix != null)
                                {
                                    if (!Psuffix[e.Player.Index].Status)
                                    {
                                        Psuffix[e.Player.Index].Status = true;
                                        e.Player.SendSuccessMessage("Your current custom suffix will now be in used.");
                                    }
                                    else
                                    {
                                        Psuffix[e.Player.Index].Status = true;
                                        e.Player.SendErrorMessage("You've been already using custom suffix.");
                                    }
                                }
                                else
                                {
                                    Psuffix[e.Player.Index].Status = false;
                                    e.Player.SendErrorMessage("You have no custom suffix yet. Please set by \'/suffix set\'");
                                }
                            }
                            else if (e.Parameters[1].ToLower() == "off")
                            {
                                if (Psuffix[e.Player.Index].Status)
                                {
                                    Psuffix[e.Player.Index].Status = false;
                                    e.Player.SendSuccessMessage("You will be using normal group suffix instead.");
                                }
                                else
                                {
                                    Psuffix[e.Player.Index].Status = false;
                                    e.Player.SendErrorMessage("You've been already ignoring custom suffix.");
                                }
                            }
                            else if (e.Parameters[1].ToLower() == "del" || e.Parameters[1].ToLower() == "delete")
                            {
                                if (Psuffix[e.Player.Index].Suffix != null)
                                {
                                    Psuffix[e.Player.Index].Status = false;
                                    Psuffix[e.Player.Index].Suffix = null;
                                    e.Player.SendSuccessMessage("Your custom suffix is now removed.");
                                }
                                else
                                {
                                    Psuffix[e.Player.Index].Status = false;
                                    Psuffix[e.Player.Index].Suffix = null;
                                    e.Player.SendErrorMessage("You have no custom suffix to remove.");
                                }
                            }
                            else
                            {
                                e.Player.SendErrorMessage("Insufficient arguments. Use \'help\' as argument for proper usage.");
                            }
                        }
                        else
                        {
                            if (e.Parameters.Count == 2)
                            {
                                if (e.Parameters[1].ToLower() == "on")
                                {
                                    if (Psuffix[e.Player.Index].Suffix != null)
                                    {
                                        if (!Psuffix[e.Player.Index].Status)
                                        {
                                            Psuffix[e.Player.Index].Status = true;
                                            e.Player.SendSuccessMessage("Your current custom suffix will now be in used.");
                                        }
                                        else
                                        {
                                            Psuffix[e.Player.Index].Status = true;
                                            e.Player.SendErrorMessage("You've been already using custom suffix.");
                                        }
                                    }
                                    else
                                    {
                                        Psuffix[e.Player.Index].Status = false;
                                        e.Player.SendErrorMessage("You have no custom suffix yet. Please set by \'/suffix set\'");
                                    }
                                }
                                else if (e.Parameters[1].ToLower() == "off")
                                {
                                    if (Psuffix[e.Player.Index].Status)
                                    {
                                        Psuffix[e.Player.Index].Status = false;
                                        e.Player.SendSuccessMessage("You will be using normal group suffix instead.");
                                    }
                                    else
                                    {
                                        Psuffix[e.Player.Index].Status = false;
                                        e.Player.SendErrorMessage("You've been already ignoring custom suffix.");
                                    }
                                }
                                else if (e.Parameters[1].ToLower() == "del" || e.Parameters[1].ToLower() == "delete" || e.Parameters[1].ToLower() == "remove")
                                {
                                    if (Psuffix[e.Player.Index].Suffix != null)
                                    {
                                        Psuffix[e.Player.Index].Status = false;
                                        Psuffix[e.Player.Index].Suffix = null;
                                        e.Player.SendSuccessMessage("Your custom suffix is now removed.");
                                    }
                                    else
                                    {
                                        Psuffix[e.Player.Index].Status = false;
                                        Psuffix[e.Player.Index].Suffix = null;
                                        e.Player.SendErrorMessage("You have no custom suffix to remove.");
                                    }
                                }
                                else
                                {
                                    e.Player.SendErrorMessage("Insufficient arguments. Use \'help\' as argument for proper usage.");
                                }
                            }
                            else
                            {
                                List<TSPlayer> PlayerFound = TShock.Utils.FindPlayer(e.Parameters[1]);
                                if (PlayerFound.Count == 0)
                                    e.Player.SendErrorMessage("Invalid Player!");
                                else if (PlayerFound.Count > 1)
                                    e.Player.SendErrorMessage("More than one player matched!");
                                else if (!PlayerFound[0].Group.HasPermission(Permissions.sufset))
                                    e.Player.SendErrorMessage("{0} doesn't have permission to use custom suffix.", PlayerFound[0].Name);
                                else
                                {
                                    var PlyID = PlayerFound[0].Index;
                                    if (e.Parameters[2].ToLower() == "on")
                                    {
                                        if (Psuffix[PlyID].Suffix != null)
                                        {
                                            if (!Psuffix[PlyID].Status)
                                            {
                                                Psuffix[PlyID].Status = true;
                                                e.Player.SendSuccessMessage("{0}'s current custom suffix will now be in used.", PlayerFound[0].Name);
                                                PlayerFound[0].SendInfoMessage("{0} has turned on your custom suffix usage.", e.Player.Name);
                                            }
                                            else
                                            {
                                                Psuffix[PlyID].Status = true;
                                                e.Player.SendErrorMessage("{0} has been already using custom suffix.", PlayerFound[0].Name);
                                            }
                                        }
                                        else
                                        {
                                            Psuffix[PlyID].Status = false;
                                            e.Player.SendErrorMessage("{0} has no custom suffix yet.", PlayerFound[0].Name);
                                        }
                                    }
                                    else if (e.Parameters[2].ToLower() == "off")
                                    {
                                        if (Psuffix[PlyID].Status)
                                        {
                                            Psuffix[PlyID].Status = false;
                                            e.Player.SendSuccessMessage("{0} will be using normal group suffix instead.", PlayerFound[0].Name);
                                            PlayerFound[0].SendInfoMessage("{0} has turned off your custom suffix usage.", e.Player.Name);
                                        }
                                        else
                                        {
                                            Psuffix[PlyID].Status = false;
                                            e.Player.SendErrorMessage("{0} has been already ignoring custom suffix.", PlayerFound[0].Name);
                                        }
                                    }
                                    else if (e.Parameters[2].ToLower() == "del" || e.Parameters[1].ToLower() == "delete" || e.Parameters[1].ToLower() == "remove")
                                    {
                                        if (Psuffix[PlyID].Suffix != null)
                                        {
                                            Psuffix[PlyID].Status = false;
                                            Psuffix[PlyID].Suffix = null;
                                            e.Player.SendSuccessMessage("{0r}'s custom suffix is now removed.", PlayerFound[0].Name);
                                            PlayerFound[0].SendInfoMessage("{0} has removed your custom suffix.", e.Player.Name);
                                        }
                                        else
                                        {
                                            Psuffix[PlyID].Status = false;
                                            Psuffix[PlyID].Suffix = null;
                                            e.Player.SendErrorMessage("{0} has no custom suffix to remove.", PlayerFound[0].Name);
                                        }
                                    }
                                    else
                                    {
                                        e.Player.SendErrorMessage("Insufficient arguments. Use \'help\' as argument for proper usage.");
                                    }
                                }
                            }
                        }
                    }
                }
                else
                    e.Player.SendErrorMessage("You do not have access to this command.");
            }
        }
    }
}
