using CppHeaderTool.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool
{
    class ParserCacheResult
    {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
        private static BinaryFormatter binaryFormatter = new();
#pragma warning restore SYSLIB0011 // Type or member is obsolete
        static ParserCacheResult()
        {

        }

        [Serializable]
        class CacheData
        {
            public HtTables htTables;


            public static CacheData Deserialize(string path)
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                return binaryFormatter.Deserialize(stream) as CacheData;
            }
            public void Serialize(string path)
            {
                using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
                binaryFormatter.Serialize(stream, this);
            }
        }


        public static void Load(string path)
        {
            var cacheData = CacheData.Deserialize(path);
            Session.tables = cacheData.htTables;
        }

        public static void Save(string path)
        {
            new CacheData() { htTables = Session.tables }.Serialize(path);
        }
    }
}
