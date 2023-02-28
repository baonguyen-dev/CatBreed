using System;
namespace CatBreed.ServiceLocators.Services
{
	public interface IDeviceService
	{
		public bool IsDeviceOnline();

        public int GetScreenWidth();
	}
}

