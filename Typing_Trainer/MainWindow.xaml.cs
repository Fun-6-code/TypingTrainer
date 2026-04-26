using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;

namespace TypingTrainer
{
    public partial class MainWindow : Window
    {
        private string fullText = "";
        private string userInput = "";
        private int errors = 0;
        private int speed = 0;
        private int accuracy = 100;
        private bool isExerciseFinished = false;
        private bool isExerciseStarted = false;

        private DateTime startTime;
        private System.Threading.Timer? speedTimer = null;
        private System.Threading.Timer? countdownTimer = null;
        private int remainingSeconds = 60;
        private bool isTimedMode = false;

        private List<string> wordDictionary = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            LoadWords();
            this.TextInput += MainWindow_TextInput;
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
            this.Focus();
        }

        private void LoadWords()
        {
            try
            {
                string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "words.txt");
                if (File.Exists(filePath))
                {
                    string content = File.ReadAllText(filePath);
                    wordDictionary = content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                else
                {
                    wordDictionary = new List<string> { "дом", "лес", "кот", "мир", "труд", "свет", "небо", "вода", "земля", "огонь", "книга", "школа", "окно", "дверь", "стол", "стул", "ручка", "бумага", "экран", "мышь", "код", "сайт", "файл", "данные", "сервер", "браузер", "интернет", "программа", "клавиатура", "монитор", "алгоритм", "функция", "переменная", "массив", "объект", "класс", "метод", "система", "сеть", "логин", "пароль", "доступ", "загрузка", "папка", "документ", "текст", "редактор", "консоль", "терминал", "солнце", "луна", "звезда", "планета", "космос", "ракета", "самолёт", "машина", "дорога", "город", "деревня", "поле", "река", "море", "океан", "гора", "парк", "сад", "цветок", "дерево", "лист", "ветка", "корень", "трава", "песок", "камень", "металл", "стекло", "информация", "технология", "компьютер", "устройство", "приложение", "разработка", "исследование", "образование", "университет", "преподаватель", "студент", "проекты", "документация", "интерфейс", "пользователь", "настройка", "безопасность", "защищённый", "подключение", "обнаружение", "управление", "контроль", "анализ", "системный", "программный", "аппаратный", "виртуальный", "реальный", "абстрактный", "конкретный", "время", "пространство", "скорость", "точность", "ошибка", "исправление", "результат", "успех", "победа", "достижение", "цель", "задача", "проблема", "решение", "вопрос", "ответ", "знание", "умение", "навык", "опыт" };
                }
            }
            catch { }
        }

        private void BtnModeTimed_Click(object sender, RoutedEventArgs e) => StartExercise(true);
        private void BtnModeInfinite_Click(object sender, RoutedEventArgs e) => StartExercise(false);

        private void BtnBackToMenu_Click(object sender, RoutedEventArgs e)
        {
            StopTimers();
            GameGrid.Visibility = Visibility.Collapsed;
            MenuGrid.Visibility = Visibility.Visible;
            Title = "Тренажер слепой печати";
        }

        private void BtnFinishSession_Click(object sender, RoutedEventArgs e) => FinishExercise();
        private void BtnSaveResults_Click(object sender, RoutedEventArgs e) => SaveResultsToFile();

        private void StartExercise(bool timed)
        {
            isTimedMode = timed;
            userInput = "";
            errors = 0;
            speed = 0;
            accuracy = 100;
            isExerciseFinished = false;
            isExerciseStarted = true;
            remainingSeconds = 60;
            fullText = "";

            MenuGrid.Visibility = Visibility.Collapsed;
            GameGrid.Visibility = Visibility.Visible;
            BtnSaveResults.Visibility = Visibility.Collapsed;
            TimerText.Visibility = timed ? Visibility.Visible : Visibility.Collapsed;
            BtnFinishSession.Visibility = timed ? Visibility.Collapsed : Visibility.Visible;

            if (timed)
            {
                Random rnd = new Random();
                fullText = string.Join(" ", wordDictionary.OrderBy(x => rnd.Next()).Take(40));
                TimerText.Text = "01:00";
                countdownTimer = new System.Threading.Timer(CountdownTick, null, 1000, 1000);
            }
            else
            {
                EnsureInfiniteText();
            }

            DisplayText();
            UpdateStatsDisplay();
            startTime = DateTime.Now;

            speedTimer?.Dispose();
            speedTimer = new System.Threading.Timer(CalculateSpeed, null, 1000, 1000);
            this.Focus();
        }

        private void EnsureInfiniteText()
        {
            if (isTimedMode) return;
            Random rnd = new Random();
            while (fullText.Length - userInput.Length < 200)
            {
                var newWords = wordDictionary.OrderBy(x => rnd.Next()).Take(20);
                fullText += (string.IsNullOrEmpty(fullText) ? "" : " ") + string.Join(" ", newWords);
            }
        }

