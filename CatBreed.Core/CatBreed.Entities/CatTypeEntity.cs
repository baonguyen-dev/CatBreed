using CatBreed.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatBreed.Entities
{
    public class CatTypeEntity : BaseEntity
    {
        public string Name { get; set; }

        public string Type { get; set; }
    }
}
