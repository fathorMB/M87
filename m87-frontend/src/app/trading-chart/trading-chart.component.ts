// File: m87-frontend/src/app/trading-chart/trading-chart.component.ts

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
  private candleSeriesMap: Map<string, ISeriesApi<'Candlestick'>> = new Map();
  private candleUpdateSubscription!: Subscription;
  private priceUpdateSubscription!: Subscription;
  private lastCandleTimeMap: Map<string, Time> = new Map();

  public selectedTimeframe: string = '1m'; // Default timeframe
  public availableTimeframes: string[] = ['1m', '5m', '15m', '30m', '60m'];

  // To store the current candle data for the selected timeframe
  private currentCandle: Candle | null = null;

  constructor(private signalRService: SignalRService) { }

  ngOnInit(): void {
    // Initialize SignalR connection
    this.signalRService.startConnection();

    // Subscribe to CandleUpdates
    this.candleUpdateSubscription = this.signalRService.candleUpdates$.subscribe((update: CandleUpdate) => {
      console.log(`TradingChartComponent - Received CandleUpdate: ${update.timeframe} - ${update.candle.time}`);

      if (update.stockSymbol !== 'AAPL') return; // Modify if handling multiple stocks

      const timeframe = update.timeframe;
      const candle = update.candle;

      // Only process updates for the selected timeframe
      if (timeframe !== this.selectedTimeframe) return;

      // Finalize and add the completed candle to the chart
      const series = this.candleSeriesMap.get(timeframe);
      if (series) {
        series.update({
          time: candle.time as Time,
          open: candle.open,
          high: candle.high,
          low: candle.low,
          close: candle.close
        });
        console.log(`TradingChartComponent - Added completed candle for ${timeframe} at ${candle.time}`);
      }
    });

    // Subscribe to PriceUpdates to update the current candle in real-time
    this.priceUpdateSubscription = this.signalRService.priceUpdates$.subscribe((update: PriceUpdate) => {
      console.log(`TradingChartComponent - Received PriceUpdate: ${update.stockSymbol} - ${update.price} - ${update.timestamp}`);

      if (update.stockSymbol !== 'AAPL') return; // Modify if handling multiple stocks

      const timeframe = this.selectedTimeframe;
      const price = update.price;

      // Initialize the series if it doesn't exist
      if (!this.candleSeriesMap.has(timeframe)) {
        const series = this.chart.addCandlestickSeries({
          upColor: '#4CAF50',
          downColor: '#F44336',
          borderDownColor: '#F44336',
          borderUpColor: '#4CAF50',
          wickDownColor: '#F44336',
          wickUpColor: '#4CAF50',
        });
        this.candleSeriesMap.set(timeframe, series);
        console.log(`TradingChartComponent - Added series for timeframe: ${timeframe}`, series);
      }

      const series = this.candleSeriesMap.get(timeframe);

      if (!this.currentCandle) {
        // Create the initial current candle immediately
        this.currentCandle = {
          time: Math.floor(new Date(update.timestamp).getTime() / 1000) as Time,
          open: price,
          high: price,
          low: price,
          close: price
        };
        series?.update(this.currentCandle);
        this.lastCandleTimeMap.set(timeframe, this.currentCandle.time);
        console.log(`TradingChartComponent - Created initial candle for ${timeframe} at ${this.currentCandle.time}`);
      } else {
        // Update the current candle's high, low, and close
        if (price > this.currentCandle.high) this.currentCandle.high = price;
        if (price < this.currentCandle.low) this.currentCandle.low = price;
        this.currentCandle.close = price;

        series?.update({
          time: this.currentCandle.time,
          open: this.currentCandle.open,
          high: this.currentCandle.high,
          low: this.currentCandle.low,
          close: this.currentCandle.close
        });

        console.log(`TradingChartComponent - Updated current candle for ${timeframe} at ${this.currentCandle.time}`);
      }
    });
  }

  ngAfterViewInit(): void {
    // Initialize the chart after the view is initialized
    this.chart = createChart(this.chartContainer.nativeElement, {
      width: this.chartContainer.nativeElement.clientWidth,
      height: 600, // Adjust height as needed
      layout: {
        textColor: '#000000',
      },
      grid: {
        vertLines: {
          color: '#e1e1e1',
        },
        horzLines: {
          color: '#e1e1e1',
        },
      },
    });

    console.log('TradingChartComponent - Chart initialized:', this.chart);

    // Ensure the chart resizes with the window
    window.addEventListener('resize', this.resizeChart.bind(this));
  }

  ngOnDestroy(): void {
    // Clean up subscriptions and connections
    this.signalRService.stopConnection();
    if (this.candleUpdateSubscription) {
      this.candleUpdateSubscription.unsubscribe();
    }
    if (this.priceUpdateSubscription) {
      this.priceUpdateSubscription.unsubscribe();
    }
    window.removeEventListener('resize', this.resizeChart.bind(this));
  }

  private resizeChart(): void {
    if (this.chart && this.chartContainer) {
      this.chart.applyOptions({ width: this.chartContainer.nativeElement.clientWidth });
      console.log('TradingChartComponent - Chart resized');
    }
  }

  public onTimeframeChange(newTimeframe: string): void {
    console.log(`TradingChartComponent - Timeframe changed to: ${newTimeframe}`);
    this.selectedTimeframe = newTimeframe;

    // Hide all series except the selected timeframe
    this.candleSeriesMap.forEach((series, timeframe) => {
      if (timeframe === newTimeframe) {
        series.applyOptions({ visible: true });
        console.log(`TradingChartComponent - Showed series for timeframe: ${timeframe}`);
      } else {
        series.applyOptions({ visible: false });
        console.log(`TradingChartComponent - Hid series for timeframe: ${timeframe}`);
      }
    });

    // Reset the current candle if changing timeframe
    const previousTimeframe = Array.from(this.candleSeriesMap.keys()).find(tf => tf !== newTimeframe);
    if (previousTimeframe) {
      this.currentCandle = null;
      this.lastCandleTimeMap.set(newTimeframe, undefined as unknown as Time);
      console.log(`TradingChartComponent - Reset current candle for timeframe: ${newTimeframe}`);
    }
  }

  public formatTimeframe(timeframe: string): string {
    switch (timeframe) {
      case 'tick':
        return 'Tick';
      case '1m':
        return '1 Minuto';
      case '5m':
        return '5 Minuti';
      case '15m':
        return '15 Minuti';
      case '30m':
        return '30 Minuti';
      case '60m':
        return '60 Minuti';
      default:
        return timeframe;
    }
  }

  // Debug method to log data (currently not implemented)
  public logData(): void {
    console.log('TradingChartComponent - Data logging not implemented.');
  }
}
