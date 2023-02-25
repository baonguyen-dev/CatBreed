using System;
using System.IO;
using CatBreed.Entities;
using CatBreed.Repositories.Base;

namespace CatBreed.Repositories
{
    public class CatBreedRepository : BaseRepository<CatEntity>
    {
        protected override string DbPath => Path.Combine(GetPath(), "CATBREED.DB");

        public string GetPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }
    }
}

