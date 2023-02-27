using System;
using CatBreed.Entities.Base;

namespace CatBreed.Entities
{
    [Serializable]
    public class CatEntity : BaseEntity
	{
        public string Type { get; set; }
        public string Name { get; set; }
        public string ReferenceId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Url { get; set; }
    }
}

