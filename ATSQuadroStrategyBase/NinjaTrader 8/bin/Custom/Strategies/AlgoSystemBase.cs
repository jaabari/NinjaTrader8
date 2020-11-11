//MIT License/OPEN SOURCE LICENSE
//Copyright(C) 2020, Algo Trading Systems LLC <www.algotradingsystems.net>
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notices, this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//ABOUT ATSQuadroStrategyBase
//Algo Trading Systems (ATS) reserves the right to modify or overwrite this NinjaScript component with each release without notice.
//Legal Forum: In using this product you agree to the terms and NYC Jurisdiction
//Developer: Tom Leeson of MicroTrends LTd www.microtrends.pro
//About: ATSQuadroStrategyBase is a NinjaTrader 8 Strategy unmanaged mode trade engine base foundation for futures, comprising of 4 Bracket capacity, all In scale out non position compounding,  prevents overfills and builds on functionality provided by the Managed approach for NinjaTrader Strategies. 
//Updates: Visit www.microtrends.pro for updates and GIT open source project code latest: https://github.com/MicroTrendsLtd/NinjaTrader8/
//Version: 2020.11.11.1
//History: See gitHub history


#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using NinjaTrader.Core;
#endregion

namespace NinjaTrader.NinjaScript.Strategies
{
    #region enums

    [TypeConverter(typeof(CoreEnumConverter))]
    public enum AlgoSystemState
    {
        None = 0,
        Starting = 1,
        DataLoaded = 2,
        Historical = 3,
        Transition = 4,
        HisTradeRT = 5,
        Realtime = 6,
        Terminated = 7
    }

    [TypeConverter(typeof(CoreEnumConverter))]
    public enum AlgoSystemMode
    {
        UnKnown = 0,
        Sim = 1,
        Test = 2,
        Replay = 3,
        Live = 4
    }

    public enum StrategyTradeWorkFlowState
    {
        Waiting = 0,
        GoLong,
        GoLongCancelWorkingOrders,
        GoLongCancelWorkingOrdersPending,
        GoLongCancelWorkingOrdersConfirmed,
        GoLongClosePositions,
        GoLongClosedPositionsPending,
        GoLongClosedPositionsConfirmed,
        GoLongSubmitOrder,
        GoLongValidationRejected,
        GoLongSubmitOrderPending,
        GoLongSubmitOrderWorking,
        GoLongSubmitOrderFilled,
        GoLongFilledQuantityTest1,
        GoLongFilledQuantityTest1Ok,
        GoLongFilledQuantityTest1Fail,
        GoLongFilledQuantityTest2,
        GoLongFilledQuantityTest2Ok,
        GoLongFilledQuantityTest2Fail,
        GoLongFilledQuantityVerified,
        GoLongPlaceStops,
        GoLongPlaceStopsPending,
        GoLongPlaceStopsConfirmed,
        GoLongPlaceProfitTargets,
        GoLongPlaceProfitTargetsPending,
        GoLongPlaceProfitTargetsConfirmed,
        GoShort,
        GoShortCancelWorkingOrders,
        GoShortCancelWorkingOrdersPending,
        GoShortCancelWorkingOrdersConfirmed,
        GoShortClosePositions,
        GoShortClosedPositionsPending,
        GoShortClosedPositionsConfirmed,
        GoShortSubmitOrder,
        GoShortValidationRejected,
        GoShortSubmitOrderPending,
        GoShortSubmitOrderWorking,
        GoShortSubmitOrderFilled,
        GoShortFilledQuantityTest1,
        GoShortFilledQuantityTest1Ok,
        GoShortFilledQuantityTest1Fail,
        GoShortFilledQuantityTest2,
        GoShortFilledQuantityTest2Ok,
        GoShortFilledQuantityTest2Fail,
        GoShortFilledQuantityVerified,
        GoShortPlaceStops,
        GoShortPlaceStopsPending,
        GoShortPlaceStopsConfirmed,
        GoShortPlaceProfitTargets,
        GoShortPlaceProfitTargetsPending,
        GoShortPlaceProfitTargetsConfirmed,
        GoOCOLongShort,
        GoOCOLongShortCancelWorkingOrders,
        GoOCOLongShortCancelWorkingOrdersPending,
        GoOCOLongShortCancelWorkingOrdersConfirmed,
        GoOCOLongShortClosePositions,
        GoOCOLongShortClosedPositionsPending,
        GoOCOLongShortClosedPositionsConfirmed,
        GoOCOLongShortSubmitOrder,
        GoOCOLongShortValidationRejected,
        GoOCOLongShortSubmitOrderPending,
        GoOCOLongShortSubmitOrderWorking,
        ExitTradeLong,
        ExitTradeShort,
        ExitTrade,
        ExitTradeCancelWorkingOrders,
        ExitTradeCancelWorkingOrderPending,
        ExitTradeCancelWorkingOrderConfirmed,
        ExitTradeClosePositions,
        ExitTradeClosePositionsPending,
        ExitTradeClosePositionsConfirmed,
        ExitOnCloseOrderPending,
        ExitOnCloseWaitingConfirmation,
        ExitOnCloseOrderFilled,
        ExitOnCloseConfirmed,
        ErrorTimeOut,
        Error = 100,
        ErrorFlattenAll,
        ErrorFlattenAllPending,
        ErrorFlattenAllConfirmed,
        CycleComplete = 10000,

    }
    #endregion
    #region Event Args
    public sealed class StrategyTradeWorkFlowUpdatedEventArgs : EventArgs
    {
        private StrategyTradeWorkFlowState strategyTradeWorkFlowState = StrategyTradeWorkFlowState.Waiting;

        public StrategyTradeWorkFlowUpdatedEventArgs(StrategyTradeWorkFlowState strategyTradeWorkFlowState)
        {
            this.strategyTradeWorkFlowState = strategyTradeWorkFlowState;
        }
        public StrategyTradeWorkFlowState StrategyTradeWorkFlowState
        {
            get { return strategyTradeWorkFlowState; }
            set { strategyTradeWorkFlowState = value; }
        }
    }
    #endregion
    #region SignalActions

    public enum AlgoSignalAction
    {
        None = 0,
        ExitTrade = 1,
        GoLong = 2,
        GoShort = 3,
        ExitTradeLong = 4,
        ExitTradeShort = 5
    }

    public class AlgoSignalActionMsq
    {
        public AlgoSignalAction Action = AlgoSignalAction.None;
        public DateTime ActionDateTime = DateTime.Now;
        public string Reason = string.Empty;

        public AlgoSignalActionMsq(AlgoSignalAction action, DateTime actionDateTime, string reason)
        {
            this.Action = action;
            this.ActionDateTime = actionDateTime;
            this.Reason = reason;
        }

        public override string ToString()
        {
            return ActionDateTime.ToString() + " | " + Action.ToString() + " | " + Reason;
        }

    }

    #endregion
    #region DebugTrace
    public class DebugTraceHelper
    {

        private DefaultTraceListener tracing = new DefaultTraceListener();
        public static DebugTraceHelper Default = new DebugTraceHelper();

        public DefaultTraceListener Tracing
        {
            get
            {
                return tracing;

            }

            set
            {
                tracing = value;
            }
        }


        public static void WriteLine(string message)
        {
            Default.Tracing.LogFileName = string.Format("{0}\\trace\\ATS.NT8.{1}{2}{3}.Trace.txt", NinjaTrader.Core.Globals.UserDataDir, DateTime.Now.Year.ToString("d2"), DateTime.Now.Month.ToString("d2"), DateTime.Now.Day.ToString("d2"));
            Default.Tracing.WriteLine(message);
        }

        public static void OpenTraceFile()
        {
            if (File.Exists(Default.Tracing.LogFileName)) System.Diagnostics.Process.Start(Default.Tracing.LogFileName);

        }


        public DebugTraceHelper()
        {
            Tracing.Name = "ASB.NT8.Trace";
            Tracing.LogFileName = string.Format("{0}\\trace\\ASB.NT8.{1}{2}{3}.Trace", NinjaTrader.Core.Globals.UserDataDir, DateTime.Now.Year.ToString("d2"), DateTime.Now.Month.ToString("d2"), DateTime.Now.Day.ToString("d2"));
        }


    }
    #endregion
    #region partial StrategyBase Class
    public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
    {
        //can be handy to have this reference when creating indicator wrappers in derived class instances
        internal Indicators.Indicator StrategyIndicator { get { return indicator; } }
    }
    #endregion

    /// <summary>
    /// ATSQuadroStrategyBase is a NinjaTrader 8 Strategy unmanaged mode trade engine base foundation for futures design and developed by Tom Leeson, MicroTrends LTd, www.microtrends.pro
    /// </summary>
    public abstract class ATSQuadroStrategyBase : Strategy, INotifyPropertyChanged
    {
        #region variables,constants,EventHandlers

        //OCOBreakout simulated entry orders when triggering set entryOrder to catch the execution etc
        private double lastPrice;
        private DateTime onMarketDataTimeNextAllowed;
        private readonly object onOrderUpdateLockObject = new object();


        private bool inOnMarketData;
        private readonly object inOnMarketDataLock = new object();


        [XmlIgnore]
        public AlgoSignalAction AlgoSignalAction = AlgoSignalAction.None;
        [XmlIgnore]
        public ConnectionStatus connectionStatusOrder = ConnectionStatus.Connected;
        [XmlIgnore]
        public ConnectionStatus connectionStatusPrice = ConnectionStatus.Connected;

        private StrategyTradeWorkFlowState tradeWorkFlow = StrategyTradeWorkFlowState.Waiting;
        private StrategyTradeWorkFlowState tradeWorkFlowPrior = StrategyTradeWorkFlowState.Waiting;

        internal bool IsTradeWorkFlowOnMarketData = false;
        internal bool IsTEQOnMarketData = false;

        protected bool tEQTimerInProgress = false;
        protected bool tEQTimerStarted = false;

        protected long tradeWorkFlowTimerTickCounter = 0;
        protected int tradeWorkFlowTimerInterval = 1000;
        protected int tradeWorkFlowTimerIntervalReset = 100;
        protected int tradeWorkFlowAlarmTimeOutSeconds = 10;
        protected int tradeWorkFlowRetryAlarm = 3;
        protected int tradeWorkFlowRetryCount = 0;

        protected readonly object lockTradeWorkFlowTimerObject = new Object();

        protected int tEQTimerInterval = 1000;
        protected readonly object lockTEQTimerObject = new Object();

        private bool realtimeTradingOnly = false;
        private bool useQueue = false;

        //lock objects
        private readonly object tradeWorkFlowNewOrderLockObject = new Object();
        private readonly object tradeWorkFlowExitTradeLockObject = new Object();
        private readonly object lockObjectClose = new object();
        private bool isLockPositionClose = false;
        private readonly object lockObjectPositionClose = new object();

        #region orders
        protected const string arrowUp = "↑";
        protected const string arrowDown = "↓";

        protected const string entry1NameLong = "↑EL";
        protected const string entry1NameShort = "↓ES";
        protected const string closeNameLong = "XL";
        protected const string closeNameShort = "XS";
        protected const string closeOrderName = "Close";

        protected const string target1Name = "PT1";
        protected const string target2Name = "PT2";
        protected const string target3Name = "PT3";
        protected const string target4Name = "PT4";
        protected const string stop1Name = "SL1";
        protected const string stop2Name = "SL2";
        protected const string stop3Name = "SL3";
        protected const string stop4Name = "SL4";

        protected int entryCount = 1;

        protected Order orderEntry = null;
        protected Order orderEntryPrior = null;
        protected Order orderTarget1 = null;
        protected Order orderStop1 = null;
        protected Order orderTarget2 = null;
        protected Order orderStop2 = null;
        protected Order orderTarget3 = null;
        protected Order orderStop3 = null;
        protected Order orderTarget4 = null;
        protected Order orderStop4 = null;
        protected Order orderExit = null;
        protected Order orderClose = null;
        protected Order orderExitOnClose = null;
        protected Order orderEntryOCOLong = null;
        protected Order orderEntryOCOShort = null;

        private List<Order> ordersRT = new List<Order>(1000);
        private List<Order> ordersStopLoss = new List<Order>(4);
        private List<Order> ordersProfitTarget = new List<Order>(4);

        protected string oCOId = Guid.NewGuid().ToString();

        protected string orderEntryName = string.Empty;


        #endregion

        Queue<AlgoSignalActionMsq> q = new Queue<AlgoSignalActionMsq>(1000);
        private bool lockedQueue = false;
        private readonly object queueLockObject = new Object();

        protected bool onErrorShowAlertOnBarUpdate = false;
        protected bool onErrorDisableOnBarUpdate = false;
        protected bool onErrorShowAlertOnTerminate = false;
        protected bool onErrorDumpSettingsToFileOnBarUpdate = false;
        protected bool onErrorDumpSettingsToFileOnTerminate = false;

        protected string errorMsg = string.Empty;
        protected bool errorsOccured = false;
        protected string errorFileName = string.Empty;
        protected string path = string.Empty;

        //StreamWriter writer = null;
        protected string lUID = string.Empty;
        public event EventHandler<StrategyTradeWorkFlowUpdatedEventArgs> StrategyTradeWorkFlowUpdated;

        private bool tracing = false;
        private bool showOrderLabels = false;

        //Error Handling 
        private bool orderCancelStopsOnly = true;
        private bool orderCancelInspectEachOrDoBatchCancel = true;
        private bool raiseErrorOnAllOrderRejects = false;


        private readonly object lockObjectMarketData = new object();

        private Currency accountDenomination = Currency.UsDollar;


        #endregion
        #region events and overrides

        public ATSQuadroStrategyBase()
        {
            TradeSignalExpiryInterval = 3;

        }


