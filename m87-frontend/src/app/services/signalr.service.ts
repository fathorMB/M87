import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../environment';

export interface PriceUpdate {
  stockSymbol: string;
  price: number;
  timestamp: string;
}

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection!: HubConnection;
  public priceUpdates$: Subject<PriceUpdate> = new Subject();

  constructor() { }

  public startConnection(): void {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(environment.signalRUrl) // Assicurati che l'URL corrisponda al tuo server SignalR
      .configureLogging(LogLevel.Information)
      .build();

    this.hubConnection.start()
      .then(() => console.log('Connessione SignalR avviata'))
      .catch(err => console.error('Errore di connessione SignalR: ', err));

    this.hubConnection.on('ReceivePriceUpdate', (stockSymbol: string, price: number, timestamp: string) => {
      this.priceUpdates$.next({ stockSymbol, price, timestamp });
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
