using Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Pulxer
{
    public class TgService : IHostedService
    {
        private readonly ILogger<TgService> _logger;
        private readonly TgContextManager _tgContextManager;
        private readonly IServiceProvider _sp;
        private ITelegramBotClient _bot;
        private ChatId _chatId;
        private CancellationTokenSource _cts;
        private string _username = null;
        private List<string[]> _contexts;
        private const string TELEGRAM_CHAT_ID = "TelegramChatId";

        public TgService(ILogger<TgService> logger, IConfiguration config, ITgContextManager ctxMan, IServiceProvider sp)
        {
            _logger = logger;
            _tgContextManager = ctxMan as TgContextManager;
            _sp = sp;
            _cts = new CancellationTokenSource();

            var telegramSection = config.GetSection("Telegram");
            var token = telegramSection.GetValue<string>("token");
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Token not found in configuration file.");
                return;
            }
            _username = telegramSection.GetValue<string>("username");
            if (string.IsNullOrEmpty(_username))
            {
                _logger.LogError("Username not found in configuration file.");
                return;
            }

            _bot = new TelegramBotClient(token);
            _tgContextManager?.Init(this);
            _contexts = null;
            _chatId = null;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => {
                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = new UpdateType[] { UpdateType.CallbackQuery, UpdateType.Message },
                };

                if (_bot == null)
                {
                    _logger.LogError("Bot not created.");
                    return;
                }

                using (var scope = _sp.CreateScope())
                {
                    var settingsBL = scope.ServiceProvider.GetRequiredService<ISettingsBL>();
                    var chatIdStr = settingsBL.GetGlobal(TELEGRAM_CHAT_ID);
                    long cid;
                    if (long.TryParse(chatIdStr, out cid))
                    {
                        _chatId = new ChatId(cid);
                    }
                }

                _bot.StartReceiving(
                    HandleUpdateAsync,
                    HandleErrorAsync,
                    receiverOptions,
                    _cts.Token
                );

                _logger.LogInformation("Start receiving: " + _bot.GetMeAsync().Result.FirstName);
            });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => {
                _logger.LogInformation("Bot stop receiving.");
                _cts.Cancel();
            });
        }

        public async Task OnChangeContextList(IEnumerable<string[]> contextList)
        {
            lock(this)
            {
                _contexts = contextList.ToList();
            }
            await RefreshContexts();
        }

        private async Task RefreshContexts()
        {
            List<BotCommand> commands;

            lock (this)
            {
                if ((_chatId is null) || (_contexts == null)) return;

                commands = new List<BotCommand>();
                foreach (var ctx in _contexts)
                {
                    var cmd = new BotCommand();
                    cmd.Command = ctx[0];
                    cmd.Description = ctx[1];
                    commands.Add(cmd);
                }
                _contexts = null;
            }
            await _bot.SetMyCommandsAsync(commands, BotCommandScope.Chat(_chatId));
        }

        public async Task SendMessage(string msg, IEnumerable<string[]> buttons = null, IEnumerable<int> countInRows = null)
        {
            if (_chatId is null) return;

            InlineKeyboardMarkup markup = null;
            
            if ((buttons != null) && (countInRows == null))
            {
                List<List<InlineKeyboardButton>> grid = new List<List<InlineKeyboardButton>>();
                foreach(var btn in buttons)
                {
                    var ikb = new InlineKeyboardButton(btn[1]);
                    ikb.CallbackData = btn[0];
                    List<InlineKeyboardButton> row = new List<InlineKeyboardButton>() { ikb };
                    grid.Add(row);
                }

                markup = new InlineKeyboardMarkup(grid);
            }
            else if ((buttons != null) && (countInRows != null))
            {
                int index = 0;
                var arr = buttons.ToArray();
                List<List<InlineKeyboardButton>> grid = new List<List<InlineKeyboardButton>>();
                foreach (var count in countInRows)
                {
                    List<InlineKeyboardButton> row = new List<InlineKeyboardButton>();
                    for (int i = 0; i < count; ++i)
                    {
                        if (index >= arr.Length) continue;

                        string[] ss = arr[index++];
                        var b = new InlineKeyboardButton(ss[1]); 
                        b.CallbackData = ss[0];
                        row.Add(b);
                    }
                    grid.Add(row);
                }

                markup = new InlineKeyboardMarkup(grid);
            }

            await _bot.SendTextMessageAsync(
                chatId: _chatId,
                text: msg,
                replyMarkup: markup,
                parseMode: ParseMode.Html);
        }

        public static string Escape(string text)
        {
            return text
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("&", "&amp;");
        }

        private async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Bot handle error.");
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_username))
            {
                _logger.LogError("Username is empty.");
                return;
            }

            if (update.Type == UpdateType.Message)
            {
                if ((update.Message == null) || (update.Message.From == null) || (update.Message.From.Username == null)) return;

                if (update.Message.From.Username != _username)
                {
                    _logger.LogInformation("Unauthorized access: " + update.Message.From.Username);
                    return;
                }

                if ((_chatId is null) || (_chatId.Identifier != update.Message.Chat.Id))
                {
                    _chatId = update.Message.Chat;

                    using (var scope = _sp.CreateScope())
                    {
                        var settingsBL = scope.ServiceProvider.GetRequiredService<ISettingsBL>();
                        settingsBL.SetGlobal(TELEGRAM_CHAT_ID, update.Message.Chat.Id.ToString());
                    }
                }

                await RefreshContexts();

                var message = update.Message;
                if (string.IsNullOrWhiteSpace(message.Text)) return;

                var text = message.Text.ToLower();
                if ((text == "/start") || (text == "hello") || (text == "hi"))
                {
                    await botClient.SendTextMessageAsync(
                        chatId: _chatId,
                        text: "Hello from Pulxer bot");
                    return;
                }

                _tgContextManager.OnMessage(text);
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                if ((update.CallbackQuery == null) || (update.CallbackQuery.From == null) || (update.CallbackQuery.From.Username == null) || (update.CallbackQuery.Data == null))
                    return;

                if (update.CallbackQuery.From.Username != _username)
                {
                    _logger.LogInformation("Unauthorized access: " + update.CallbackQuery.From.Username);
                    return;
                }

                await RefreshContexts();
                _tgContextManager.OnButton(update.CallbackQuery.Data);
            }
        }
    }
}
