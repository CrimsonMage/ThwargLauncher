﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace MagFilter
{
    public class CharacterManager
    {
        private Dictionary<string, ServerCharacterListByAccount> _data = null;

        private CharacterManager(Dictionary<string, ServerCharacterListByAccount> dictionary)
        {
            _data = dictionary;
        }
        private CharacterManager()
        {
            _data = new Dictionary<string, ServerCharacterListByAccount>();
        }
        public IEnumerable<string> GetKeys()
        {
            log.WriteLogMsg("GetKeys function: " + _data.Keys);
            return _data.Keys;
        }
        public ServerCharacterListByAccount GetCharacters(string serverName, string accountName)
        {
            string key = GetKey(server: serverName, accountName: accountName);
            if (this._data.ContainsKey(key))
            {
                log.WriteLogMsg("GetChars sN aN Function: " + this._data[key]);
                return this._data[key];
            }
            else
            {
                return null;
            }
        }
        internal ServerCharacterListByAccount GetCharacters(string key)
        {
            log.WriteLogMsg("GetChars key Function: " + this._data[key]);
            return this._data[key];
        }
        private static string GetKey(string server, string accountName)
        {
            log.WriteLogMsg("GetKey function: " + string.Format("{0}-{1}", server, accountName));
            return string.Format("{0}-{1}", server, accountName);
        }

        public void WriteCharacters(string server, string zonename, List<Character> characters)
        {
            var launchInfo = LaunchControl.GetLaunchInfo();
            if (!launchInfo.IsValid)
            {
                log.WriteLogMsg("LaunchInfo not valid");
                return;
            }
            if (!IsValidCharacterName(launchInfo.CharacterName))
            {
                try
                {
                    LaunchControl.RecordLaunchResponse(DateTime.UtcNow);
                }
                catch
                {
                    log.WriteLogMsg("WriteCharacters: Exception trying to record launch response");
                }
            }
            log.WriteLogMsg("LaunchInfo valid");

            // Pass info to Heartbeat
            Heartbeat.RecordServer(launchInfo.ServerName);
            Heartbeat.RecordAccount(launchInfo.AccountName);
            GameRepo.Game.SetServerAccount(server: launchInfo.ServerName, account: launchInfo.AccountName);

            string key = GetKey(server: server, accountName: launchInfo.AccountName);
            var clist = new ServerCharacterListByAccount()
                {
                    ZoneId = zonename,
                    CharacterList = characters
                };
            this._data[key] = clist;
            string contents = JsonConvert.SerializeObject(_data, Formatting.Indented);
            string path = FileLocations.GetCharacterFilePath();
            using (var file = new StreamWriter(path, append: false))
            {
                file.Write(contents);
            }
        }

        private bool IsValidCharacterName(string characterName)
        {
            if (string.IsNullOrEmpty(characterName)) { return false; }
            if (characterName == "None") { return false; }
            return true;
        }

        public static CharacterManager ReadCharacters()
        {
            try
            {
                log.WriteLogMsg("ReadCharacterImpl: " + ReadCharactersImpl());
                return ReadCharactersImpl();
            }
            catch (Exception exc)
            {
                log.WriteLogMsg("ReadCharacterImpl Exception: " + exc.ToString());
                return null;
            }
        }

        private static CharacterManager ReadCharactersImpl()
        {
            string path = FileLocations.GetCharacterFilePath();
            if (!File.Exists(path))
            {
                path = FileLocations.GetOldCharacterFilePath();
            }

            if (!File.Exists(path)) { return new CharacterManager(); }
            using (var file = new StreamReader(path))
            {
                string contents = file.ReadToEnd();
                var data = JsonConvert.DeserializeObject<Dictionary<string, ServerCharacterListByAccount>>(contents);
                CharacterManager charMgr = new CharacterManager(data);
                return charMgr;
            }
        }
    }
}
