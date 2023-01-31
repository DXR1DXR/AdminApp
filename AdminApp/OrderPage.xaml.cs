using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;

namespace AdminApp
{
    /// <summary>
    /// Interaction logic for OrderPage.xaml
    /// </summary>
    public partial class OrderPage : Page
    {
        
        public OrderPage()
        {
            InitializeComponent();
            DP1.SelectedDateChanged += DP1_SelectedDateChanged;
            DP1.SelectedDate = DateTime.Now;
        }

        private void SendBT_Click(object sender, RoutedEventArgs e)
        {
            DockPanel dp = VisualTreeHelper.GetParent(sender as Button) as DockPanel;
            var tb = dp.FindName("ErrorTB") as TextBox;
            var code = tb.Text.Remove(0, tb.Text.LastIndexOf('\n') + 1);
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(AppInfo.GetInstance().baseAdress);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage responseMessage = client.GetAsync($"Orders/{code}/1337").Result;
            string ordersString = responseMessage.Content.ReadAsStringAsync().Result;
            var order = JsonSerializer.Deserialize<Order>(ordersString);
            order.Id_Status = 1;
            order.Date_of_Accepted = DateTime.Now;
            HttpResponseMessage responseMessage1 = client.PutAsync($"Orders/", new StringContent(JsonSerializer.Serialize(order), Encoding.UTF8, "application/json")).Result;
            Update();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Update();
            Task.Run(() => Async_Update());
        }
        private void Update()
        {
            MainGrid.Dispatcher.Invoke(() =>
            {
                TopGrid.Children.Clear();
                Lv1.Items.Clear();
                Lv2.Items.Clear();
                _orderList.Clear();
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(AppInfo.GetInstance().baseAdress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage responseMessage = client.GetAsync($"Orders/").Result;
                string ordersString = responseMessage.Content.ReadAsStringAsync().Result;
                _orderList = JsonSerializer.Deserialize<List<Order>>(ordersString);
                TopLoader();
                MiddleLoader();
                BottomLoader();
            });
        }
        List<Order> _orderList = new List<Order>();
        private void TopLoader()
        {
            if (_orderList.Count > 0)
            {
                try
                {
                    var lastOrder = _orderList.First(x => x.Id_Status == 4);//
                    string saved = XamlWriter.Save(TempTopBorder);
                    StringReader sReader = new StringReader(saved);
                    XmlReader xReader = XmlReader.Create(sReader);
                    Border border = (Border)XamlReader.Load(xReader);
                    (border.FindName("SendBT") as Button).Click += SendBT_Click;
                    border.Visibility = Visibility.Visible;
                    (border.FindName("ErrorTB") as TextBox).Text = lastOrder.Exception_Message + "\n" + lastOrder.Order_code;
                    var time = (lastOrder.Creation_Date.AddMinutes(5) - DateTime.Now);
                    var timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
                    {
                        (border.FindName("TimerTB") as TextBlock).Text = time.ToString().Remove(time.ToString().IndexOf('.'));
                        time = time.Add(TimeSpan.FromSeconds(-1));
                    }, Application.Current.Dispatcher);
                    timer.Start();
                    TopGrid.Children.Add(border);
                }
                catch
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = "Новых ошибок нет";
                    textBlock.FontSize = 20;
                    textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    TopGrid.Children.Add(textBlock);
                }
                
            }
            else
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = "Новых ошибок нет";
                textBlock.FontSize = 20;
                textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                TopGrid.Children.Add(textBlock);
            }
        }
        private void MiddleLoader()
        {
            if (_orderList.Count > 0)
            {
                foreach (var item in _orderList.Where(x => x.Id_Status == 1 && x.Date_of_Accepted != null))
                {
                    string saved = XamlWriter.Save(TempMiddleBorder);
                    StringReader sReader = new StringReader(saved);
                    XmlReader xReader = XmlReader.Create(sReader);
                    Border border = (Border)XamlReader.Load(xReader);
                    border.Visibility = Visibility.Visible;
                    (border.FindName("ErrorTB1") as TextBox).Text = item.Exception_Message + "\n" + item.Order_code;
                    (border.FindName("CompleteBT") as Button).Click += CompleteBT_Click;
                    (border.FindName("MoreBT") as Button).Click += MoreBT_Click;
                    (border.FindName("NotCompleteBT") as Button).Click += NotCompleteBT_Click;
                    var time = ((DateTime)item.Date_of_Accepted).AddMinutes(60) - DateTime.Now;
                    var timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
                    {
                        (border.FindName("TimerTB1") as TextBlock).Text = time.ToString().Remove(time.ToString().IndexOf('.'));
                        time = time.Add(TimeSpan.FromSeconds(-1));
                    }, Application.Current.Dispatcher);
                    timer.Start();
                    Lv1.Items.Add(border);
                }
            }
        }
        private void BottomLoader()
        {
            if (_orderList.Count > 0)
            {
                foreach (var item in _orderList.Where(x => (x.Id_Status == 2 || x.Id_Status == 3) && x.Date_of_Accepted != null && x.Creation_Date.ToString("d") == _selectedDate.ToString("d")))
                {
                    Border bd = TempBottomBorder;
                    string saved = XamlWriter.Save(TempBottomBorder);
                    StringReader sReader = new StringReader(saved);
                    XmlReader xReader = XmlReader.Create(sReader);
                    Border border = (Border)XamlReader.Load(xReader);
                    border.Visibility = Visibility.Visible;
                    (border.FindName("ErrorTB2") as TextBox).Text = item.Exception_Message + "\n" + item.Order_code;
                    TextBlock tbStatus = (border.FindName("StatusTB") as TextBlock);
                    if (item.Id_Status == 2)
                    {
                        tbStatus.Text = "Выполнено";
                    }
                    else
                    {
                        tbStatus.Text = "Не выполнено";
                    }
                    (border.FindName("DateTB") as TextBlock).Text = item.Creation_Date.ToString(@"dd.MM.yyyy") + "\n" + item.Creation_Date.ToString("HH:mm:ss");
                    TimeSpan time = (TimeSpan)(item.Date_of_Accepted - item.Creation_Date);
                    TextBlock tbDays = (border.FindName("DaysTB") as TextBlock);
                    TextBlock tbHours = (border.FindName("HoursTB") as TextBlock);
                    TextBlock tbMinutes = (border.FindName("MinutesTB") as TextBlock);
                    tbDays.Text = time.Days.ToString();
                    tbHours.Text = time.Hours.ToString();
                    tbMinutes.Text = time.Minutes.ToString();
                    Lv2.Items.Add(border);
                }
            }
        }
        private async void Async_Update()
        {
            while (true)
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(AppInfo.GetInstance().baseAdress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage responseMessage = client.GetAsync($"Orders").Result;
                string ordersString = responseMessage.Content.ReadAsStringAsync().Result;
                var newOrders = JsonSerializer.Deserialize<List<Order>>(ordersString);
                bool match = true;
                if (_orderList.Count == newOrders.Count)
                {
                    for (int i = 0; i < _orderList.Count; i++)
                    {
                        if (_orderList[i].Order_code != newOrders[i].Order_code)
                        {
                            match = false;
                            break;
                        }
                    }
                }
                else
                {
                    match = false;
                }
                
                if (!match)
                {
                    Update();
                    //var sri = Properties.Resources.elegant_notification_sound;
                    //if ((sri != null))
                    //{
                    //    using (var s = new MemoryStream(sri))
                    //    {
                    //        s.Position = 0;
                    //        using (System.Media.SoundPlayer player = new System.Media.SoundPlayer(s))
                    //        {
                    //            player.Load();
                    //            player.Play();
                    //        }
                    //    }
                    //}
                    SystemSounds.Exclamation.Play();
                }
                Thread.Sleep(5000);
            }
            
        }

        private void CompleteBT_Click(object sender, RoutedEventArgs e)
        {
            DockPanel dp = VisualTreeHelper.GetParent(sender as Button) as DockPanel;
            var tb = dp.FindName("ErrorTB1") as TextBox;
            var code = tb.Text.Remove(0, tb.Text.LastIndexOf('\n') + 1);
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(AppInfo.GetInstance().baseAdress);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage responseMessage = client.GetAsync($"Orders/{code}/1337").Result;
            string ordersString = responseMessage.Content.ReadAsStringAsync().Result;
            var order = JsonSerializer.Deserialize<Order>(ordersString);
            order.Id_Status = 2;
            HttpResponseMessage responseMessage1 = client.PutAsync($"Orders/", new StringContent(JsonSerializer.Serialize(order), Encoding.UTF8, "application/json")).Result;
            Update();
        }
        private void MoreBT_Click(object sender, RoutedEventArgs e)
        {
            DockPanel dp = VisualTreeHelper.GetParent(sender as Button) as DockPanel;
            var tb = dp.FindName("ErrorTB1") as TextBox;
            var code = tb.Text.Remove(0, tb.Text.LastIndexOf('\n') + 1);
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(AppInfo.GetInstance().baseAdress);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage responseMessage = client.GetAsync($"Orders/{code}/1337").Result;
            string ordersString = responseMessage.Content.ReadAsStringAsync().Result;
            var order = JsonSerializer.Deserialize<Order>(ordersString);
            DateTime date = Convert.ToDateTime(order.Date_of_Accepted);
            order.Date_of_Accepted = date.AddMinutes(60);
            HttpResponseMessage responseMessage1 = client.PutAsync($"Orders/", new StringContent(JsonSerializer.Serialize(order), Encoding.UTF8, "application/json")).Result;
            Update();
        }

        private void NotCompleteBT_Click(object sender, RoutedEventArgs e)
        {
            DockPanel dp = VisualTreeHelper.GetParent(sender as Button) as DockPanel;
            var tb = dp.FindName("ErrorTB1") as TextBox;
            var code = tb.Text.Remove(0, tb.Text.LastIndexOf('\n') + 1);
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(AppInfo.GetInstance().baseAdress);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage responseMessage = client.GetAsync($"Orders/{code}/1337").Result;
            string ordersString = responseMessage.Content.ReadAsStringAsync().Result;
            var order = JsonSerializer.Deserialize<Order>(ordersString);
            order.Id_Status = 3;
            HttpResponseMessage responseMessage1 = client.PutAsync($"Orders/", new StringContent(JsonSerializer.Serialize(order), Encoding.UTF8, "application/json")).Result;
            Update();
        }
        DateTime _selectedDate;

        private void DP1_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedDate = (DateTime)DP1.SelectedDate;
            Update();
        }
    }
}
