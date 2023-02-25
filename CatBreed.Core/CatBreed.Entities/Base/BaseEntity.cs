using System;
using SQLite;

namespace CatBreed.Entities.Base
{
    [Serializable]
    public class BaseEntity : BaseObject
    {
        [PrimaryKey, AutoIncrement]
        public UInt32 Id { get; set; }

        [Indexed]
        public UInt32 CloudId { get; set; }

        public int IsStatus { get; set; }

        public byte[] Blob { get; set; }

        public BaseEntity()
        {

        }
    }
}

