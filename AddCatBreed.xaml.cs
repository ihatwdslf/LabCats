using System.Collections.ObjectModel;
using System.Formats.Tar;
using System.Text.Json;

namespace MAUIApplication;

public partial class AddCatBreed : ContentPage
{
    public AddCatBreed()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Опрацьовує результат натиску на кнопку "Зберегти"
    /// </summary>
    private async void SaveButtonHandler(object sender, EventArgs e)
    {
        // Перевірка на створення "пустої породи"
        if (new[] { NameEntry.Text, OriginEntry.Text, LifespanEntry.Text, SizeEntry.Text, CoatEntry.Text }
            .Any(string.IsNullOrWhiteSpace)
        )
        {
            await DisplayAlert("Попередження!", "Не можливо додати породу без повністю введених даних.\nНеобхідно заповнити всі поля", "ОК");
            return;
        }

        if (OriginEntry.Text.Any(char.IsDigit))
        {
            await DisplayAlert("Помилка", $"Сталася помилка при додаванні: Країна не може мати числа", "OK");
            return;
        }

        // Створення нової породи
        var newCatBreed = new CatBreed
        {
            Name = NameEntry?.Text?.Trim(),
            Origin = OriginEntry?.Text?.Trim(),
            Lifespan = LifespanEntry?.Text?.Trim(),
            Size = SizeEntry?.Text?.Trim(),
            Coat = CoatEntry?.Text?.Trim()
        };

        // Завантаження існуючих порід з файлу
        var filePath = Path.Combine(FileSystem.AppDataDirectory, MainPage.FilePath);
        List<CatBreed> catsBreedsList;

        if (File.Exists(filePath))
        {
            var json = await File.ReadAllTextAsync(filePath);
            catsBreedsList = JsonSerializer.Deserialize<List<CatBreed>>(json);
        }
        else catsBreedsList = new List<CatBreed>();

        // Додавання нової породи до списку
        catsBreedsList.Add(newCatBreed);

        // Збереження оновленого списку у файл
        var updatedJson = JsonSerializer.Serialize(catsBreedsList, new JsonSerializerOptions 
        { 
            WriteIndented = true, // Форматуємо JSON з відступами для зручного читання
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Уникаємо екранованих символів
        });
        await File.WriteAllTextAsync(filePath, updatedJson);

        // Повернення до головної сторінки
        await Navigation.PopAsync();
    }

    /// <summary>
    /// Опрацьовує результат натиску на кнопку "Відмінити"
    /// </summary>
    private async void CancelButtonHandler(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}