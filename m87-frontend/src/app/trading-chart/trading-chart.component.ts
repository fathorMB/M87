// trading-chart.component.ts

import { Component, OnInit, OnDestroy, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { createChart, IChartApi, ISeriesApi, Time } from 'lightweight-charts';
import { SignalRService, PriceUpdate } from '../services/signalr.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-trading-chart',
  templateUrl: './trading-chart.component.html',
  styleUrls: ['./trading-chart.component.scss'],
  standalone: true
})
export class TradingChartComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('chartContainer') chartContainer!: ElementRef<HTMLDivElement>;

  private chart!: IChartApi;
  private candleSeries!: ISeriesApi<'Candlestick'>;
  private data: { time: Time, open: number, high: number, low: number, close: number }[] = [];
  private priceUpdateSubscription!: Subscription;

  constructor(private signalRService: SignalRService) { }

  ngOnInit(): void {
    // Initialize SignalR connection
    this.signalRService.startConnection();

    // Subscribe to price updates
    this.priceUpdateSubscription = this.signalRService.priceUpdates$.subscribe((update: PriceUpdate) => {
      console.log(`Received Price Update: ${update.stockSymbol} - ${update.price} - ${update.timestamp}`);
      const time = Math.floor(new Date(update.timestamp).getTime() / 1000); // Convert to Unix timestamp in seconds

      // Find if a candle already exists for this timestamp
      const existingCandleIndex = this.data.findIndex(candle => candle.time === time);
      if (existingCandleIndex !== -1) {
        // Update existing candle
        const existingCandle = this.data[existingCandleIndex];
        existingCandle.close = update.price;
        existingCandle.high = Math.max(existingCandle.high, update.price);
        existingCandle.low = Math.min(existingCandle.low, update.price);
        console.log(`Updated Candle at ${time}:`, existingCandle);
      } else {
        // Add new candle
        const newCandle = {
          time: time as Time,
          open: update.price,
          high: update.price,
          low: update.price,
          close: update.price
        };
        this.data.push(newCandle);
        console.log(`Added New Candle:`, newCandle);
      }

      // Update the chart with the new data
      this.candleSeries.setData(this.data);
    });
  }

  ngAfterViewInit(): void {
    // Initialize the chart after the view has been initialized
    this.chart = createChart(this.chartContainer.nativeElement, {
      width: this.chartContainer.nativeElement.clientWidth,
      height: 300, // You can make this dynamic if needed
      layout: {
        //backgroundColor: '#ffffff',
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
    this.candleSeries = this.chart.addCandlestickSeries();

    // Optionally, set initial data or styling
    this.candleSeries.applyOptions({
      upColor: '#4CAF50',
      downColor: '#F44336',
      borderDownColor: '#F44336',
      borderUpColor: '#4CAF50',
      wickDownColor: '#F44336',
      wickUpColor: '#4CAF50',
    });

    // Ensure the chart resizes with the window
    window.addEventListener('resize', this.resizeChart.bind(this));
  }

  ngOnDestroy(): void {
    // Clean up subscriptions and connections
    this.signalRService.stopConnection();
    if (this.priceUpdateSubscription) {
      this.priceUpdateSubscription.unsubscribe();
    }
    window.removeEventListener('resize', this.resizeChart.bind(this));
  }

  private resizeChart(): void {
    if (this.chart && this.chartContainer) {
      this.chart.applyOptions({ width: this.chartContainer.nativeElement.clientWidth });
    }
  }
}
