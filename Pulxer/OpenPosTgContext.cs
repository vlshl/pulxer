using Common;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pulxer
{
    public class OpenPosTgContext : ITgContext
    {
        private const string PRICE_FORMAT = "0.######";
        private const string SUMMA_FORMAT = "0.##";
        private const string PERC_FORMAT = "0.##";
        private const string DATETIME_FORMAT = "dd/MM/yyyy HH:mm:ss";

        private readonly OpenPositions _openPos;
        private readonly ITgContextManager _ctxMgr;
        private string _curState;
        private decimal _alertLevel;
        private Dictionary<int, int> _posId_prevLevel; // предыдущие уровни
        private Dictionary<int, int> _posId_eventLevel; // уровни последнего оповещения
        private List<OpenPosItem> _upItems;
        private List<OpenPosItem> _downItems;

        public OpenPosTgContext(OpenPositions openPos, ITgContextManager ctxMgr) 
        {
            _openPos = openPos;
            _ctxMgr = ctxMgr;
            _curState = "";
            _alertLevel = 0.5m;
            _posId_prevLevel = new Dictionary<int, int>();
            _posId_eventLevel = new Dictionary<int, int>();
            _upItems = new List<OpenPosItem>();
            _downItems = new List<OpenPosItem>();
        }

        public string GetTgName()
        {
            return "Позиции";
        }

        public string GetTgCommand()
        {
            return "/openpos";
        }

        public void OnSetTgContext()
        {
            ShowPositions();
        }

        public void OnCommand(string cmd)
        {
            if (cmd == "/openpos")
            {
                ShowPositions();
            }
            else if (cmd == "/openpos-settings")
            {
                ShowSettings();
            }
            else
            {
                var items = _openPos.GetPositions();
                var pos = items.FirstOrDefault(r => r.PosId.ToString() == cmd);
                if (pos == null) return;

                string msg = string.Format("<b>{0}</b>: {1}\n<b>Дата:</b> {2}\n<b>Лотов:</b> {3} ({4})\n<b>Цена отк:</b> {5} ({6})\n<b>Цена тек:</b> {7} ({8})\n<b>Прибыль:</b> {9} ({10}%)",
                    TgService.Escape(pos.Ticker),
                    TgService.Escape(pos.ShortName),
                    pos.OpenTime.ToString(DATETIME_FORMAT), 
                    pos.Lots.ToString(),
                    pos.Count.ToString(),
                    pos.OpenPrice.ToString(PRICE_FORMAT),
                    pos.OpenSumma.ToString(SUMMA_FORMAT),
                    pos.CurPrice.ToString(PRICE_FORMAT),
                    pos.CurSumma.ToString(SUMMA_FORMAT),
                    pos.Profit.ToString(SUMMA_FORMAT), 
                    pos.ProfitPerc.ToString(PERC_FORMAT));
                _ctxMgr.SendMessage(this, msg, new List<string[]>() { new string[] { "/openpos", "Возврат в список" } }, new int[] { 1 }).Wait();
                _curState = "pos:" + pos.PosId.ToString();
            }
        }

        public void OnMessage(string msg)
        {
            if (_curState == "settings")
            {
                decimal level;
                if (!decimal.TryParse(msg, out level))
                {
                    _ctxMgr.SendMessage(this, "Неверный ввод").Wait();
                    return;
                }

                if (level <= 0)
                {
                    _ctxMgr.SendMessage(this, "Текущее значение: " + _alertLevel.ToString()).Wait();
                    ShowPositions();
                    return;
                }

                _alertLevel = level;
                _ctxMgr.SendMessage(this, "Новое значение: " + _alertLevel.ToString()).Wait();
                ShowPositions();
            }
        }

        private void ShowPositions()
        {
            _curState = "list";
            var items = _openPos.GetPositions();

            if (items.Any())
            {
                var buttons = items.Select(r => new string[] { r.PosId.ToString(),
                    string.Format("{0} | {1} | {2} ({3}%)",
                        r.Ticker,
                        r.Lots.ToString(),
                        r.Profit.ToString(SUMMA_FORMAT),
                        r.ProfitPerc.ToString(PERC_FORMAT))
                }).ToList();

                buttons.Add(new string[] { "/openpos-settings", "Настройки" });

                _ctxMgr.SendMessage(this, "Позиции", buttons).Wait();
            }
            else
            {
                _ctxMgr.SendMessage(this, "Открытых позиций нет").Wait();
            }
        }

        private void ShowSettings()
        {
            _curState = "settings";
            string text = string.Format("Настройка уровня оповещения\nТекущее значение: {0}\nВведите новое значение (0-отмена):", _alertLevel);
            _ctxMgr.SendMessage(this, text).Wait();
        }

        public async Task Pulse(OpenPosItem[] curItems)
        {
            _upItems.Clear(); _downItems.Clear();

            foreach (var curItem in curItems)
            {
                if (curItem.CurPrice == 0) continue;

                if (_posId_prevLevel.ContainsKey(curItem.PosId))
                {
                    int prevLevel = _posId_prevLevel[curItem.PosId];
                    int newLevel = (int)Math.Floor(curItem.ProfitPerc / _alertLevel);
                    if (newLevel != prevLevel)
                    {
                        _posId_prevLevel[curItem.PosId] = newLevel;
                        int eventLevel;
                        if (newLevel > prevLevel)
                        {
                            eventLevel = newLevel; // уровень оповещения для возрастающей позиции

                            if (_posId_eventLevel.ContainsKey(curItem.PosId)) // оповещение на эту позицию раньше уже было
                            {
                                if (_posId_eventLevel[curItem.PosId] != eventLevel) // уровень старого оповещения не совпадает с новым, значит опять оповещаем
                                {
                                    _upItems.Add(curItem);
                                    _posId_eventLevel[curItem.PosId] = eventLevel;
                                }
                            }
                            else // раньше оповещения на эту позицию никакого не было, значит оповещаем
                            {
                                _posId_eventLevel.Add(curItem.PosId, eventLevel);
                                _upItems.Add(curItem);
                            }
                        }
                        else
                        {
                            eventLevel = newLevel + 1; // уровень оповещения для убывающей позиции

                            if (_posId_eventLevel.ContainsKey(curItem.PosId))
                            {
                                if (_posId_eventLevel[curItem.PosId] != eventLevel)
                                {
                                    _downItems.Add(curItem);
                                    _posId_eventLevel[curItem.PosId] = eventLevel;
                                }
                            }
                            else
                            {
                                _posId_eventLevel.Add(curItem.PosId, eventLevel);
                                _downItems.Add(curItem);
                            }
                        }
                    }
                }
                else
                {
                    _posId_prevLevel.Add(curItem.PosId, 0);
                }
            }

            // удаляем позиции, которых больше нет
            var posIds = _posId_prevLevel.Keys.ToArray();
            foreach (var posId in posIds)
            {
                var found = curItems.FirstOrDefault(r => r.PosId == posId);
                if (found == null)
                {
                    _posId_prevLevel.Remove(posId);
                    if (_posId_eventLevel.ContainsKey(posId)) // если были уровни оповещения, то их тоже удаляем, т.к. сама позиция удаляется
                    {
                        _posId_eventLevel.Remove(posId);
                    }
                }
            }

            // отправляем оповещения в Телеграм
            List<string> list = new List<string>();
            list.AddRange(_upItems  .Select(r => string.Format("﻿\u25b2 {0} | {1} | {2} ({3}%)", r.Ticker, r.Lots.ToString(), r.Profit.ToString(SUMMA_FORMAT), r.ProfitPerc.ToString(PERC_FORMAT))));
            list.AddRange(_downItems.Select(r => string.Format("\u25bc {0} | {1} | {2} ({3}%)", r.Ticker, r.Lots.ToString(), r.Profit.ToString(SUMMA_FORMAT), r.ProfitPerc.ToString(PERC_FORMAT))));
            if (list.Any()) 
            {
                var msg = string.Join('\n', list);
                await _ctxMgr.SendMessage(this, msg);
            }
        }
    }
}
