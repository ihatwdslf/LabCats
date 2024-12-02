using System.Collections.ObjectModel;
using System.Text.Json;

namespace MAUIApplication
{
    public partial class MainPage : ContentPage
    {
        private static string _filePath;

        // Шлях до файлу
        public static string FilePath { get => _filePath; set => _filePath = value; }

        // Колекція порід для прив'язки
        public ObservableCollection<CatBreed> CatBreedsCollection { get; set; }
        public readonly double CatBreedListFrameHeight = App.WindowHeight * 0.6225;

        public MainPage()
        {
            InitializeComponent();

            CatBreedList.HeightRequest = CatBreedListFrameHeight;

            CatBreedsCollection = new ObservableCollection<CatBreed>(); // Ініціалізуємо колекцію
            BindingContext = this; // Прив'язуємо ViewModel
        }

        /// <summary>
        /// Кожного разу після повернення на головну стоорінку список порід оновлюється
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Оновлюємо список порід
            if (FilePath != null)
            {
                UpdateCatBreedList();
            }
        }

        /// <summary>
        /// Метод оновлює список порід на головній сторінці на основі даних з *json файлу
        /// </summary>
        private void UpdateCatBreedList()
        {
            // Преревіряємо наявність шляху до файлу
            if (string.IsNullOrEmpty(FilePath))
            {
                return;
            }

            try
            {
                // Зчитати JSON-файл як текст
                string jsonContent = File.ReadAllText(FilePath);

                // Десеріалізувати JSON у список порід
                var catsBreeds = JsonSerializer.Deserialize<List<CatBreed>>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Ігноруємо регістр імен властивостей
                });
                if (catsBreeds != null)
                {
                    CatBreedsCollection.Clear(); // Очищаємо старі дані
                    foreach (var catsBreed in catsBreeds)
                    {
                        if (new[] { catsBreed.Name, catsBreed.Origin, catsBreed.Lifespan, catsBreed.Size, catsBreed.Coat }.All(string.IsNullOrWhiteSpace) == false)
                        {
                            CatBreedsCollection.Add(catsBreed); // Додаємо нові дані
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("File deserializing error");
            }
        }

        /// <summary>
        /// Опрацьовує результат натиску на кнопку "Обрати файл"
        /// </summary>
        private async void OpenFileHandler(object sender, EventArgs e)
        {
            // Вибираємо допустимий тип файлу *.json
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".json" } }
            });

            try
            {
                var result = await FilePicker.PickAsync(new PickOptions { FileTypes = customFileType });
                if (result != null)
                {
                    FilePath = result.FullPath; // Зберігаємо шлях до файлу
                    FilePathLabel.Text = $"Обрано: {FilePath}";
                    try
                    {
                        UpdateCatBreedList(); // Зчитуємо дані та опрацьовуємо дані
                    }
                    catch (Exception ex)
                    {
                        throw ex;  // Викидаємо викюченя, якщо зчитування відбулось
                    }

                }
            }
            catch (Exception ex)
            {
                // Обробка випадку, коли обраний хибний файл
                await DisplayAlert("Помилка", $"Неправильний файл!", "OK");
            }
        }

        /// <summary>
        /// Опрацьовує результат натиску на кнопку "Інформація"
        /// </summary>
        private async void InfoButtonHandler(object sender, EventArgs e)
        {
            string studentInfo = "Роботу виконала Гребенюк Марія, студентка групи К-26" +
                                 "\n\nПрограма дозволяє працювати з *.json файлами, а саме відкривати та редагувати їх, додавати нові елементи та знійснювати пошук по вже існуючим." +
                                 "\n\nТематика даних - \"Породи котиків\"";

            await DisplayAlert("Інформація про програму", studentInfo, "ОК");
        }

        /// <summary>
        /// Опрацьовує результат натиску на кнопку "Вихід"
        /// </summary>
        private async void ExitButtonHandler(object sender, EventArgs e)
        {
            bool result = await DisplayAlert(
                       "Підтвердженння",
                       "Ви впевнені, що хочете завершити?",
                       "Так",
                       "Ні");

            if (result)
            {
                App.Current.Quit();
            }
        }

        /// <summary>
        /// Опрацьовує результат натиску на кнопку "Додати породу"
        /// </summary>
        private async void AddCatBreedHander(object sender, EventArgs e)
        {
            if (FilePath == null)
                await DisplayAlert("Помилка!", "Ви намагаєтесь додати породу котика у файл, який ще не обрали." +
                    "\nДля початку коректної роботи з програмою оберіть відповідний *.json файл", "OK");
            else
            {
                await Navigation.PushAsync(new AddCatBreed());
            }
        }

        /// <summary>
        /// Опрацьовує результат натиску на кнопку "Знайти"
        /// </summary>
        private async void SearchHandler(object sender, EventArgs e)
        {
            try
            {
                if (CatBreedsCollection == null || !CatBreedsCollection.Any())
                {
                    await DisplayAlert("Попередження", "Список порід котиків порожній.", "OK");
                    return;
                }

                // Збираємо значення з фільтрів
                string nameFilter = NameEntry?.Text?.Trim().ToLower() ?? string.Empty;
                string originFilter = OriginEntry?.Text?.Trim().ToLower() ?? string.Empty;
                string lifespanFilter = LifespanEntry?.Text?.Trim().ToLower() ?? string.Empty;
                string sizeFilter = SizeEntry?.Text?.Trim().ToLower() ?? string.Empty;
                string coatFilter = CoatEntry?.Text?.Trim().ToLower() ?? string.Empty;

                if (originFilter.Any(char.IsDigit))
                {
                    await DisplayAlert("Помилка", $"Сталася помилка при пошуку: Країна не може мати числа", "OK");
                    return;
                }

                // Фільтруємо колекцію
                var filteredCatBreeds = CatBreedsCollection.Where(catsBreed =>
                    (string.IsNullOrEmpty(nameFilter) || catsBreed.Name.ToLower().Contains(nameFilter)) &&
                    (string.IsNullOrEmpty(originFilter) || catsBreed.Origin.ToLower().Contains(originFilter)) &&
                    (string.IsNullOrEmpty(lifespanFilter) || catsBreed.Lifespan.ToLower().Contains(lifespanFilter)) &&
                    (string.IsNullOrEmpty(sizeFilter) || catsBreed.Size.ToLower().Contains(sizeFilter)) &&
                    (string.IsNullOrEmpty(coatFilter) || catsBreed.Coat.ToLower().Contains(coatFilter))
                ).ToList();


                // Оновлюємо CollectionView
                if (filteredCatBreeds.Any())
                {
                    CatBreedsCollectionView.ItemsSource = new ObservableCollection<CatBreed>(filteredCatBreeds);
                }
                else
                {
                    await DisplayAlert("Результат", "Жодна порода не відповідає критеріям пошуку.", "OK");
                    CatBreedsCollectionView.ItemsSource = new ObservableCollection<CatBreed>(); // Порожній список
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", $"Сталася помилка при пошуку: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Опрацьовує результат натиску на кнопку "Очистити"
        /// </summary>
        private void ClearFiltersHandler(object sender, EventArgs e)
        {
            NameEntry.Text = string.Empty;
            OriginEntry.Text = string.Empty;
            LifespanEntry.Text = string.Empty;
            SizeEntry.Text = string.Empty;
            CoatEntry.Text = string.Empty;
        }

        /// <summary>
        /// Опрацьовує результат натиску на кнопку "Оглянути"
        /// </summary>
        private async void ViewDescriptionHandler(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is CatBreed catsBreed)
            {
                string catBreedDescription = $"{catsBreed.Name}, пішла порода родом з країни '{catsBreed.Origin}'\n" +
                                             $"Загальні розміри: {catsBreed.Size}; Шерсть: {catsBreed.Coat}\n\n" +
                                             $"Середня тривалісь життя улюбленця: {catsBreed.Lifespan}";
                await DisplayAlert("Опис породи котика", catBreedDescription, "OK");
            }
        }

        /// <summary>
        /// Опрацьовує результат натиску на кнопку "Редагувати"
        /// </summary>
        private async void EditCatBreedHandler(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is CatBreed catsBreed)
            {
                await Navigation.PushAsync(new EditPage(catsBreed));
            }
        }
    }
}
