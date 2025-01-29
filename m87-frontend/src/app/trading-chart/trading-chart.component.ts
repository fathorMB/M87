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

  public selectedTimeframe: string = '1m'; // Timeframe predefinito
  public availableTimeframes: string[] = ['1m', '5m', '15m', '30m', '60m'];

  constructor(private signalRService: SignalRService) { }

  ngOnInit(): void {
    // Inizializza la connessione SignalR
    this.signalRService.startConnection();

    // Sottoscrizione agli aggiornamenti delle candele
    this.candleUpdateSubscription = this.signalRService.candleUpdates$.subscribe((update: CandleUpdate) => {
      console.log(`TradingChartComponent - Ricevuto CandleUpdate: ${update.timeframe} - ${update.candle.time}`);
      if (update.stockSymbol !== 'AAPL') return; // Modificare se si gestiscono più stock

      const timeframe = update.timeframe;
      const candle = update.candle;

      // Verifica se la serie per il timeframe esiste, altrimenti crea una nuova serie
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
        console.log(`TradingChartComponent - Serie aggiunta per timeframe: ${timeframe}`, series);
      }

      const series = this.candleSeriesMap.get(timeframe);

      // Aggiungi la nuova candela al grafico
      series?.update({
        time: candle.time as Time, // Cast esplicito a Time
        open: candle.open,
        high: candle.high,
        low: candle.low,
        close: candle.close
      });

      console.log(`TradingChartComponent - Candela aggiornata per timeframe ${timeframe}:`, candle);

      // Opzionale: rimuovi dati vecchi per limitare il numero di candele
      // Implementa se necessario
    });
  }

  ngAfterViewInit(): void {
    // Inizializza il grafico dopo che la vista è stata inizializzata
    this.chart = createChart(this.chartContainer.nativeElement, {
      width: this.chartContainer.nativeElement.clientWidth,
      height: 300, // Può essere reso dinamico se necessario
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

    console.log('TradingChartComponent - Grafico inizializzato:', this.chart);

    // Inizializza la serie di candele per il timeframe predefinito
    // Ora viene gestita dinamicamente al primo aggiornamento
    // Non è più necessario creare una serie qui

    // Assicurati che il grafico si ridimensiona con la finestra
    window.addEventListener('resize', this.resizeChart.bind(this));
  }

  ngOnDestroy(): void {
    // Pulisci le sottoscrizioni e le connessioni
    this.signalRService.stopConnection();
    if (this.candleUpdateSubscription) {
      this.candleUpdateSubscription.unsubscribe();
    }
    window.removeEventListener('resize', this.resizeChart.bind(this));
  }

  private resizeChart(): void {
    if (this.chart && this.chartContainer) {
      this.chart.applyOptions({ width: this.chartContainer.nativeElement.clientWidth });
      console.log('TradingChartComponent - Grafico ridimensionato');
    }
  }

  public onTimeframeChange(newTimeframe: string): void {
    console.log(`TradingChartComponent - Timeframe cambiato a: ${newTimeframe}`);
    this.selectedTimeframe = newTimeframe;

    // Nascondi tutte le serie
    this.candleSeriesMap.forEach((series, timeframe) => {
      series.applyOptions({ visible: false });
      console.log(`TradingChartComponent - Serie nascosta: ${timeframe}`);
    });

    // Mostra la serie del timeframe selezionato
    const selectedSeries = this.candleSeriesMap.get(newTimeframe);
    if (selectedSeries) {
      selectedSeries.applyOptions({ visible: true });
      console.log(`TradingChartComponent - Serie mostrata: ${newTimeframe}`);
    } else {
      // Se la serie non esiste ancora, verrà creata quando arriverà il primo CandleUpdate
      console.log(`TradingChartComponent - Serie non esiste ancora per timeframe: ${newTimeframe}`);
      // La serie verrà creata al primo aggiornamento
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

  // Metodo di debug per loggare i dati
  public logData(): void {
    console.log('TradingChartComponent - Attualmente non supportato il log dei dati cumulativi.');
  }
}
