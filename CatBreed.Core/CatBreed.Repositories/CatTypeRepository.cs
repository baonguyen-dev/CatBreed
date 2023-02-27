using CatBreed.Entities;
using CatBreed.Repositories.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CatBreed.Repositories
{
    public class CatTypeRepository : BaseRepository<CatTypeEntity>
    {
        protected override string DbPath => Path.Combine(GetPath(), "CATTYPE.DB");

        public string GetPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }
    }
}