        protected override void OnStateChange()
        {
            try
            {

                switch (State)
                {
                    case State.SetDefaults:
                        Description = @"ATS.NT8.QuadroStrategyBase";
                        Name = "ATS.NT8.QuadroStrategyBase";
                        Calculate = Calculate.OnBarClose;
                        EntriesPerDirection = 1;
                        EntryHandling = EntryHandling.AllEntries;
                        IsExitOnSessionCloseStrategy = false;
                        ExitOnSessionCloseSeconds = 30;
                        IsFillLimitOnTouch = false;
                        OrderFillResolution = OrderFillResolution.Standard;
                        Slippage = 0;
                        StartBehavior = StartBehavior.WaitUntilFlat;
                        TimeInForce = TimeInForce.Gtc;
                        TraceOrders = true;
                        RealtimeErrorHandling = RealtimeErrorHandling.IgnoreAllErrors;
                        StopTargetHandling = StopTargetHandling.PerEntryExecution;
                        BarsRequiredToTrade = 0;
                        BarsRequiredToPlot = 1;
                        // Disable this property for performance gains in Strategy Analyzer optimizations
                        // See the Help Guide for additional information
                        IsInstantiatedOnEachOptimizationIteration = true;
                        MaximumBarsLookBack = MaximumBarsLookBack.Infinite;
                        IgnoreOverfill = true;
                        IsUnmanaged = true;
                        this.DefaultQuantity = 4;


                        break;
                    case State.Configure:
                        ATSAlgoSystemState = AlgoSystemState.Starting;
                        InstrumentFullName = this.Instrument.FullName;

                        break;
                    case State.Active:

                        break;
                    case State.DataLoaded:

                        if (Account.Name == Account.BackTestAccountName)
                            ATSAlgoSystemMode = AlgoSystemMode.Test;
                        else if (Account.Name == Account.SimulationAccountName)
                            ATSAlgoSystemMode = AlgoSystemMode.Sim;
                        else if (Account.Name == Account.PlaybackAccountName)
                            ATSAlgoSystemMode = AlgoSystemMode.Replay;

                        ATSAlgoSystemState = AlgoSystemState.DataLoaded;

                        break;
                    case State.Historical:
                        ATSAlgoSystemState = AlgoSystemState.Historical;
                        PositionInfo = string.Format("{0}", Position.MarketPosition.ToString());

                        break;
                    case State.Transition:

                        if (ATSAlgoSystemMode == AlgoSystemMode.UnKnown)
                        {
                            if (Account.Connection.Options.Mode == Mode.Live && !string.IsNullOrEmpty(Account.Fcm))
                            {
                                ATSAlgoSystemMode = AlgoSystemMode.Live;
                            }
                            else
                                ATSAlgoSystemMode = AlgoSystemMode.Sim;
                        }

                        ATSAlgoSystemState = AlgoSystemState.Transition;

                        //validate test to the state to realtime with historical orders
                        //NT8 can be realtime state yet historical orders exist...
                        if (Position.MarketPosition != MarketPosition.Flat)
                            ATSAlgoSystemState = AlgoSystemState.HisTradeRT;


                        break;
                    case State.Realtime:

                        if (ATSAlgoSystemState == AlgoSystemState.Transition)
                            ATSAlgoSystemState = AlgoSystemState.Realtime;

                        // one time only, as we transition from historical to real-time - doesnt seem to work  for unmanaged mode
                        // the work around was to use order names and reference them OnOrderUpdate
                        //https://ninjatrader.com/support/helpGuides/nt8/?getrealtimeorder.htm

                        if (orderEntry != null)
                        {
                            Order order = GetRealtimeOrder(orderEntry);
                            if (order != null) orderEntry = order;
                        }
                        if (orderClose != null)
                        {
                            Order order = GetRealtimeOrder(orderClose);
                            if (order != null) orderClose = order;
                        }
                        if (orderExit != null)
                        {
                            Order order = GetRealtimeOrder(orderExit);
                            if (order != null) orderExit = order;
                        }
                        if (orderExitOnClose != null)
                        {
                            Order order = GetRealtimeOrder(orderExitOnClose);
                            if (order != null) orderExitOnClose = order;
                        }
                        if (orderStop1 != null)
                        {
                            Order order = GetRealtimeOrder(orderStop1);
                            if (order != null) orderStop1 = order;
                        }
                        if (orderStop2 != null)
                        {
                            Order order = GetRealtimeOrder(orderStop2);
                            if (order != null) orderStop2 = order;
                        }
                        if (orderStop3 != null)
                        {
                            Order order = GetRealtimeOrder(orderStop3);
                            if (order != null) orderStop3 = order;
                        }
                        if (orderStop4 != null)
                        {
                            Order order = GetRealtimeOrder(orderStop4);
                            if (order != null) orderStop4 = order;
                        }
                        if (orderTarget1 != null)
                        {
                            Order order = GetRealtimeOrder(orderTarget1);
                            if (order != null) orderTarget1 = order;
                        }
                        if (orderTarget2 != null)
                        {
                            Order order = GetRealtimeOrder(orderTarget2);
                            if (order != null) orderTarget2 = order;
                        }
                        if (orderTarget3 != null)
                        {
                            Order order = GetRealtimeOrder(orderTarget3);
                            if (order != null) orderTarget3 = order;
                        }
                        if (orderTarget4 != null)
                        {
                            Order order = GetRealtimeOrder(orderTarget4);
                            if (order != null) orderTarget4 = order;
                        }


                        AskPrice = GetCurrentAsk(0);
                        BidPrice = GetCurrentBid(0);

                        PositionInfo = string.Format("{0} {1} @ {2}", Position.MarketPosition.ToString().Substring(0, 1), Position.Quantity, Position.AveragePrice);

                        if (Position.MarketPosition == MarketPosition.Long)
                            PositionState = 1;
                        else
                            PositionState = -1;

                        UnRealizedPL = Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, lastPrice);

                        break;
                    case State.Terminated:

                        //if (ATSAlgoSystemMode != AlgoSystemMode.Live && connectionStatusOrder == ConnectionStatus.Connected)
                        //{
                        //    CancelAllOrders();
                        //    Account.Flatten(new[] { Instrument });
                        //}

                        break;
                    case State.Finalized:
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                string errorMsg = string.Format("OnStateChange {0}", ex.ToString());
                Debug.Print(errorMsg);
                Log(errorMsg, LogLevel.Error);
            }
        }


        protected override void OnConnectionStatusUpdate(ConnectionStatusEventArgs connectionStatusUpdate)
        {

            if (tracing)
                Print("OnConnectionStatusUpdate(" + connectionStatusUpdate.ToString() + ")");

            connectionStatusOrder = connectionStatusUpdate.Status;
            connectionStatusPrice = connectionStatusUpdate.PriceStatus;
            base.OnConnectionStatusUpdate(connectionStatusUpdate);
        }

        protected override void OnAccountItemUpdate(Account account, AccountItem accountItem, double value)
        {
            accountDenomination = account.Denomination;
        }

        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string comment)
        {
            if (tracing)
                Print("OnOrderUpdate(" + order.Name + " OrderId=" + order.OrderId + " State=" + order.OrderState.ToString() + ")");


            if (IsHistorical)
                return;


            if (State == State.Realtime && ATSAlgoSystemState == AlgoSystemState.HisTradeRT && !order.IsBacktestOrder)
            {
                ATSAlgoSystemState = AlgoSystemState.Realtime;
            }

            if (ATSAlgoSystemState == AlgoSystemState.HisTradeRT)
            {
                return;
            }

            //add this bit to take care of caveats and problems over submitorder returning slowly missing the order ref or the order ref chaning due to realtime transition
            if (order.OrderState == OrderState.Submitted || order.OrderState == OrderState.Accepted)
            {

                if ((order.Name == orderEntryName) || order.Name.Contains(entry1NameLong) || order.Name.Contains(entry1NameShort))
                {
                    orderEntry = order;

                }
                else if (order.Name.Contains(closeOrderName))
                {
                    orderClose = order;
                }
                else if (order.OrderType == OrderType.StopMarket)
                {
                    if (order.Name.Contains(stop1Name)) { orderStop1 = order; }
                    else if (order.Name.Contains(stop2Name)) { orderStop2 = order; }
                    else if (order.Name.Contains(stop3Name)) { orderStop3 = order; }
                    else if (order.Name.Contains(stop4Name)) { orderStop4 = order; }
                }
                else if (order.OrderType == OrderType.Limit)
                {
                    if (order.Name.Contains(target1Name)) { orderTarget1 = order; }
                    else if (order.Name.Contains(target2Name)) { orderTarget2 = order; }
                    else if (order.Name.Contains(target3Name)) { orderTarget3 = order; }
                    else if (order.Name.Contains(target4Name)) { orderTarget4 = order; }

                }
            }

            if (Account.Connection == Connection.PlaybackConnection) return;

            lock (onOrderUpdateLockObject)
            {

                #region order tracking for entry, stops and targets

                #region OrdersRT

                if (order.OrderState == OrderState.Submitted || order.OrderState == OrderState.Accepted)
                {
                    if (!OrdersActive.Contains(order))
                        OrdersActive.Add(order);
                }
                else if (!OrderIsActive(order))
                {
                    OrdersActive.Remove(order);
                }



                //if (orderEntry == order
                //    || orderStop1 == order || orderStop2 == order || orderStop3 == order || orderStop4 == order
                //    || orderTarget1 == order || orderTarget2 == order || orderTarget3 == order || orderTarget4 == order
                //    )
                //{
                //    if (order.OrderState == OrderState.Submitted || order.OrderState == OrderState.Accepted)
                //    {
                //        if (!OrdersActive.Contains(order))
                //            OrdersActive.Add(order);
                //    }
                //    else if (!OrderIsActive(order))
                //    {
                //        OrdersActive.Remove(order);
                //    }

                //}
                #endregion



                #endregion

                #region order state process
                switch (order.OrderState)
                {
                    case OrderState.Accepted:
                        goto case OrderState.Working;
                    case OrderState.Cancelled:
                        if (order.HasOverfill) OnOrderOverFillDetected(order);
                        break;
                    case OrderState.Filled:
                        if (order.HasOverfill) OnOrderOverFillDetected(order);

                        break;
                    case OrderState.Initialized:
                        break;
                    case OrderState.PartFilled:
                        if (order.HasOverfill) OnOrderOverFillDetected(order);
                        break;
                    case OrderState.CancelPending:
                        if (order.HasOverfill) OnOrderOverFillDetected(order);
                        break;
                    case OrderState.ChangePending:
                        if (order.HasOverfill) OnOrderOverFillDetected(order);
                        break;
                    case OrderState.Submitted:

                        //if (showOrderLabels)
                        //    DrawText(order.Name, order.Name, 0, order.AverageFillPrice != 0 ? order.AverageFillPrice : order.StopPrice != 0 ? order.StopPrice : order.LimitPrice != 0 ? order.LimitPrice : GetCurrentAsk(), Color.Black);

                        if (order == orderEntry)
                        {
                            if (TradeWorkFlow == StrategyTradeWorkFlowState.GoLongSubmitOrder)
                            {
                                TradeWorkFlow = StrategyTradeWorkFlowState.GoLongSubmitOrderPending;
                            }
                            else if (TradeWorkFlow == StrategyTradeWorkFlowState.GoShortSubmitOrder)
                            {
                                TradeWorkFlow = StrategyTradeWorkFlowState.GoShortSubmitOrderPending;
                            }

                        }
                        else if (order == orderClose)
                        {
                            if (tradeWorkFlow == StrategyTradeWorkFlowState.GoLongClosePositions) TradeWorkFlow = StrategyTradeWorkFlowState.GoLongClosedPositionsPending;
                            else if (tradeWorkFlow == StrategyTradeWorkFlowState.GoShortClosePositions) TradeWorkFlow = StrategyTradeWorkFlowState.GoShortClosedPositionsPending;
                            else if (tradeWorkFlow == StrategyTradeWorkFlowState.ExitTradeClosePositions) TradeWorkFlow = StrategyTradeWorkFlowState.ExitTradeClosePositionsPending;
                        }
                        else if (IsExitOnSessionCloseStrategy && order.Name.ToLower().Contains("exit on close"))
                        {
                            orderExitOnClose = order;
                            TradeWorkFlow = StrategyTradeWorkFlowState.ExitOnCloseOrderPending;
                        }

                        break;
                    case OrderState.Rejected:
                        if (tracing)
                        {
                            Print("\r\n");
                            Print("OnOrderUpdate > Rejected(" + order.ToString() + ")");
                            Print(order.ToString());
                        }

                        bool raiseAsError = false;
                        raiseAsError = IsRaiseErrorOnAllOrderRejects;

                        if (!raiseAsError)
                        {
                            if (order == orderEntry)
                            {
                                raiseAsError = true;
                            }
                            else if (StopLossOrders.Contains(order))
                            {
                                raiseAsError = true;

                            }
                            else if (ProfitTargetOrders.Contains(order))
                            {

                                raiseAsError = true;
                            }
                        }
                        if (tracing) Print("Raise Error " + raiseAsError.ToString());
                        if (raiseAsError) TradeWorkFlow = StrategyTradeWorkFlowState.Error;
                        ProcessWorkFlow();
                        break;
                    case OrderState.Unknown:
                        if (tracing)
                            Print("OnOrderUpdate > Unknown(" + order.ToString() + ")");

                        TradeWorkFlow = StrategyTradeWorkFlowState.Error;
                        ProcessWorkFlow();
                        break;
                    case OrderState.Working:

                        if (order == orderEntry)
                        {
                            if (order.OrderAction < OrderAction.Sell)
                                TradeWorkFlow = StrategyTradeWorkFlowState.GoLongSubmitOrderWorking;
                            else
                                TradeWorkFlow = StrategyTradeWorkFlowState.GoShortSubmitOrderWorking;
                        }
                        else if (order == orderEntryOCOLong || order == orderEntryOCOLong)
                        {
                            TradeWorkFlow = StrategyTradeWorkFlowState.GoOCOLongShortSubmitOrderWorking;
                        }
                        else if (StopLossOrders.Contains(order))
                        {
                            //if (Account.Connection == Connection.PlaybackConnection) return;


                            if (TradeWorkFlow == StrategyTradeWorkFlowState.GoShortPlaceStopsPending || TradeWorkFlow == StrategyTradeWorkFlowState.GoLongPlaceStopsPending)
                            {
                                bool result = IsOrdersAllActiveOrWorking(StopLossOrders) && StopLossOrders.Sum(o => o.Quantity) == orderEntry.Quantity;


                                if (result)
                                {
                                    if (TradeWorkFlow == StrategyTradeWorkFlowState.GoShortPlaceStopsPending) TradeWorkFlow = StrategyTradeWorkFlowState.GoShortPlaceStopsConfirmed;
                                    else if (TradeWorkFlow == StrategyTradeWorkFlowState.GoLongPlaceStopsPending) TradeWorkFlow = StrategyTradeWorkFlowState.GoLongPlaceStopsConfirmed;
                                    ProcessWorkFlow();
                                }
                            }
                        }
                        else if (ProfitTargetOrders.Contains(order))
                        {

                            //if (Account.Connection == Connection.PlaybackConnection) return;  //let the marketdata roll it

                            if (TradeWorkFlow == StrategyTradeWorkFlowState.GoShortPlaceProfitTargetsPending || TradeWorkFlow == StrategyTradeWorkFlowState.GoLongPlaceProfitTargetsPending)
                            {
                                bool result = IsOrdersAllActiveOrWorkingOrFilled(ProfitTargetOrders) && ProfitTargetOrders.Sum(o => o.Quantity) == orderEntry.Quantity;


                                if (result)
                                {
                                    if (TradeWorkFlow == StrategyTradeWorkFlowState.GoShortPlaceProfitTargetsPending) TradeWorkFlow = StrategyTradeWorkFlowState.GoShortPlaceProfitTargetsConfirmed;
                                    else if (TradeWorkFlow == StrategyTradeWorkFlowState.GoLongPlaceProfitTargetsPending) TradeWorkFlow = StrategyTradeWorkFlowState.GoLongPlaceProfitTargetsConfirmed;
                                    ProcessWorkFlow();
                                }
                            }
                        }


                        break;
                }

                //if (Historical || Account.Connection == Connection.PlaybackConnection) return;
                //to do reaplace with the onmarketdata way
                #region confirm orders are cancelled
                if (!OrderIsActive(order) && (TradeWorkFlow == StrategyTradeWorkFlowState.GoLongCancelWorkingOrdersPending
                    || TradeWorkFlow == StrategyTradeWorkFlowState.GoShortCancelWorkingOrdersPending
                    || TradeWorkFlow == StrategyTradeWorkFlowState.ExitTradeCancelWorkingOrderPending
                    || TradeWorkFlow == StrategyTradeWorkFlowState.GoOCOLongShortCancelWorkingOrdersPending
                    ) && order != orderClose)
                {

                    //check all stops and targets and working orders have gone through workflow and are all cancelled or inactive
                    if (!OrdersActiveExist())
                    {
                        if (TradeWorkFlow == StrategyTradeWorkFlowState.GoLongCancelWorkingOrdersPending)
                        {
                            TradeWorkFlow = StrategyTradeWorkFlowState.GoLongCancelWorkingOrdersConfirmed;
                        }
                        else if (TradeWorkFlow == StrategyTradeWorkFlowState.GoShortCancelWorkingOrdersPending)
                        {
                            TradeWorkFlow = StrategyTradeWorkFlowState.GoShortCancelWorkingOrdersConfirmed;
                        }
                        else if (TradeWorkFlow == StrategyTradeWorkFlowState.ExitTradeCancelWorkingOrderPending)
                        {
                            TradeWorkFlow = StrategyTradeWorkFlowState.ExitTradeCancelWorkingOrderConfirmed;
                        }
                        else if (TradeWorkFlow == StrategyTradeWorkFlowState.GoOCOLongShortCancelWorkingOrdersPending)
                        {
                            TradeWorkFlow = StrategyTradeWorkFlowState.GoOCOLongShortCancelWorkingOrdersConfirmed;
                        }
                        ProcessWorkFlow();
                    }
                }


                #endregion



                #endregion
            }

        }

        protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
        {
            if (tracing)
                Print("OnExecution(" + execution.ToString() + ")");

            if (execution.Order.HasOverfill)
            {
                OnOrderOverFillDetected(execution.Order);
                return;
            }


            if (State == State.Realtime && ATSAlgoSystemState == AlgoSystemState.HisTradeRT && !execution.Order.IsBacktestOrder)
            {
                ATSAlgoSystemState = AlgoSystemState.Realtime;
            }

            if (execution.Order.OrderState != OrderState.Filled) return;

            if (execution.Order == orderEntryOCOLong || execution.Order == orderEntryOCOShort)
            {
                orderEntry = execution.Order;
            }

            if (execution.Order == orderEntry)
            {

                //filled
                if (orderEntry.OrderAction < OrderAction.Sell)
                    TradeWorkFlow = StrategyTradeWorkFlowState.GoLongSubmitOrderFilled;
                else
                    TradeWorkFlow = StrategyTradeWorkFlowState.GoShortSubmitOrderFilled;

                ProcessWorkFlow();
                return;
            }
            else if (execution.Order == orderClose)
            {

                if (tradeWorkFlow == StrategyTradeWorkFlowState.GoLongClosedPositionsPending) TradeWorkFlow = StrategyTradeWorkFlowState.GoLongClosedPositionsConfirmed;
                else if (tradeWorkFlow == StrategyTradeWorkFlowState.GoShortClosedPositionsPending) TradeWorkFlow = StrategyTradeWorkFlowState.GoShortClosedPositionsConfirmed;
                else if (tradeWorkFlow == StrategyTradeWorkFlowState.ExitTradeClosePositionsPending) TradeWorkFlow = StrategyTradeWorkFlowState.ExitTradeClosePositionsConfirmed;
                else if (tradeWorkFlow == StrategyTradeWorkFlowState.ErrorFlattenAllPending) TradeWorkFlow = StrategyTradeWorkFlowState.ErrorFlattenAllConfirmed;
                ProcessWorkFlow(TradeWorkFlow);
                return;
            }
            else if (execution.Order == orderExitOnClose || IsExitOnSessionCloseStrategy && execution.Order.Name.ToLower().Contains("exit on close"))
            {
                orderExitOnClose = execution.Order;
                TradeWorkFlow = StrategyTradeWorkFlowState.ExitOnCloseOrderFilled;
                ProcessWorkFlow();
                return;
            }
            else if (execution.Order == orderEntryPrior)
            {
                entryOrderInFlightCollision = true;
                if (tracing)
                    Print("- PRIOR ENTRY ORDER EXECUTED OnExecution(" + execution.ToString() + ")");

            }

        }
        protected override void OnPositionUpdate(Position position, double averagePrice, int quantity, MarketPosition marketPosition)
        {
            if (tracing)
                Print("OnPositionUpdate" + marketPosition.ToString());

            if (IsHistorical) return;


            if (ATSAlgoSystemState == AlgoSystemState.HisTradeRT && marketPosition == MarketPosition.Flat)
            {
                ATSAlgoSystemState = AlgoSystemState.Realtime;
            }
        }

        protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
        {

            if (DateTime.Now < onMarketDataTimeNextAllowed) return;

            if (marketDataUpdate.MarketDataType == MarketDataType.Bid || marketDataUpdate.MarketDataType == MarketDataType.Ask)
            {
                onMarketDataTimeNextAllowed = DateTime.Now.AddMilliseconds(250);
                AskPrice = marketDataUpdate.Ask;
                BidPrice = marketDataUpdate.Bid;
                return;
            }

            if (marketDataUpdate.MarketDataType != MarketDataType.Last) return;
            this.MarketDataUpdate = marketDataUpdate;

            if (!IsTradeWorkFlowReady() && TradeWorkFlowLastChanged < DateTime.Now.AddSeconds(-1 * TradeWorkFlowTimeOut))
            {

                if (tracing)
                    Print("OnMarketData >> TWF ErrorTimeOut");

                TradeWorkFlow = StrategyTradeWorkFlowState.ErrorTimeOut;
                ProcessWorkFlow(StrategyTradeWorkFlowState.ErrorTimeOut);

            }


            if (this.LastPrice == marketDataUpdate.Price) return;
            LastPrice = marketDataUpdate.Price;

            onMarketDataTimeNextAllowed = DateTime.Now.AddSeconds(1);

            if (this.inOnMarketData) return;
            lock (this.inOnMarketDataLock)
            {
                if (this.inOnMarketData) return;
                this.inOnMarketData = true;
            }

            try
            {


                if (Position.MarketPosition == MarketPosition.Flat)
                {
                    PositionInfo = string.Format("{0}", Position.MarketPosition.ToString());
                    PositionState = 0;
                    UnRealizedPL = 0;
                }
                else
                {
                    PositionInfo = string.Format("{0} {1} @ {2}", Position.MarketPosition.ToString().Substring(0, 1), Position.Quantity, Position.AveragePrice);

                    if (Position.MarketPosition == MarketPosition.Long)
                        PositionState = 1;
                    else
                        PositionState = -1;

                    UnRealizedPL = Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency);
                }

                if (ATSAlgoSystemState != AlgoSystemState.Realtime)
                {
                    this.inOnMarketData = false;
                    return;
                }

                //process the signal q
                if (IsTEQOnMarketData && Now >= tEQNextTimeValid)
                {
                    tEQNextTimeValid = Now.AddSeconds(TEQTimerInterval);
                    if (tracing)
                        Print("OnMarketData >> TEQ");


                    if (TEQ.Count > 0)
                    {
                        ProcessTradeEventQueue();
                        this.inOnMarketData = false;
                        return;
                    }
                }


                //process the trade workflow state engine
                if (IsTradeWorkFlowOnMarketData && Now >= tradeWorkFlowNextTimeValid)
                {

                    tradeWorkFlowNextTimeValid = Now.AddMilliseconds(TradeWorkFlowTimerInterval);
                    if (tracing)
                        Print("OnMarketData >> TWF");
                    ProcessWorkFlow();
                }
            }
            catch (Exception ex)
            {
                Print("OnMarketData >> Error: " + ex.ToString());
            }

