// File: m87-frontend/src/app/services/signalr.service.ts

import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../environment';
import { Candle } from '../models/candle.model';

export interface PriceUpdate {
  stockSymbol: string;
  price: number;
  timestamp: string; // ISO string
}

export interface CandleUpdate {
  stockSymbol: string;
  timeframe: string;
  candle: Candle;
}

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection!: HubConnection;
  public priceUpdates$: Subject<PriceUpdate> = new Subject();
  public candleUpdates$: Subject<CandleUpdate> = new Subject();

  constructor() { }

  public startConnection(): void {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(environment.signalRUrl)
      .configureLogging(LogLevel.Information)
      .build();

    this.hubConnection.start()
      .then(() => {
        console.log('SignalR connection started');
        // Optionally, notify the backend to start sending data
      })
      .catch(err => console.error('Error while starting SignalR connection: ', err));

    // Listen for PriceUpdates
    this.hubConnection.on('ReceivePriceUpdate', (stockSymbol: string, price: number, timestamp: string) => {
      console.log(`Received PriceUpdate: ${stockSymbol} - ${price} - ${timestamp}`);
      this.priceUpdates$.next({ stockSymbol, price, timestamp });
    });

    // Listen for CandleUpdates
    this.hubConnection.on('ReceiveCandleUpdate', (stockSymbol: string, timeframe: string, candle: any) => {
      // Ensure that candle.time is a number
      if (typeof candle.time !== 'number') {
        console.error('Received Candle with invalid time:', candle.time);
        return;
      }

      const validatedCandle: Candle = {
        time: candle.time,
        open: candle.open,
        high: candle.high,
        low: candle.low,
        close: candle.close
      };

      console.log(`Received CandleUpdate: ${stockSymbol} - ${timeframe} - ${validatedCandle.time}`);
      this.candleUpdates$.next({ stockSymbol, timeframe, candle: validatedCandle });
    });
  }

  public stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop()
        .then(() => console.log('SignalR connection stopped'))
        .catch(err => console.error('Error while stopping SignalR connection: ', err));
    }
  }
}
