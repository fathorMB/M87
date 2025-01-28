// File: m87-frontend/src/app/services/signalr.service.ts

import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../environment';
import { Candle } from '../models/candle.model';

export interface PriceUpdate {
  stockSymbol: string;
  price: number;
  timestamp: string;
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
        console.log('Connessione SignalR avviata');
        // Opzionale: Invia un messaggio al backend per confermare la connessione
      })
      .catch(err => console.error('Errore di connessione SignalR: ', err));
  
    this.hubConnection.on('ReceivePriceUpdate', (stockSymbol: string, price: number, timestamp: string) => {
      console.log(`Ricevuto PriceUpdate: ${stockSymbol} - ${price} - ${timestamp}`);
      this.priceUpdates$.next({ stockSymbol, price, timestamp });
    });
  
    this.hubConnection.on('ReceiveCandleUpdate', (stockSymbol: string, timeframe: string, candle: Candle) => {
      console.log(`Ricevuto CandleUpdate: ${stockSymbol} - ${timeframe} - ${candle.time}`);
      this.candleUpdates$.next({ stockSymbol, timeframe, candle });
    });
  }

  public stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop()
        .then(() => console.log('Connessione SignalR terminata'))
        .catch(err => console.error('Errore durante la disconnessione SignalR: ', err));
    }
  }
}
