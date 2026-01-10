using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FlickUAC
{
    public partial class MainWindow : Window
    {
        public class UacItem : INotifyPropertyChanged
        {
            public required string ItemPath { get; set; }
            public required ImageSource Icon { get; set; }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string? name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public ObservableCollection<UacItem> DisplayItems { get; set; } = new();
        private static readonly string registryPath = @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";
        private Dictionary<string, string> languageResource = new Dictionary<string, string>();
        private static Dictionary<string, string> languageList = new Dictionary<string, string>{
            { "中文繁體", "TraditionalChinese" },
            { "中文简体", "SimplifiedChinese" },
            { "English", "English" }
        };
        private bool ReFlashItemPath()
        {
            bool hasValue = false;
            DisplayItems.Clear();
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(registryPath))
                {
                    if (key != null)
                    {
                        foreach (string valueName in key.GetValueNames())
                        {
                            if (key.GetValue(valueName)?.ToString()?.Contains("RunAsInvoker") == true)
                            {
                                hasValue = true;
                                ImageSource iconSource;
                                try
                                {
                                    using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(valueName))
                                    {
                                        iconSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                                            icon!.Handle, System.Windows.Int32Rect.Empty,
                                            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                                    }
                                }
                                catch
                                {
                                    using (var defaultIcon = System.Drawing.SystemIcons.Application)
                                    {
                                        iconSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                                            defaultIcon.Handle, System.Windows.Int32Rect.Empty,
                                            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                                    }
                                }
                                DisplayItems.Add(new UacItem { ItemPath = valueName, Icon = iconSource });
                            }
                        }
                    }
                }
                ItemList.ItemsSource = DisplayItems;
                ItemList.UnselectAll();
            }
            catch (Exception ex)
            {
                string errorTitle = languageResource.GetValueOrDefault("error", "Error");
                MessageBox.Show(ex.Message, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return hasValue;
        }
        private void ChangeLanguage(string language)
        {
            try
            {
                language = languageList[language];
            }
            catch
            {
                switch (language)
                {
                    case "zh-TW":
                        language = "TraditionalChinese";
                        break;
                    case "zh-CN":
                        language = "SimplifiedChinese";
                        break;
                    case "en-US":
                        language = "English";
                        break;
                    default:
                        language = "English";
                        break;
                }
                LanguageMenu.Text = languageList.FirstOrDefault(x => x.Value == language).Key;
            }
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.Resource.Language.{language}.json";

            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                MessageBox.Show($"找不到語言資源: {language}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(stream);

            if (data != null)
                languageResource = data;
            ReFlash.Content = languageResource["ReFlash"];
            ItemDelete.Content = languageResource["ItemDelete"];
            ItemLocation.Content = languageResource["ItemLocation"];
            ItemAdd.Content = languageResource["ItemAdd"];
        }
        public MainWindow()
        {
            InitializeComponent();
            LanguageMenu.Items.Clear();
            foreach (var lang in languageList.Keys)
                LanguageMenu.Items.Add(lang);
            ChangeLanguage(CultureInfo.CurrentUICulture.Name);
            ReFlashItemPath();
        }

        private void LanguageMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageMenu.SelectedItem is string selectedLang)
                ChangeLanguage(selectedLang);
        }

        private async void ReFlash_Click(object sender, RoutedEventArgs e)
        {
            ReFlash.Content = languageResource["ReFlash"] + (ReFlashItemPath() ? "✔️" : "❌");
            await Task.Delay(1000);
            ChangeLanguage(LanguageMenu.Text);
        }

        private void ItemAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*",
                    Title = "Select Application"
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    MessageBoxResult result = MessageBox.Show(
                        $"{languageResource["theSelectedFilePathIs"]}\n{selectedFilePath}",
                        languageResource["message"],
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(registryPath))
                            {
                                key.SetValue(selectedFilePath, "RunAsInvoker");
                                MessageBox.Show(
                                    languageResource["registryValueAdded"],
                                    languageResource["message"],
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, languageResource["error"], MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show(languageResource["noActionTaken"], languageResource["message"], MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                ReFlashItemPath();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, languageResource["message"], MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ItemLocation_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = ItemList.SelectedItems.Cast<UacItem>().ToList();
            foreach (var item in selectedItems)
            {

                if (File.Exists(item.ItemPath))
                {
                    Process.Start("explorer.exe", $"/select,\"{item.ItemPath}\"");
                }
                else
                {
                    string? directory = item.ItemPath;
                    do
                    {
                        if (Directory.Exists(directory))
                        {
                            Process.Start("explorer.exe", $"\"{directory}\"");
                            break;
                        }
                        directory = System.IO.Path.GetDirectoryName(directory);
                    } while (!File.Exists(directory));
                }
            }
        }

        private void ItemDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = ItemList.SelectedItems.Cast<UacItem>().ToList();

            if (selectedItems.Count == 0) return;
            foreach (var item in selectedItems)
            {
                string message = $"{languageResource["whetherToDeleteTheValue"]}\n{item.ItemPath}";
                if (MessageBox.Show(message, languageResource["message"], MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(registryPath, true))
                        {
                            if (key == null) return;
                            key.DeleteValue(item.ItemPath, false);
                            DisplayItems.Remove(item);
                        }
                        MessageBox.Show(languageResource["registryValueDeleted"], languageResource["message"], MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, languageResource["error"], MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ItemList_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            int selectedCount = ItemList.SelectedItems.Count;
            ItemDelete.IsEnabled = selectedCount > 0;
            ItemLocation.IsEnabled = selectedCount > 0;
        }
    }
}