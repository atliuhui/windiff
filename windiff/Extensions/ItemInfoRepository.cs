using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace windiff.Extensions
{
    public class ItemInfoRepository : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        readonly FileSizeConverter s_converter;
        readonly FileHashConverter h_converter;
        readonly Channel<ItemInfo> channel;

        string searching = string.Empty;
        public string Searching
        {
            get => searching;
            set
            {
                if (searching != value)
                {
                    searching = value;
                    OnPropertyChanged(nameof(Searching));
                    Search();
                }
            }
        }
        bool running = false;
        public bool Running
        {
            get => running;
            set
            {
                if (running != value)
                {
                    running = value;
                    OnPropertyChanged(nameof(Running));
                }
            }
        }
        string messaging = string.Empty;
        public string Messaging
        {
            get => messaging;
            set
            {
                if (messaging != value)
                {
                    messaging = value;
                    OnPropertyChanged(nameof(Messaging));
                }
            }
        }

        public ObservableCollection<ItemInfo> Items { get; } = new();

        public ItemInfoRepository()
        {
            this.s_converter = new FileSizeConverter { Precision = 2 };
            this.h_converter = new FileHashConverter();
            this.channel = Channel.CreateBounded<ItemInfo>(new BoundedChannelOptions(1024)
            {
                SingleWriter = false,     // 并发生产者请设为 false
                SingleReader = true,      // 如消费者是单线程设为 true
                FullMode = BoundedChannelFullMode.Wait, // 满了时等待，形成背压
            });

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await foreach (var item in channel.Reader.ReadAllAsync())
                {
                    Items.Add(item);
                }
            });

            Messaging = $"请输入命令...";
        }

        async void Search()
        {
            if (Directory.Exists(searching) == false) return;

            var dir = new DirectoryInfo(Searching);
            if (dir.Exists == false) return;

            Items.Clear();
            Running = true;
            Messaging = $"正在扫描文件，请稍候...";
            var index = 0;

            await Task.Run(async () =>
            {
                foreach (var item in dir.GetFiles("*", SearchOption.AllDirectories))
                {
                    var info = new ItemInfo
                    {
                        Name = item.Name,
                        Directory = Path.GetRelativePath(dir.FullName, item.DirectoryName ?? dir.FullName),
                        Extension = item.Extension,
                        Hash = h_converter.ConvertOrDefault(item.FullName),
                        Size = item.Length,
                        SizeText = s_converter.ConvertOrDefault(item.Length),
                        CreationTime = item.CreationTime,
                        UpdateTime = item.LastWriteTime,
                    };

                    await channel.Writer.WriteAsync(info);
                    index++;

                    if (index % 100 == 0)
                    {
                        Messaging = $"{index} 个对象";
                    }
                }
            });

            Running = false;
            Messaging = $"{index} 个对象";
        }
    }

    public class ItemInfo
    {
        public string? Directory { get; set; }
        public string? Name { get; set; }
        public string? Extension { get; set; }
        public string? Hash { get; set; }
        public long? Size { get; set; }
        public string? SizeText { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
