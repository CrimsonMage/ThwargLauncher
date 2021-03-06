﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ThwargLauncher.UtilityCode;

namespace ThwargLauncher
{
    public class ServerManager
    {
        public static ObservableCollection<ServerModel> ServerList = new ObservableCollection<ServerModel>();
        public static bool IsLoaded;


        public static void LoadServerLists()
        {
            try
            {
                string folder = GetServerDataFolder();
                var persister = new GameManagement.ServerPersister(folder);
                var publishedPhatServers = persister.GetPublishedPhatServerList();
                var publishedAceServers = persister.GetPublishedACEServerList();
                var publishedDFServers = persister.GetPublishedDFServerList();
                var userServers = persister.ReadUserServers();

                var servers = new List<GameManagement.ServerPersister.ServerData>();
                servers.AddRange(publishedPhatServers);
                servers.AddRange(publishedAceServers);
                servers.AddRange(publishedDFServers);
                servers.AddRange(userServers);
                var distinctServers = servers.Distinct().ToList();
                foreach (var sdata in distinctServers)
                {
                    AddOrUpdateServer(sdata);

                }
                IsLoaded = true;
            }
            catch(Exception exc)
            {
                Logger.WriteError("Unable to Load server list: " + exc.ToString());
            }
        }
        private static string GetServerDataFolder()
        {
            string folderpath = System.IO.Path.Combine(MagFilter.FileLocations.AppFolder, "Servers");
            folderpath = MagFilter.FileLocations.ExpandFilepath(folderpath);
            MagFilter.FileLocations.CreateAnyNeededFoldersOfFolder(folderpath);
            return folderpath;
        }
        private static void AddOrUpdateServer(GameManagement.ServerPersister.ServerData servdata)
        {
            var existing = ServerList.FirstOrDefault(s => s.IsEqual(servdata));
            if (existing != null)
            {
                existing.ServerName = servdata.ServerName;
                existing.ServerDescription = servdata.ServerDesc;
                existing.ServerIpAndPort = servdata.ConnectionString;
                existing.RodatSetting = servdata.RodatSetting;
                existing.VisibilitySetting = servdata.VisibilitySetting;
                existing.EMU = servdata.EMU;
            }
            else
            {
                ServerModel model = ServerModel.Create(servdata);
                ServerList.Add(model);
            }
        }
        internal static void AddNewServer(GameManagement.ServerPersister.ServerData servdata)
        {
            AddOrUpdateServer(servdata);
            SaveServerListToDisk();
        }
        internal static void DeleteServerById(Guid id)
        {
            var existing = ServerList.FirstOrDefault(s => s.ServerId == id);
            if (existing == null) { return; }
            ServerList.Remove(existing);
            SaveServerListToDisk();
        }
        internal static void SaveServerListToDisk()
        {
            var userServers = ServerList.Where(s => s.ServerSource != ServerModel.ServerSourceEnum.Published);

            var persister = new GameManagement.ServerPersister(GetServerDataFolder());
            persister.WriteServerListToFile(userServers);
        }
    }
}
