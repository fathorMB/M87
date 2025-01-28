import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TradingChartComponent } from './trading-chart/trading-chart.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, TradingChartComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'm87-frontend';
}
