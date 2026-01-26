// Angular (nesta configuração) precisa do Zone.js em runtime.
// Sem este import, a aplicação compila e sobe, mas quebra no navegador (NG0908).
import 'zone.js';

import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app';

bootstrapApplication(AppComponent, appConfig)
  .catch(err => console.error(err));
