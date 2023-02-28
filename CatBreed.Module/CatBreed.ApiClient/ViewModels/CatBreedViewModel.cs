using CatBreed.ApiClient.Models;
using CatBreed.Entities;
using CatBreed.Repositories;
using CatBreed.ServiceLocators.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatBreed.ApiClient.ViewModels
{
    public class CatBreedViewModel
    {
        private const int _count = 15;

        private ICatBreedClient _catBreedClient => ServiceLocator.Instance.Get<ICatBreedClient>();

        public async Task<List<CatTypeEntity>> GetAllCatType()
        {
            var catTypeRepository = new CatTypeRepository();

            List<CatTypeEntity> entities = catTypeRepository.LoadAll().ToList();

            if (entities == null || entities.Count == 0)
            {
                var allBread = await _catBreedClient.GetCatBreed() as List<ApiClient.TheCatModel>;

                foreach (var item in allBread)
                {
                    entities.Add(new CatTypeEntity()
                    {
                        Name = item.Name,
                        Type = item.Id
                    });
                }

                catTypeRepository.InsertAll(entities);
            }

            return entities;
        }

        public async Task<List<CatBreedModel>> QueryCatBreed(int size)
        {
            var catBreedViewModels = new List<CatBreedModel>();

            var tempCatBreedModels = await _catBreedClient.GetCatBreed(size) as List<TheCatModel>;

            if (tempCatBreedModels.Count > _count)
            {
                tempCatBreedModels = tempCatBreedModels.GetRange(tempCatBreedModels.Count - _count, _count);
            }

            foreach (var item in tempCatBreedModels)
            {
                if (!string.IsNullOrEmpty(item.ReferenceImageId))
                {
                    var referenceImage = await _catBreedClient.GetReferenceImage(item.ReferenceImageId);

                    catBreedViewModels.Add(item.ToCatBreedModel(referenceImage));
                }
                else
                {
                    catBreedViewModels.Add(item.ToCatBreedModel(new ReferenceImage()
                    {
                        Width = 1000,
                        Height = 1000,
                    }));
                }
            }

            return catBreedViewModels;
        }

        public async Task<List<CatBreedModel>> QueryCatImageType(string queryBreed, int size = 15)
        {
            var referenceModels = await _catBreedClient.GetCatBreedIds(queryBreed, size) as List<ReferenceImage>;

            if (referenceModels.Count > _count)
            {
                referenceModels = referenceModels.GetRange(referenceModels.Count - _count, _count);
            }

            return referenceModels.ToCatBreedModels(QueryType.SAMPLE);
        }
    }
}
