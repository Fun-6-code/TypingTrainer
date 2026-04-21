using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace TypingTrainer
{
    public partial class MainWindow : Window
    {
        private string currentText = "";
        private string userInput = "";
        private int errors = 0;
        private int speed = 0;
        private int accuracy = 100;
        private bool isExerciseFinished = false;
        private bool isExerciseStarted = false;

        private DateTime startTime;
        private System.Threading.Timer? speedTimer = null;

        private List<string> words = new List<string>
        {
            "дом", "лес", "кот", "мир", "труд", "свет", "небо", "вода", "земля", "огонь",
            "книга", "школа", "окно", "дверь", "стол", "стул", "ручка", "бумага", "экран", "мышь",
            "код", "сайт", "файл", "данные", "сервер", "браузер", "интернет", "программа", "клавиатура", "монитор",
            "алгоритм", "функция", "переменная", "массив", "объект", "класс", "метод", "система", "сеть", "логин",
            "пароль", "доступ", "загрузка", "папка", "документ", "текст", "редактор", "консоль", "терминал",
            "солнце", "луна", "звезда", "планета", "космос", "ракета", "самолёт", "машина", "дорога", "город",
            "деревня", "поле", "река", "море", "океан", "гора", "парк", "сад", "цветок",
            "дерево", "лист", "ветка", "корень", "трава", "песок", "камень", "металл", "стекло",
            "информация", "технология", "компьютер", "устройство", "приложение", "разработка", "исследование", "образование", "университет", "преподаватель",
            "студент", "проекты", "документация", "интерфейс", "пользователь", "настройка", "безопасность", "защищённый", "подключение", "обнаружение",
            "управление", "контроль", "анализ", "системный", "программный", "аппаратный", "виртуальный", "реальный", "абстрактный", "конкретный",
            "время", "пространство", "скорость", "точность", "ошибка", "исправление", "результат", "успех", "победа", "достижение",
            "цель", "задача", "проблема", "решение", "вопрос", "ответ", "знание", "умение", "навык", "опыт"
        };

        public MainWindow()
        {
            InitializeComponent();
            TextToType.Text = "Нажмите кнопку 'Начать тренировку', чтобы начать...";

            this.TextInput += MainWindow_TextInput;
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
            this.Focus();
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!isExerciseStarted || isExerciseFinished)
                return;

            if (e.Key == Key.Space)
            {
                e.Handled = false;
            }
        }

        private void UpdateStatsDisplay()
        {
            ErrorsText.Text = errors.ToString();
            SpeedText.Text = $"{speed} з/мин";
            AccuracyText.Text = $"{accuracy}%";
        }

        private void CalculateSpeed(object? state)
        {
            try
            {
                if (!isExerciseStarted || isExerciseFinished)
                    return;

                var elapsed = DateTime.Now - startTime;
                double minutes = elapsed.TotalMinutes;

                if (minutes > 0)
                {
                    speed = (int)(userInput.Length / minutes);

                    Dispatcher.Invoke(() =>
                    {
                        SpeedText.Text = $"{speed} з/мин";
                    });
                }
            }
            catch (TaskCanceledException)
            {
                // Таймер был отменён
            }
            catch (Exception)
            {
                // Другие ошибки игнорируем
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            userInput = "";
            errors = 0;
            speed = 0;
            accuracy = 100;
            isExerciseFinished = false;
            isExerciseStarted = true;

            UpdateStatsDisplay();
            GenerateText();

            startTime = DateTime.Now;

            if (speedTimer != null)
            {
                speedTimer.Dispose();
            }
            speedTimer = new System.Threading.Timer(CalculateSpeed, null, 1000, 1000);

            this.Focus();
        }

        private void GenerateText()
        {
            Random random = new Random();
            var selectedWords = words.OrderBy(x => random.Next()).Take(10);
            currentText = string.Join(" ", selectedWords);
            TextToType.Text = currentText;
        }

        private void MainWindow_TextInput(object sender, TextCompositionEventArgs e)
        {
            if (!isExerciseStarted || isExerciseFinished)
                return;

            if (string.IsNullOrEmpty(currentText))
                return;

            string text = e.Text;

            if (string.IsNullOrEmpty(text))
                return;

            char pressedChar = text[0];

            if (char.IsControl(pressedChar) && pressedChar != ' ')
                return;

            userInput += pressedChar;
            CheckInputAndUpdateDisplay();
        }

        private void CheckInputAndUpdateDisplay()
        {
            TextToType.Inlines.Clear();
            errors = 0;

            for (int i = 0; i < currentText.Length; i++)
            {
                char expectedChar = currentText[i];

                var run = new System.Windows.Documents.Run();
                run.Text = expectedChar.ToString();

                if (i < userInput.Length)
                {
                    char userChar = userInput[i];

                    if (userChar == expectedChar)
                    {
                        run.Foreground = Brushes.ForestGreen;
                        run.Background = Brushes.LightGreen;
                    }
                    else
                    {
                        run.Foreground = Brushes.DarkRed;
                        run.Background = Brushes.Pink;
                        errors++;
                    }
                }
                else
                {
                    run.Foreground = Brushes.Gray;
                }

                TextToType.Inlines.Add(run);
            }

            int totalTyped = userInput.Length;
            if (totalTyped > 0)
            {
                int correctChars = totalTyped - errors;
                accuracy = (int)((double)correctChars / totalTyped * 100);
            }
            else
            {
                accuracy = 100;
            }

            UpdateStatsDisplay();

            if (userInput.Length >= currentText.Length)
            {
                isExerciseFinished = true;
                speedTimer?.Dispose();

                MessageBox.Show(
                    $"Упражнение завершено!\n\n" +
                    $"Скорость: {speed} з/мин\n" +
                    $"Точность: {accuracy}%\n" +
                    $"Ошибки: {errors}",
                    "Результат",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                StartButton.Content = "Новое упражнение";
                SaveButton.Visibility = Visibility.Visible;
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveButton.Visibility = Visibility.Collapsed;

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                FileName = $"Результаты_{DateTime.Now:yyyy-MM-dd_HH-mm}.txt",
                Title = "Сохранить результаты тренировки"
            };


            if (saveFileDialog.ShowDialog() == true)
            {

                string content = $@"ТРЕНАЖЁР СЛЕПОЙ ПЕЧАТИ — РЕЗУЛЬТАТЫ

Дата: {DateTime.Now:dd.MM.yyyy HH:mm}

СТАТИСТИКА УПРАЖНЕНИЯ:

Скорость: {speed} з/мин
Точность: {accuracy}%
Ошибки: {errors}
Всего символов: {currentText.Length}

ТЕКСТ УПРАЖНЕНИЯ:

{currentText}";

                System.IO.File.WriteAllText(saveFileDialog.FileName, content);

                MessageBox.Show(
                    $"Результаты сохранены!\n\nФайл:\n{saveFileDialog.FileName}",
                    "Успешно",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
    }
}