        private void DisplayText()
        {
            int cursorPos = userInput.Length;
            int viewStart = Math.Max(0, cursorPos - 40);
            int viewEnd = Math.Min(fullText.Length, viewStart + 80);

            if (viewEnd - viewStart < 80 && viewStart > 0)
            {
                viewStart = Math.Max(0, viewEnd - 80);
            }

            string visibleText = fullText.Substring(viewStart, viewEnd - viewStart);

            TextToType.Inlines.Clear();
            int offset = viewStart;

            foreach (char c in visibleText)
            {
                var run = new Run(c.ToString());
                if (offset < cursorPos)
                {
                    run.Foreground = Brushes.ForestGreen;
                    run.Background = Brushes.LightGreen;
                }
                else if (offset == cursorPos)
                {
                    run.Foreground = Brushes.Black;
                    run.Background = Brushes.Yellow;
                }
                else
                {
                    run.Foreground = Brushes.Gray;
                }
                TextToType.Inlines.Add(run);
                offset++;
            }

            if (TextScroll != null)
            {
                double charWidth = 16.5;
                double targetOffset = Math.Max(0, (cursorPos - viewStart - 10) * charWidth);
                TextScroll.ScrollToHorizontalOffset(targetOffset);
            }
        }

        private void CountdownTick(object? state)
        {
            remainingSeconds--;
            int m = remainingSeconds / 60;
            int s = remainingSeconds % 60;

            Dispatcher.Invoke(() =>
            {
                TimerText.Text = $"{m:D2}:{s:D2}";

                if (remainingSeconds <= 0)
                {
                    FinishExercise();
                }
            });
        }

        private void CalculateSpeed(object? state)
        {
            try
            {
                if (!isExerciseStarted || isExerciseFinished) return;
                double mins = (DateTime.Now - startTime).TotalMinutes;
                if (mins > 0)
                {
                    speed = (int)(userInput.Length / mins);
                    Dispatcher.Invoke(() => SpeedText.Text = $"{speed} з/мин");
                }
            }
            catch { }
        }

        private void MainWindow_TextInput(object sender, TextCompositionEventArgs e)
        {
            if (!isExerciseStarted || isExerciseFinished) return;
            if (string.IsNullOrEmpty(fullText)) return;

            char pressed = e.Text.Length > 0 ? e.Text[0] : '\0';
            if (char.IsControl(pressed) && pressed != ' ') return;

            int currentIdx = userInput.Length;
            if (currentIdx < fullText.Length)
            {
                if (pressed == fullText[currentIdx])
                {
                    userInput += pressed;
                    DisplayText();
                    CalculateStats();

                    EnsureInfiniteText();

                    if (userInput.Length >= fullText.Length && isTimedMode) FinishExercise();
                }
                else
                {
                    errors++;
                    CalculateStats();
                }
            }
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e) { }

        private void CalculateStats()
        {
            int total = userInput.Length;
            accuracy = total > 0 ? (int)(((double)(total - errors) / total) * 100) : 100;
            UpdateStatsDisplay();
        }

        private void UpdateStatsDisplay()
        {
            ErrorsText.Text = errors.ToString();
            SpeedText.Text = $"{speed} з/мин";
            AccuracyText.Text = $"{accuracy}%";
        }

        private void FinishExercise()
        {
            isExerciseFinished = true;
            StopTimers();

            BtnFinishSession.Visibility = Visibility.Collapsed;

            string title = isTimedMode ? "Упражнение завершено!" : "Сессия завершена!";
            string extra = isTimedMode ? "" : $"\nВсего символов: {userInput.Length}";
            MessageBox.Show($"{title}\n\nСкорость: {speed} з/мин\nТочность: {accuracy}%\nОшибки: {errors}{extra}", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
            BtnSaveResults.Visibility = Visibility.Visible;
        }

        private void SaveResultsToFile()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt",
                FileName = $"Результаты_{DateTime.Now:yyyyMMdd_HHmm}.txt",
                Title = "Сохранить результаты"
            };
            if (dlg.ShowDialog() == true)
            {
                string modeText = isTimedMode ? "На время (60 сек)" : "Бесконечный";
                string content = $@"
  ТРЕНАЖЕР СЛЕПОЙ ПЕЧАТИ - РЕЗУЛЬТАТЫ

Дата: {DateTime.Now:dd.MM.yyyy HH:mm}
Режим: {modeText}

  СТАТИСТИКА:

Скорость: {speed} з/мин
Точность: {accuracy}%
Ошибки: {errors}
Всего символов: {userInput.Length}";
                File.WriteAllText(dlg.FileName, content);
                MessageBox.Show($"Сохранено!\n{dlg.FileName}", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void StopTimers()
        {
            speedTimer?.Dispose();
            countdownTimer?.Dispose();
        }
    }
}