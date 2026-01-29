import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Endereco } from './api.models';

@Injectable({ providedIn: 'root' })
export class EnderecoApi {
  constructor(private http: HttpClient) {}

  getByCep(cep: string) {
    const safe = (cep ?? '').replace(/\D/g, '');
   
    return this.http.get<Endereco>(`https://viacep.com.br/ws/${safe}/json/`);
  }
}
