// File: m87-frontend/src/app/models/candle.model.ts

import { Time } from 'lightweight-charts';

export interface Candle {
  time: Time; // Unix timestamp in secondi, string o Date
  open: number;
  high: number;
  low: number;
  close: number;
}
