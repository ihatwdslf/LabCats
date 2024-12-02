using System.Collections.ObjectModel;
using System.Text.Json;

namespace MAUIApplication;

public partial class EditPage : ContentPage
{
    private CatBreed CatBreed { get; set; }

    public EditPage(CatBreed catsBreed)
    {
        InitializeComponent();

        CatBreed = catsBreed;

        BindingContext = CatBreed;
    }

    private void RewriteJson(ObservableCollection<CatBreed> data)
    {
        // Оновлюємо вміст JSON
        string updatedJson = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true, // Форматуємо JSON з відступами для зручного читання
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Уникаємо екранованих символів
        });
        File.WriteAllText(MainPage.FilePath, updatedJson); // Записуємо оновленні дані в JSON
    }

    /// <summary>
    /// Опрацьовує результат натиску на кнопку "Зберегти"
    /// </summary>
    private async void SaveButtonHandler(object sender, EventArgs e)
    {
        try
        {
            // Перевіряємо, чи MainPage є root-сторінкою
            if (Application.Current.MainPage is NavigationPage navigationPage &&
                navigationPage.RootPage is MainPage mainPage)
            {
                // Знаходимо породу в колекції
                var catsBreedInCollection = mainPage.CatBreedsCollection.FirstOrDefault(b => b.Name == CatBreed.Name);

                // Якщо усі поля порожні => користувач їх повидаляв і можливо хоче видалити дану книигу
                if (new[] { NameEntry.Text, OriginEntry.Text, LifespanEntry.Text, SizeEntry.Text, CoatEntry.Text }
                    .All(string.IsNullOrWhiteSpace))
                {
                    if (OriginEntry.Text.Any(char.IsDigit))
                    {
                        await DisplayAlert("Помилка", $"Сталася помилка при оновленні: Країна не може бути числом", "OK");
                        return;
                    }

                    // Підтвердження видалення
                    bool confirmDelete = await DisplayAlert("Підтвердження", "Усі поля порожні. Видалити цю породу?", "Так", "Ні");
                    if (confirmDelete)
                    {
                        // Видалення породи
                        mainPage.CatBreedsCollection.Remove(catsBreedInCollection); 

                        // Перезаписуємо оновлені дані в JSON
                        RewriteJson(mainPage.CatBreedsCollection); 

                        await DisplayAlert("Успіх", "Породу видалено!", "OK");
                        await Navigation.PopAsync();
                        return;
                    }
                    else
                    {
                        // Користувач відмінив видалення
                        await Navigation.PopAsync();
                    }
                }

                if (string.IsNullOrEmpty(NameEntry.Text))
                {
                    await DisplayAlert("Помилка", $"Не можна зберегти породу без унікального імені!", "OK");
                    return;
                }

                // Оновлюємо дані породи
                catsBreedInCollection.Name = NameEntry.Text?.Trim() ?? string.Empty;
                catsBreedInCollection.Origin = OriginEntry.Text?.Trim() ?? string.Empty;
                catsBreedInCollection.Lifespan = LifespanEntry.Text?.Trim() ?? string.Empty;
                catsBreedInCollection.Size = SizeEntry.Text?.Trim() ?? string.Empty;
                catsBreedInCollection.Coat = CoatEntry.Text?.Trim() ?? string.Empty;

                // Перезаписуємо наш JSON файл
                RewriteJson(mainPage.CatBreedsCollection);

                // Повідомлення про успішне збереження
                await DisplayAlert("Успіх", "Порода успішно збережена!", "OK");

                // Повертаємося на головну сторінку
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Помилка", "Головну сторінку програми не знайдено.", "OK");
            }
        }
        catch (UnauthorizedAccessException uex)
        {
            await DisplayAlert("Помилка доступу", $"Немає доступу до файлу: {uex.Message}", "OK");
        }
        catch (IOException ioex)
        {
            await DisplayAlert("Помилка вводу/виводу", $"Помилка при записі у файл: {ioex.Message}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", $"Не вдалося зберегти породу через несподівану помилку: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Повертає користувача на попередню(головну) сторінку
    /// </summary>
    private async void CancelButtonHandler(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}