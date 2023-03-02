using CatBreed.ApiClient;
using CatBreed.ApiClient.Models;
using CatBreed.ApiClient.ViewModels;
using CatBreed.ServiceLocators.DI;
using CatBreed.ServiceLocators.Services;

namespace CatBreed.NUnitTest
{
    [TestFixture]
    public class ViewModelTests
    {
        private CatBreedViewModel _viewModel;
        private ICatBreedClient _catBreedClient => ServiceLocator.Instance.Get<ICatBreedClient>();

        private IFileService _fileService => ServiceLocator.Instance.Get<IFileService>();

        const string BASE_URL = "https://api.thecatapi.com/";

        [SetUp]
        public void Setup()
        {
            ServiceLocator.Instance.Register<ICatBreedClient, CatBreedClient>(new Uri(BASE_URL), string.Empty);

            _viewModel = new CatBreedViewModel(_catBreedClient, _fileService);
        }

        [Test]
        public async Task GetAllCatType_WhenCalled_ReturnListOfCatType()
        {
            var samples = new List<CatTypeModel>();

            samples.Add(new CatTypeModel()
            {

            });

            var catTypes = await _viewModel.GetAllCatType();

            Assert.IsNotNull(catTypes);
        }

        [Test]
        public async Task QueryCatBreed_WhenCalled_ReturnListOfBreed()
        {
            var size = 15;

            var catTypes = await _viewModel.QueryCatBreed(size);

            var count = catTypes.Count();

            Assert.That(count, Is.EqualTo(size));
        }

        [Test]
        public void QueryAllImageFromDataBase_WhenCalled_ReturnListOfAllImage()
        {
            var images = _viewModel.QueryAllImageFromDatabase();

            Assert.That(images, Is.Not.Null);
        }

        [Test]
        public async Task QueCatImageType_WhenCalled_ReturnListOfCatImageType()
        {
            var catType = "beng";

            var catImageTypes = await _viewModel.QueryCatImageType(catType, 15);

            Assert.That(catImageTypes, Is.Not.Null);
        }
    }
}