            this.inOnMarketData = false;


        }


        protected override void OnBarUpdate()
        {

            if ((State == State.Historical && (IsRealtimeTradingOnly || IsPlayBack)) || CurrentBar < 1)
                return;

            lastPrice = Closes[0][0];

            if (IsFirstTickOfBar)
            {
                if (Bars.IsFirstBarOfSessionByIndex(0))
                {
                    if (IsExitOnSessionCloseStrategy)
                    {
                        OnExitOnCloseDetected();
                    }
                }
            }

            if (AlgoSignalAction != AlgoSignalAction.None)
                TEQ.Enqueue(new AlgoSignalActionMsq(AlgoSignalAction, Account.Connection.Now, "Auto Signal " + AlgoSignalAction));

            //Reset to avoid duplicate action
            AlgoSignalAction = AlgoSignalAction.None;

            if (!IsRealtimeTradingUseQueue && TEQ.Count > 0)
                ProcessTradeEventQueue();
            else if (TradeWorkFlow == StrategyTradeWorkFlowState.ErrorFlattenAll && TradeWorkFlowLastChanged < DateTime.Now.AddSeconds(3))//belt and braces
                ProcessWorkFlow();

        }

        /// <summary>
        /// OnStrategyTradeWorkFlowUpdated
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnStrategyTradeWorkFlowUpdated(StrategyTradeWorkFlowUpdatedEventArgs e)
        {
            EventHandler<StrategyTradeWorkFlowUpdatedEventArgs> handler = StrategyTradeWorkFlowUpdated;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion
        #region methods
        #region NotifyPropertyChanged
        //public void NotifyPropertyChanged()
        //{
        //    NotifyPropertyChanged(string.Empty);
        //}

        //public void NotifyPropertyChanged(String info)
        //{
        //    if (PropertyChanged != null)
        //    {
        //        PropertyChanged(this, new PropertyChangedEventArgs(info.Replace("P_", "")));
        //    }
        //}
        #endregion
        #region timers

        private void TradeWorkFlowOnMarketDataEnable(int mSeconds)
        {
            if (IsTradeWorkFlowOnMarketData) return;

            if (tracing)
                Print("TradeWorkFlowOnMarketDataEnable");

            tradeWorkFlowNextTimeValid = Now.AddMilliseconds(mSeconds);
            IsTradeWorkFlowOnMarketData = true;

        }


        private void TradeWorkFlowOnMarketDataEnable()
        {
            TradeWorkFlowOnMarketDataEnable(TradeWorkFlowTimerInterval);
        }

        private void TradeWorkFlowOnMarketDataDisable()
        {
            if (!IsTradeWorkFlowOnMarketData) return;

            IsTradeWorkFlowOnMarketData = false;
            if (tracing)
                Print("TradeWorkFlowOnMarketDataDisable");

        }


        private void TEQOnMarketDataEnable()
        {
            if (IsTEQOnMarketData) return;

            tEQNextTimeValid = Now.AddSeconds(TEQTimerInterval);
            IsTEQOnMarketData = true;
            if (tracing)
                Print("TEQOnMarketDataEnable");

        }

        private void TEQwOnMarketDataDisable()
        {
            if (!IsTEQOnMarketData) return;

            IsTEQOnMarketData = false;
            if (tracing)
                Print("TEQwOnMarketDataDisable");

        }

        public bool CurrentBarIsLastBar()
        {
            bool result = false;
            result = base.CurrentBar > (base.Bars.Count - 2);

#if DEBUG

            if (result) Print(base.CurrentBar);
#endif

            return result;
        }


        #endregion
        #region orderHelpers

        /// <summary>
        /// PreTradeValidateNoActiveOrdersExist for working or active orders and returns true if none are found - all clear is true - returns false if orders are found
        /// </summary>
        /// <returns></returns>
        public virtual bool PreTradeValidateNoActiveOrdersExist()
        {
            if (tracing)
                Print("PreTradeValidateNoActiveOrdersExist()");

            return !OrdersActiveExist();
        }

        /// <summary>
        /// PreTradeValidatePositionIsFlat checks and returns true if Position.Quantity==0 - returns false if not
        /// </summary>
        /// <returns></returns>
        public virtual bool PreTradeValidatePositionIsFlat()
        {
            if (tracing)
                Print("PreTradeValidatePositionIsFlat()");

            return Position.Quantity == 0;
        }



        /// <summary>
        /// preTradeValidateCanEnterTrade - returns true if PreTradeValidateNoActiveOrdersExist and PreTradeValidatePositionIsFlat are true - returns false if not.
        /// Can be overidden in derived child class so that a user/developer defined set of conditions can be assessed to allow or prevent a trade entry.
        /// </summary>
        /// <param name="isLong"></param>
        /// <returns></returns>
        private bool PreTradeValidateCanEnterTrade(bool isLong)
        {
            if (tracing)
                Print("preTradeValidateCanEnterTrade()");

            return PreTradeValidateNoActiveOrdersExist() && (PreTradeValidatePositionIsFlat() || OrderIsInFlightOrActive(orderClose)) && OnPreTradeEntryValidate(isLong);
        }


        private bool PreTradeValidateCanEnterTradeOCO()
        {
            if (tracing)
                Print("PreTradeValidateCanEnterTradeOCO()");

            return PreTradeValidateNoActiveOrdersExist() && (PreTradeValidatePositionIsFlat() || OrderIsInFlightOrActive(orderClose));
        }


        public virtual bool OnPreTradeEntryValidate(bool isLong)
        {
            if (tracing)
                Print("OnpreTradeValidateCanEnterTrade()");
            return true;

        }




        /// <summary>
        /// OnExitOnCloseDetected() called when exit on close order has been detected from NT
        /// </summary>
        public virtual void OnExitOnCloseDetected()
        {
            if (tracing)
                Print("OnExitOnCloseDetected()");

            if (TradeWorkFlow != StrategyTradeWorkFlowState.Waiting)
            {
                ProcessWorkFlow(StrategyTradeWorkFlowState.ExitTrade);
            }
        }


        public virtual void OnOrderOverFillDetected(Order order)
        {
            if (tracing)
                Print("OnOrderOverFillDetected(" + order.ToString() + ")");

            ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
        }


        public void CancelAllOrders()
        {

            if (tracing)
                Print("CancelAllOrders()");

            //historical or realtime batch mode
            if (IsHistoricalTradeOrPlayBack || !IsOrderCancelInspectEachOrDoBatchCancel)
            {

                CancelOrder(orderStop1);
                CancelOrder(orderStop2);
                CancelOrder(orderStop3);
                CancelOrder(orderStop4);
                CancelOrder(orderTarget1);
                CancelOrder(orderTarget2);
                CancelOrder(orderTarget3);
                CancelOrder(orderTarget4);
                CancelOrder(orderEntry);

                return;
            }

            Account.CancelAllOrders(this.Instrument);
            OrdersActive.Clear();

        }

        protected bool OrdersCheckInactiveAndPurge()
        {

            if (tracing)
                Print("OrdersCheckInactiveAndPurge() > OrdersRT : " + OrdersActive.Count.ToString());


            foreach (Order o in OrdersActive.ToArray())
            {
                //if (tracing)
                //    Print(o.ToString());

                if (!OrderIsActive(o))
                {
                    //if (tracing)
                    //    if (State == State.Realtime) Print("OrdersCheckInactiveAndPurge() > true : " + o.ToString());

                    OrdersActive.Remove(o);

                }
            }
            return false;
        }

        protected bool OrdersActiveExist()
        {
            if (tracing)
            {
                Print("OrdersActiveExist() > OrdersRT : " + OrdersActive.ToArray().Count(O => !Order.IsTerminalState(O.OrderState)).ToString());
            }

            return OrdersActive.ToArray().Count(O => !Order.IsTerminalState(O.OrderState)) > 0;

        }

        protected bool OrderIsAcceptedOrWorking(Order o)
        {
            return (o != null ? (o.OrderState == OrderState.Accepted || o.OrderState == OrderState.Working) : false);
        }

        protected bool OrderIsActive(Order o)
        {
            return (o != null && !OrderIsTerminated(o));
        }

        protected bool OrderIsInFlight(Order o)
        {
            return o.OrderState == OrderState.PartFilled;
        }

        private bool OrderIsInFlightOrActive(Order o)
        {
            return OrderIsActive(o) || OrderIsActive(o);
        }


        protected bool OrderIsActiveCanChangeOrCancel(Order o)
        {
            return OrderIsActive(o) && o.OrderState != OrderState.CancelPending && o.OrderState != OrderState.ChangePending;
        }


        protected bool OrderIsTerminated(Order o)
        {
            return o != null && Order.IsTerminalState(o.OrderState) || ((State == State.Realtime ? o.OrderState == OrderState.Submitted && o.Time.AddSeconds(10) < DateTime.Now : false));
        }

        #endregion
        #region SignalActions
        public void SubmitSignalAction(AlgoSignalAction AlgoSignalActions, string signalLabel)
        {
            if (tracing) Print("SubmitSignalAction " + AlgoSignalActions.ToString() + " WF=" + this.TradeWorkFlow.ToString());

            TEQ.Enqueue(new AlgoSignalActionMsq(AlgoSignalActions, IsHistoricalTradeOrPlayBack ? Time[0] : DateTime.Now, signalLabel));
            #region Signal Execution
            if (IsHistoricalTradeOrPlayBack || !IsRealtimeTradingUseQueue)
            {
                if (TEQ.Count > 0) ProcessTradeEventQueue(this);
            }
            else
            {
                if (State == State.Realtime)
                {
                    TEQOnMarketDataEnable();
                }
            }
            #endregion
        }
        #endregion
        #region process queue

        public void ProcessTradeEventQueue()
        {
            ProcessTradeEventQueue(this.TEQ);
        }

        public void ProcessTradeEventQueue(object state)
        {
            ProcessTradeEventQueue();
        }

        //in the case of many signals coming in e.g. each tick
        public void ProcessTradeEventQueue(Queue<AlgoSignalActionMsq> q)
        {
            //if (tracing)
            //    Print("ProcessTradeEventQueue(Queue q) ");

            #region queueLock
            if (lockedQueue) return;
            lock (queueLockObject)
            {
                if (lockedQueue) return;
                lockedQueue = true;
            }
            #endregion

            if (q.Count == 0)
            {
                lockedQueue = false;
                return;
            }
            else if (!IsTradeWorkFlowReady())
            {
                if (tracing)
                    Print("ProcessTradeEventQueue(Queue count " + q.Count.ToString() + " TradeWF " + tradeWorkFlow.ToString() + "  ) ");

                if (State == State.Realtime)
                {
                    TEQOnMarketDataEnable();
                }
                lockedQueue = false;
                return;
            }

            #region process Queue
            try
            {
                //get last most recent item in
                AlgoSignalActionMsq a = null;
                while (q.Count > 1)
                {
                    a = (AlgoSignalActionMsq)q.Dequeue();

                    if (tracing)
                        Print("ProcessTradeEventQueue>Dequeue " + a.ToString());

                }
                if ((IsHistorical && IsRealtimeTradingOnly))
                {
                    lockedQueue = false;
                    return;
                }

                //Try process action
                a = (AlgoSignalActionMsq)q.Peek();
                if (tracing)
                    Print("ProcessTradeEventQueue> Try> AlgoSignalActions " + a.ToString());


                bool dQ = false;

                if (DateTime.Now > a.ActionDateTime.AddSeconds(TradeSignalExpiryInterval))
                {

                    if (dQ) q.Dequeue();

                    if (tracing)
                        Print("ProcessTradeEventQueue> Trade Signal TimeOut > AlgoSignalActions " + a.ToString());

                }

                switch (a.Action)
                {
                    case AlgoSignalAction.GoLong:
                        if (this.IsTradeWorkFlowCanGoLong())
                        {
                            dQ = true;
                            TradeWorkFlowNewOrder(StrategyTradeWorkFlowState.GoLong);
                        }
                        break;
                    case AlgoSignalAction.GoShort:
                        if (this.IsTradeWorkFlowCanGoShort())
                        {
                            dQ = true;
                            TradeWorkFlowNewOrder(StrategyTradeWorkFlowState.GoShort);
                        }
                        break;
                    case AlgoSignalAction.ExitTrade:
                        if (this.IsTradeWorkFlowCanExit())
                        {
                            dQ = true;
                            TradeWorkFlowTradeExit();
                        }
                        break;
                    case AlgoSignalAction.ExitTradeLong:
                        if (this.IsTradeWorkFlowCanExit())
                        {
                            dQ = true;
                            TradeWorkFlowTradeExitLong();
                        }
                        break;
                    case AlgoSignalAction.ExitTradeShort:
                        if (this.IsTradeWorkFlowCanExit())
                        {
                            dQ = true;
                            TradeWorkFlowTradeExitShort();
                        }
                        break;
                }

                if (dQ) q.Dequeue();

                if (tracing)
                {
                    if (dQ)
                        Print("ProcessTradeEventQueue> Processed> AlgoSignalActions " + a.ToString());
                    else
                        Print("ProcessTradeEventQueue> Retry Later> AlgoSignalActions " + a.ToString());
                }
            }
            catch (Exception ex)
            {
                Print(ex.ToString());
                Debug.Print(ex.ToString());
                Log(ex.ToString(), LogLevel.Error);
            }
            #endregion
            lockedQueue = false;
            TEQwOnMarketDataDisable();
        }

        #endregion
        #region process workflow

        protected void ProcessWorkFlow()
        {
            TradeWorkFlow = ProcessWorkFlow(TradeWorkFlow);
        }

        private void ProcessWorkFlow(object state)
        {
            ProcessWorkFlow(this.tradeWorkFlow);
        }

        protected StrategyTradeWorkFlowState ProcessWorkFlow(StrategyTradeWorkFlowState tradeWorkFlow)
        {
            if (tracing)
                Print("ProcessWorkFlow(" + tradeWorkFlow.ToString() + ")");
            //to do post fill check is correct prior to stoploss/targets


            TradeWorkFlow = tradeWorkFlow;

            switch (tradeWorkFlow)
            {
                case StrategyTradeWorkFlowState.GoLong:
                    //trade per direction test
                    if (Position.MarketPosition == MarketPosition.Long) return ProcessWorkFlow(StrategyTradeWorkFlowState.GoLongValidationRejected);

                    if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
                    {
                        TradeWorkFlow = StrategyTradeWorkFlowState.GoLongCancelWorkingOrders;
                        goto case StrategyTradeWorkFlowState.GoLongCancelWorkingOrders;
                    }
                    //orders to cancel test
                    if (OrdersActiveExist()) return ProcessWorkFlow(StrategyTradeWorkFlowState.GoLongCancelWorkingOrders);

                    //position to close test
                    if (Position.Quantity != 0) return ProcessWorkFlow(StrategyTradeWorkFlowState.GoLongClosePositions);

                    //if midflight working orders wait ie exist in iOrders- have been terminated/ filled or part filled but onOrderUpdate has not yet been raised and processed on this thread retry on timer if realtime come back later and retry
                    //inflight order test   -but what about stuck pending orders
                    //prioWF= working && the priorEntryOrder==filled && still in OrdersRT - ie onOrderUpdate,exec, positionupdate not fired for it yet?
                    if (State == State.Realtime && OrdersActive.Contains(orderEntry) && (tradeWorkFlowPrior == StrategyTradeWorkFlowState.GoShortSubmitOrderWorking || tradeWorkFlowPrior == StrategyTradeWorkFlowState.GoLongSubmitOrderWorking) && (orderEntry.OrderState == OrderState.Filled))
                    {
                        TradeWorkFlowOnMarketDataEnable();
                        //will continue to loop back here forever unless we have a timeout
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    //goto case StrategyTradeWorkFlowState.GoLongSubmitOrder;
                    return ProcessWorkFlow(StrategyTradeWorkFlowState.GoLongSubmitOrder);
                case StrategyTradeWorkFlowState.GoLongCancelWorkingOrders:

                    if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
                    {
                        CancelAllOrders();
                        TradeWorkFlow = StrategyTradeWorkFlowState.GoLongClosePositions;
                        goto case StrategyTradeWorkFlowState.GoLongClosePositions;

                    }
                    else
                    {
                        if (connectionStatusOrder == ConnectionStatus.Connected)
                        {
                            TradeWorkFlow = StrategyTradeWorkFlowState.GoLongCancelWorkingOrdersPending;

                            CancelAllOrders();
                            //OnOrderUpdate Event or WorkFlow Timer will re-enter Workflow
                        }
                        TradeWorkFlowOnMarketDataEnable();
                        //will continue to loop back here forever unless we have a timeout
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);

                    }

                    break;
                case StrategyTradeWorkFlowState.GoLongCancelWorkingOrdersPending:

                    if (OrdersActiveExist())
                    {
                        TradeWorkFlowOnMarketDataEnable();
                        //will continue to loop back here forever unless we have a timeout
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    else
                    {
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.GoLongCancelWorkingOrdersConfirmed);
                    }
                    break;
                case StrategyTradeWorkFlowState.GoLongCancelWorkingOrdersConfirmed:
                    TradeWorkFlowOnMarketDataDisable();
                    if (Position.Quantity != 0) return ProcessWorkFlow(StrategyTradeWorkFlowState.GoLongClosePositions);
                    else return ProcessWorkFlow(StrategyTradeWorkFlowState.GoLongSubmitOrder);
                case StrategyTradeWorkFlowState.GoLongClosePositions:
                    if (Position.Quantity != 0)
                    {
                        if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
                        {
                            TradeWorkFlow = StrategyTradeWorkFlowState.GoLongClosedPositionsPending;
                            PositionCloseInternal();
                            break;
                        }
                        else
                        {
                            if (connectionStatusOrder == ConnectionStatus.Connected)
                            {
                                //avoid a double exit multithreaded race condition
                                if (TradeWorkFlow == StrategyTradeWorkFlowState.GoLongClosedPositionsPending) break;
                                lock (lockObjectClose)
                                {
                                    if (TradeWorkFlow == StrategyTradeWorkFlowState.GoLongClosedPositionsPending) break;
                                    TradeWorkFlow = StrategyTradeWorkFlowState.GoLongClosedPositionsPending;
                                }
                                PositionCloseInternal();
                                break;
                            }
                            //if still here then connection issues must come back and try or raise alarm
                            TradeWorkFlowOnMarketDataEnable();
                            //will continue to loop back here forever unless we have a timeout
                            tradeWorkFlowRetryCount++;
                            if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                                return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);

                        }
                    }
                    else
                    {
                        TradeWorkFlow = StrategyTradeWorkFlowState.GoLongSubmitOrder;
                        goto case StrategyTradeWorkFlowState.GoLongSubmitOrder;
                    }


                    break;
                case StrategyTradeWorkFlowState.GoLongClosedPositionsPending:
                    //execution event or realtime event or timer moves event on
                    if (Position.Quantity != 0)
                    {
                        TradeWorkFlowOnMarketDataEnable();
                        //will continue to loop back here forever unless we have a timeout
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    else
                    {
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.GoLongClosedPositionsConfirmed);
                    }

                    break;
                case StrategyTradeWorkFlowState.GoLongClosedPositionsConfirmed:
                    TradeWorkFlowOnMarketDataDisable();
                    return ProcessWorkFlow(StrategyTradeWorkFlowState.GoLongSubmitOrder);
                case StrategyTradeWorkFlowState.GoLongSubmitOrder:
                    if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
                    {
                        if (PreTradeValidateCanEnterTrade(true))
                        {
                            orderEntryPrior = orderEntry;
                            orderEntry = SubmitLongTrade();
                        }
                        else
                        {
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.GoLongValidationRejected);
                        }
                        break;
                    }

                    if (connectionStatusOrder == ConnectionStatus.Connected)
                    {
                        if (PreTradeValidateCanEnterTrade(true))
                        {
                            orderEntryPrior = orderEntry;
                            orderEntry = SubmitLongTrade();
                        }
                        else
                        {
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.GoLongValidationRejected);
                        }
                    }
                    else
                    {
                        //flatten all and cancel the trade by the time it reconnects it might be too late.
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }

                    break;
                case StrategyTradeWorkFlowState.GoLongValidationRejected:
                    if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode) return ProcessWorkFlow(StrategyTradeWorkFlowState.Waiting);
                    return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                case StrategyTradeWorkFlowState.GoLongSubmitOrderPending:
                    if (orderEntry.OrderState == OrderState.Filled)
                    {
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.GoLongSubmitOrderFilled);
                    }
                    else if (orderEntry.OrderState == OrderState.Accepted || orderEntry.OrderState == OrderState.Working)
                    {
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.GoLongSubmitOrderWorking);
                    }

                    TradeWorkFlowOnMarketDataEnable();
                    //will continue to loop back here forever unless we have a timeout
                    tradeWorkFlowRetryCount++;
                    if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    break;
                case StrategyTradeWorkFlowState.GoLongSubmitOrderWorking:
                    TradeWorkFlowOnMarketDataDisable();
                    break;
                case StrategyTradeWorkFlowState.GoLongSubmitOrderFilled:
                    TradeWorkFlowOnMarketDataDisable();
                    if (SubmitStopLossWillOccur()) return ProcessWorkFlow(StrategyTradeWorkFlowState.GoLongPlaceStops);
                    if (SubmitProfitTargetWillOccur()) return ProcessWorkFlow(StrategyTradeWorkFlowState.GoLongPlaceProfitTargets);
                    return ProcessWorkFlow(StrategyTradeWorkFlowState.CycleComplete);
                case StrategyTradeWorkFlowState.GoLongPlaceStops:
                    if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
                    {
                        TradeWorkFlow = StrategyTradeWorkFlowState.GoLongPlaceStopsPending;
                        SubmitStopLossInternal();
                        goto case StrategyTradeWorkFlowState.GoLongPlaceStopsPending;
                    }
                    else
                    {
                        if (connectionStatusOrder == ConnectionStatus.Connected)
                        {
                            TradeWorkFlow = StrategyTradeWorkFlowState.GoLongPlaceStopsPending;
                            SubmitStopLossInternal();
                        }
                        TradeWorkFlowOnMarketDataEnable();
                        //will continue to loop back here forever unless we have a timeout
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);

                    }
                    break;
                case StrategyTradeWorkFlowState.GoLongPlaceStopsPending:
                    if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
                    {
                        TradeWorkFlow = StrategyTradeWorkFlowState.GoLongPlaceStopsConfirmed;
                        goto case StrategyTradeWorkFlowState.GoLongPlaceStopsConfirmed;
                    }

                    bool allConfirmedGoLongPlaceStops = IsOrdersAllActiveOrWorkingOrFilled(StopLossOrders);


                    if (!allConfirmedGoLongPlaceStops)
                    {
                        TradeWorkFlowOnMarketDataEnable();
                        //will continue to loop back here forever unless we have a timeout
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    else
                    {
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.GoLongPlaceStopsConfirmed);
                    }
                    break;
                case StrategyTradeWorkFlowState.GoLongPlaceStopsConfirmed:
                    TradeWorkFlowOnMarketDataDisable();
                    if (SubmitProfitTargetWillOccur()) return ProcessWorkFlow(StrategyTradeWorkFlowState.GoLongPlaceProfitTargets);
                    return ProcessWorkFlow(StrategyTradeWorkFlowState.CycleComplete);
                case StrategyTradeWorkFlowState.GoLongPlaceProfitTargets:
                    if (IsHistoricalTradeOrPlayBack || !IsSubmitTargetsAndConfirm)
                    {
                        TradeWorkFlow = StrategyTradeWorkFlowState.GoLongPlaceProfitTargetsPending;
                        SubmitProfitTargetInternal();
                        goto case StrategyTradeWorkFlowState.GoLongPlaceProfitTargetsPending;
                    }
                    else
                    {
                        if (connectionStatusOrder == ConnectionStatus.Connected)
                        {
                            TradeWorkFlow = StrategyTradeWorkFlowState.GoLongPlaceProfitTargetsPending;
                            SubmitProfitTargetInternal();
                            if (!IsSubmitTargetsAndConfirm)
                                goto case StrategyTradeWorkFlowState.GoLongPlaceProfitTargetsPending;

                        }
                        TradeWorkFlowOnMarketDataEnable();
                        //will continue to loop back here forever unless we have a timeout
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    break;
                case StrategyTradeWorkFlowState.GoLongPlaceProfitTargetsPending:
                    if (IsHistoricalTradeOrPlayBack || !IsSubmitTargetsAndConfirm)
                    {
                        TradeWorkFlow = StrategyTradeWorkFlowState.GoLongPlaceProfitTargetsConfirmed;
                        goto case StrategyTradeWorkFlowState.GoLongPlaceProfitTargetsConfirmed;
                    }

                    bool allConfirmedGoLongPlaceProfitTargets = (IsOrdersAllActiveOrWorkingOrFilled(ProfitTargetOrders) || ProfitTargetOrders.Sum(o => o.Quantity) == orderEntry.Quantity);

                    if (!allConfirmedGoLongPlaceProfitTargets)
                    {
                        TradeWorkFlowOnMarketDataEnable();
                        //will continue to loop back here forever unless we have a timeout
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    else
                    {
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.GoLongPlaceProfitTargetsConfirmed);
                    }
                    break;
                case StrategyTradeWorkFlowState.GoLongPlaceProfitTargetsConfirmed:
                    TradeWorkFlowOnMarketDataDisable();
                    return ProcessWorkFlow(StrategyTradeWorkFlowState.CycleComplete);
                case StrategyTradeWorkFlowState.GoShort:
                    //trade per direction test
                    if (Position.MarketPosition == MarketPosition.Short) return ProcessWorkFlow(StrategyTradeWorkFlowState.GoShortValidationRejected);

                    if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
                    {
                        TradeWorkFlow = StrategyTradeWorkFlowState.GoShortCancelWorkingOrders;
                        goto case StrategyTradeWorkFlowState.GoShortCancelWorkingOrders;
                    }


                    //orders to cancel test
                    //if (Historical || OrdersActiveExist() || Account.Name.ToLower() == Connection.ReplayAccountName) 
                    if (OrdersActiveExist()) return ProcessWorkFlow(StrategyTradeWorkFlowState.GoShortCancelWorkingOrders);

                    //position to close test
                    if (Position.Quantity != 0) return ProcessWorkFlow(StrategyTradeWorkFlowState.GoShortClosePositions);

                    //inflight entry order test
                    if (State == State.Realtime && OrdersActive.Contains(orderEntry) && (tradeWorkFlowPrior == StrategyTradeWorkFlowState.GoShortSubmitOrderWorking || tradeWorkFlowPrior == StrategyTradeWorkFlowState.GoLongSubmitOrderWorking) && (orderEntry.OrderState == OrderState.Filled))
                    {
                        TradeWorkFlowOnMarketDataEnable();
                        //will continue to loop back here forever unless we have a timeout
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    return ProcessWorkFlow(StrategyTradeWorkFlowState.GoShortSubmitOrder);

                case StrategyTradeWorkFlowState.GoShortCancelWorkingOrders:
                    if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
                    {
                        CancelAllOrders();
                        TradeWorkFlow = StrategyTradeWorkFlowState.GoShortCancelWorkingOrdersConfirmed;
                        goto case StrategyTradeWorkFlowState.GoShortCancelWorkingOrdersConfirmed;

                    }
                    else
                    {
                        if (connectionStatusOrder == ConnectionStatus.Connected)
                        {
                            TradeWorkFlow = StrategyTradeWorkFlowState.GoShortCancelWorkingOrdersPending;

                            CancelAllOrders();
                        }
                        TradeWorkFlowOnMarketDataEnable();
                        //will continue to loop back here forever unless we have a timeout
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    break;
                case StrategyTradeWorkFlowState.GoShortCancelWorkingOrdersPending:
                    if (OrdersActiveExist())
                    {

                        TradeWorkFlowOnMarketDataEnable();
                        //will continue to loop back here forever unless we have a timeout
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    else
                    {
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.GoShortCancelWorkingOrdersConfirmed);
                    }
                    break;
                case StrategyTradeWorkFlowState.GoShortCancelWorkingOrdersConfirmed:
                    TradeWorkFlowOnMarketDataDisable();
                    if (Position.Quantity != 0) return ProcessWorkFlow(StrategyTradeWorkFlowState.GoShortClosePositions);
                    else return ProcessWorkFlow(StrategyTradeWorkFlowState.GoShortSubmitOrder);
                case StrategyTradeWorkFlowState.GoShortClosePositions:
                    if (Position.Quantity != 0)
                    {
                        if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
                        {
                            TradeWorkFlow = StrategyTradeWorkFlowState.GoShortClosedPositionsPending;
                            PositionCloseInternal();
                            break;
                        }
                        else
                        {
                            if (connectionStatusOrder == ConnectionStatus.Connected)
                            {
                                //avoid a double exit multithreaded race condition
                                if (TradeWorkFlow == StrategyTradeWorkFlowState.GoShortClosedPositionsPending) break;
                                lock (lockObjectClose)
                                {
                                    if (TradeWorkFlow == StrategyTradeWorkFlowState.GoShortClosedPositionsPending) break;
                                    TradeWorkFlow = StrategyTradeWorkFlowState.GoShortClosedPositionsPending;
                                }
                                PositionCloseInternal();

                                break;
                            }
                            //if still here then connection issues must come back and try or raise alarm
                            TradeWorkFlowOnMarketDataEnable();
                            //wait for event or timer confirmation
                            tradeWorkFlowRetryCount++;
                            if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                                return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                        }
                    }
                    else
                    {
                        TradeWorkFlow = StrategyTradeWorkFlowState.GoShortSubmitOrder;
                        goto case StrategyTradeWorkFlowState.GoShortSubmitOrder;
                    }
                    break;
                case StrategyTradeWorkFlowState.GoShortClosedPositionsPending:
                    //execution event or realtime event or timer moves event on

                    if (Position.Quantity != 0)
                    {
                        TradeWorkFlowOnMarketDataEnable();
                        //will continue to loop back here forever unless we have a timeout
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    else
                    {
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.GoShortClosedPositionsConfirmed);
                    }
                    break;
                case StrategyTradeWorkFlowState.GoShortClosedPositionsConfirmed:
                    TradeWorkFlowOnMarketDataDisable();
                    return ProcessWorkFlow(StrategyTradeWorkFlowState.GoShortSubmitOrder);
                case StrategyTradeWorkFlowState.GoShortSubmitOrder:
                    if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
                    {
                        if (PreTradeValidateCanEnterTrade(false))
                        {
                            orderEntryPrior = orderEntry;
                            orderEntry = SubmitShortTrade();
                        }
                        else
                        {
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.GoShortValidationRejected);
                        }
                        break;
                    }


                    if (connectionStatusOrder == ConnectionStatus.Connected)
                    {
                        if (PreTradeValidateCanEnterTrade(false))
                        {
                            orderEntryPrior = orderEntry;
                            orderEntry = SubmitShortTrade();
                        }
                        else
                        {
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.GoShortValidationRejected);
                        }
                    }
                    else
                    {
                        //flatten all and cancel the trade by the time it reconnects it might be too late.
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }

                    //TradeWorkFlow = StrategyTradeWorkFlowState.GoShortSubmitOrderPending;

                    break;
                case StrategyTradeWorkFlowState.GoShortValidationRejected:
                    if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode) return ProcessWorkFlow(StrategyTradeWorkFlowState.Waiting);
                    return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                case StrategyTradeWorkFlowState.GoShortSubmitOrderPending:

                    if (orderEntry.OrderState == OrderState.Filled)
                    {
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.GoShortSubmitOrderFilled);
                    }
                    else if (orderEntry.OrderState == OrderState.Accepted || orderEntry.OrderState == OrderState.Working)
                    {
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.GoShortSubmitOrderWorking);
                    }

                    TradeWorkFlowOnMarketDataEnable();
                    //will continue to loop back here forever unless we have a timeout
                    tradeWorkFlowRetryCount++;
                    if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);

                    break;
                case StrategyTradeWorkFlowState.GoShortSubmitOrderFilled:
                    TradeWorkFlowOnMarketDataDisable();
                    if (SubmitStopLossWillOccur()) return ProcessWorkFlow(StrategyTradeWorkFlowState.GoShortPlaceStops);
                    if (SubmitProfitTargetWillOccur()) return ProcessWorkFlow(StrategyTradeWorkFlowState.GoShortPlaceProfitTargets);
                    return ProcessWorkFlow(StrategyTradeWorkFlowState.CycleComplete);
                case StrategyTradeWorkFlowState.GoShortPlaceStops:
                    if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
                    {
                        TradeWorkFlow = StrategyTradeWorkFlowState.GoShortPlaceStopsPending;
                        SubmitStopLossInternal();
                        goto case StrategyTradeWorkFlowState.GoShortPlaceStopsPending;

                    }
                    else
                    {
                        if (connectionStatusOrder == ConnectionStatus.Connected)
                        {
                            TradeWorkFlow = StrategyTradeWorkFlowState.GoShortPlaceStopsPending;
                            SubmitStopLossInternal();
                        }
                        TradeWorkFlowOnMarketDataEnable();
                        //will continue to loop back here forever unless we have a timeout
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    break;
                case StrategyTradeWorkFlowState.GoShortPlaceStopsPending:

                    if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
                    {
                        TradeWorkFlow = StrategyTradeWorkFlowState.GoShortPlaceStopsConfirmed;
                        goto case StrategyTradeWorkFlowState.GoShortPlaceStopsConfirmed;
                    }

                    bool allConfirmedGoShortPlaceStops = IsOrdersAllActiveOrWorkingOrFilled(ProfitTargetOrders);


                    if (!allConfirmedGoShortPlaceStops)
                    {
                        TradeWorkFlowOnMarketDataEnable();
                        //will continue to loop back here forever unless we have a timeout
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    else
                    {
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.GoShortPlaceStopsConfirmed);
                    }
                    break;
                case StrategyTradeWorkFlowState.GoShortPlaceStopsConfirmed:
                    TradeWorkFlowOnMarketDataDisable();
                    if (SubmitProfitTargetWillOccur()) return ProcessWorkFlow(StrategyTradeWorkFlowState.GoShortPlaceProfitTargets);
                    return ProcessWorkFlow(StrategyTradeWorkFlowState.CycleComplete);
                case StrategyTradeWorkFlowState.GoShortPlaceProfitTargets:
                    if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
                    {
                        TradeWorkFlow = StrategyTradeWorkFlowState.GoShortPlaceProfitTargetsPending;
                        SubmitProfitTargetInternal();
                        goto case StrategyTradeWorkFlowState.GoShortPlaceProfitTargetsPending;

                    }
                    else
                    {
                        if (connectionStatusOrder == ConnectionStatus.Connected)
                        {
                            TradeWorkFlow = StrategyTradeWorkFlowState.GoShortPlaceProfitTargetsPending;
                            SubmitProfitTargetInternal();
                            if (!IsSubmitTargetsAndConfirm)
                                goto case StrategyTradeWorkFlowState.GoShortPlaceProfitTargetsPending;

                        }
                        TradeWorkFlowOnMarketDataEnable();
                        //will continue to loop back here forever unless we have a timeout
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    break;
                case StrategyTradeWorkFlowState.GoShortPlaceProfitTargetsPending:
                    if (IsHistoricalTradeOrPlayBack || !IsSubmitTargetsAndConfirm)
                    {
                        TradeWorkFlow = StrategyTradeWorkFlowState.GoShortPlaceProfitTargetsConfirmed;
                        goto case StrategyTradeWorkFlowState.GoShortPlaceProfitTargetsConfirmed;
                    }

                    bool allConfirmedGoShortPlaceProfitTargets = (IsOrdersAllActiveOrWorkingOrFilled(ProfitTargetOrders) || ProfitTargetOrders.Sum(o => o.Quantity) == orderEntry.Quantity);
                    //what about in the case whereby they were immediately filled etc

                    if (!allConfirmedGoShortPlaceProfitTargets)
                    {
                        TradeWorkFlowOnMarketDataEnable();
                        //will continue to loop back here forever unless we have a timeout
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    else
                    {
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.GoShortPlaceProfitTargetsConfirmed);
                    }
                    break;
                case StrategyTradeWorkFlowState.GoShortPlaceProfitTargetsConfirmed:
                    TradeWorkFlowOnMarketDataDisable();
                    return ProcessWorkFlow(StrategyTradeWorkFlowState.CycleComplete);
                //Begin GoOCO
                case StrategyTradeWorkFlowState.GoOCOLongShort:
                    //trade per direction test
                    //if (Position.MarketPosition == MarketPosition.Short) return ProcessWorkFlow(StrategyTradeWorkFlowState.GoOCOLongShortValidationRejected);

                    if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
                    {
                        TradeWorkFlow = StrategyTradeWorkFlowState.GoOCOLongShortCancelWorkingOrders;
                        goto case StrategyTradeWorkFlowState.GoOCOLongShortCancelWorkingOrders;
                    }


                    //orders to cancel test
                    //if (Historical || OrdersActiveExist() || Account.Name.ToLower() == Connection.ReplayAccountName) 
                    if (OrdersActiveExist()) return ProcessWorkFlow(StrategyTradeWorkFlowState.GoOCOLongShortCancelWorkingOrders);

                    //position to close test
                    if (Position.Quantity != 0) return ProcessWorkFlow(StrategyTradeWorkFlowState.GoOCOLongShortClosePositions);

                    //inflight entry order test
                    if (State == State.Realtime && OrdersActive.Contains(orderEntry) && (tradeWorkFlowPrior == StrategyTradeWorkFlowState.GoOCOLongShortSubmitOrderWorking || tradeWorkFlowPrior == StrategyTradeWorkFlowState.GoLongSubmitOrderWorking) && (orderEntry.OrderState == OrderState.Filled))
                    {
                        TradeWorkFlowOnMarketDataEnable();
                        //will continue to loop back here forever unless we have a timeout
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    return ProcessWorkFlow(StrategyTradeWorkFlowState.GoOCOLongShortSubmitOrder);

                case StrategyTradeWorkFlowState.GoOCOLongShortCancelWorkingOrders:
                    if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
                    {
                        CancelAllOrders();
                        TradeWorkFlow = StrategyTradeWorkFlowState.GoOCOLongShortCancelWorkingOrdersConfirmed;
                        goto case StrategyTradeWorkFlowState.GoOCOLongShortCancelWorkingOrdersConfirmed;

                    }
                    else
                    {
                        if (connectionStatusOrder == ConnectionStatus.Connected)
                        {
                            TradeWorkFlow = StrategyTradeWorkFlowState.GoOCOLongShortCancelWorkingOrdersPending;

                            CancelAllOrders();
                        }
                        TradeWorkFlowOnMarketDataEnable();
                        //will continue to loop back here forever unless we have a timeout
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    break;
                case StrategyTradeWorkFlowState.GoOCOLongShortCancelWorkingOrdersPending:
                    if (OrdersActiveExist())
                    {

                        TradeWorkFlowOnMarketDataEnable();
                        //will continue to loop back here forever unless we have a timeout
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    else
                    {
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.GoOCOLongShortCancelWorkingOrdersConfirmed);
                    }
                    break;
                case StrategyTradeWorkFlowState.GoOCOLongShortCancelWorkingOrdersConfirmed:
                    TradeWorkFlowOnMarketDataDisable();
                    if (Position.Quantity != 0) return ProcessWorkFlow(StrategyTradeWorkFlowState.GoOCOLongShortClosePositions);
                    else return ProcessWorkFlow(StrategyTradeWorkFlowState.GoOCOLongShortSubmitOrder);
                case StrategyTradeWorkFlowState.GoOCOLongShortClosePositions:
                    if (Position.Quantity != 0)
                    {
                        if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
                        {
                            TradeWorkFlow = StrategyTradeWorkFlowState.GoOCOLongShortClosedPositionsPending;
                            PositionCloseInternal();
                            break;
                        }
                        else
                        {
                            if (connectionStatusOrder == ConnectionStatus.Connected)
                            {
                                //avoid a double exit multithreaded race condition
                                if (TradeWorkFlow == StrategyTradeWorkFlowState.GoOCOLongShortClosedPositionsPending) break;
                                lock (lockObjectClose)
                                {
                                    if (TradeWorkFlow == StrategyTradeWorkFlowState.GoOCOLongShortClosedPositionsPending) break;
                                    TradeWorkFlow = StrategyTradeWorkFlowState.GoOCOLongShortClosedPositionsPending;
                                }
                                PositionCloseInternal();

                                break;
                            }
                            //if still here then connection issues must come back and try or raise alarm
                            TradeWorkFlowOnMarketDataEnable();
                            //wait for event or timer confirmation
                            tradeWorkFlowRetryCount++;
                            if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                                return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                        }
                    }
                    else
                    {
                        TradeWorkFlow = StrategyTradeWorkFlowState.GoOCOLongShortSubmitOrder;
                        goto case StrategyTradeWorkFlowState.GoOCOLongShortSubmitOrder;
                    }
                    break;
                case StrategyTradeWorkFlowState.GoOCOLongShortClosedPositionsPending:
                    //execution event or realtime event or timer moves event on

                    if (Position.Quantity != 0)
                    {
                        TradeWorkFlowOnMarketDataEnable();
                        //will continue to loop back here forever unless we have a timeout
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    else
                    {
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.GoOCOLongShortClosedPositionsConfirmed);
                    }
                    break;
                case StrategyTradeWorkFlowState.GoOCOLongShortClosedPositionsConfirmed:
                    TradeWorkFlowOnMarketDataDisable();
                    return ProcessWorkFlow(StrategyTradeWorkFlowState.GoOCOLongShortSubmitOrder);
                case StrategyTradeWorkFlowState.GoOCOLongShortSubmitOrder:
                    if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
                    {
                        if (PreTradeValidateCanEnterTradeOCO())
                        {
                            orderEntryPrior = orderEntry;
                            //orderEntry = SubmitShortTrade();
                            orderEntry = null;
                            SubmitOCOBreakoutInternal();
                        }
                        else
                        {
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.GoOCOLongShortValidationRejected);
                        }
                        break;
                    }


                    if (connectionStatusOrder == ConnectionStatus.Connected)
                    {
                        if (PreTradeValidateCanEnterTradeOCO())
                        {
                            orderEntryPrior = orderEntry;
                            //orderEntry = SubmitShortTrade();
                            orderEntry = null;
                            SubmitOCOBreakoutInternal();
                        }
                        else
                        {
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.GoOCOLongShortValidationRejected);
                        }
                    }
                    else
                    {
                        //flatten all and cancel the trade by the time it reconnects it might be too late.
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }

                    //TradeWorkFlow = StrategyTradeWorkFlowState.GoOCOLongShortSubmitOrderPending;

                    break;
                case StrategyTradeWorkFlowState.GoOCOLongShortValidationRejected:
                    if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode) return ProcessWorkFlow(StrategyTradeWorkFlowState.Waiting);
                    return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                case StrategyTradeWorkFlowState.GoOCOLongShortSubmitOrderPending:


                    if ((orderEntryOCOLong.OrderState == OrderState.Accepted || orderEntryOCOLong.OrderState == OrderState.Working)
                        && (orderEntryOCOShort.OrderState == OrderState.Accepted || orderEntryOCOShort.OrderState == OrderState.Working))
                    {
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.GoOCOLongShortSubmitOrderWorking);
                    }

                    TradeWorkFlowOnMarketDataEnable();
                    //will continue to loop back here forever unless we have a timeout
                    tradeWorkFlowRetryCount++;
                    if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);

                    break;
                case StrategyTradeWorkFlowState.GoOCOLongShortSubmitOrderWorking:

                    break;
                //End GoOCO
                case StrategyTradeWorkFlowState.ExitTradeLong:
                    if (Position.MarketPosition != MarketPosition.Long) return ProcessWorkFlow(StrategyTradeWorkFlowState.Waiting);
                    return ProcessWorkFlow(StrategyTradeWorkFlowState.ExitTrade);
                case StrategyTradeWorkFlowState.ExitTradeShort:
                    if (Position.MarketPosition != MarketPosition.Short) return ProcessWorkFlow(StrategyTradeWorkFlowState.Waiting);
                    return ProcessWorkFlow(StrategyTradeWorkFlowState.ExitTrade);
                case StrategyTradeWorkFlowState.ExitTrade:
                    //orders to cancel test

                    if (IsHistoricalTradeOrPlayBack || OrdersActiveExist()) return ProcessWorkFlow(StrategyTradeWorkFlowState.ExitTradeCancelWorkingOrders);
                    //position to close test
                    if (Position.Quantity != 0) return ProcessWorkFlow(StrategyTradeWorkFlowState.ExitTradeClosePositions);
                    //nothing to do
                    return ProcessWorkFlow(StrategyTradeWorkFlowState.CycleComplete);
                case StrategyTradeWorkFlowState.ExitTradeCancelWorkingOrders:
                    if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
                    {

                        CancelAllOrders();
                        //Assume all cancelled and move workflow onwards
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.ExitTradeCancelWorkingOrderConfirmed);
                    }
                    else
                    {
                        if (connectionStatusOrder == ConnectionStatus.Connected)
                        {
                            TradeWorkFlow = StrategyTradeWorkFlowState.ExitTradeCancelWorkingOrderPending;
                            //CancelAllTrackedOrders();
                            CancelAllOrders();

                        }
                        TradeWorkFlowOnMarketDataEnable();
                        //will continue to loop back here forever unless we have a timeout
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    break;
                case StrategyTradeWorkFlowState.ExitTradeCancelWorkingOrderPending:
                    if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.ExitTradeCancelWorkingOrderConfirmed);

                    if (OrdersActiveExist())
                    {
                        TradeWorkFlowOnMarketDataEnable();
                        //will continue to loop back here forever unless we have a timeout
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    else
                    {
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.ExitTradeCancelWorkingOrderConfirmed);
                    }
                    break;
                case StrategyTradeWorkFlowState.ExitTradeCancelWorkingOrderConfirmed:
                    TradeWorkFlowOnMarketDataDisable();
                    if (Position.Quantity != 0) return ProcessWorkFlow(StrategyTradeWorkFlowState.ExitTradeClosePositions);
                    return ProcessWorkFlow(StrategyTradeWorkFlowState.CycleComplete);
                case StrategyTradeWorkFlowState.ExitTradeClosePositions:
                    if (Position.Quantity != 0)
                    {
                        if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
                        {
                            TradeWorkFlow = StrategyTradeWorkFlowState.ExitTradeClosePositionsPending;
                            PositionCloseInternal();
                            //event will return
                        }
                        else
                        {
                            if (connectionStatusOrder == ConnectionStatus.Connected)
                            {
                                TradeWorkFlow = StrategyTradeWorkFlowState.ExitTradeClosePositionsPending;
                                PositionCloseInternal();
                            }
                            TradeWorkFlowOnMarketDataEnable();
                            //wait for event or timer confirmation
                            tradeWorkFlowRetryCount++;
                            if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                                return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                        }
                    }
                    else
                    {
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.ExitTradeClosePositionsConfirmed);
                    }
                    break;
                case StrategyTradeWorkFlowState.ExitTradeClosePositionsPending:
                    //execution event or realtime event or timer moves event on
                    if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode) break;
                    if (Position.Quantity != 0)
                    {
                        TradeWorkFlowOnMarketDataEnable();
                        //will continue to loop back here forever unless we have a timeout
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    else
                    {
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.ExitTradeClosePositionsConfirmed);
                    }
                    break;
                case StrategyTradeWorkFlowState.ExitTradeClosePositionsConfirmed:
                    if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.CycleComplete);

                    TradeWorkFlowOnMarketDataDisable();
                    if (Position.MarketPosition != MarketPosition.Flat || OrdersActiveExist())
                    {
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    return ProcessWorkFlow(StrategyTradeWorkFlowState.CycleComplete);
                case StrategyTradeWorkFlowState.ErrorTimeOut:
                    return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                case StrategyTradeWorkFlowState.Error:
                    if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
                    {
                        CancelAllOrders();
                        //Assume all cancelled and move workflow onwards
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.ErrorFlattenAll);
                    }
                    else
                    {
                        if (tracing)
                            DebugTraceHelper.OpenTraceFile();

                        if (connectionStatusOrder == ConnectionStatus.Connected)
                        {
                            CancelAllOrders();
                            TradeWorkFlow = StrategyTradeWorkFlowState.ErrorFlattenAll;
                            OrdersActive.Clear();
                            ordersStopLoss.Clear();
                            ordersProfitTarget.Clear();
                        }
                        TradeWorkFlowOnMarketDataEnable();
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                        {
                            Log("Unable to resolove error - unable to check and cancel orders - reseting alarm to try again", LogLevel.Error);
                            Log("Unable to resolve error - unable to check and cancel orders - reseting alarm to try again", LogLevel.Alert);
                            tradeWorkFlowRetryCount = 0;
                            //                            Position.Close();
                            //                            this.Disable();
                        }
                        //wait for event or timer
                    }

                    break;
                case StrategyTradeWorkFlowState.ErrorFlattenAll:
                    if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
                    {
                        TradeWorkFlow = StrategyTradeWorkFlowState.ErrorFlattenAllPending;
                        PositionCloseInternal();
                        //event will return
                    }
                    else
                    {
                        if (connectionStatusOrder == ConnectionStatus.Connected)
                        {
                            TradeWorkFlow = StrategyTradeWorkFlowState.ErrorFlattenAllPending;
                            PositionCloseInternal();
                            TradeWorkFlowOnMarketDataEnable();
                            break;
                        }

                        TradeWorkFlowOnMarketDataEnable();
                        //wait for event or timer confirmation
                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                        {
                            Log("Unable to check and close position - reseting alarm to try again", LogLevel.Error);
                            Log("Unable to check and close position - reseting alarm to try again", LogLevel.Alert);
                            tradeWorkFlowRetryCount = 0;
                        }

                    }
                    break;
                case StrategyTradeWorkFlowState.ErrorFlattenAllPending:
                    if (connectionStatusOrder == ConnectionStatus.Connected)
                    {
                        if (Position.MarketPosition == MarketPosition.Flat)
                        {
                            TradeWorkFlow = StrategyTradeWorkFlowState.ErrorFlattenAllConfirmed;
                            TradeWorkFlowOnMarketDataEnable();
                            break;
                        }

                    }
                    TradeWorkFlowOnMarketDataEnable();
                    //wait for event or timer confirmation
                    tradeWorkFlowRetryCount++;
                    if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                    {
                        Log("Unable verify ErrorFlattenAllPending - reseting alarm to try again", LogLevel.Error);
                        Log("Unable verify ErrorFlattenAllPending - reseting alarm to try again", LogLevel.Alert);
                        tradeWorkFlowRetryCount = 0;
                        Account.Flatten(new[] { this.Instrument });
                    }

                    break;
                case StrategyTradeWorkFlowState.ErrorFlattenAllConfirmed:
                    if (State == State.Realtime)
                    {
                        TradeWorkFlowOnMarketDataDisable();
                        if (OrdersActiveExist() || Position.MarketPosition != MarketPosition.Flat)
                        {
                            Log("Unable to verify ErrorFlattenAllConfirmed", LogLevel.Error);
                            Log("Unable verify ErrorFlattenAllConfirmed - reseting state to error to try again", LogLevel.Alert);
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                        }
                    }
                    return ProcessWorkFlow(StrategyTradeWorkFlowState.CycleComplete);
                case StrategyTradeWorkFlowState.ExitOnCloseOrderPending:
                    if (State == State.Realtime) break;

                    TradeWorkFlow = StrategyTradeWorkFlowState.ExitOnCloseWaitingConfirmation;
                    TradeWorkFlowOnMarketDataEnable();

                    break;
                case StrategyTradeWorkFlowState.ExitOnCloseWaitingConfirmation:
                    if (State == State.Realtime) break;

                    if (OrdersActiveExist() || Position.MarketPosition != MarketPosition.Flat)
                    {
                        TradeWorkFlowOnMarketDataEnable();

                        tradeWorkFlowRetryCount++;
                        if (tradeWorkFlowRetryCount > tradeWorkFlowRetryAlarm)
                            return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    else
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.ExitOnCloseConfirmed);
                    break;
                case StrategyTradeWorkFlowState.ExitOnCloseOrderFilled:
                    TradeWorkFlowOnMarketDataDisable();
                    return ProcessWorkFlow(StrategyTradeWorkFlowState.ExitOnCloseConfirmed);
                case StrategyTradeWorkFlowState.ExitOnCloseConfirmed:
                    if (OrdersActiveExist() || Position.MarketPosition != MarketPosition.Flat)
                    {
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.Error);
                    }
                    else
                    {
                        return ProcessWorkFlow(StrategyTradeWorkFlowState.CycleComplete);
                    }

                case StrategyTradeWorkFlowState.CycleComplete:
                    TradeWorkFlowOnMarketDataDisable();
                    TradeWorkFlow = StrategyTradeWorkFlowState.Waiting;
                    break;
                default:
                    TradeWorkFlowOnMarketDataDisable();
                    break;
            }

            return TradeWorkFlow;

        }

        public bool IsTradeWorkFlowReady()
        {
            return (TradeWorkFlow == StrategyTradeWorkFlowState.Waiting || TradeWorkFlow == StrategyTradeWorkFlowState.GoLongSubmitOrderWorking || TradeWorkFlow == StrategyTradeWorkFlowState.GoShortSubmitOrderWorking || TradeWorkFlow == StrategyTradeWorkFlowState.GoOCOLongShortSubmitOrderWorking);
        }

        public bool IsTradeWorkFlowCanGoLong()
        {
            return (TradeWorkFlow == StrategyTradeWorkFlowState.Waiting || TradeWorkFlow == StrategyTradeWorkFlowState.GoShortSubmitOrderWorking || TradeWorkFlow == StrategyTradeWorkFlowState.GoOCOLongShortSubmitOrderWorking);
        }

        public bool IsTradeWorkFlowCanGoShort()
        {
            return (TradeWorkFlow == StrategyTradeWorkFlowState.Waiting || TradeWorkFlow == StrategyTradeWorkFlowState.GoLongSubmitOrderWorking || TradeWorkFlow == StrategyTradeWorkFlowState.GoOCOLongShortSubmitOrderWorking);
        }

        public bool IsTradeWorkFlowCanExit()
        {
            return (TradeWorkFlow == StrategyTradeWorkFlowState.Waiting || TradeWorkFlow == StrategyTradeWorkFlowState.GoLongSubmitOrderWorking || TradeWorkFlow == StrategyTradeWorkFlowState.GoShortSubmitOrderWorking || TradeWorkFlow == StrategyTradeWorkFlowState.GoOCOLongShortSubmitOrderWorking);
        }

        public virtual void TradeWorkFlowTradeExitLong()
        {
            if (tracing)
                Print("TradeWorkFlowTradeExitLong()");

            lock (tradeWorkFlowExitTradeLockObject)
            {
                if (IsTradeWorkFlowCanExit())
                {
                    TradeWorkFlow = StrategyTradeWorkFlowState.ExitTradeLong;
                    ProcessWorkFlow();
                }
            }

        }

        public virtual void TradeWorkFlowTradeExitShort()
        {
            if (tracing)
                Print("TradeWorkFlowTradeExitShort()");

            lock (tradeWorkFlowExitTradeLockObject)
            {
                if (IsTradeWorkFlowCanExit())
                {
                    TradeWorkFlow = StrategyTradeWorkFlowState.ExitTradeShort;
                    ProcessWorkFlow();
                }
            }

        }

        public void TradeWorkFlowTradeExit(object state)
        {
            this.TradeWorkFlowTradeExit();
        }

        public virtual void TradeWorkFlowTradeExit()
        {
            if (tracing)
                Print("TradeWorkFlowTradeExit()");


            lock (tradeWorkFlowExitTradeLockObject)
            {
                if (IsTradeWorkFlowCanExit())
                {
                    TradeWorkFlow = StrategyTradeWorkFlowState.ExitTrade;
                    ProcessWorkFlow();
                }
            }

        }


        public virtual void TradeWorkFlowNewOrderCustom(object tradeDirection)
        {
            this.TradeWorkFlowNewOrder((StrategyTradeWorkFlowState)tradeDirection);

        }

        public virtual void TradeWorkFlowNewOrder(StrategyTradeWorkFlowState tradeDirection)
        {
            if (tracing)
                Print("TradeWorkFlowNewOrder tradeDirection " + tradeDirection.ToString());


            //Scenarios to consider
            //Enqueue GoLong WF=GoShortSubmitOrderWorking
            //Enqueue GoShort WF=Waiting - go ahead



            //if   ATSAlgoSystemState = AlgoSystemState.HisTradeRT;  then make sure the old orders are cancelled etc... as it cant see them


            lock (tradeWorkFlowNewOrderLockObject)
            {
                if (tradeDirection == StrategyTradeWorkFlowState.GoLong && !IsTradeWorkFlowCanGoLong())
                {
                    if (tracing)
                        Print("TradeWorkFlowNewOrder rejected " + tradeDirection.ToString());

                    return;
                }
                else if (tradeDirection == StrategyTradeWorkFlowState.GoShort && !IsTradeWorkFlowCanGoShort())
                {
                    if (tracing)
                        Print("TradeWorkFlowNewOrder rejected " + tradeDirection.ToString());

                    return;
                }
                tradeWorkFlowPrior = TradeWorkFlow;
                TradeWorkFlow = tradeDirection;
                ProcessWorkFlow(TradeWorkFlow);
            }
        }

        #endregion
        #region Submit Orders

        private void SubmitOCOBreakoutInternal()
        {
            entryCount++;
            oCOId = "OCO-" + Guid.NewGuid().ToString();
            orderEntryPrior = orderEntry;
            orderEntry = null;
            SubmitOCOBreakout(oCOId);
        }

        public virtual void SubmitOCOBreakout(string oCOId)
        {
            orderEntryOCOLong = SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.StopMarket, this.DefaultQuantity, 0, GetCurrentAsk(0) + 10 * TickSize, oCOId, "OCO-L");
            orderEntryOCOShort = SubmitOrderUnmanaged(0, OrderAction.SellShort, OrderType.StopMarket, this.DefaultQuantity, 0, GetCurrentBid(0) - 10 * TickSize, oCOId, "OCO-S");
        }

        private bool IsOrdersAllActiveOrWorking(List<Order> orders)
        {
            if (orders == null || orders.Count() == 0) return false;

            return orders.Count(o => o.OrderState == OrderState.Accepted || o.OrderState == OrderState.Working) == orders.Count();
        }

        private bool IsOrdersAllActiveOrWorkingOrFilled(List<Order> orders)
        {
            if (orders == null || orders.Count() == 0) return false;

            return orders.Count(o => o.OrderState == OrderState.Accepted || o.OrderState == OrderState.Working || o.OrderState == OrderState.PartFilled || o.OrderState == OrderState.Filled) == orders.Count();
        }



        public virtual bool SubmitStopLossWillOccur()
        {
            return true;
        }

        public virtual bool SubmitProfitTargetWillOccur()
        {
            return true;
        }

        private void SubmitStopLossInternal()
        {
            if (tracing)
                Print("submitStopLossInternal(" + orderEntry.ToString() + ")");

            if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
            {
                SubmitStopLoss(this.orderEntry);
                return;
            }



            //stopLossOrderConfirmations.Clear();
            StopLossOrders.Clear();
            orderStop1 = null;
            orderStop2 = null;
            orderStop3 = null;
            orderStop4 = null;

            if (State == State.Realtime && connectionStatusOrder != ConnectionStatus.Connected)
            {
                throw new Exception("submitStopLossInternal Error - no order connection - unable to submit stoploss");
            }


            SubmitStopLoss(this.orderEntry);
            //add stops to list set graphics


            if (Account.Connection == Connection.PlaybackConnection) return;

            StopLossOrders.Add(orderStop1);
            StopLossOrders.Add(orderStop2);
            StopLossOrders.Add(orderStop3);
            StopLossOrders.Add(orderStop4);


        }

        private void SubmitProfitTargetInternal()
        {
            if (tracing)
                Print("submitProfitTargetInternal(" + orderEntry.ToString() + ")");


            if (IsHistoricalTradeOrPlayBack || IsStrategyUnSafeMode)
            {
                SubmitProfitTarget(this.orderEntry, this.oCOId);
                return;
            }

            ProfitTargetOrders.Clear();
            orderTarget1 = null;
            orderTarget2 = null;
            orderTarget3 = null;
            orderTarget4 = null;

            if (State == State.Realtime && connectionStatusOrder != ConnectionStatus.Connected)
            {
                throw new Exception("submitProfitTargetInternal Error - no order connection - unable to submit profit target");
            }

            SubmitProfitTarget(this.orderEntry, this.oCOId);

            ProfitTargetOrders.Add(orderTarget1);
            ProfitTargetOrders.Add(orderTarget2);
            ProfitTargetOrders.Add(orderTarget3);
            ProfitTargetOrders.Add(orderTarget4);


        }

        private void PositionCloseInternal()
        {
            if (tracing)
                Print("PositionCloseInternal()");

            if (State == State.Realtime && connectionStatusOrder != ConnectionStatus.Connected)
            {
                throw new Exception("positionCloseInternal Error - no order connection - unable to cancel working orders and close position");
            }
            if (isLockPositionClose) return;
            lock (lockObjectPositionClose)
            {
                if (isLockPositionClose) return;
                isLockPositionClose = true;
            }
            PositionClose();
            isLockPositionClose = false;

        }

        public void Flatten(object state)
        {
            this.Flatten();
        }

        public void Flatten()
        {
            CancelAllOrders();
            PositionClose();
            TradeWorkFlow = StrategyTradeWorkFlowState.Waiting;
        }


        public virtual void PositionClose()
        {
            if (tracing)
                Print("PositionClose()");

            if (Position.MarketPosition == MarketPosition.Long)
            {
                //if(OrderIsActive(orderClose))
                string orderEntryName = orderEntry != null ? orderEntry.Name.Replace(arrowUp, string.Empty) : "Long";
                orderEntryName = orderEntryName.Substring(3);
                orderClose = null;
                orderClose = SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Market, Position.Quantity, 0, 0, string.Empty, arrowDown + closeOrderName + orderEntryName);
                //orderClose = SubmitOrder(0, OrderAction.Sell, OrderType.Market, Position.Quantity, 0, 0, string.Empty, );
            }
            else if (Position.MarketPosition == MarketPosition.Short)
            {
                string orderEntryName = orderEntry != null ? orderEntry.Name.Replace(arrowDown, string.Empty) : "Short";
                orderEntryName = orderEntryName.Substring(3);
                orderClose = null;
                //orderClose = SubmitOrder(0, OrderAction.BuyToCover, OrderType.Market, Position.Quantity, 0, 0, string.Empty, arrowUp + closeOrderName + orderEntryName);
                orderClose = SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Market, Position.Quantity, 0, 0, string.Empty, arrowUp + closeOrderName + orderEntryName);
            }
            else if (Position.Quantity != 0)
            {
                Position.Close();
            }
        }

        public Order SubmitShortTrade(bool isUser = false)
        {


            if (State == State.Realtime && connectionStatusOrder != ConnectionStatus.Connected)
            {
                throw new Exception("submitShortInternal Error - no order connection - unable to submit short order");
            }

            //check for short states working or open
            if (Position.MarketPosition == MarketPosition.Short || (OrderIsActive(orderEntry) && orderEntry.OrderAction == OrderAction.SellShort)) return null;
            string signal = entry1NameShort + "M#" + entryCount.ToString() + (IsHistorical ? ".H" : isUser ? ".U" : string.Empty);

            //set the new trade operation state
            entryCount++;
            oCOId = Guid.NewGuid().ToString();

            orderEntryPrior = orderEntry;
            orderEntry = null;

            orderEntryName = signal;

            orderEntry = SubmitShort(signal);

            if (tracing)
                Print("SubmitShortTrade() >> " + orderEntry.ToString());


            return orderEntry;
        }

        public Order SubmitLongTrade(bool isUser = false)
        {



            if (State == State.Realtime && connectionStatusOrder != ConnectionStatus.Connected)
            {
                throw new Exception("submitLongInternal Error - no order connection - unable to submit short order");
            }


            //check for long states working or open
            if (Position.MarketPosition == MarketPosition.Long || (OrderIsActive(orderEntry) && orderEntry.OrderAction == OrderAction.Buy)) return null;
            string signal = entry1NameLong + "M#" + entryCount.ToString() + (IsHistorical ? ".H" : isUser ? ".U" : string.Empty);

            orderEntryName = signal;
            //set the new trade operation state
            entryCount++;
            oCOId = Guid.NewGuid().ToString();

            orderEntryPrior = orderEntry;
            orderEntry = null;
            orderEntry = SubmitLong(signal);

            if (tracing)
                Print("SubmitLongTrade() >> " + orderEntry.ToString());

            return orderEntry;
        }

        public virtual void SubmitProfitTarget(Order orderEntry, string oCOId)
        {
            if (orderEntry.OrderAction == OrderAction.Buy)
            {
                string orderEntryName = orderEntry != null ? orderEntry.Name.Replace(arrowUp, string.Empty) : "Long";
                orderEntryName = orderEntryName.Substring(3);
                orderTarget1 = SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Limit, 1, orderEntry.AverageFillPrice + 6 * TickSize, 0, orderEntryName + ".OCO1." + oCOId, arrowDown + target1Name + orderEntryName);
                orderTarget2 = SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Limit, 1, orderEntry.AverageFillPrice + 10 * TickSize, 0, orderEntryName + ".OCO2." + oCOId, arrowDown + target2Name + orderEntryName);
                orderTarget3 = SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Limit, 1, orderEntry.AverageFillPrice + 16 * TickSize, 0, orderEntryName + ".OCO3." + oCOId, arrowDown + target3Name + orderEntryName);
                orderTarget4 = SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Limit, 1, orderEntry.AverageFillPrice + 24 * TickSize, 0, orderEntryName + ".OCO4." + oCOId, arrowDown + target4Name + orderEntryName);
            }
            else if (orderEntry.OrderAction == OrderAction.SellShort)
            {
                string orderEntryName = orderEntry != null ? orderEntry.Name.Replace(arrowDown, string.Empty) : "Short";
                orderEntryName = orderEntryName.Substring(3);
                orderTarget1 = SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Limit, 1, orderEntry.AverageFillPrice - 6 * TickSize, 0, orderEntryName + ".OCO1." + oCOId, arrowUp + target1Name + orderEntryName);
                orderTarget2 = SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Limit, 1, orderEntry.AverageFillPrice - 10 * TickSize, 0, orderEntryName + ".OCO2." + oCOId, arrowUp + target2Name + orderEntryName);
                orderTarget3 = SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Limit, 1, orderEntry.AverageFillPrice - 16 * TickSize, 0, orderEntryName + ".OCO3." + oCOId, arrowUp + target3Name + orderEntryName);
                orderTarget4 = SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Limit, 1, orderEntry.AverageFillPrice - 24 * TickSize, 0, orderEntryName + ".OCO4." + oCOId, arrowUp + target4Name + orderEntryName);
            }
        }

        public virtual void SubmitStopLoss(Order orderEntry)
        {
            if (orderEntry.OrderAction == OrderAction.Buy)
            {
                string orderEntryName = orderEntry != null ? orderEntry.Name.Replace(arrowUp, string.Empty) : "Long";
                orderEntryName = orderEntryName.Substring(3);

                orderStop1 = SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.StopMarket, 1, orderEntry.AverageFillPrice - 16 * TickSize, orderEntry.AverageFillPrice - 16 * TickSize, orderEntryName + ".OCO1." + oCOId, arrowDown + stop1Name + orderEntryName);
                orderStop2 = SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.StopMarket, 1, orderEntry.AverageFillPrice - 18 * TickSize, orderEntry.AverageFillPrice - 16 * TickSize, orderEntryName + ".OCO2." + oCOId, arrowDown + stop2Name + orderEntryName);
                orderStop3 = SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.StopMarket, 1, orderEntry.AverageFillPrice - 20 * TickSize, orderEntry.AverageFillPrice - 16 * TickSize, orderEntryName + ".OCO3." + oCOId, arrowDown + stop3Name + orderEntryName);
                orderStop4 = SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.StopMarket, 1, orderEntry.AverageFillPrice - 22 * TickSize, orderEntry.AverageFillPrice - 16 * TickSize, orderEntryName + ".OCO4." + oCOId, arrowDown + stop4Name + orderEntryName);
            }
            else if (orderEntry.OrderAction == OrderAction.SellShort)
            {
                string orderEntryName = orderEntry != null ? orderEntry.Name.Replace(arrowDown, string.Empty) : "Short";
                orderEntryName = orderEntryName.Substring(3);

                orderStop1 = SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.StopMarket, 1, orderEntry.AverageFillPrice + 16 * TickSize, orderEntry.AverageFillPrice + 16 * TickSize, orderEntryName + ".OCO1." + oCOId, arrowUp + stop1Name + orderEntryName);
                orderStop2 = SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.StopMarket, 1, orderEntry.AverageFillPrice + 18 * TickSize, orderEntry.AverageFillPrice + 16 * TickSize, orderEntryName + ".OCO2." + oCOId, arrowUp + stop2Name + orderEntryName);
                orderStop3 = SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.StopMarket, 1, orderEntry.AverageFillPrice + 20 * TickSize, orderEntry.AverageFillPrice + 16 * TickSize, orderEntryName + ".OCO3." + oCOId, arrowUp + stop3Name + orderEntryName);
                orderStop4 = SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.StopMarket, 1, orderEntry.AverageFillPrice + 22 * TickSize, orderEntry.AverageFillPrice + 16 * TickSize, orderEntryName + ".OCO4." + oCOId, arrowUp + stop4Name + orderEntryName);
            }
        }

        public virtual Order SubmitShort(string signal)
        {
            orderEntry = SubmitOrderUnmanaged(0, OrderAction.SellShort, OrderType.Market, this.DefaultQuantity, 0, 0, String.Empty, signal);
            return orderEntry;
        }

        public virtual Order SubmitLong(string signal)
        {
            orderEntry = SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.Market, this.DefaultQuantity, 0, 0, String.Empty, signal);
            return orderEntry;
        }


        #endregion
        #region  Logging Tracing
        public void Print(string msg)
        {

            PrintTo = PrintTo.OutputTab1;


            base.Print(string.Format("{0}:>{1}:> {2}", ATSAlgoSystemState.ToString(), TradeWorkFlow.ToString(), msg));



            PrintTo = PrintTo.OutputTab2;
            string txt = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff")
            + " " + this.Account.Name
            + " " + this.Name
            + " " + this.Instrument.FullName
            + " " + (string.IsNullOrEmpty(Thread.CurrentThread.Name) ? "CurrentThread.Name.?" : Thread.CurrentThread.Name);

            if (State > State.DataLoaded && State < State.Terminated)
            {
                txt += "|DS=" + (this.Bars != null ? " " + this.Bars.ToChartString() : string.Empty);

                if (this.Bars != null && Bars.Count > 0)
                {
                    txt += "|BT=" + Time[0].ToString("yyyy.MM.dd HH:mm:ss:fff")
                    + "|HR=" + (IsHistorical ? "H" : "R")
                    + "|CB=" + CurrentBar.ToString()
                    + "|LC=" + Close[0].ToString();
                }
                txt += "|RX=" + Executions.Count.ToString()
                + "|RO=" + Orders.Count.ToString()
                + "|MP=" + Position.MarketPosition.ToString()
                + "|PQ=" + Position.Quantity.ToString()
                + "|AO=" + OrdersActive.Count.ToString()
                + "|WF=" + this.tradeWorkFlow.ToString()
                + "|S=" + ATSAlgoSystemState.ToString()
                + "|: "
                + msg;
            }
            else
            {
                txt += "|PreInit|: " + msg;
            }

            base.Print(txt);

            if (tracing)
                TraceToFile(txt);

        }

        public new void Log(string msg, LogLevel logLevel)
        {
            NinjaScript.Log(msg, logLevel);
            if (tracing)
                TraceToFile(msg);

        }


        public void TraceToFile(string msg)
        {
            DebugTraceHelper.WriteLine(msg);

#if DEBUG
            Debug.Print(msg);
#endif
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }


        #endregion
        #region DateTime

        private DateTime tEQNextTimeValid = DateTime.MinValue;
        private DateTime tradeWorkFlowNextTimeValid = DateTime.MinValue;
        private DateTime now = Core.Globals.Now;


        [Browsable(false)]
        public DateTime Now
        {
            get
            {
                now = (Cbi.Connection.PlaybackConnection != null ? Cbi.Connection.PlaybackConnection.Now : Core.Globals.Now);

                if (now.Millisecond > 0)
                    now = Core.Globals.MinDate.AddSeconds((long)Math.Floor(now.Subtract(Core.Globals.MinDate).TotalSeconds));

                return now;
            }
        }
        #endregion

        #endregion
        #region properties





        private bool stratCanTrade = true;

        [XmlIgnore, Browsable(false)]
        public virtual bool StratCanTrade
        {
            get
            {
                return this.stratCanTrade;
            }
            set
            {
                if (value != this.stratCanTrade)
                {
                    this.stratCanTrade = value;
                    this.NotifyPropertyChanged("StratCanTrade");
                }
            }
        }

        private bool stratCanTradeLong = true;

        [XmlIgnore, Browsable(false)]
        public virtual bool StratCanTradeLong
        {
            get
            {
                return this.stratCanTradeLong;
            }
            set
            {
                if (value != this.stratCanTradeLong)
                {
                    this.stratCanTradeLong = value;
                    this.NotifyPropertyChanged("StratCanTradeLong");
                }
            }
        }

        private bool stratCanTradeShort = true;

        [XmlIgnore, Browsable(false)]
        public virtual bool StratCanTradeShort
        {
            get
            {
                return this.stratCanTradeShort;
            }
            set
            {
                if (value != this.stratCanTradeShort)
                {
                    this.stratCanTradeShort = value;
                    this.NotifyPropertyChanged("StratCanTradeShort");
                }
            }
        }



        private AlgoSystemState aTSAlgoSystemState = AlgoSystemState.None;

        [XmlIgnore, Browsable(false)]
        public AlgoSystemState ATSAlgoSystemState
        {
            get
            {
                return this.aTSAlgoSystemState;
            }
            set
            {
                if (value != this.aTSAlgoSystemState)
                {
                    this.aTSAlgoSystemState = value;
                    if (aTSAlgoSystemState == AlgoSystemState.HisTradeRT)
                        SystemState = "Hist.RT!";
                    else
                        SystemState = this.aTSAlgoSystemState.ToString();
                }
            }

        }

        private AlgoSystemMode aTSAlgoSystemMode = AlgoSystemMode.UnKnown;
        [XmlIgnore, Browsable(false)]
        public AlgoSystemMode ATSAlgoSystemMode
        {
            get
            {
                return this.aTSAlgoSystemMode;
            }
            set
            {
                if (value != this.aTSAlgoSystemMode)
                {
                    this.aTSAlgoSystemMode = value;
                    SystemMode = aTSAlgoSystemMode.ToString();
                }
            }
        }




        private string systemMode = AlgoSystemMode.UnKnown.ToString();
        [XmlIgnore, Browsable(false)]
        public string SystemMode
        {
            get
            {
                return this.systemMode;
            }
            set
            {
                if (value != this.systemMode)
                {
                    this.systemMode = value;
                    this.NotifyPropertyChanged("SystemMode");
                }
            }
        }

        private string systemState = AlgoSystemState.None.ToString();
        [XmlIgnore, Browsable(false)]
        public string SystemState
        {
            get
            {
                return this.systemState;
            }
            set
            {
                if (value != this.systemState)
                {
                    this.systemState = value;
                    this.NotifyPropertyChanged("SystemState");
                }
            }

        }


        private string instrumentFullName = string.Empty;

        [XmlIgnore, Browsable(false)]
        public string InstrumentFullName
        {
            get
            {
                return this.instrumentFullName;
            }
            set
            {
                if (value != this.instrumentFullName)
                {
                    this.instrumentFullName = value;
                    this.NotifyPropertyChanged("InstrumentFullName");
                }
            }
        }



        private string positionInfo = "Algo Trade Manager";
        [XmlIgnore, Browsable(false)]
        public string PositionInfo
        {
            get
            {
                return this.positionInfo;
            }
            set
            {
                if (value != this.positionInfo)
                {
                    this.positionInfo = value;
                    this.NotifyPropertyChanged("PositionInfo");
                }
            }
        }


        private int positionState = 0;

        [XmlIgnore, Browsable(false)]
        public int PositionState
        {
            get { return positionState; }
            set
            {
                if (value != this.positionState)
                {
                    this.positionState = value;
                    this.NotifyPropertyChanged("PositionState");
                }
            }

        }


        private double unrealizedPL = 0;

        [XmlIgnore, Browsable(false)]
        public double UnRealizedPL
        {
            get
            {
                return this.unrealizedPL;
            }
            set
            {
                if (value != this.unrealizedPL)
                {
                    this.unrealizedPL = value;
                    //this.NotifyPropertyChanged("PositionState");
                    this.NotifyPropertyChanged("UnRealizedPL");
                    this.NotifyPropertyChanged("UnRealizedPLString");
                }
            }
        }

        [XmlIgnore, Browsable(false)]
        public string UnRealizedPLString
        {
            get
            {

                if (PositionState != 0)
                    return Core.Globals.FormatCurrency(UnRealizedPL, accountDenomination);
                else return string.Empty;
            }

        }

        [XmlIgnore, Browsable(false)]
        public string AskPriceString
        {
            get { return FormatPriceMarker(AskPrice); }
        }

        [XmlIgnore, Browsable(false)]
        public string BidPriceString
        {
            get { return FormatPriceMarker(BidPrice); }
        }


        [XmlIgnore, Browsable(false)]
        public double LastPrice
        {
            get
            {
                return this.lastPrice;
            }
            set
            {
                if (value != this.lastPrice)
                {
                    this.lastPrice = value;
                    this.NotifyPropertyChanged("LastPriceString");
                }
            }
        }

        [XmlIgnore, Browsable(false)]
        public string LastPriceString
        {
            get { return FormatPriceMarker(LastPrice); }
        }



        private double askPrice = 0;
        [XmlIgnore, Browsable(false)]
        public double AskPrice
        {
            get
            {
                return this.askPrice;
            }
            set
            {
                if (value != this.askPrice)
                {
                    this.askPrice = value;
                    this.NotifyPropertyChanged("AskPriceString");
                }
            }
        }

        private double bidPrice = 0;

        [XmlIgnore, Browsable(false)]
        public double BidPrice
        {
            get
            {
                return this.bidPrice;
            }
            set
            {
                if (value != this.bidPrice)
                {
                    this.bidPrice = value;
                    this.NotifyPropertyChanged("BidPriceString");

                }
            }
        }





        [Display(GroupName = "Zystem Params", Order = 0, Name = "Strategy Error Handling - OrderCancelStopsOnly", Description = "MT Errors - Error Handling Order Cancel Stops Only -  when using \"Error Handling Single or Batch Order Cancel\"=true - the trade engine will cancel only stop loss exits so that OCOC's are handled broker side for the cancellation of the profit target - avoiding some superflous messaages and rejects for some brokers")]
        public bool IsOrderCancelStopsOnly
        {
            get { return orderCancelStopsOnly; }
            set { orderCancelStopsOnly = value; }
        }

        [Display(GroupName = "Zystem Params", Order = 0, Name = "Strategy Error Handling - Single or Batch Order Cancel", Description = "MT Errors - Error Handling Single or Batch Order Cancel -  when set to true when reversing or cancelling - the trade engine will inspect the state of eaach order that is working before cancelling - also only cancelling OCO brackets one side the stops - this is slower but will help deal with order rejection notices from some brokers  - where by the order is not cancellable, part filled, filled or rejected - it is faster to use batch mode - but less resourceful and can cause unneccessary order messages- false - errors flagged by the trade platform in that mode can be handled by setting parameter \"Error Raise On All Order Rejects\"=false and specifying the error code to trap and ignore in the list in the parameter \"Error Native Errors To Ignore\"=error1|error2|etc")]
        public bool IsOrderCancelInspectEachOrDoBatchCancel
        {
            get { return orderCancelInspectEachOrDoBatchCancel; }
            set { orderCancelInspectEachOrDoBatchCancel = value; }
        }

        [Display(GroupName = "Zystem Params", Order = 0, Name = "Strategy Error Handling - RaiseErrorOnAllOrderRejects", Description = "MT Errors - Raise Error On All Order Rejects or just when a pending order was rejected but ignoring if the error scenario was in the list of exceptions in \"Error Native Errors To Ignore\"")]
        public bool IsRaiseErrorOnAllOrderRejects
        {
            get { return raiseErrorOnAllOrderRejects; }
            set { raiseErrorOnAllOrderRejects = value; }
        }

        [Display(GroupName = "Zystem Params", Order = 0, Name = "Strategy Error Handling - RealtimeErrorHandling", Description = "RealtimeErrorHandling")]
        public new RealtimeErrorHandling RealtimeErrorHandling
        {
            get { return base.RealtimeErrorHandling; }
            set { base.RealtimeErrorHandling = value; }
        }



        [Display(GroupName = "Zystem Params", Order = 0, Name = "DEBUG - IsTracingMode", Description = "Tracing mode true or false - DEBUG developer usage only")]
        public bool IsTracingMode
        {
            get { return tracing; }
            set { tracing = value; }
        }

        [Display(GroupName = "Zystem Params", Order = 0, Name = "Trade Engine - Strategy Realtime Trading Use Queue", Description = "Realtime Trading Use Queued Signals: True/False - When using small timeseries or every tick this can smooth out performance as signals are queued and the last in is executed others are purged")]
        public bool IsRealtimeTradingUseQueue
        {
            get { return useQueue; }
            set { useQueue = value; }
        }

        [Display(GroupName = "Zystem Params", Order = 0, Name = "Trade Engine - Strategy Trade Signal Expiry Period", Description = "For any buffered signals due to a series of fast reversals or pending order actions - invalidate and Ignore trade signals of age longer than 3 seconds")]
        public int TradeSignalExpiryInterval
        {
            get; set;

        }


        private int tradeWorkFlowTimeOut = 10;
        private bool entryOrderInFlightCollision;

        [Display(GroupName = "Zystem Params", Order = 0, Name = "Trade Engine - S TradeWorkFlowTimeOut", Description = "Trade Work Flow Time Out Seconds 10 - if  Workflow is stuck for time out seconds set state to error 1 to 30 seconds.")]
        public int TradeWorkFlowTimeOut
        {
            get { return tradeWorkFlowTimeOut; }
            set { tradeWorkFlowTimeOut = Math.Max(1, Math.Min(30, value)); }
        }



        [Display(GroupName = "Zystem Params", Order = 0, Name = "Trade Engine - MS TradeWorkFlowTimerInterval", Description = "Trade Work Flow Timer Interval Seconds 1 to 10")]
        public int TradeWorkFlowTimerInterval
        {
            get { return tradeWorkFlowTimerInterval; }
            set { tradeWorkFlowTimerInterval = Math.Max(100, Math.Min(3000, value)); }
        }

        [Display(GroupName = "Zystem Params", Order = 0, Name = "Trade Engine - MS TradeWorkFlowTimerInterval Reset", Description = "Trade Work Flow Timer Cycle Reset to allow new trade workflow action - Interval Seconds 1 to 10")]
        public int TradeWorkFlowTimerIntervalReset
        {
            get { return tradeWorkFlowTimerIntervalReset; }
            set { tradeWorkFlowTimerIntervalReset = Math.Max(10, Math.Min(1000, value)); }
        }



        [Display(GroupName = "Zystem Params", Order = 0, Name = "Trade Engine - TradeWorkFlowRetryAlarm", Description = "Trade Work Flow Retry Alarm 1 to 5 - the number of times to wait for confirmation or retry an action before going to Error state - cancel all and flatten")]
        public int TradeWorkFlowRetryAlarm
        {
            get { return tradeWorkFlowRetryAlarm; }
            set { tradeWorkFlowRetryAlarm = Math.Max(1, Math.Min(5, value)); }
        }

        [Display(GroupName = "Zystem Params", Order = 0, Name = "Trade Engine - TEQTimerInterval", Description = "Trade Event Timer Interval Seconds 1 to 5")]
        public int TEQTimerInterval
        {
            get { return tEQTimerInterval; }
            set { tEQTimerInterval = Math.Max(1, Math.Min(5, value)); }
        }

        [Display(GroupName = "Zystem Params", Order = 0, Name = "Trade Engine - IsStrategyUnSafeMode", Description = "WARNING do not use this!!! IsStrategyUnSafeMode allows faster operation for scalping and less latency on entry - however this mode is only for very competent experienced traders as this can result in oder fills and unexpected position fills and order balances... Always keep this off unless you fully understand the risks are yours, and you are an experienced trader in attendance and plan to interact and control any order issues, positions which might result in unsafe mode due to fast market reversal and other anomalies")]
        public bool IsStrategyUnSafeMode { get; set; }





        [Display(GroupName = "Zystem Params", Order = 0, Name = "Trade Engine - RealtimeTradingOnly", Description = "Realtime Trading Only True or False")]
        public bool IsRealtimeTradingOnly
        {
            get { return realtimeTradingOnly; }
            set { realtimeTradingOnly = value; }
        }

        [Display(GroupName = "Zystem Params", Order = 0, Name = "Trade Engine - IsSubmitTargetsAndConfirm", Description = "Confirm Target Placement or skip")]
        public bool IsSubmitTargetsAndConfirm { get; set; }



        [Display(GroupName = "Zystem Params", Order = 0, Name = "Visuals - ShowOrderLabels", Description = "Show Entry Order Labels on chart")]
        public bool IsShowOrderLabels
        {
            get { return showOrderLabels; }
            set { showOrderLabels = value; }
        }




        #region Non browsable


        [Browsable(false)]
        [XmlIgnore()]
        protected Queue<AlgoSignalActionMsq> TEQ
        {
            get
            {
                return this.q;
            }
        }



        [Browsable(false)]
        [XmlIgnore()]
        public DateTime TradeWorkFlowLastChanged { get; private set; }

        [Browsable(false)]
        [XmlIgnore()]
        public int TradeWorkFlowLastChangedBar { get; private set; }


        [Browsable(false)]
        [XmlIgnore()]
        public StrategyTradeWorkFlowState TradeWorkFlow
        {
            get { return tradeWorkFlow; }
            set
            {
                if (tradeWorkFlow != value)
                {
                    TradeWorkFlowLastChanged = DateTime.Now;
                    tradeWorkFlowRetryCount = 0;
                    TradeWorkFlowLastChangedBar = CurrentBars[0];

                    if (tracing)
                        Print("StrategyTradeWorkFlowStates=" + value.ToString());

                    tradeWorkFlow = value;
                    OnStrategyTradeWorkFlowUpdated(new StrategyTradeWorkFlowUpdatedEventArgs(value));
                }

            }
        }

        [Browsable(false)]
        [XmlIgnore()]
        public List<Order> OrdersActive
        {
            get
            {
                return ordersRT;
            }
        }

        [Browsable(false)]
        [XmlIgnore()]
        public List<Order> StopLossOrders
        {
            get
            {
                return ordersStopLoss;
            }
        }

        [Browsable(false)]
        [XmlIgnore()]
        public List<Order> ProfitTargetOrders
        {
            get
            {
                return ordersProfitTarget;
            }
        }

        [Browsable(false)]
        [XmlIgnore()]
        public bool IsHistorical { get { return State == State.Historical; } }

        [Browsable(false)]
        [XmlIgnore()]
        public bool IsHistoricalTradeOrPlayBack { get { return State == State.Historical || ATSAlgoSystemState == AlgoSystemState.HisTradeRT || IsPlayBack; } }


        [Browsable(false)]
        [XmlIgnore()]
        public bool IsPlayBack { get { return Account.Connection == Connection.PlaybackConnection; } }


        [Browsable(false)]
        [XmlIgnore()]
        public MarketDataEventArgs MarketDataUpdate { get; private set; }


        #endregion

        #endregion
    }

}