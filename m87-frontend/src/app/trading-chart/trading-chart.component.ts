import { Component, OnInit, OnDestroy, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { createChart, IChartApi, ISeriesApi, Time } from 'lightweight-charts';
import { SignalRService, PriceUpdate, CandleUpdate } from '../services/signalr.service';
import { Subscription } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Candle } from '../models/candle.model';

@Component({
  selector: 'app-trading-chart',
  templateUrl: './trading-chart.component.html',
  styleUrls: ['./trading-chart.component.scss'],
  standalone: true,
  imports: [FormsModule, CommonModule]
})
export class TradingChartComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('chartContainer') chartContainer!: ElementRef<HTMLDivElement>;

  private chart!: IChartApi;
  private candleSeries!: ISeriesApi<'Candlestick'>;
  public candleBuffer: Candle | null = null;
  private lastCandleStartTime!: number;

  private priceUpdateSubscription!: Subscription;
  private candleUpdateSubscription!: Subscription;

  public selectedTimeframe: string = '1m';
  public availableTimeframes: string[] = ['1m', '5m', '15m', '30m', '60m'];

  constructor(private signalRService: SignalRService) {}

  ngOnInit(): void {
    this.signalRService.startConnection();

    this.priceUpdateSubscription = this.signalRService.priceUpdates$.subscribe((update: PriceUpdate) => {
      if (update.stockSymbol !== 'AAPL') return;
      this.handlePriceUpdate(update);
    });

    this.candleUpdateSubscription = this.signalRService.candleUpdates$.subscribe((update: CandleUpdate) => {
      if (update.stockSymbol !== 'AAPL') return;
      if (update.timeframe === this.selectedTimeframe) {
        this.addFinalizedCandle(update.candle);
      }
    });
  }

  ngAfterViewInit(): void {
    this.initializeChart();
  }

  ngOnDestroy(): void {
    this.signalRService.stopConnection();
    if (this.priceUpdateSubscription) this.priceUpdateSubscription.unsubscribe();
    if (this.candleUpdateSubscription) this.candleUpdateSubscription.unsubscribe();
  }

  private initializeChart(): void {
    if (!this.chartContainer) return;
    this.chart = createChart(this.chartContainer.nativeElement, {
      width: this.chartContainer.nativeElement.clientWidth,
      height: 600,
      layout: { textColor: '#000000' },
      grid: { vertLines: { color: '#e1e1e1' }, horzLines: { color: '#e1e1e1' } },
    });

    this.candleSeries = this.chart.addCandlestickSeries({
      upColor: '#4CAF50',
      downColor: '#F44336',
      borderDownColor: '#F44336',
      borderUpColor: '#4CAF50',
      wickDownColor: '#F44336',
      wickUpColor: '#4CAF50',
    });

    window.addEventListener('resize', this.resizeChart.bind(this));
  }

  /** ✅ Correctly aligns price updates to selected timeframe */
  private handlePriceUpdate(update: PriceUpdate): void {
    const currentTime = Math.floor(new Date(update.timestamp).getTime() / 1000);
    const candleStartTime = this.alignToTimeframe(currentTime, this.selectedTimeframe);

    if (!this.candleBuffer || candleStartTime !== this.lastCandleStartTime) {
      // Start a new candle
      this.candleBuffer = {
        time: candleStartTime as Time,
        open: update.price,
        high: update.price,
        low: update.price,
        close: update.price
      };
      this.lastCandleStartTime = candleStartTime;
    } else {
      // Update existing candle
      this.candleBuffer.high = Math.max(this.candleBuffer.high, update.price);
      this.candleBuffer.low = Math.min(this.candleBuffer.low, update.price);
      this.candleBuffer.close = update.price;
    }

    this.candleSeries.update(this.candleBuffer);
  }

  /** ✅ Adds finalized candle to the chart */
  private addFinalizedCandle(candle: Candle): void {
    this.candleSeries.update({
      time: candle.time as Time,
      open: candle.open,
      high: candle.high,
      low: candle.low,
      close: candle.close
    });

    console.log(`Finalized candle added for timeframe ${this.selectedTimeframe} at ${candle.time}`);

    this.candleBuffer = null;
  }

  /** ✅ Aligns timestamps to selected timeframe */
  private alignToTimeframe(timestamp: number, timeframe: string): number {
    const timeframeInSeconds = this.getTimeframeSeconds(timeframe);
    return timestamp - (timestamp % timeframeInSeconds);
  }

  /** ✅ Converts timeframe string to seconds */
  private getTimeframeSeconds(timeframe: string): number {
    switch (timeframe) {
      case '1m': return 60;
      case '5m': return 300;
      case '15m': return 900;
      case '30m': return 1800;
      case '60m': return 3600;
      default: return 60;
    }
  }

  public onTimeframeChange(newTimeframe: string): void {
    console.log(`Timeframe changed to: ${newTimeframe}`);
    this.selectedTimeframe = newTimeframe;

    this.chart.remove();
    this.initializeChart();

    this.candleBuffer = null;
  }

  private resizeChart(): void {
    if (this.chart && this.chartContainer) {
      this.chart.applyOptions({ width: this.chartContainer.nativeElement.clientWidth });
    }
  }

  public formatTimeframe(timeframe: string): string {
    switch (timeframe) {
      case '1m': return '1 Min';
      case '5m': return '5 Min';
      case '15m': return '15 Min';
      case '30m': return '30 Min';
      case '60m': return '60 Min';
      default: return timeframe;
    }
  }
}
