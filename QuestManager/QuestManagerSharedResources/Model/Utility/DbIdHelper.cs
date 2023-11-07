using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace QuestManager.Utility
{
    public static class DbIdHelper
    {
        internal static readonly char[] chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

        const byte uniqueKeyLength = 5;//short key to make it easy to type into console but hard to assure unique
        const char seperator = '-';
        public static string GenerateQuestId(int currentCollectionCount, string idPrefix)
        {
            //The id format is based on a prefix from manager,
            //the id item will have and a random string to prevent repetition(where id is reused)          
            //format looks something like Qst9999aH348
            return $"{idPrefix}{currentCollectionCount + 1}{GetUniqueKey(uniqueKeyLength)}";
        }

        public static string GenerateSubObjectID(string questId,string subObjectPrefix)
        {
            return $"{questId}{seperator}{subObjectPrefix}{GetUniqueKey(uniqueKeyLength)}";
        }

        public static string ReadQuestIDFromSubObjectId(string subObjectId)
        {
            return subObjectId.Split(seperator)[0];
        }

        //an unbiased,cryptographically sound string generator taken from https://stackoverflow.com/questions/1344221
        private static string GetUniqueKey(int size)
        {
            byte[] data = new byte[4 * size];
            using (var crypto = RandomNumberGenerator.Create())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % chars.Length;

                result.Append(chars[idx]);
            }

            return result.ToString();
        }
    }
}
