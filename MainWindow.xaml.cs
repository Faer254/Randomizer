using MaterialDesignThemes.Wpf;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Randomizer
{
    public partial class MainWindow : Window
    {
        private readonly PaletteHelper paletteHelper = new PaletteHelper();
        private string windowTheme = string.Empty;
        private bool windowImage = false;

        public MainWindow()
        {
            InitializeComponent();

            var workArea = SystemParameters.WorkArea;
            MaxHeight = workArea.Height + 15;

            minTB.Focus();

            if (!Directory.Exists("resources")) 
                Directory.CreateDirectory("resources");

            const string themeFilePath = "resources/theme.dat";
            if (File.Exists(themeFilePath))
            {
                string savedTheme = File.ReadAllText(themeFilePath);
                windowTheme = savedTheme;
            }
            else
            {
                File.WriteAllText(themeFilePath, "light");
                windowTheme = "light";
            }

            const string imageFilePath = "resources/image.dat";
            if (File.Exists(imageFilePath))
            {
                string savedParameter = File.ReadAllText(imageFilePath);
                bool parameter;
                try
                {
                    parameter = Convert.ToBoolean(savedParameter);
                }
                catch
                {
                    parameter = false;
                }
                windowImage = parameter;
            }
            else
            {
                File.WriteAllText(imageFilePath, "false");
                windowImage = false;
            }

            checkTheme();
            checkImage();
        }

        private void completeButton_Click(object sender, RoutedEventArgs e)
        {
            if (minTB.Text == string.Empty || maxTB.Text == string.Empty || countTB.Text == string.Empty)
            {
                ShowAndHideError("Необходимо заполнить все поля!");
                return;
            }

            titleResultLabel.Content = "Результат";

            int min, max, count;

            try
            {
                min = Convert.ToInt32(minTB.Text);
                max = Convert.ToInt32(maxTB.Text);
                count = Convert.ToInt32(countTB.Text);

            }
            catch
            {
                ShowAndHideError("Неверно заданы параметры!");
                return;
            }

            if (count > 1000)
            {
                ShowAndHideError("Слишком большое количество чисел!");
                return;
            }

            if (min > max)
            {
                ShowAndHideError("Минимальное число не минимально!");
                return;
            }

            if (count > (max - min) + 1)
            {
                ShowAndHideError("Расчёт невозможен!");
                return;
            }

            Random rand = new Random();
            HashSet<int> numbers = new HashSet<int>();

            while (numbers.Count < count)
            {
                int num = rand.Next(min, max + 1);
                numbers.Add(num);
            }

            List<int> resultList = numbers.ToList();
            resultList.Sort();

            resultTB.Text = string.Join(", ", resultList);
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            titleResultLabel.Content = "Результат";
            resultTB.Text = string.Empty;

            minTB.Text = string.Empty;
            maxTB.Text = string.Empty;
            countTB.Text = string.Empty;
            minTB.Focus();
        }

        private void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != '-')
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void countTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private CancellationTokenSource _cancellationTokenSource;
        public async void ShowAndHideError(string errorName)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            var showError = (Storyboard)FindResource("ShowError");
            var hideError = (Storyboard)FindResource("HideError");

            errorText.Text = errorName;
            errorLabel.Visibility = Visibility.Visible;
            showError.Begin(errorLabel);

            try
            {
                await Task.Delay(2500, cancellationToken);
                hideError.Begin(errorLabel);

                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                cancellationToken = _cancellationTokenSource.Token;
                await Task.Delay(2500, cancellationToken);
                errorLabel.Visibility = Visibility.Collapsed;
            }
            catch
            {
            }
        }

        private void checkTheme()
        {
            Theme theme = paletteHelper.GetTheme();

            if (windowTheme == "light")
            {
                theme.SetBaseTheme(BaseTheme.Light);
                themeCB.IsChecked = false;
            }
            else
            {
                theme.SetBaseTheme(BaseTheme.Dark);
                themeCB.IsChecked = true;
            }

            paletteHelper.SetTheme(theme);
        }

        private void checkImage()
        {
            if (windowImage)
            {
                string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources\\background.jpg");

                if (File.Exists(imagePath))
                {
                    ImageBrush imgBrush = new ImageBrush();
                    imgBrush.ImageSource = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
                    imgBrush.Stretch = Stretch.UniformToFill;
                    mainBorder.Background = imgBrush;
                }
                else
                {
                    ShowAndHideError("Картинка не найдена!");

                    mainBorder.SetResourceReference(Border.BackgroundProperty, "MaterialDesignPaper");
                    windowImage = false;
                    imageCB.IsChecked = false;
                    File.WriteAllText("resources/image.dat", "false");

                    return;
                }

                imageCB.IsChecked = true;
            }
            else
            {
                mainBorder.SetResourceReference(Border.BackgroundProperty, "MaterialDesignPaper");
                imageCB.IsChecked = false;
            }
        }

        private void themeCB_Click(object sender, RoutedEventArgs e)
        {
            Theme theme = paletteHelper.GetTheme();

            if (theme.GetBaseTheme() == BaseTheme.Dark)
            {
                theme.SetBaseTheme(BaseTheme.Light);
                windowTheme = "light";
                File.WriteAllText("resources/theme.dat", "light");
            }
            else
            {
                theme.SetBaseTheme(BaseTheme.Dark);
                windowTheme = "dark";
                File.WriteAllText("resources/theme.dat", "dark");
            }

            paletteHelper.SetTheme(theme);
        }

        private void imageCB_Click(object sender, RoutedEventArgs e)
        {
            if (windowImage)
            {
                mainBorder.SetResourceReference(Border.BackgroundProperty, "MaterialDesignPaper");

                windowImage = false;
                File.WriteAllText("resources/image.dat", "false");
            }
            else
            {
                string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources\\background.jpg");

                if (File.Exists(imagePath))
                {
                    ImageBrush imgBrush = new ImageBrush();
                    imgBrush.ImageSource = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
                    imgBrush.Stretch = Stretch.UniformToFill;
                    mainBorder.Background = imgBrush;
                }
                else
                {
                    ShowAndHideError("Картинка не найдена!");

                    mainBorder.SetResourceReference(Border.BackgroundProperty, "MaterialDesignPaper");
                    windowImage = false;
                    imageCB.IsChecked = false;
                    File.WriteAllText("resources/image.dat", "false");

                    return;
                }

                windowImage = true;
                File.WriteAllText("resources/image.dat", "true");
            }
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            Border border = sender as Border;

            var clip = new RectangleGeometry
            {
                RadiusX = border.CornerRadius.TopLeft,
                RadiusY = border.CornerRadius.TopLeft,
                Rect = new Rect(0, 0, border.ActualWidth, border.ActualHeight)
            };
            border.Clip = clip;

            border.SizeChanged += (s, args) =>
            {
                clip.Rect = new Rect(0, 0, border.ActualWidth, border.ActualHeight);
            };
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Return when minTB.IsFocused:
                    maxTB.Focus();
                    break;

                case Key.Down when minTB.IsFocused:
                    maxTB.Focus();
                    break;

                case Key.Return when maxTB.IsFocused:
                    countTB.Focus();
                    break;

                case Key.Down when maxTB.IsFocused:
                    countTB.Focus();
                    break;

                case Key.Up when maxTB.IsFocused:
                    minTB.Focus();
                    break;

                case Key.Return when countTB.IsFocused:
                    completeButton_Click(sender, e);
                    break;

                case Key.Up when countTB.IsFocused:
                    maxTB.Focus();
                    break;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(this);
            var screenPosition = PointToScreen(mousePosition);
            double relativeX = mousePosition.X / ActualWidth;

            if (mousePosition.Y <= 26)
            {
                if (e.ClickCount == 2)
                {
                    if (WindowState == WindowState.Maximized)
                    {
                        WindowState = WindowState.Normal;
                    }
                    else
                    {
                        var workArea = SystemParameters.WorkArea;
                        MaxHeight = workArea.Height + 15;
                        WindowState = WindowState.Maximized;
                    }
                } 
                else
                {
                    if (WindowState == WindowState.Maximized)
                    {
                        WindowState = WindowState.Normal;

                        var point = PointToScreen(mousePosition);

                        Left = screenPosition.X - (relativeX * ActualWidth);
                        Top = screenPosition.Y - 13;

                        DragMove();
                    }
                    else
                    {
                        DragMove();
                    }
                }
            }
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void maximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                var workArea = SystemParameters.WorkArea;
                MaxHeight = workArea.Height + 15;
                WindowState = WindowState.Maximized;
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            var workArea = SystemParameters.WorkArea;
            MaxHeight = workArea.Height + 15;

            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}