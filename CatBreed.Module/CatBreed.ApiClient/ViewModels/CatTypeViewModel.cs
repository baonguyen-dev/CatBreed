using CatBreed.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatBreed.ApiClient.ViewModels
{
    public class CatTypeViewModel
    {
        public string Name { get; set; }

        public string Type { get; set; }
    }

    public static class CatTypeViewModelEmm
    {
        public static CatTypeViewModel ToCatTypeViewModel(this CatTypeEntity catBreedModel)
        {
            return new CatTypeViewModel()
            {
                Type = catBreedModel.Type,
                Name = catBreedModel.Name
            };
        }

        public static List<CatTypeViewModel> ToCatBreedViewModels(this List<CatTypeEntity> entities)
        {
            var catTypeViewModels = new List<CatTypeViewModel>();

            if (entities != null)
            {
                foreach (var item in entities)
                {
                    catTypeViewModels.Add(new CatTypeViewModel()
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
