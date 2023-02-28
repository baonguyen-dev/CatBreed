using CatBreed.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatBreed.ApiClient.Models
{
    public class CatTypeModel
    {
        public string Name { get; set; }

        public string Type { get; set; }
    }

    public static class CatTypeModelEmm
    {
        public static CatTypeModel ToCatTypeViewModel(this CatTypeEntity catBreedModel)
        {
            return new CatTypeModel()
            {
                Type = catBreedModel.Type,
                Name = catBreedModel.Name
            };
        }

        public static List<CatTypeModel> ToCatBreedViewModels(this List<CatTypeEntity> entities)
        {
            var catTypeViewModels = new List<CatTypeModel>();

            if (entities != null)
            {
                foreach (var item in entities)
                {
                    catTypeViewModels.Add(new CatTypeModel()
                    {
                        Name = item.Name,
                        Type = item.Type,
                    });
                }

                return catTypeViewModels;
            }

            return catTypeViewModels;
        }
    }
}
