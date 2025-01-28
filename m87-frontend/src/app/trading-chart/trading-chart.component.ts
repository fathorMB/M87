// src/app/trading-chart/trading-chart.component.ts
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
    // Inizializza la connessione SignalR tramite il servizio
    this.signalRService.startConnection();

    // Sottoscrivi agli aggiornamenti di prezzo
    this.priceUpdateSubscription = this.signalRService.priceUpdates$.subscribe((update: PriceUpdate) => {
      console.log(`Ricevuto aggiornamento: ${update.stockSymbol} - ${update.price} - ${update.timestamp}`);
      const time = new Date(update.timestamp).getTime() / 1000; // TradingView utilizza timestamp in secondi

      // Verifica se esiste già una candela per questo timestamp
      const existingCandleIndex = this.data.findIndex(candle => candle.time === time);
      if (existingCandleIndex !== -1) {
        // Aggiorna la candela esistente
        const existingCandle = this.data[existingCandleIndex];
        existingCandle.close = update.price;
        existingCandle.high = Math.max(existingCandle.high, update.price);
        existingCandle.low = Math.min(existingCandle.low, update.price);
      } else {
        // Crea una nuova candela
        this.data.push({
          time: time as Time,
          open: update.price,
          high: update.price,
          low: update.price,
          close: update.price
        });
      }

      // Aggiorna il grafico
      this.candleSeries.setData(this.data);
    });
  }

  ngAfterViewInit(): void {
    // Inizializza il grafico dopo che il view è stato inizializzato
    this.chart = createChart(this.chartContainer.nativeElement, {
      width: this.chartContainer.nativeElement.clientWidth,
      height: 300 // Puoi rendere questa dinamica se necessario
    });
    this.candleSeries = this.chart.addCandlestickSeries();

    // Adatta il grafico alle dimensioni del contenitore
    window.addEventListener('resize', this.resizeChart.bind(this));
  }

  ngOnDestroy(): void {
    // Disconnetti SignalR e annulla la sottoscrizione
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
