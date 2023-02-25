using System;
using System.Collections.Generic;
using CatBreed.Entities;

namespace CatBreed.ApiClient.ViewModels
{
	public class CatBreedViewModel
	{
		public string Name { get; set; }
		public string ReferenceId { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public string Url { get; set; }
	}

	public static class CatBreedViewModelEmm
	{
		public static CatBreedViewModel ToCatBreedViewModel(this CatBreedModel catBreedModel, ReferenceImage referenceImage)
		{
			return new CatBreedViewModel()
			{
				Name = catBreedModel.Name,
				ReferenceId = referenceImage.Id,
				Width = referenceImage.Width,
				Height = referenceImage.Height,
				Url = referenceImage.Url
			};
		}

		public static List<CatBreedViewModel> ToCatBreedViewModels(this List<CatEntity> entities)
        {
            var catBreedViewModels = new List<CatBreedViewModel>();

            if (entities != null)
			{
				foreach (var item in entities)
				{
					catBreedViewModels.Add(new CatBreedViewModel()
					{
						Name = item.Name,
						Width = item.Width,
						Height = item.Height,
						Url = item.Url
					});
				}

				return catBreedViewModels;
			}

			return catBreedViewModels;
        }

		public static List<CatBreedViewModel> ToCatBreedViewModels(this List<ReferenceImage> entities)
		{
            var catBreedViewModels = new List<CatBreedViewModel>();

            if (entities != null)
            {
                foreach (var item in entities)
                {
                    catBreedViewModels.Add(new CatBreedViewModel()
                    {
                        Name = item.Id,
                        Width = item.Width,
                        Height = item.Height,
                        Url = item.Url
                    });
                }

                return catBreedViewModels;
            }

            return catBreedViewModels;
        }
	}
}

