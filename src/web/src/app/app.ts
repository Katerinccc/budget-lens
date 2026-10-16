import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TranslocoDirective } from '@jsverse/transloco';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, TranslocoDirective],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {}
