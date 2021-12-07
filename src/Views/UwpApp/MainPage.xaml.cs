using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace UwpApp
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;

            var tb = ApplicationView.GetForCurrentView().TitleBar;

            //Устанавливает фона заголовка, но не затрагивает часть с кнопками управления окном.
            tb.BackgroundColor = Windows.UI.Colors.LightBlue;
            //Устанавливает фон под кнопками управления окном
            tb.ButtonBackgroundColor = Windows.UI.Colors.LightBlue;
            //Устанавливает цвет символов внутри кнопок управления окном (черточка, крестик, квадратик).
            tb.ButtonForegroundColor = Windows.UI.Colors.Black;
            //Устанавливает цвет фона кнопок управления окном в момент наведения на них мыши. ВНИМАНИЕ! Не влияет на кнопку закрытия окна.
            tb.ButtonHoverBackgroundColor = Windows.UI.Colors.Blue;
            //Устанавливает цвет символов внутри кнопок управления окном в момент наведения на них мыши. ВНИМАНИЕ! Не влияет на кнопку закрытия окна.
            tb.ButtonHoverForegroundColor = Windows.UI.Colors.White;
            //Устанавливает цвет фона кнопок управления окном в момент, когда приложение не активно
            tb.ButtonInactiveBackgroundColor = Windows.UI.Colors.Gray;
            //Устанавливает цвет символов внутри кнопок управления окном в момент, когда приложение не активно
            tb.ButtonInactiveForegroundColor = Windows.UI.Colors.White;
            //Устанавливает цвет фона кнопок управления окном в момент нажатия на них. ВНИМАНИЕ! Не влияет на кнопку закрытия окна.
            tb.ButtonPressedBackgroundColor = Windows.UI.Colors.DarkBlue;
            //Устанавливает цвет символов внутри кнопок управления окном в момент нажатия на них. ВНИМАНИЕ! Не влияет на кнопку закрытия окна.
            tb.ButtonPressedForegroundColor = Windows.UI.Colors.White;
            //Устанавливает цвет символов заголовка окна
            tb.ForegroundColor = Windows.UI.Colors.Black;
            //Устанавливает фона заголовка в момент, когда приложение не активно
            tb.InactiveBackgroundColor = Windows.UI.Colors.Gray;
            //Устанавливает цвет символов заголовка окна в момент, когда приложение не активно
            tb.InactiveForegroundColor = Windows.UI.Colors.White;

            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(1024, 600));
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(CustomTitleBar);
            Window.Current.SizeChanged += Current_SizeChanged;
            FullScreenButton.Click += FullScreenButton_Click;
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            var view = ApplicationView.GetForCurrentView();

            if (!view.TryEnterFullScreenMode())
            {
                CustomTitleBar.Visibility = Visibility.Visible;
                FullScreenButton.Visibility = Visibility.Collapsed;
            }
        }

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            var view = ApplicationView.GetForCurrentView();

            if (!view.IsFullScreenMode)
            {
                view.ExitFullScreenMode();
                CustomTitleBar.Visibility = Visibility.Visible;
                FullScreenButton.Visibility = Visibility.Visible;
            }
        }
    }
}
